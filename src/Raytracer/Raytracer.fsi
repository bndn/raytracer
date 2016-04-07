module Raytracer

open Vector
open Point
open Camera

type dummy = unit

type vector = Vector
type point = Point
type colour = dummy
type material = dummy
type shape = dummy
type baseShape = dummy
type texture = dummy
type camera = Camera
type scene = dummy
type light = dummy
type ambientLight = dummy
type transformation = dummy

val mkVector : x:float -> y:float -> z:float -> vector
val mkPoint : x:float -> y:float -> c:float -> point
val fromColor : c : System.Drawing.Color -> colour
val mkColour : r:float -> g:float -> b:float -> colour

/// Construct a material with the given colour and reflectivity. Reflectivity
/// is the ratio of how much the material reflects (1.0 means it is a perfect
/// mirror and 0.0 means that the material does not reflect anything).
val mkMaterial : colour -> reflectivity : float -> material
/// Textures are functions that take x-y coordinates and produce a material.
/// The x-y coordinates range over the texture space specified for the
/// individual basic shapes (mkSphere, mkPlane etc.).
val mkTexture : (float -> float -> material) -> texture
/// Construct a texture with a constant material for each point.
val mkMatTexture : material -> texture


/// Construct a textured shape from a base shape.
/// Basic shapes are textured according to the texture space given.
val mkShape : baseShape -> texture -> shape

/// Construct a sphere.
/// texture coordinates: [0,1] X [0,1]
val mkSphere : center : point -> radius : float -> texture -> shape
/// Construct a rectangle.
/// texture coordinates: [0,1] X [0,1]
val mkRectangle : corner : point -> width : float -> height : float -> texture -> shape
/// Constructe a triangle.
val mkTriangle : a:point -> b:point -> c:point -> material -> shape
/// Construct a plane with the equation z = 0,
/// i.e. the x-y plane
/// texture coordinates: R X R
val mkPlane : texture -> shape
/// Construct an implicit surface.
/// texture coordinates: {(0,0)}, i.e. has only a single material
val mkImplicit : string -> baseShape
/// Load a triangle mesh from a PLY file.
/// texture coordinates: [0,1] X [0,1]
val mkPLY : filename : string -> smoothShading : bool -> baseShape
/// construct a hollow cylinder (i.e. open on both ends)
/// texture coordinates: [0,1] X [0,1]
val mkHollowCylinder : center : point -> radius : float -> height : float -> texture -> shape
/// construct a solid cylinder (i.e. closed on either end by a disk)
/// texture space: hollow cylinder part is textured like mkHollowCylinder;
///                top and bottom disk are textured like mkDisk
val mkSolidCylinder : center : point -> radius : float -> height : float ->
                        cylinder: texture -> bottom : texture -> top : texture -> shape
/// construct a disk at point p in the plane parallel
/// to the x-y plane
/// texture coordinates: [0,1] X [0,1]
val mkDisc : p : point -> radius : float -> texture -> shape
/// construct an axis-aligned box with lower corner low
/// and higher corner high
/// textures: the six faces of the box are textured like mkRectangle
val mkBox : low : point -> high : point -> front : texture -> back : texture ->
            top : texture -> bottom : texture -> left : texture -> right : texture  -> shape

val union : shape -> shape -> shape
val intersection : shape -> shape -> shape
val subtraction : shape -> shape -> shape
val group : shape -> shape -> shape

/// Construct a camera at 'position' pointed at 'lookat'. The 'up' vector describes which way is up.
/// The viewplane is has dimensions 'unitWidth' and 'unitHeight', has a pixel resolution of 'pixelWidth'
/// times 'pixelHeight', and is 'distance' units in front of the camera.
val mkCamera : position : point -> lookat : point -> up : vector -> distance : float ->
    unitWidth : float -> unitHeight : float -> pixelWidth : int -> pixelHeight : int -> camera

val mkLight : position : point -> colour : colour -> intensity : float -> light

val mkAmbientLight : colour : colour -> intensity : float -> ambientLight

val mkScene : shapes : shape list -> lights : light list -> ambientLight -> camera -> max_reflect : int -> scene
val renderToScreen : scene -> unit
val renderToFile : scene -> filename : string -> unit

val rotateX : angle : float -> transformation
val rotateY : angle : float -> transformation
val rotateZ : angle : float -> transformation
val sheareXY : distance : float -> transformation
val sheareXZ : distance : float -> transformation
val sheareYX : distance : float -> transformation
val sheareYZ : distance : float -> transformation
val sheareZX : distance : float -> transformation
val sheareZY : distance : float -> transformation
val scale : x : float -> y : float -> z : float -> transformation
val translate : x : float -> y : float -> z : float -> transformation
val mirrorX : transformation
val mirrorY : transformation
val mirrorZ : transformation
/// Merge the givne list of transformations into one, such that the resulting
/// transformation is equivalent to applying the individual transformations
/// from left to right (i.e. starting with the first element in the list).
val mergeTransformations : transformation list -> transformation
val transform : shape -> transformation -> shape
