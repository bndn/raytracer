/// Copyright (C) 2016 The Authors.
module RenderTest

open System.Drawing
open Xunit
open FsUnit.Xunit

open Raytracer
open Render

let scene =
    let sphereLoc = Raytracer.mkPoint 0. 2. 0.
    let lookat = Raytracer.mkPoint 0. 0. 0.
    let cameraOrigin = Raytracer.mkPoint 6. 0. 0.5
    let cameraUp = Raytracer.mkVector 0. 0. 1.
    let zoom = 2.
    let camera = Raytracer.mkCamera cameraOrigin lookat cameraUp zoom 1. 1. 6 6

    let sphereColor = Raytracer.fromColor Color.Red
    let sphereTexture = Raytracer.mkMatTexture (Raytracer.mkMaterial sphereColor 0.5)
    let sphere = Raytracer.mkSphere sphereLoc 1. sphereTexture

    let planeColor = Raytracer.fromColor Color.Green
    let planeTexture = Raytracer.mkMatTexture (Raytracer.mkMaterial planeColor 0.1)
    let plane = Raytracer.mkPlane planeTexture

    let lightColor = Raytracer.fromColor Color.White
    let ambientLight = Raytracer.mkAmbientLight lightColor 0.1

    Raytracer.mkScene [sphere; plane] [] ambientLight camera 2

[<Fact>]
let ``render constructs a Bitmap of the raytracer scene`` () =
    let bmp = Render.render scene

    bmp |> should be instanceOfType<Bitmap>

[<Fact>]
let ``render constructs a raytracer scene with correct colors`` () =
    let bmp = Render.render scene

    bmp.GetPixel(2, 2) |> should equal (Color.FromArgb(255, Color.Red))
    bmp.GetPixel(1, 3) |> should equal (Color.FromArgb(255, Color.Green))
