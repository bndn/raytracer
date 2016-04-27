module Render

open System.Drawing

open Scene

/// <summary>
/// Render a scene to a Bitmap object.
/// </summary>
/// <param name=s>The scene to render.</param>
/// <returns>The rendered bitmap of the scene.</returns>
val render : s:Scene -> Bitmap
