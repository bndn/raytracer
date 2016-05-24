/// Copyright (C) 2016 The Authors.
module Scene

open Shape
open Light
open Kdtree

[<NoComparison>]
type Scene = Scene of Shape list * Shape kdtree * Light list

[<Literal>]
let GammaFactor = 2.

/// <summary>
/// Create a scene with a list of shapes and lights.
/// </summary>
/// <param name=ss>The shapes in the scene.</param>
/// <param name=ls>The lights in the scene.</param>
/// <returns>The constructed scene.</returns>
let make ss ls =
    if List.isEmpty ss
    then invalidArg "ss" "Cannot be empty"

    if List.isEmpty ls
    then invalidArg "ls" "Cannot be empty"

    let folder (ss, ts) s =
        match Shape.bounds s with
        | Some b -> (ss, (s, b) :: ts)
        | None   -> (s :: ss, ts)

    let (ss, ts) = List.fold folder ([], []) ss

    Scene(ss, Kdtree.make ts, ls)

/// <summary>
/// Get the list of shapes in a scene.
/// </summary>
/// <param name=s>The scene to get the shapes from.</param>
/// <returns>The list of shapes in the scene.</returns>
let getShapes = function Scene(ss, _, _) -> ss

/// <summary>
/// Get the list of lights in a scene.
/// </summary>
/// <param name=s>The scene to get the lights from.</param>
/// <returns>The list of lights in the scene.</returns>
let getLights = function Scene(_, _, ls) -> ls

/// </summary>
/// Find the closest of a list of hitpoints.
/// </summary>
/// <param name=hps>The list of hitpoints.</param>
/// <returns>The hitpoint with the closest hit distance.</returns>
let getClosestHitpoint = function
    | hp :: hps ->
        let folder (hpb, db) hp =
            let d = Shape.hitDistance hp
            if d < db then (hp, d) else (hpb, db)

        Some (List.fold folder (hp, Shape.hitDistance hp) hps)
    | _ -> None

/// <summary>
/// Given a ray, find all hitpoints within a scene.
/// </summary>
/// <param name=r>The ray to check.</param>
/// <param name=d>The maximum distance that the ray travels.</param>
/// <param name=s>The scene to check.</param>
/// <returns>The list of hit points.</returns>
let getHitpoints r d (Scene(ss, ts, _)) =
    let folder d hps s =
        let hits = Shape.hit r s

        match getClosestHitpoint hits with
        | Some(_, chd) -> if chd <= d then hits @ hps else hps
        | None -> hps

    let traverser es =
        match Array.fold (folder d) [] es with
        | []  -> None
        | hps -> Some hps

    let hps = List.fold (folder d) [] ss

    match Kdtree.traverse traverser r ts with
    | Some hp -> hp @ hps
    | None    -> hps

/// <summary>
/// Creates a point that is placed slightly outside the shape hit by a ray.
/// </summary>
/// <param name=r>The given ray.</param>
/// <param name=hp>The hitpoint of the ray.</param>
/// <param name=hd>The hit distance.</param>
/// <returns>A Point placed slight outside the shape.</returns>
let getRetracePoint r hp hd =
    // Get the ray normalised vector.
    let rv = Ray.getVector r

    // Get the ray origin point
    let ro = Ray.getOrigin r

    // Get the point along the ray that we hit.
    let p = Point.move ro (hd * rv)

    // Get the direction of the hit normal.
    let hnd = Shape.hitNormal hp

    // Get the point from which we should retrace the ray.
    Point.move p (Shape.Epsilon * hnd)

/// <summary>
/// Given a shadowpoint and its lightvectors we find the color, intensity and
/// dot product value for each light source.
/// </summary>
/// <param name=s>The shapes in the scene.</param>
/// <param name=chp>The point that was hit.</param>
/// <param name=shp>The shadow origin point.</param>
/// <param name=ls>The light vectors for the given shadow origin point.</param>
/// <returns>A list of triples (one triple for each light source) containing values for color mixing.</returns>
let getShadingColors s chp shp ls =
    let folder cls l =
        let c = Light.getColor l
        let i = Light.getIntensity l
        let v = Light.getVector shp l

        match v with
        // In case of the null vector, we've encountered an ambient light.
        // The dot product of ambient lights will always be 1.
        | v when v = Vector.make 0. 0. 0. -> (c, i, 1.) :: cls
        | _ ->
            // Construct a ray from the shadow point in the direction of the light.
            let r = Ray.make shp v

            let hn = Shape.hitNormal chp
            let dp = max 0. (Vector.normalise v * hn)

            // Gets the magnitude of the light vector to use as maxDistance
            // for getting hitpoints.
            let vd = Vector.magnitude v

            // Check if the ray hit any shapes on its way to the light source.
            match getHitpoints r vd s with
            | [] -> (c, i, dp) :: cls
            | _  -> (c, i, 0.) :: cls

    List.fold folder [] ls

