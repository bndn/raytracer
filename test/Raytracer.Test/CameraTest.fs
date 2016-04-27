/// Copyright (C) 2016 The Authors.
module CameraTest

open Xunit
open FsUnit.Xunit

let createValidCamera =
    let pos = Point.make 0.0 0.0 4.0
    let lookat = Point.make 0.0 0.0 0.0
    let upvector = Vector.make 0.0 1.0 0.0
    let zoom = 1.0
    let uw = 2.0 // unit width
    let uh = 2.0 // unit height
    let pw = 500 // pixel width
    let ph = 500 // pixel height

    Camera.make pos lookat upvector zoom uw uh pw ph

[<Fact>]
let ``make constructs a camera given a set of valid arguments`` () =
    let c = createValidCamera

    // Check that the camera was constructed.
    c |> should be instanceOfType<Camera.Camera>

[<Fact>]
let ``getPosition and getLookat gets the position- and lookat point`` () =
    let c = createValidCamera

    Camera.getPosition c |> should equal (Point.make 0.0 0.0 4.0)
    Camera.getLookat c |> should equal (Point.make 0.0 0.0 0.0)

[<Fact>]
let ``getUpVector gets the up vector`` () =
    let c = createValidCamera

    Camera.getUpVector c |> should equal (Vector.make 0.0 1.0 0.0)

[<Fact>]
let ``getZoom, getUnitWidth and getUnitHeight gets their values`` () =
    let c = createValidCamera

    Camera.getZoom c |> should equal 1.0
    Camera.getUnitWidth c |> should equal 2.0
    Camera.getUnitHeight c |> should equal 2.0

[<Fact>]
let ``getPixelWidth and getPixelHeight gets their values`` () =
    let c = createValidCamera

    Camera.getPixelWidth c |> should equal 500
    Camera.getPixelHeight c |> should equal 500

[<Fact>]
let ``make for a camera with the same position as it looks at fails`` () =
    let pos = Point.make 0.0 0.0 4.0
    let lookat = pos

    (fun () -> Camera.make pos lookat (Vector.make 0.0 1.0 0.0) 1.0 2.0 2.0 500 500 |> ignore)
    |> should throw typeof<Camera.InvalidCameraException>
