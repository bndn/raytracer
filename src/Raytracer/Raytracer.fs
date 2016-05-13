module Raytracer

open System.Drawing
open System.Drawing.Imaging
open System.Windows.Forms

////////////////////////////////////////////////////////////////////////////////
// Core Libraries
////////////////////////////////////////////////////////////////////////////////

open Color
open Light
open Material
open Point
open Shape
open Texture
open Transform
open Vector

////////////////////////////////////////////////////////////////////////////////
// Tracer Libraries
////////////////////////////////////////////////////////////////////////////////

open Camera
open Render
open Scene

////////////////////////////////////////////////////////////////////////////////
// Type Aliases
////////////////////////////////////////////////////////////////////////////////

type ambientLight = Light
type baseShape = Shape
type camera = Camera
type colour = Color
type light = Light
type material = Material
type point = Point
type scene = Scene * Camera * int
type shape = Shape
type texture = Texture
type transformation = Transformation
type vector = Vector

////////////////////////////////////////////////////////////////////////////////
// Primitives
////////////////////////////////////////////////////////////////////////////////

let mkVector x y z = Vector.make x y z
let mkPoint x y z = Point.make x y z
let fromColor (c:System.Drawing.Color) = Color.make (float c.R / 255.) (float c.G / 255.) (float c.B / 255.)
let mkColour r g b = Color.make r g b
let mkMaterial c r = Material.make c r
let mkTexture f = Texture.make f
let mkMatTexture m = Texture.make (fun _ _ -> m)

////////////////////////////////////////////////////////////////////////////////
// Shapes
////////////////////////////////////////////////////////////////////////////////

// let mkShape s t = failwith "mkShape not implemented"
let mkSphere p r t = Shape.mkSphere p r t
// let mkRectangle p w h t = failwith "mkRectangle not implemented"
let mkTriangle a b c m = Shape.mkTriangle a b c m
let mkPlane t = Shape.mkPlane (Point.make 0. 0. 0.) (Vector.make 0. 1. 0.) t
// let mkImplicit e = failwith "mkImplicit not implemented"
// let mkPLY f s = failwith "mkPLY not implemented"
// let mkHollowCylinder c r h t = failwith "mkHollowCylinder not implemented"
// let mkSolidCylinder c r h s b t = failwith "mkSolidCylinder not implemented"
// let mkDisc p r t = failwith "mkDisc not implemented"
// let mkBox lo hi fr ba t b l r = failwith "mkBox not implemented"

////////////////////////////////////////////////////////////////////////////////
// Constructive Solid Geometry
////////////////////////////////////////////////////////////////////////////////

// let group r s = failwith "group not implemented"
// let union r s = failwith "union not implemented"
// let intersection r s = failwith "intersection not implemented"
// let subtraction r s = failwith "subtraction not implemented"

////////////////////////////////////////////////////////////////////////////////
// Rendering
////////////////////////////////////////////////////////////////////////////////

let mkCamera p l u z w h pw ph = Camera.make p l u z w h pw ph
let mkLight p c i = Light.make (Direction.Omni(p)) c i
let mkAmbientLight c i = Light.make (Direction.Ambient) c i
let mkScene ss ls al (c:Camera) (mr:int) = Scene.make ss (al :: ls), c, mr
let renderToScreen (s, c, mr:int) =
    let p = new PictureBox()
    do p.SizeMode <- PictureBoxSizeMode.AutoSize
    do p.Image    <- render c s

    let f = new Form()
    do f.AutoSize <- true
    do f.Controls.Add p

    Application.Run(f)
let renderToFile (s, c, mr:int) (f:string) =
    let b : Bitmap = render c s in b.Save(f, ImageFormat.Png)

////////////////////////////////////////////////////////////////////////////////
// Affine Transformations
////////////////////////////////////////////////////////////////////////////////

let translate x y z = Transform.translate x y z
let rotateX a = Transform.rotate X a
let rotateY a = Transform.rotate Y a
let rotateZ a = Transform.rotate Z a
let sheareXY d = Transform.shear X Y d
let sheareXZ d = Transform.shear X Z d
let sheareYX d = Transform.shear Y X d
let sheareYZ d = Transform.shear Y Z d
let sheareZX d = Transform.shear Z X d
let sheareZY d = Transform.shear Z Y d
let scale x y z = Transform.scale x y z
let mirrorX = Transform.mirror X
let mirrorY = Transform.mirror Y
let mirrorZ = Transform.mirror Z
let mergeTransformations ts = Transform.merge ts
// let transform s t = failwith "transform not implemented"
