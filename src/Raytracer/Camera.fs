/// Copyright (C) 2016 The Authors.
module Camera

open System.Drawing

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
/// <param name=u>The upvector of the camera.</param>
/// <param name=z>The zoom of the camera (distance from camera to pixel grid).</param>
/// <param name=uw>The unitwidth of the camera.</param>
/// <param name=uh>The unitheight of the camera.</param>
/// <param name=pw>The pixelwidth of the pixelgrid.</param>
/// <param name=ph>The pixelheight of the pixelgrid.</param>
/// <returns>The created camera.</returns>
let make p l u z uw uh pw ph =
    if p = l then raise InvalidCameraException
    C(p, l, u, z, uw, uh, pw, ph)

/// <summary>
/// Render a scene to a Bitmap object.
/// </summary>
/// <param name=c>The camera.</param>
/// <param name=s>The scene to render.</param>
/// <returns>The rendered bitmap of the scene.</returns>
let render (C(p, q, u, z, w, h, x, y)) s =
    let bm = new Bitmap(x, y)

    let l = Point.direction p q
    let r = Vector.normalise (Vector.crossProduct u l)
    let d = Vector.normalise (Vector.crossProduct r l)

    let p' = Point.move p (z * l)
    let p' = Point.move p' ((+h / 2.) * u)
    let p' = Point.move p' ((-w / 2.) * r)

    let W = w / float x
    let H = h / float y

    let rs = seq {
        for n in 0 .. x * y - 1 ->
            let a = float (n % x)
            let b = float (n / y)

            let p' = Point.move p' (W * (a + 0.5) * r)
            let p' = Point.move p' (H * (b + 0.5) * d)

            n, Ray.make p (Point.distance p p')
    }

    let m (p, r) = async {
        return p, Scene.getHit s 5 infinity r
    }

    let cs = rs |> Seq.map m
                |> Async.Parallel
                |> Async.RunSynchronously

    for n, c in cs do
        let r = Color.getR c * 255.
        let g = Color.getG c * 255.
        let b = Color.getB c * 255.
        let c = Color.FromArgb(int r, int g, int b)

        do bm.SetPixel(x - (n % x), n / y, c)

    bm
