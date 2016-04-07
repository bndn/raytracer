/// Copyright (C) 2016 The Authors.
module Scene

open Camera
open Light
open Shape

type Scene

/// <summary>
/// Create a scene with a list of shapes, a list of directional lights,
/// a camera and a maximum of reflections for that scene.
/// </summary>
/// <param name=sl>List of shapes to render in the scene.</param>
/// <param name=ll>List of lights to light up the scene.</param>
/// <param name=c>Camera for the scene, from which to render.</param>
/// <param name=mr>Maximum reflections for the scene.</param>
/// <returns>The scene of shapes, lights and camera.</returns>
val make : sl:Shape list -> ll:Light list -> c:Camera -> mr:int -> Scene

/// <summary>
/// Get the list of shapes in a scene.
/// </summary>
/// <param name=s>The scene to get the shapes from.</param>
/// <returns>The list of shapes in the scene.</returns>
val getShapes : s:Scene -> Shape list

/// <summary>
/// Get the list of lights in a scene.
/// </summary>
/// <param name=s>The scene to get the lights from.</param>
/// <returns>The list of lights in the scene.</returns>
val getLights : s:Scene -> Light list

/// <summary>
/// Get the camera in a scene.
/// </summary>
/// <param name=s>The scene to get the camera from.</param>
/// <returns>The camera in the scene.</returns>
val getCamera : s:Scene -> Camera

/// <summary>
/// Get the maximum reflection count of a scene.
/// </summary>
/// <param name=s>The scene to get the maximum reflection count from.</param>
/// <returns>The maximum reflection count of the scene.</returns>
val getMR : s:Scene -> int
