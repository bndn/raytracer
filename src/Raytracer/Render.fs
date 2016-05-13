module Render

open System.Drawing

/// <summary>
/// Render a scene to a Bitmap object.
/// </summary>
/// <param name=c>The camera.</param>
/// <param name=s>The scene to render.</param>
/// <returns>The rendered bitmap of the scene.</returns>
let render c s =
    let p = Camera.getPosition c
    let q = Camera.getLookat c
    let u = Camera.getUpVector c
    let z = Camera.getZoom c
    let w = Camera.getUnitWidth c
    let h = Camera.getUnitHeight c
    let x = Camera.getPixelWidth c
    let y = Camera.getPixelHeight c

    let b = new Bitmap(x, y)
    let i = Graphics.FromImage(b)

    // Find l
    let l = Point.direction p q

    // Find r and d
    let r = Vector.normalise (Vector.crossProduct u l)
    let d = Vector.normalise (Vector.crossProduct r l)

    let p' = Point.move p (z * l)

    // Move to the top
    let p' = Point.move p' ((h / 2.) * u)

    // Move to the left
    let p' = Point.move p' ((-w / 2.) * r)

    let W = w / (float x)
    let H = h / (float y)

    let rs = seq {
        for n in 0 .. x * y - 1 ->
            let a = float (n % x)
            let b = float (n / y)

            // Move to the current column
            let p' = Point.move p' (W * (a + 0.5) * r)

            // Move to the current row
            let p' = Point.move p' (H * (b + 0.5) * d)

            (n, Ray.make p (Point.distance p p'))
    }

    let m (p, r) = async {
        return p, Scene.getHit s 5 infinity r
    }

    let cs = rs |> Seq.map m
                |> Async.Parallel
                |> Async.RunSynchronously

    for (p, c) in cs do
        let r = Color.getR c * 255.
        let g = Color.getG c * 255.
        let b = Color.getB c * 255.

        let c = new SolidBrush(Color.FromArgb(int r, int g, int b))

        i.FillRectangle(c, x - (p % x), p / y, 1, 1)

    b
