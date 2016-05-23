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

/// <summary>
/// Create a vector with three components.
/// </summary>
/// <param name=x>The x component of the vector.</param>
/// <param name=y>The y component of the vector.</param>
/// <param name=z>The z component of the vector.</param>
/// <returns>The vector.</returns>
val mkVector : x:float -> y:float -> z:float -> vector

/// <summary>
/// Create a point with three coordinates.
/// </summary>
/// <param name=x>The x coordinate of the point.</param>
/// <param name=y>The y coordinate of the point.</param>
/// <param name=z>The z coordinate of the point.</param>
/// <returns>The point.</returns>
val mkPoint : x:float -> y:float -> c:float -> point

/// <summary>
/// Create a color from a system-dependant representation.
/// </summary>
/// <param name=c>The system-dependant color.</param>
/// <returns>The color.</returns>
val fromColor : c:System.Drawing.Color -> colour

/// <summary>
/// Create a color from three float values.
/// </summary>
/// <param name=r>The red colorspace floating value.</param>
/// <param name=g>The green colorspace floating value.</param>
/// <param name=b>The blue colorspace floating value.</param>
/// <returns>The color.</returns>
val mkColour : r:float -> g:float -> b:float -> colour

/// <summary>
/// Create a material from a color and a reflective float value.
/// </summary>
/// <param name=c>The color to give the material.</params>
/// <param name=r>The reflective float value.</params>
/// <returns>The material.</returns>
val mkMaterial : c:colour -> r:float -> material

/// <summary>
/// Create a texture from a function which describes the mapping in [0,1] x [0,1] to a texture.
/// </summary>
/// <params name=f>The function which describes the texture mapping.</params>
/// <returns>The texture.</return
val mkTexture : f:(float -> float -> material) -> texture

/// <summary>
/// Create a texture which always maps to a single material.
/// </summary>
/// <param name=m>The material of the texture.</param>
/// <returns>The texture.</returns>
val mkMatTexture : m:material -> texture

////////////////////////////////////////////////////////////////////////////////
// Shapes
// -----------------------------------------------------------------------------
// The following functions are constructors for the different shapes that can be
// rendered by the raytracer. These are all composed from the various primitives
// found above.
////////////////////////////////////////////////////////////////////////////////

val mkShape : s:baseShape -> t:texture -> shape

/// <summary>
/// Construct a sphere with a point of origin, a radius, and a texture.
/// </summary>
/// <param name=p>The center point of the sphere.</param>
/// <param name=r>The radius of the sphere.</param>
/// <param name=t>The texture of the sphere.</param>
/// <returns>The sphere.</returns>
val mkSphere : p:point -> r:float -> t:texture -> shape

val mkRectangle : p:point -> w:float -> h:float -> t:texture -> shape

/// <summary>
/// Construct a triangle with points <c>a</c>, <c>b</c>, and <c>c</c>.
/// </summary>
/// <param name=a>Point <c>a</c> in the triangle.</param>
/// <param name=b>Point <c>b</c> in the triangle.</param>
/// <param name=c>Point <c>c</c> in the triangle.</param>
/// <param name=m>The material of the triangle.</param>
/// <returns>The triangle.</returns>
val mkTriangle : a:point -> b:point -> c:point -> m:material -> shape

/// <summary>
/// Construct a plane with a texture.
/// </summary>
/// <param name=t>The texture of the plane.</param>
/// <returns>The plane.</returns>
val mkPlane : t:texture -> shape

val mkImplicit : e:string -> baseShape
val mkPLY : f:string -> s:bool -> baseShape
val mkHollowCylinder : c:point -> r:float -> h:float -> t:texture -> shape
val mkSolidCylinder : c:point -> r:float -> h:float -> s:texture -> b:texture -> t:texture -> shape

/// <summary>
/// Construct a disc with a center point, a radius, and a texture.
/// </summary>
/// <param name=p>The center point of the disc.</param>
/// <param name=r>The radius of the disc.</param>
/// <param name=t>The texture of the disc.</param>
/// <returns>The disc.</returns>
val mkDisc : p:point -> r:float -> t:texture -> shape

val mkBox : low : point -> high : point -> front : texture -> back : texture -> top : texture -> bottom : texture -> left : texture -> right : texture  -> shape

////////////////////////////////////////////////////////////////////////////////
// Constructive Solid Geometry
// -----------------------------------------------------------------------------
// The following functions are used for performing constructive solid geometry
// operations on the shapes from above.
////////////////////////////////////////////////////////////////////////////////

/// <summary>
/// Construct a union of two shapes.
/// </summary>
/// <param name=r>The first shape.</param>
/// <param name=s>The second shape.</param>
/// <returns>The union.</returns>
val union : r:shape -> s:shape -> shape

/// <summary>
/// Construct an intersection of two shapes.
/// </summary>
/// <param name=r>The first shape.</param>
/// <param name=s>The second shape.</param>
/// <returns>The intersection.</returns>
val intersection : r:shape -> s:shape -> shape

/// <summary>
/// Construct an subtraction of two shapes.
/// </summary>
/// <param name=r>The first shape.</param>
/// <param name=s>The second shape.</param>
/// <returns>The subtraction.</returns>
val subtraction : r:shape -> s:shape -> shape

/// <summary>
/// Construct a group of two shapes.
/// </summary>
/// <param name=r>The first shape.</param>
/// <param name=s>The second shape.</param>
/// <returns>The group.</returns>
val group : r:shape -> r:shape -> shape

////////////////////////////////////////////////////////////////////////////////
// Rendering
// -----------------------------------------------------------------------------
//
////////////////////////////////////////////////////////////////////////////////

