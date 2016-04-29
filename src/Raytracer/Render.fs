module Render

open System.Drawing

open Camera
open Color
open Light
open Point
open Ray
open Scene
open Vector

/// <summary>
/// Get the hitpoints, for each shape, for each pixel and ray pair.
/// Flattens the hitpoints list such that information regarding
/// which shape was hit, is dropped.
/// </summary>
/// <param name=shapes>The shapes to check hitpoints on.</param>
/// <param name=ray>The ray to check intersection with on the shapes.</param>
/// <returns>The list of hit points.</returns>
let getHitpoints shapes ray = List.fold (fun acc s -> Shape.hitFunction ray s @ acc) [] shapes

/// </summary>
/// Given a list of hitpoints, find the closest one.
/// </summary>
let getClosestHitpoint = function
    | hp :: hps ->
        let folder (hpb, db) hp =
            let d = Shape.getHitDistance hp
            if d < db then (hp, d) else (hpb, db)

        Some (List.fold folder (hp, Shape.getHitDistance hp) hps)
    | _ -> None

/// <summary>
/// Given a shadow point and a list of light, construct a list of tuples
/// containing the lights and the direction vector to the light.
/// </summary>
let getLightVectors shp ls =
    let folder lvs l =
        (l, Light.getVector shp l) :: lvs

    List.fold folder [] ls

/// <summary>
/// Given a shadowpoint and its lightvectors we find the color, intensity and
/// dot product value for each light source.
/// </summary>
/// <param name=ss>The shapes in the scene.</param>
/// <param name=p>The pixel to shade.</param>
/// <param name=chp>The point that was hit.</param>
/// <param name=shp>The shadow origin point.</param>
/// <param name=lv>The light vector.</param>
let getShadingColors ss hp shp lvs =
    let folder cls (l, lv) =
        let c = Light.getColor l
        let i = Light.getIntensity l

        match lv with
        // In case of the null vector, we've encountered an ambient light. The
        // dot product of ambient lights will always be 1.
        | v when v = Vector.make 0. 0. 0. -> (c, i, 1.) :: cls
        | _ ->
            // Construct a ray from the shadow point in the direction of the light.
            let r = Ray.make shp lv

            let dp = max 0. (lv * Shape.getHitNormal hp)

            // Check if the ray hit any shapes on its way to the light source.
            match getHitpoints ss r with
            | [] -> (c, i, dp) :: cls
            | _  -> (c, i, 0.) :: cls

    List.fold folder [] lvs

/// <summary>
/// </summary>
let mixShadingColors cl cls =
    let r = Color.getR cl
    let g = Color.getG cl
    let b = Color.getB cl

    let folder (r, g, b) (cl', i, dp) =
        let r' = Color.getR cl'
        let g' = Color.getG cl'
        let b' = Color.getB cl'

        let f = dp * i

        (r + (r' * f), g + (g' * f), b + (b' * f))

    let (rf, gf, bf) = List.fold folder (0., 0., 0.) cls

    (r * rf, g * gf, b * bf)

/// <summary>
///
/// </summary>
let setColor ss ls (g:Graphics) c (p, hps) =
    let cp = Camera.getPosition c
    let cl = Camera.getLookat c
    let pw = Camera.getPixelWidth c
    let ph = Camera.getPixelHeight c

    // Get the closest hitpoint and the distance to it.
    match getClosestHitpoint hps with
    | Some (chp, chd) ->
        // Get the camera lookat direction vector.
        let cld = Vector.normalise (Point.distance cp cl)

        // Get the point along the ray that we hit.
        let hp = Point.move cp (chd * cld)

        // Get the direction of the hit normal.
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

        let c = new SolidBrush(Color.FromArgb(int r', int g', int b'))

        g.FillRectangle(c, p % pw, p / ph, 1, 1)

    | None ->
        let c = new SolidBrush(Color.FromArgb(0, 0, 0))

        g.FillRectangle(c, p % pw, p / ph, 1, 1)

/// <summary>
/// Render a scene to a Bitmap object.
/// </summary>
/// <param name=s>The scene to render.</param>
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
    let z = Vector.multScalar l zoom

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

    let mapper ss (p, r) = async {
        return p, getHitpoints ss r
    }

    // Map the async task to the gridRays sequence in parallel and wait for it.
    let hits = gridRays |> Seq.map (mapper shapes)
                        |> Async.Parallel
                        |> Async.RunSynchronously

    // Map the setColor function to the hits sequence. Ignore the output.
    Seq.map (setColor shapes lights g cam) hits |> Seq.iter ignore

    bitmap
