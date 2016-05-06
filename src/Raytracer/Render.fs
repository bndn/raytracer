module Render

open System.Drawing

/// <summary>
/// Render a scene to a Bitmap object.
/// </summary>
/// <param name=c>The camera.</param>
/// <param name=s>The scene to render.</param>
/// <returns>The rendered bitmap of the scene.</returns>
let render c s =
    let cp = Camera.getPosition c
    let ca = Camera.getLookat c
    let cu = Camera.getUpVector c
    let cz = Camera.getZoom c
    let uw = Camera.getUnitWidth c
    let uh = Camera.getUnitHeight c
    let pw = Camera.getPixelWidth c
    let ph = Camera.getPixelHeight c

    let b = new Bitmap(pw, ph)

    // The Graphics object allows us to manipulate the bitmap.
    let g = Graphics.FromImage(b)

    // Find l (direction between p and q)
    let l = Point.direction cp ca

    // Find r, d and z
    let r = Vector.normalise (Vector.crossProduct cu l)
    let d = Vector.normalise (Vector.crossProduct r l)
    let z = cz * l

    let w = uw / (float pw)
    let h = uh / (float ph)

    let p' = Point.move cp z

    // Move to top
    let p' = Point.move p' ((uh / 2.) * cu)

    // Move to top left
    let p' = Point.move p' ((-uw / 2.) * r)

    let gridRays = seq {
        for p in 0..(pw * ph - 1) ->
            let row = p / ph

            // Move the point to work with to the right by i amount
            let p'' = Point.move p' (w * ((float (p % pw)) + 0.5) * r)

            // Move the point down onto the current row
            let p'' = Point.move p'' (h * ((float row) + 0.5) * d)

            (p, Ray.make cp (Point.distance cp p''))
    }
    // Map the async task to the gridRays sequence in parallel and wait for it.
    let colors = gridRays |> Seq.map (fun (p, r) -> async {
                               return p, Scene.getHit s 5 infinity r
                             })
                          |> Async.Parallel
                          |> Async.RunSynchronously

    for (p, c) in colors do
        let cr = Color.getR c * 255.
        let cg = Color.getG c * 255.
        let cb = Color.getB c * 255.

        let c = new SolidBrush(Color.FromArgb(int cr, int cg, int cb))

        g.FillRectangle(c, p % pw, p / ph, 1, 1)

    b
