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
        match Shape.getBounds s with
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
            let d = Shape.getHitDistance hp
            if d < db then (hp, d) else (hpb, db)

        Some (List.fold folder (hp, Shape.getHitDistance hp) hps)
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
        let hits = Shape.hitFunction r s

        match getClosestHitpoint hits with
        | Some(_, chd) -> if chd <= d then hits @ hps else hps
        | None -> hps

    let traverser d es =
        match List.fold (folder d) [] es with
        | []  -> None
        | hps -> Some hps

    let hps = List.fold (folder d) [] ss

    match Kdtree.traverse traverser r ts with
    | Some hp -> hp @ hps
    | None    -> hps

/// <summary>
/// Given a shadowpoint and its lightvectors we find the color, intensity and
/// dot product value for each light source.
/// </summary>
/// <param name=ss>The shapes in the scene.</param>
/// <param name=chp>The point that was hit.</param>
/// <param name=shp>The shadow origin point.</param>
/// <param name=lvs>The light vectors for the given shadow origin point.</param>
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

            let hn = Shape.getHitNormal chp
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
/// Gets the color to shade and finds out which values to mix the color with.
/// </summary>
/// <param name=ss>A list of shapes in the scene.</param>
/// <param name=ls>A list of lights in the scene.</param>
/// <param name=r>A ray in the scene.</param>
/// <param name=hps>A list of hitpoints for a ray in the scene.</param>
/// <returns>A SolidBrush with the final mixed color.</returns>
let getColor s ls (r, hps) =
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

        // Get the colors that the pixel should be shaded by.
        let cls = getShadingColors s chp shp ls

        // Get the material that the ray hit.
        let hm = Shape.getHitMaterial chp

        // Get the color of the material that the ray hit.
        let mc = Material.getColor hm

        mixShadingColors mc cls

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
    let r, g, b = getColor s (getLights s) (r, getHitpoints r md s)
    let u = 1. / GammaFactor in Color.make (r ** u) (g ** u) (b ** u)
