/// Copyright (C) 2016 The Authors.
module Scene

open Camera
open Light
open Shape
open Ray

type Scene

/// <summary>
/// Create a scene with a list of shapes, a list of directional lights,
/// a camera and a maximum of reflections for that scene.
/// </summary>
/// <param name=ss>List of shapes to render in the scene.</param>
/// <param name=ls>List of lights to light up the scene.</param>
/// <param name=c>Camera for the scene, from which to render.</param>
/// <param name=r>Maximum reflections for the scene.</param>
/// <returns>The scene of shapes, lights and camera.</returns>
val make : ss:Shape list -> ls:Light list -> c:Camera -> r:int -> Scene

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
val getReflections : s:Scene -> int

/// </summary>
/// Find the closest of a list of hitpoints.
/// </summary>
/// <param name=hps>The list of hitpoints.</param>
/// <returns>The hitpoint with the closest hit distance.</returns>
val getClosestHitpoint : hps:Hitpoint list -> (Hitpoint * float) option

/// <summary>
/// Given a ray, find all hitpoints within a scene.
/// </summary>
/// <param name=r>The ray to check.</param>
/// <param name=d>The maximum distance that the ray travels.</param>
/// <param name=s>The scene to check.</param>
/// <returns>The list of hit points.</returns>
val getHitpoints : r:Ray -> d:float -> s:Scene -> Hitpoint list