/// <summary>
/// Mixes the color of a hitpoint with the shading colors from the light sources.
/// </summary>
/// <param name=c>The original color containing RGB values.</param>
/// <param name=cs>A list of triples representing the shading colors.</param>
/// <returns>A mixed color.</returns>
let mixShadingColors c cs =
    let folder (r, g, b) (c', i, dp) =
        let r' = Color.getR c' ** GammaFactor
        let g' = Color.getG c' ** GammaFactor
        let b' = Color.getB c' ** GammaFactor

        let f = dp * i

        (r + (r' * f), g + (g' * f), b + (b' * f))

    let (rf, gf, bf) = List.fold folder (0., 0., 0.) cs

    let r = Color.getR c ** GammaFactor
    let g = Color.getG c ** GammaFactor
    let b = Color.getB c ** GammaFactor

    (r * rf, g * gf, b * bf)

/// <summary>
/// Takes a ray and a hitpoint and calculates the direction of the reflected ray.
/// </summary>
/// <param name=r>The ray that hits the shape.</param>
/// <param name=hp>The hitpoint for the ray.</param>
/// <returns>The direction vector for the reflected ray.</returns>
let getReflectionDirection r hp =
    // get the direction vector of the ray
    let dv = Ray.getVector r
    // get the normal of the hitpoint
    let n = Shape.hitNormal hp
    // get the new direction angle
    let nd = 2. * dv * n
    // get the new, reflected direction vector
    Vector.normalise (dv - (nd * n))

/// <summary>
/// Shoots a given number of reflection rays and gets
/// the color and reflection index for each hit.
/// </summary>
/// <param name=s>The scene.</param>
/// <param name=i>Reflection counter.</param>
/// <param name=cs>The list to which the reflection colors are added.</param>
/// <param name=r>The current ray.</param>
/// <param name=hp>The current hitpoint.</param>
/// <param name=hd>The current hit distance.</param>
/// <returns>A list of tuples containing each hitpoints
/// color and reflection index.</returns>
let rec getReflectionColors s ls (mr, i) cs r hp hd =
    // Get the material of the ray origin
    let om = Shape.hitMaterial hp
    // Get the reflection index of the ray origin hp
    let ri = Material.getReflect om

    // Check if the reflection max has been passed
    // or if the reflection index is 0.
    if i <= 0 || ri <= 0. then cs else

    // Get the direction of the reflection ray.
    let d = getReflectionDirection r hp
    // Get retrace point
    let hp' = getRetracePoint r hp hd
    // Create a ray from the retrace point
    let r' = Ray.make hp' d
    // Get the hits of the ray
    let hps = getHitpoints r' infinity s

    // Check if there are any hits
    match getClosestHitpoint hps with
    | None -> cs
    | Some(chp, chd) ->
        // Make a retrace point for the hp
        let rp = getRetracePoint r' chp chd
        // Get the shading colors for the new hp
        let scs = getShadingColors s chp rp ls
        // Get the material of the ray hit
        let m = Shape.hitMaterial chp
        // Get the color of the ray hit
        let c = Material.getColor m
        let mi = Material.getReflect m

        let (rs, gs, bs) = mixShadingColors c scs

        // Make the shaded color
        let sc = Color.make rs gs bs

        // make diminish factor
        let df = float mr / float i
        // scale the color with the diminish factor
        let ssc = Color.scale sc df

        getReflectionColors s ls (mr, i - 1) ((ssc, mi) :: cs) r' chp chd

/// <summary>
/// Merges colors for reflection.
/// </summary>
/// <param name=cs>A list of tuples containing color
/// and reflection index.</param>
/// <returns>A merged color.</returns>
let mergeReflectionColors cs =
    let c, _ = cs |> List.reduce (fun (c, _) (c', i') ->
        Color.merge i' c c', 0.
    )

    Color.getR c, Color.getG c, Color.getB c

/// <summary>
/// Gets the color to shade and finds out which values to mix the color with.
/// </summary>
/// <param name=s>A list of shapes in the scene.</param>
/// <param name=ls>A list of lights in the scene.</param>
/// <param name=r>A ray in the scene.</param>
/// <param name=hps>A list of hitpoints for a ray in the scene.</param>
/// <returns>A SolidBrush with the final mixed color.</returns>
let getColor s ls (r, hps) mr =
    // Get the closest hitpoint and the distance to it.
    match getClosestHitpoint hps with
    | Some (chp, chd) ->
        // Get the point from which we should cast the shadow ray.
        let shp = getRetracePoint r chp chd

        // Get the colors that the pixel should be shaded by.
        let cls = getShadingColors s chp shp ls

        // Get the material that the ray hit.
        let hm = Shape.hitMaterial chp

        // Get the color of the material that the ray hit.
        let mc = Material.getColor hm

        // Get the reflection index of the material that the ray hit.
        let ri = Material.getReflect hm

        let r', g', b' = mixShadingColors mc cls

        if ri <= 0. then r', g', b'
        else
            // Get a list of reflection colors and indexes
            let rcls = getReflectionColors s ls (mr, mr) [mc,ri] r chp chd

            mergeReflectionColors rcls

    | None -> 0., 0., 0.

/// <summary>
/// Given a ray, find the color of the thing it hits in the scene.
/// </summary>
/// <param name=s>The scene to look into.</param>
/// <param name=mr>The maximum number of times to reflect the ray.</param>
/// <param name=md>The maximum distance that the ray travels.</param>
/// <param name=r>The ray shot into the scene.</param>
/// <returns>The color of the thing that was hit.</returns>
let getHit s (mr:int) (md:float) r =
    let r, g, b = getColor s (getLights s) (r, getHitpoints r md s) mr
    let u = 1. / GammaFactor in Color.make (r ** u) (g ** u) (b ** u)
