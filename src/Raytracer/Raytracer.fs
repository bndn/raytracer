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
// Tracer Modules
////////////////////////////////////////////////////////////////////////////////

open Camera
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

let mkShape (s:baseShape) (t:texture) : shape = failwith "mkShape not implemented"
let mkSphere p r t = Shape.mkSphere p r t
let mkRectangle (p:point) (w:float) (h:float) (t:texture) : shape = failwith "mkRectangle not implemented"
let mkTriangle a b c m = Shape.mkTriangle a b c m
let mkPlane t = Shape.mkPlane (Point.make 0. 0. 0.) (Vector.make 0. 1. 0.) t
let mkImplicit (e:string) : baseShape = failwith "mkImplicit not implemented"
let mkPLY (f:string) (s:bool) : baseShape = failwith "mkPLY not implemented"
let mkHollowCylinder c r h t = Shape.mkHollowCylinder c r h t
let mkSolidCylinder (c:point) (r:float) (h:float) (s:texture) (b:texture) (t:texture) : shape = failwith "mkSolidCylinder not implemented"
let mkDisc (p:point) (r:float) (t:texture) : shape = failwith "mkDisc not implemented"
let mkBox (lo:point) (hi:point) (fr:texture) (ba:texture) (t:texture) (b:texture) (l:texture) (r:texture) : shape = failwith "mkBox not implemented"

////////////////////////////////////////////////////////////////////////////////
// Constructive Solid Geometry
////////////////////////////////////////////////////////////////////////////////

let group r s = Shape.mkUnion r s
let union r s = Shape.mkUnion r s
let intersection r s = Shape.mkIntersection r s
let subtraction r s = Shape.mkSubtraction r s

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
let transform (s:shape) (t:transformation) : shape = failwith "transform not implemented"
