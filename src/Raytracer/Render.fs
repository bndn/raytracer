module Render

open System.Drawing

open Camera
open Color
open Light
open Point
open Ray
open Scene
open Vector

/// </summary>
/// Given a list of hitpoints, the closest one is found.
/// </summary>
/// <param name=hitpoints>The list of hitpoints.</param>
/// <returns>The hitpoint with the closest hit distance.</returns>
let getClosestHitpoint = function
    | hp :: hps ->
        let folder (hpb, db) hp =
            let d = Shape.getHitDistance hp
            if d < db then (hp, d) else (hpb, db)

        Some (List.fold folder (hp, Shape.getHitDistance hp) hps)
    | _ -> None

/// <summary>
/// Get the hitpoints, for each shape, for each pixel and ray pair.
/// </summary>
/// <param name=ss>The shapes to check hitpoints on.</param>
/// <param name=r>The ray to check intersection with on the shapes.</param>
/// <param name=md>The maximum distance to check for hitpoints.</param>
/// <returns>The list of hit points.</returns>
let getHitpoints ss r md =
    let folder hps s =
        let hits = Shape.hitFunction r s

        match getClosestHitpoint hits with
        | Some (chp, chd) -> if chd <= md then hits @ hps else hps
        | None -> hps

    List.fold folder [] ss

/// <summary>
/// Given a shadow point and a list of light, construct a list of tuples
/// containing the lights and the direction vector to the light.
/// </summary>
/// <param name=shp>The shadow origin point from which light vectors are created.</param>
/// <param name=ls>The list of lights to create light vectors for.</param>
/// <returns>A list of lightvectors from one shadow origin point to each light source in the scene.</returns>
let getLightVectors shp ls =
    let folder lvs l =
        (l, Light.getVector shp l) :: lvs

    List.fold folder [] ls

/// <summary>
/// Given a shadowpoint and its lightvectors we find the color, intensity and
/// dot product value for each light source.
/// </summary>
/// <param name=ss>The shapes in the scene.</param>
/// <param name=chp>The point that was hit.</param>
/// <param name=shp>The shadow origin point.</param>
/// <param name=lvs>The light vectors for the given shadow origin point.</param>
/// <returns>A list of triples (one triple for each light source) containing values for color mixing.</returns>
let getShadingColors ss chp shp lvs =
    let folder cls (l, lv) =
        let c = Light.getColor l
        let i = Light.getIntensity l

        match lv with
        // In case of the null vector, we've encountered an ambient light.
        // The dot product of ambient lights will always be 1.
        | v when v = Vector.make 0. 0. 0. -> (c, i, 1.) :: cls
        | _ ->
            // Construct a ray from the shadow point in the direction of the light.
            let r = Ray.make shp lv

            let hn = Shape.getHitNormal chp
            let dp = max 0. (Vector.normalise lv * hn)

            // Gets the magnitude of the light vector to use as maxDistance
            // for getting hitpoints.
            let lvd = Vector.magnitude lv

            // Check if the ray hit any shapes on its way to the light source.
            match getHitpoints ss r lvd with
            | [] -> (c, i, dp) :: cls
            | _  -> (c, i, 0.) :: cls

    List.fold folder [] lvs

