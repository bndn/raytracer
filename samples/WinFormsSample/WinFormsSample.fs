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
    let sphereLoc = Raytracer.mkPoint 0. 0.8 0.
    let lookat = Raytracer.mkPoint 0. 0. 0.
    let cameraOrigin = Raytracer.mkPoint 0. 2. -10.
    let cameraUp = Raytracer.mkVector 0. 1. 0.
    let zoom = 2.
    let camera = Raytracer.mkCamera cameraOrigin lookat cameraUp zoom 1. 1. pwidth pheight

    let green = Raytracer.fromColor Color.Green
    let red = Raytracer.fromColor Color.Red
    let blue = Raytracer.fromColor Color.Blue
    let white = Raytracer.fromColor Color.White
    let black = Raytracer.fromColor Color.Black

    let sphereTexture =
        Raytracer.mkTexture (
            fun u v ->
                Raytracer.mkMaterial red 0.5
            )
    let sphere = Raytracer.mkSphere sphereLoc 0.5 sphereTexture

    let planeTexture =
        Raytracer.mkTexture (
            fun u v ->
                Raytracer.mkMaterial blue 0.5
            )
    let plane = Raytracer.mkPlane planeTexture

    let (a, b, c) = (Raytracer.mkPoint 1. 1. 1.), (Raytracer.mkPoint 1. 2. 1.), (Raytracer.mkPoint 1. 2. 3.)
    let triangleMaterial = Raytracer.mkMaterial blue 0.5
    let triangle = Raytracer.mkTriangle a b c triangleMaterial

    let lightColor = Raytracer.fromColor Color.White
    let lightColor2 = Raytracer.fromColor Color.Red
    let ambientLight = Raytracer.mkAmbientLight lightColor 0.5
    let light = Raytracer.mkLight (Raytracer.mkPoint -1. 1. -2.) lightColor 0.95

    let discCenter = Point.make 2. 2. 0.
    let discTexture =
        Raytracer.mkTexture (
            fun u v ->
                if u > 0.5 && v > 0.5
                then Raytracer.mkMaterial green 0.5
                else Raytracer.mkMaterial white 0.5
            )
    let disc = Raytracer.mkDisc discCenter 0.25 discTexture

    let scene = Raytracer.mkScene [sphere;plane;disc] [light] ambientLight camera 2

    let pb = new PictureBox()
    do pb.SizeMode      <- PictureBoxSizeMode.AutoSize
    do pb.BorderStyle   <- BorderStyle.FixedSingle
    do pb.Top           <- 10 // margin from top
    do pb.Left          <- 10 // margin from left

    // Put the bitmap into the PictureBox.
    do pb.Image <- Render.render camera scene

    // Return the PictureBox
    pb

do frm.Controls.Add(scene 500 500)

type public Program() =
    member this.Main() =
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault false

        Application.Run(frm)
