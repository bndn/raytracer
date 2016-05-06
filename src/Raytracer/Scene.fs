/// Copyright (C) 2016 The Authors.
module Scene

open Camera
open Light
open Shape

type Scene = S of Shape list * Light list * Camera * int

/// <summary>
/// Create a scene with a list of shapes, a list of directional lights,
/// a camera and a maximum of reflections for that scene.
/// </summary>
/// <param name=ss>List of shapes to render in the scene.</param>
/// <param name=ls>List of lights to light up the scene.</param>
/// <param name=c>Camera for the scene, from which to render.</param>
/// <param name=r>Maximum reflections for the scene.</param>
/// <returns>The scene of shapes, lights and camera.</returns>
let make ss ls c r =
    if List.isEmpty ss then failwith "No shapes were injected into the scene"
    if List.isEmpty ls then failwith "No lights were injected into the scene"
    if r < 0
    then S(ss, ls, c, 0)
    else S(ss, ls, c, r)

/// <summary>
/// Get the list of shapes in a scene.
/// </summary>
/// <param name=s>The scene to get the shapes from.</param>
/// <returns>The list of shapes in the scene.</returns>
let getShapes = function S(ss, _, _, _) -> ss

/// <summary>
/// Get the list of lights in a scene.
/// </summary>
/// <param name=s>The scene to get the lights from.</param>
/// <returns>The list of lights in the scene.</returns>
let getLights = function S(_, ls, _, _) -> ls

/// <summary>
/// Get the camera in a scene.
/// </summary>
/// <param name=s>The scene to get the camera from.</param>
/// <returns>The camera in the scene.</returns>
let getCamera = function S(_, _, c, _) -> c

/// <summary>
/// Get the maximum reflection count of a scene.
/// </summary>
/// <param name=s>The scene to get the maximum reflection count from.</param>
/// <returns>The maximum reflection count of the scene.</returns>
let getReflections = function S(_, _, _, r) -> r

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
let getHitpoints r d s =
    let folder hps s =
        let hits = Shape.hitFunction r s

        match getClosestHitpoint hits with
        | Some (chp, chd) -> if chd <= d then hits @ hps else hps
        | None -> hps

    List.fold folder [] (getShapes s)
