module Raytracer

open Camera
open Color
open Light
open Material
open Point
open Scene
open Shape
open Texture
open Vector
open Transform

type vector = Vector
type point = Point
type colour = Color
type material = Material
type shape = Shape
type baseShape = Shape
type texture = Texture
type camera = Camera
type scene = Scene
type light = Light
type ambientLight = Light
type transformation = Transformation

let mkVector (x : float) (y : float) (z : float) : vector = Vector.make x y z
let mkPoint (x : float) (y : float) (z : float) : point = Point.make x y z
let fromColor (c : System.Drawing.Color) : colour =
    Color.make (float c.R / 255.) (float c.G / 255.) (float c.B / 255.)
let mkColour (r : float) (g : float) (b : float) : colour = Color.make r g b

let mkMaterial (c : colour) (r : float) : material = Material.make c r
let mkTexture (f : float -> float -> material) : texture = Texture.make f
let mkMatTexture (m : material) : texture = Texture.make (fun _ _ -> m)

let mkShape (b : baseShape) (t : texture) : shape = failwith "mkShape not implemented"
let mkSphere (p : point) (r : float) (t : texture) : shape =
    Shape.mkSphere p r t
let mkRectangle (corner : point) (width : float) (height : float) (t : texture) : shape
    = failwith "mkRectangle not implemented"
let mkTriangle (a:point) (b:point) (c:point) (m : material) : shape =
    Shape.mkTriangle a b c m
let mkPlane (m : texture) : shape =
    Shape.mkPlane (Point.make 0. 0. 0.) (Vector.make 0. 1. 0.) m
let mkImplicit (s : string) : baseShape = failwith "mkImplicit not implemented"
let mkPLY (filename : string) (smooth : bool) : baseShape = failwith "mkPLY not implemented"

let mkHollowCylinder (c : point) (r : float) (h : float) (t : texture) : shape = failwith "mkHollowCylinder not implemented"
let mkSolidCylinder (c : point) (r : float) (h : float) (t : texture) (top : texture) (bottom : texture) : shape
    = failwith "mkSolidCylinder not implemented"
let mkDisc (c : point) (r : float) (t : texture) : shape =
    Shape.mkDisc c (Vector.make 0. 0. 1.) r t
let mkBox (low : point) (high : point) (front : texture) (back : texture) (top : texture) (bottom : texture) (left : texture) (right : texture) : shape
    = failwith "mkBox not implemented"


let group (s1 : shape) (s2 : shape) : shape = failwith "group not implemented"
let union (s1 : shape) (s2 : shape) : shape = failwith "union not implemented"
let intersection (s1 : shape) (s2 : shape) : shape = failwith "intersection not implemented"
let subtraction (s1 : shape) (s2 : shape) : shape = failwith "subtraction not implemented"

let mkCamera (pos : point) (look : point) (up : vector) (zoom : float) (width : float) (height : float) (pwidth : int) (pheight : int) : camera =
    Camera.make pos look up zoom width height pwidth pheight
let mkLight (p : point) (c : colour) (i : float) : light =
    Light.make (Direction.Omni(p)) c i
let mkAmbientLight (c : colour) (i : float) : ambientLight =
    Light.make (Direction.Ambient) c i

let mkScene (s : shape list) (l : light list) (a : ambientLight) (c : camera) (m : int) : scene =
    Scene.make s (a :: l)

let renderToScreen (sc : scene) : unit = failwith "renderToScreen not implemented"
let renderToFile (sc : scene) (path : string) : unit = failwith "renderToFile not implemented"

let translate (x : float) (y : float) (z : float) : transformation = Transform.translate x y z
let rotateX (angle : float) : transformation = Transform.rotate X angle
let rotateY (angle : float) : transformation = Transform.rotate Y angle
let rotateZ (angle : float) : transformation = Transform.rotate Z angle
let sheareXY (distance : float) : transformation = Transform.shear X Y distance
let sheareXZ (distance : float) : transformation = Transform.shear X Z distance
let sheareYX (distance : float) : transformation = Transform.shear Y X distance
let sheareYZ (distance : float) : transformation = Transform.shear Y Z distance
let sheareZX (distance : float) : transformation = Transform.shear Z X distance
let sheareZY (distance : float) : transformation = Transform.shear Z Y distance
let scale (x : float) (y : float) (z : float) : transformation = Transform.scale x y z
let mirrorX : transformation = Transform.mirror X
let mirrorY : transformation = Transform.mirror Y
let mirrorZ : transformation = Transform.mirror Z
let mergeTransformations (ts : transformation list) : transformation = Transform.merge ts
let transform (sh : shape) (tr : transformation) : shape = failwith "transform not implemented"