/// <summary>
/// Create a camera.
/// </summary>
/// <param name=p>The position of the camera.</param>
/// <param name=l>The position of the lookat point.</param>
/// <param name=u>The upvector of the camera.</param>
/// <param name=z>The zoom of the camera (distance from camera to pixel grid).</param>
/// <param name=w>The unitwidth of the camera.</param>
/// <param name=h>The unitheight of the camera.</param>
/// <param name=pw>The pixelwidth of the pixelgrid.</param>
/// <param name=ph>The pixelheight of the pixelgrid.</param>
/// <returns>The camera.</returns>
val mkCamera : p:point -> l:point -> u:vector -> z:float -> w:float -> h:float -> pw:int -> ph:int -> camera

/// <summary>
/// Create a light with an origin, a color, and an intensity.
/// </summary>
/// <param name=p>The origin of the light.</param>
/// <param name=c>The color of the light.</param>
/// <param name=i>The intensity of the light.</param>
/// <returns>The light.</returns>
val mkLight : p:point -> c:colour -> i:float -> light

/// <summary>
/// Create an ambient light with a color and an intensity.
/// </summary>
/// <param name=c>The color of the light.</param>
/// <param name=i>The intensity of the light.</param>
/// <returns>The light.</returns>
val mkAmbientLight : c:colour -> i:float -> ambientLight

/// <summary>
/// Create a scene with a list of shapes and lights.
/// </summary>
/// <param name=ss>The shapes in the scene.</param>
/// <param name=ls>The lights in the scene.</param>
/// <returns>The scene.</returns>
val mkScene : ss:shape list -> ls:light list -> al:ambientLight -> c:camera -> mr:int -> scene

/// <summary>
/// Render a scene to screen.
/// </summary>
/// <param name=s>The scene to render.</param>
val renderToScreen : s:scene -> unit

/// <summary>
/// Render a scene to file.
/// </summary>
/// <param name=s>The scene to render.</param>
/// <param name=f>The file to render the scene to.</param>
val renderToFile : s:scene -> f:string -> unit

////////////////////////////////////////////////////////////////////////////////
// Affine Transformations
// -----------------------------------------------------------------------------
//
////////////////////////////////////////////////////////////////////////////////

/// <summary>
/// Construct a rotation transformation around the x axis.
/// </summary>
/// <param name=t>The angle to rotate in radians.</param>
/// <returns>The rotation transformation.</returns>
val rotateX : a:float -> transformation

/// <summary>
/// Construct a rotation transformation around the y axis.
/// </summary>
/// <param name=t>The angle to rotate in radians.</param>
/// <returns>The rotation transformation.</returns>
val rotateY : a:float -> transformation

/// <summary>
/// Construct a rotation transformation around the z axis.
/// </summary>
/// <param name=t>The angle to rotate in radians.</param>
/// <returns>The rotation transformation.</returns>
val rotateZ : a:float -> transformation

/// <summary>
/// Construct a shearing transformation around the x and y axis.
/// </summary>
/// <param name=d>The distance to shear.</param>
/// <returns>The shearing transformation.</returns>
val sheareXY : d:float -> transformation

/// <summary>
/// Construct a shearing transformation around the x and z axis.
/// </summary>
/// <param name=d>The distance to shear.</param>
/// <returns>The shearing transformation.</returns>
val sheareXZ : d:float -> transformation

/// <summary>
/// Construct a shearing transformation around the y and x axis.
/// </summary>
/// <param name=d>The distance to shear.</param>
/// <returns>The shearing transformation.</returns>
val sheareYX : d:float -> transformation

/// <summary>
/// Construct a shearing transformation around the y and z axis.
/// </summary>
/// <param name=d>The distance to shear.</param>
/// <returns>The shearing transformation.</returns>
val sheareYZ : d:float -> transformation

/// <summary>
/// Construct a shearing transformation around the z and x axis.
/// </summary>
/// <param name=d>The distance to shear.</param>
/// <returns>The shearing transformation.</returns>
val sheareZX : d:float -> transformation

/// <summary>
/// Construct a shearing transformation around the z and y axis.
/// </summary>
/// <param name=d>The distance to shear.</param>
/// <returns>The shearing transformation.</returns>
val sheareZY : d:float -> transformation

/// <summary>
/// Construct a scaling transformation.
/// </summary>
/// <param name=x>The x component of the transformation.</param>
/// <param name=y>The y component of the transformation.</param>
/// <param name=z>The z component of the transformation.</param>
/// <returns>The scaling transformation.</returns>
val scale : x:float -> y:float -> z:float -> transformation

/// <summary>
/// Construct a translation transformation.
/// </summary>
/// <param name=x>The x component of the transformation.</param>
/// <param name=y>The y component of the transformation.</param>
/// <param name=z>The z component of the transformation.</param>
/// <returns>The translation transformation.</returns>
val translate : x:float -> y:float -> z:float -> transformation

/// <summary>
/// Construct a mirroring transformation around the x axis.
/// </summary>
/// <returns>The mirroring transformation.</returns>
val mirrorX : transformation

/// <summary>
/// Construct a mirroring transformation around the y axis.
/// </summary>
/// <returns>The mirroring transformation.</returns>
val mirrorY : transformation

/// <summary>
/// Construct a mirroring transformation around the z axis.
/// </summary>
/// <returns>The mirroring transformation.</returns>
val mirrorZ : transformation

/// <summary>
/// Merge a list of transformations.
/// </summary>
/// <param name=ts>The transformations to merge.</param>
/// <returns>The merged transformation.</returns>
val mergeTransformations : ts:transformation list -> transformation

/// <summary>
/// Apply a transformation to a shape.
/// </summary>
/// <param name=s>The shape to apply the transformation to.</param>
/// <param name=t>The transformation to apply.</param>
/// <returns>The shape with transformation applied.</returns>
val transform : s:shape -> t:transformation -> shape
