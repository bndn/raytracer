/// Copyright (C) 2016 The Authors.
module SceneTest

open Xunit
open FsUnit.Xunit

open Scene

let camera =
    let pos = Point.make 0.0 0.0 4.0
    let lookat = Point.make 0.0 0.0 0.0
    let upvector = Vector.make 0.0 1.0 0.0
    let distance = 1.0
    let uw = 2.0 // unit width
    let uh = 2.0 // unit height
    let pw = 500 // pixel width
    let ph = 500 // pixel height

    Camera.make pos lookat upvector distance uw uh pw ph

let omniLight =
    let p = Point.make 1.5 2.5 3.5 // Point
    let c = Color.make 0.1 0.2 0.3 // Color
    let i = 0.5                    // intensity

    Light.make (Light.Omni(p)) c i

let ambientLight =
    let c = Color.make 0.1 0.2 0.3 // Color
    let i = 0.5                    // intensity

    Light.make Light.Ambient c i

let sphereTexture = Texture.make (fun u v ->
    Material.make (Color.make 1. 0. 0.) 0.5)

let sphere = Shape.mkSphere (Point.make 0. 0. 0.) 1. sphereTexture

[<Fact>]
let ``make constructs a scene given a set of valid arguments`` () =
    let s = Scene.make [sphere] [omniLight; ambientLight] camera 2

    s |> should be instanceOfType<Scene>

[<Fact>]
let ``can not construct a scene without shapes`` () =
    (fun () -> Scene.make [] [ambientLight] camera 2 |> ignore) |> shouldFail

[<Fact>]
let ``can not construct a scene without lights`` () =
    (fun () -> Scene.make [sphere] [] camera 2 |> ignore) |> shouldFail

[<Fact>]
let ``constructing a scene with max reflect less than 0, should set it to 0`` () =
    let s = Scene.make [sphere] [omniLight; ambientLight] camera -3

    Scene.getMR s |> should equal 0
