module WinFormsSample

open System
open System.Drawing
open System.Windows.Forms

// Create the Form object to render on the screen.
// The form object will contain the rendered scene.
let frm =
    let f = new Form()
    do f.Text      <- "Team TeamWorks Raytracer Sample"
    do f.BackColor <- Color.White
    do f.Width     <- 500 + 50
    do f.Height    <- 500 + 50
    f

open Point
open Ray
open Raytracer
open Vector

// Creates the scene containing a 3-dimensional sphere.
let scene (pwidth : int) (pheight : int) =
    let sphereLoc = Raytracer.mkPoint 0. 2. 0.
    let lookat = Raytracer.mkPoint 0. 0. 0.
    let cameraOrigin = Raytracer.mkPoint 20. 0. 0.5
    let cameraUp = Raytracer.mkVector 0. 0. 1.
    let zoom = 2.
    let camera = Raytracer.mkCamera cameraOrigin lookat cameraUp zoom 1. 1. pwidth pheight

    let green = Raytracer.fromColor Color.Green
    let red = Raytracer.fromColor Color.Red
    let blue = Raytracer.fromColor Color.Blue

    let sphereTexture = Raytracer.mkMatTexture (Raytracer.mkMaterial red 0.5)
    let sphere = Raytracer.mkSphere sphereLoc 1. sphereTexture


    let planeTexture =
        Raytracer.mkTexture (
            fun u v ->
                let color =
                    if u > 0.5 && v > 0.5 then green
                    elif u > 0.5 || v > 0.5 then red
                    else blue
                Raytracer.mkMaterial color 0.5
            )
    let plane = Raytracer.mkPlane planeTexture

    let (a, b, c) = (Raytracer.mkPoint 1. 1. 1.), (Raytracer.mkPoint 1. 2. 1.), (Raytracer.mkPoint 1. 2. 3.)
    let triangleMaterial = Raytracer.mkMaterial blue 0.5
    let triangle = Raytracer.mkTriangle a b c triangleMaterial

    let lightColor = Raytracer.fromColor Color.White
    let ambientLight = Raytracer.mkAmbientLight lightColor 0.1

    let scene = Raytracer.mkScene [triangle;sphere;plane] [] ambientLight camera 2

    let pb = new PictureBox()
    do pb.SizeMode      <- PictureBoxSizeMode.AutoSize
    do pb.BorderStyle   <- BorderStyle.FixedSingle
    do pb.Top           <- 10 // margin from top
    do pb.Left          <- 10 // margin from left

    // Put the bitmap into the PictureBox.
    do pb.Image <- Render.render scene

    // Return the PictureBox
    pb

do frm.Controls.Add(scene 500 500)

type public Program() =
    member this.Main() =
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault false

        Application.Run(frm)
