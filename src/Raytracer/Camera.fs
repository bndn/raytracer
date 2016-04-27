/// Copyright (C) 2016 The Authors.
module Camera

open Vector
open Point

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

let getPosition (C(pos, _, _, _, _, _, _, _)) = pos

let getLookat (C(_, lookat, _, _, _, _, _, _)) = lookat

let getUpVector (C(_, _, up, _, _, _, _, _)) = up

let getZoom (C(_, _, _, zoom, _, _, _, _)) = zoom

let getUnitWidth (C(_, _, _, _, unitWidth, _, _, _)) = unitWidth

let getUnitHeight (C(_, _, _, _, _, unitHeight, _, _)) = unitHeight

let getPixelWidth (C(_, _, _, _, _, _, pixelWidth, _)) = pixelWidth

let getPixelHeight (C(_, _, _, _, _, _, _, pixelHeight)) = pixelHeight

let getCamera (C(pos, lookat, up, zoom, unitWidth, unitHeight, pixelWidth, pixelHeight)) = (pos, lookat, up, zoom, unitWidth, unitHeight, pixelWidth, pixelHeight)
