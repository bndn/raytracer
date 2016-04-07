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
/// <param name=sl>List of shapes to render in the scene.</param>
/// <param name=ll>List of lights to light up the scene.</param>
/// <param name=c>Camera for the scene, from which to render.</param>
/// <param name=mr>Maximum reflections for the scene.</param>
/// <returns>The scene of shapes, lights and camera.</returns>
let make sl ll c mr =
    if List.isEmpty sl then failwith "No shapes were injected into the scene"
    if List.isEmpty ll then failwith "No lights were injected into the scene"
    if mr < 0
    then S(sl, ll, c, 0)
    else S(sl, ll, c, mr)

/// <summary>
/// Get the list of shapes in a scene.
/// </summary>
/// <param name=s>The scene to get the shapes from.</param>
/// <returns>The list of shapes in the scene.</returns>
let getShapes = function S(sl, _, _, _) -> sl

/// <summary>
/// Get the list of lights in a scene.
/// </summary>
/// <param name=s>The scene to get the lights from.</param>
/// <returns>The list of lights in the scene.</returns>
let getLights = function S(_, ll, _, _) -> ll

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
let getMR = function S(_, _, _, mr) -> mr
