/// Copyright (C) 2016 The Authors.
module Camera

open Point
open Vector

[<Sealed>]
type Camera

/// Raised in case of attempting to create a camera that looks at its position.
exception InvalidCameraException

/// Create a camera with a position, a lookat point, an up vector,
/// a distance (zoom), a unitwidth, a unitheight, a pixelwidth and a pixelheight.
val make : Point -> Point -> Vector -> float -> float -> float -> int -> int -> Camera

/// Get the position Point of a camera.
val getPosition : Camera -> Point

/// Get the lookat Point of a camera.
val getLookat : Camera -> Point

/// Get the upvector of a camera.
val getUpVector : Camera -> Vector

/// Get the distance to the pixelgrid for a camera (zoom).
val getDistance : Camera -> float

/// Get the unitwidth of a camera.
val getUnitWidth : Camera -> float

/// Get the unitheight of a camera.
val getUnitHeight : Camera -> float

/// Get the pixelwidth of a camera.
val getPixelWidth : Camera -> int

/// Get the pixelheight of a camera.
val getPixelHeight : Camera -> int
