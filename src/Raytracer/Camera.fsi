/// Copyright (C) 2016 The Authors.
module Camera

open Point
open Vector
open Scene
open Color

type Camera

/// Raised in case of attempting to create a camera that looks at its position.
exception InvalidCameraException

/// Create a camera with a position, a lookat point, an up vector,
/// a distance (zoom), a unitwidth, a unitheight, a pixelwidth and a pixelheight.
val make : Point -> Point -> Vector -> float -> float -> float -> int -> int -> Camera

/// <summary>
/// Render a scene to a sequence of pixel coordinates and colors.
/// </summary>
/// <param name=c>The camera.</param>
/// <param name=mr>The number of times to reflect rays.</param>
/// <param name=s>The scene to render.</param>
/// <returns>The rendered scene.</returns>
val render : c:Camera -> mr:int -> s:Scene -> (int * int * Color) seq
