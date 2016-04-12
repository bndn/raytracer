module RaytracerSample

open Raytracer
open System
open System.Drawing

  (* Input the path (local or absolute) where you want your files to be stored *)
let path_to_files = "../../../"

let doRender scene toFile =
    match toFile with
    | Some filename -> renderToFile scene filename
    | None -> renderToScreen scene

let renderSphere toScreen =
    let light = mkLight (mkPoint 0.0 0.0 4.0) (fromColor Color.White) 1.0 in
    let ambientLight = mkAmbientLight (fromColor Color.White) 0.1 in
    let camera = mkCamera (mkPoint 0.0 0.0 4.0) (mkPoint 0.0 0.0 0.0) (mkVector 0.0 1.0 0.0) 1.0 2.0 2.0 500 500 in
    let sphere = mkSphere (mkPoint 0.0 0.0 0.0) 1.0 (mkMatTexture (mkMaterial (fromColor Color.Blue) 0.0)) in
    let scene = mkScene [sphere] [light] ambientLight camera 0 in
    if toScreen then
        doRender scene None
    else
        doRender scene (Some (path_to_files + "renderSphere.png"))

let renderInsideSphere toScreen =
    let light = mkLight (mkPoint 0.0 0.0 0.0) (fromColor Color.White) 3.0 in
    let ambientLight = mkAmbientLight (fromColor Color.White) 0.1 in
    let camera = mkCamera (mkPoint 0.0 0.0 0.0) (mkPoint 0.0 0.0 4.0) (mkVector 0.0 1.0 0.0) 1.0 2.0 2.0 500 500 in
    let sphere = mkSphere (mkPoint 0.0 0.0 0.0) 1.0 (mkMatTexture (mkMaterial (fromColor Color.Red) 0.0)) in
    let scene = mkScene [sphere] [light] ambientLight camera 0 in
    if toScreen then
        doRender scene None
    else
        doRender scene (Some (path_to_files + "renderInsideSphere.png"))

type public Program() =
  member this.Main() =
    renderSphere true