/// <summary>
/// Mixes the color of a hitpoint with the shading colors from the light sources.
/// </summary>
/// <param name=c>The original color containing RGB values.</param>
/// <param name=cs>A list of triples representing the shading colors.</param>
/// <returns>A mixed color.</returns>
let mixShadingColors c cs =
    let folder (r, g, b) (c', i, dp) =
        let r' = Color.getR c'
        let g' = Color.getG c'
        let b' = Color.getB c'

        let f = dp * i

        (r + (r' * f), g + (g' * f), b + (b' * f))

    let (rf, gf, bf) = List.fold folder (0., 0., 0.) cs

    (Color.getR c * rf, Color.getG c * gf, Color.getB c * bf)

/// <summary>
/// Gets the color to shade and finds out which values to mix the color with.
/// </summary>
/// <param name=ss>A list of shapes in the scene.</param>
/// <param name=ls>A list of lights in the scene.</param>
/// <param name=r>A ray in the scene.</param>
/// <param name=hps>A list of hitpoints for a ray in the scene.</param>
/// <returns>A SolidBrush with the final mixed color.</returns>
let getColor ss ls (r, hps) =
    // Get the closest hitpoint and the distance to it.
    match getClosestHitpoint hps with
    | Some (chp, chd) ->
        // Get the ray normalised vector.
        let rv = Ray.getVector r

        // Get the ray origin point
        let ro = Ray.getOrigin r

        // Get the point along the ray that we hit.
        let hp = Point.move ro (chd * rv)

        // Get the direction of the hit normal.
        // OBS: The normalisation should occur in Shape, and should not be needed below. It is just a precaution.
        let hnd = Vector.normalise (Shape.getHitNormal chp)

        // Get the point from which we should cast the shadow ray.
        let shp = Point.move hp (0.0001 * hnd)

        // For each of the lights, get the vectors
        let lvs = getLightVectors shp ls

        // Get the colors that the pixel should be shaded by.
        let cls = getShadingColors ss chp shp lvs

        // Get the material that the ray hit.
        let hm = Shape.getHitMaterial chp

        // Get the color of the material that the ray hit.
        let mc = Material.getColor hm

        let (r', g', b') = mixShadingColors mc cls

        let r' = max 0. (min 255. (r' * 255.))
        let g' = max 0. (min 255. (g' * 255.))
        let b' = max 0. (min 255. (b' * 255.))

        new SolidBrush(Color.FromArgb(int r', int g', int b'))

    | None -> new SolidBrush(Color.FromArgb(0, 0, 0))

/// <summary>
/// Render a scene to a Bitmap object.
/// </summary>
/// <param name=scene>The scene to render.</param>
/// <returns>The rendered bitmap of the scene.</returns>
let render scene =
    let cam = Scene.getCamera scene
    let (camOrigin, camLookat, camUp, zoom, uwidth, uheight, pwidth, pheight) = Camera.getCamera cam
    let shapes = Scene.getShapes scene
    let lights = Scene.getLights scene
    let bitmap = new Bitmap(pwidth, pheight)

    // The Graphics object allows us to manipulate the bitmap.
    let g = Graphics.FromImage(bitmap)

    // Find l (direction between p and q)
    let l = Point.direction camOrigin camLookat
    // Find r, d and z
    let r = Vector.normalise (Vector.crossProduct camUp l)
    let d = Vector.normalise (Vector.crossProduct r l)
    let z = zoom * l

    let W = uwidth / (float pwidth)
    let H = uheight / (float pheight)

    let p' = Point.move camOrigin z
    // Move to top
    let p' = Point.move p' ((uheight / 2.) * camUp)
    // Move to top left
    let p' = Point.move p' ((-uwidth / 2.) * r)

    let gridRays = seq {
        for p in 0..((pwidth * pheight) - 1) ->
            let currentRow = p / pheight

            // Move the point to work with to the right by i amount
            let p'' = Point.move p' ((W * ((float (p % pwidth)) + 0.5)) * r)
            // Move the point down onto the current row
            let p'' = Point.move p'' ((H * ((float currentRow) + 0.5)) * d)

            (p, Ray.make camOrigin (Point.distance camOrigin p''))
    }

    let mapper ss ls (p, r) = async {
      return p, getColor ss ls (r, getHitpoints ss r infinity)
    }

    // Map the async task to the gridRays sequence in parallel and wait for it.
    let colors = gridRays |> Seq.map (mapper shapes lights)
                          |> Async.Parallel
                          |> Async.RunSynchronously

    for (p, c) in colors do
        g.FillRectangle(c, p % pwidth, p / pheight, 1, 1)

    bitmap
