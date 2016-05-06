/// Copyright (C) 2016 The Authors.
module Scene

open Shape
open Light
open Color
open Ray

type Scene

/// <summary>
/// Create a scene with a list of shapes.
/// </summary>
/// <param name=ss>The shapes in the scene.</param>
/// <param name=ls>The lights in the scene.</param>
/// <returns>The constructed scene.</returns>
val make : ss:Shape list -> ls:Light list -> Scene

/// <summary>
/// Given a ray, find the color of the thing it hits in the scene.
/// </summary>
/// <param name=s>The scene to look into.</param>
/// <param name=mr>The maximum number of times to reflect the ray.</param>
/// <param name=md>The maximum distance that the ray travels.</param>
/// <param name=r>The ray shot into the scene.</param>
/// <returns>The color of the thing that was hit.</returns>
val getHit : s:Scene -> mr:int -> md:float -> r:Ray -> Color
