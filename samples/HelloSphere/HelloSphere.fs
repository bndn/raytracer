module HelloSphere

open Raytracer

type public Program() =
    member this.Main() =
        let light = mkLight (mkPoint 0.0 0.0 2.0) (mkColour 1. 1. 1.) 1.0
        let ambientLight = mkAmbientLight (mkColour 1. 1. 1.) 0.1
        let camera = mkCamera (mkPoint 0.0 0.0 2.0) (mkPoint 0.0 0.0 0.0) (mkVector 0.0 1.0 0.0) 1.0 2.0 2.0 500 500
        let sphere = mkSphere (mkPoint 0.0 0.0 0.0) 1.0 (mkMatTexture (mkMaterial (mkColour 0. 0. 1.) 0.0))
        let scene = mkScene [sphere] [light] ambientLight camera 0

        renderToScreen scene
