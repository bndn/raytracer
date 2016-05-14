module Raytracer

////////////////////////////////////////////////////////////////////////////////
// Core Libraries
// -----------------------------------------------------------------------------
// These libraries provide the foundation of the raytracer and make available
// types and functions for doing things like point and vector arithmetic, color
// manipulations, affine transformations, and more.
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
// -----------------------------------------------------------------------------
// These modules are still somewhat specific to the raytracer and as such aren't
// made available as standalone libraries. They provide functionality for
// representing scenes of shapes and for rendering these to screen or files.
////////////////////////////////////////////////////////////////////////////////

open Camera
open Scene

////////////////////////////////////////////////////////////////////////////////
// Type Aliases
// -----------------------------------------------------------------------------
// These aliases exist as a compatibility layer between the core libraries and
// the type names that are exposed by the raytracer API.
//
// What this in essence allows us to do is keep our type design decoupled from
// the use-cases of the raytracer API, and as such keep the core libraries very
// generic and reusable.
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
// -----------------------------------------------------------------------------
// The following functions are constructors for the different primitive types
// (primitive in relation to the raytracer) exposed by the core libraries.
////////////////////////////////////////////////////////////////////////////////

val mkVector : x:float -> y:float -> z:float -> vector
val mkPoint : x:float -> y:float -> c:float -> point
val fromColor : c:System.Drawing.Color -> colour
val mkColour : r:float -> g:float -> b:float -> colour
val mkMaterial : c:colour -> r:float -> material
val mkTexture : f:(float -> float -> material) -> texture
val mkMatTexture : m:material -> texture

////////////////////////////////////////////////////////////////////////////////
// Shapes
// -----------------------------------------------------------------------------
// The following functions are constructors for the different shapes that can be
// rendered by the raytracer. These are all composed from the various primitives
// found above.
////////////////////////////////////////////////////////////////////////////////

// val mkShape : s:baseShape -> t:texture -> shape
val mkSphere : p:point -> r:float -> t:texture -> shape
// val mkRectangle : p:point -> w:float -> h:float -> t:texture -> shape
val mkTriangle : a:point -> b:point -> c:point -> m:material -> shape
val mkPlane : t:texture -> shape
// val mkImplicit : e:string -> baseShape
// val mkPLY : f:string -> s:bool -> baseShape
val mkHollowCylinder : c:point -> r:float -> h:float -> t:texture -> shape
// val mkSolidCylinder : c:point -> r:float -> h:float -> s:texture -> b:texture -> t:texture -> shape
// val mkDisc : p:point -> r:float -> t:texture -> shape
// val mkBox : lo:point -> hi:point -> fr:texture -> ba:texture -> t:texture -> b:texture -> l:texture -> r:texture -> shape

////////////////////////////////////////////////////////////////////////////////
// Constructive Solid Geometry
// -----------------------------------------------------------------------------
// The following functions are used for performing constructive solid geometry
// operations on the shapes from above.
////////////////////////////////////////////////////////////////////////////////

val union : r:shape -> s:shape -> shape
val intersection : r:shape -> s:shape -> shape
val subtraction : r:shape -> s:shape -> shape
val group : r:shape -> r:shape -> shape

////////////////////////////////////////////////////////////////////////////////
// Rendering
// -----------------------------------------------------------------------------
//
////////////////////////////////////////////////////////////////////////////////

val mkCamera : p:point -> l:point -> u:vector -> z:float -> w:float -> h:float -> pw:int -> ph:int -> camera
val mkLight : p:point -> c:colour -> i:float -> light
val mkAmbientLight : c:colour -> i:float -> ambientLight
val mkScene : ss:shape list -> ls:light list -> al:ambientLight -> c:camera -> mr:int -> scene
val renderToScreen : s:scene -> unit
val renderToFile : s:scene -> f:string -> unit

////////////////////////////////////////////////////////////////////////////////
// Affine Transformations
// -----------------------------------------------------------------------------
//
////////////////////////////////////////////////////////////////////////////////

val rotateX : a:float -> transformation
val rotateY : a:float -> transformation
val rotateZ : a:float -> transformation
val sheareXY : d:float -> transformation
val sheareXZ : d:float -> transformation
val sheareYX : d:float -> transformation
val sheareYZ : d:float -> transformation
val sheareZX : d:float -> transformation
val sheareZY : d:float -> transformation
val scale : x:float -> y:float -> z:float -> transformation
val translate : x:float -> y:float -> z:float -> transformation
val mirrorX : transformation
val mirrorY : transformation
val mirrorZ : transformation
val mergeTransformations : ts:transformation list -> transformation
// val transform : s:shape -> t:transformation -> shape
