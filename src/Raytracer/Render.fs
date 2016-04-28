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
/// <param name=pixel>The pixel through which the ray was shot.</param>
/// <param name=ray>The ray to check intersection with on the shapes.</param>
/// <returns>
/// A tuple pair, containing the pixel that the ray was shot through,
/// and a list of hitpoints.
/// </returns>
let getHitpoints shapes (pixel, ray) = async {
    return pixel, List.fold (fun acc s -> Shape.hitFunction ray s @ acc) [] shapes
}

/// </summary>
/// Given a list of hitpoints, find the closest one.
/// </summary>
let getClosestHitpoint hps =
    let (fst, hps) = match hps with
                     | hp :: hps -> (hp, hps)
                     | _         -> failwith "Expected hitpoints"

    let folder (hpb, db) hp =
        let d = Shape.getHitDistance hp
        if d < db then (hp, d) else (hpd, db)

    List.fold folder (fst, Shape.getHitDistance fst) hps

/// <summary>
/// Given a shadow point and a list of light, construct a list of tuples
/// containing the lights and the direction vector to the light.
/// </summary>
let getLightVectors shp ls =
    let folder lvs l =
        (l, Light.getVector shp l) :: lvs

    List.fold folder [] ls

let getShadingColors shp lvs =
    let folder cls (l, lv) =
        match lv with
        // In case of the null vector, we've encountered an ambient light.
        | Vector.make 0.0 0.0 0.0 -> // Return some color, intensity, and 1 dot product
        | _                       -> // Return some color, intensity, and n dot product

    List.fold folder [] lvs

/// <summary>
/// </summary>
let setColor lights (pixel, hitpoints) =
    // Get the closest hitpoint and the distance to it.
    let (ch, chd) = getClosestHitpoint hitpoints

    // Get the camera lookat direction vector.
    let cld = Vector.normalise (Point.distance camOrigin camLookat)

    // Get the point along the ray the we hit.
    let hp = Point.move camOrigin (cld * chd)

    // Get the direction of the hit normal.
    let hnd = Vector.normalise (Shape.getHitNormal ch)

    // Get the point from which we should cast the shadow ray.
    let shp = Point.move hitPoint (hnd * 0.0001)

    // For each of the lights, get the vectors
    let lvs = getLightVectors shp lights

    let cls = getShadingColors shp lvs

    let shadowHits =
        |> List.map (fun lv -> Ray.make shadowPoint lv)
        |> List.fold (fun acc sr -> (sr, (getHitpoints shapes (pixel, sr))) :: acc) []

    let color =
        if List.isEmpty hitpoints then (new SolidBrush(Color.Black)) else
          let (_, _, material) = Shape.getHitpoint closestHitpoint
          let mColor = Material.getColor material
          let rec scaleColor =
              match shadowHits with
              | [(sr, [])] :: ss ->
                      let scalefactor = Vector.dotProduct (Ray.getVector sr) (Shape.getHitNormal closestHitpoint)
                      let scaledColor = Color.scale mColor scalefactor
                      let R = Color.getR scaledColor
                      let G = Color.getG scaledColor
                      let B = Color.getB scaledColor

                      new SolidBrush(Color.FromArgb(int (R * 255.), int (G * 255.), int (B * 255.)))
              | [(sr, _)] :: ss -> new SolidBrush(Color.FromArgb(int (R * 255.), int (G * 255.), int (B * 255.)))

    let x = pixel % pwidth
    let y = pixel / pheight
    g.FillRectangle(color, x, y, 1, 1)

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

    let gridRays =
        seq { for i in 0..((pwidth * pheight) - 1) ->
                let currentRow = i / pheight
                // Move the point to work with to the right by i amount
                let p'' = Point.move p' ((W * ((float (i % pwidth)) + 0.5)) * r)
                // Move the point down onto the current row
                let p'' = Point.move p'' ((H * ((float currentRow) + 0.5)) * d)

                (i, Ray.make camOrigin (Point.distance camOrigin p'')) }

    // Map the async task to the gridRays sequence in parallel and wait for it.
    let hits = gridRays |> Seq.map (getHitpoints shapes)
                        |> Async.Parallel
                        |> Async.RunSynchronously

    // Map the setColor function to the hits sequence. Ignore the output.
    Seq.map setColor lights hits |> Seq.iter ignore

    bitmap
