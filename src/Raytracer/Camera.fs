/// Copyright (C) 2016 The Authors.
module Camera

open Point
open Vector
open Scene

[<NoComparison>]
type Camera = C of Point * Point * Vector * float * float * float * int * int

/// Raised in case of attempting to create a camera that looks at its position.
exception InvalidCameraException

/// <summary>
/// Create a camera with a position, a lookat point, an up vector,
/// a distance (zoom), a unitwidth, a unitheight, a pixelwidth and a pixelheight.
/// </summary>
/// <param name=p>The position of the camera.</param>
/// <param name=l>The position of the lookat point.</param>
/// <param name=u>The upvector of the camera, will be normalised.</param>
/// <param name=z>The zoom of the camera (distance from camera to pixel grid).</param>
/// <param name=uw>The unitwidth of the camera.</param>
/// <param name=uh>The unitheight of the camera.</param>
/// <param name=pw>The pixelwidth of the pixelgrid.</param>
/// <param name=ph>The pixelheight of the pixelgrid.</param>
/// <returns>The created camera.</returns>
let make p l u z uw uh pw ph =
    if p = l then raise InvalidCameraException
    let u = Vector.normalise u
    C(p, l, u, z, uw, uh, pw, ph)

/// <summary>
/// Render a scene to a sequence of pixel coordinates and colors.
/// </summary>
/// <param name=c>The camera.</param>
/// <param name=mr>The number of times to reflect rays.</param>
/// <param name=s>The scene to render.</param>
/// <returns>The rendered scene.</returns>
let render (C(p, q, u, z, w, h, x, y)) mr s =
    let l = Point.direction p q
    let r = Vector.normalise (Vector.crossProduct u l)
    let d = Vector.normalise (Vector.crossProduct r l)

    let p' = Point.move p (z * l)
    let p' = Point.move p' ((+h / 2.) * u)
    let p' = Point.move p' ((-w / 2.) * r)

    let W = w / float x
    let H = h / float y

    x, y, seq {
        for n in 0 .. x * y - 1 do
            let a = n % x
            let b = n / x

            let p' = Point.move p' (W * (float a + 0.5) * r)
            let p' = Point.move p' (H * (float b + 0.5) * d)

            let r = Ray.make p (Point.distance p p')

            yield a, b, Scene.getHit s mr infinity r
    }
