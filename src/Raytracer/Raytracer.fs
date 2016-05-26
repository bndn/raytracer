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
let mkSphere p r t = Sphere.make p r t
let mkRectangle p w h t = Rectangle.make p w h t
let mkTriangle a b c m = Triangle.make a b c (mkMatTexture m)
let mkPlane t = Plane.make (Point.make 0. 0. 0.) (Vector.make 0. 0. 1.) t
let mkImplicit (e:string) : baseShape = failwith "mkImplicit not implemented"
let mkPLY (f:string) (s:bool) : baseShape = failwith "mkPLY not implemented"
let mkHollowCylinder c r h t =
    // cylinder in the internal API works with a p0 center at the bottom
    // of the cylinder, but the PO expects a cylinder with a p0 center at the
    // middle of the cylinder (with respect to the y-axis)
    // we adjust midCenter to this point.
    let midCenter = Point.move c (Vector.make 0. (-h/2.) 0.)
    Cylinder.makeHollow midCenter r h t
let mkSolidCylinder (c : point) (r : float) (h : float) (t : texture) (top : texture) (bottom : texture) : shape =
    // see mkHollowCylinder
    let midCenter = Point.move c (Vector.make 0. (-h/2.) 0.)
    Cylinder.makeSolid midCenter r h t top bottom
let mkDisc p r t = Disc.make p r t
let mkBox lo hi fr ba t b l r = Box.make lo hi fr ba t b l r

////////////////////////////////////////////////////////////////////////////////
// Constructive Solid Geometry
////////////////////////////////////////////////////////////////////////////////

let group r s = Composite.make r s Composite.Group
let union r s = Composite.make r s Composite.Union
let intersection r s = Composite.make r s Composite.Intersection
let subtraction r s = Composite.make r s Composite.Subtraction

////////////////////////////////////////////////////////////////////////////////
// Rendering
////////////////////////////////////////////////////////////////////////////////

let mkCamera p l u z w h pw ph = Camera.make p l u z w h pw ph
let mkLight p c i = Light.make (Direction.Omni(p)) c i
let mkAmbientLight c i = Light.make (Direction.Ambient) c i
let mkScene ss ls al (c:Camera) (mr:int) = Scene.make ss (al :: ls), c, mr

let renderToBitmap (s, c, mr) =
    let w, h, cs = render c mr s

    let bm = new Bitmap(w, h)

    cs |> Array.Parallel.iter (fun (x, y, c) ->
        let r = Color.getR c * 255.
        let g = Color.getG c * 255.
        let b = Color.getB c * 255.
        let c = Color.FromArgb(int r, int g, int b)

        lock bm (fun () ->
            do bm.SetPixel(w - x - 1, y, c)
        )
    )

    bm

let renderToScreen s =
    let p = new PictureBox()
    do p.SizeMode <- PictureBoxSizeMode.AutoSize
    do p.Image    <- renderToBitmap s

    let f = new Form()
    do f.AutoSize <- true
    do f.Controls.Add p

    Application.Run(f)

let renderToFile s (f:string) =
    let bm : Bitmap = renderToBitmap s in bm.Save(f, ImageFormat.Png)

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
let mergeTransformations ts = Transform.merge (List.rev ts)
let transform (s:shape) (t:transformation) : shape = Shape.transform s t
