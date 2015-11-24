# Unity SVG Renderer
Sample source code for parsing and rendering SVG in Unity, to provide an example of how an SVG renderer can be built in Unity and to provide support for simple SVG rendering. It was originally developed for 4.5 or 4.6, although it has been shown to work on 5.2.3 also.

While we did develop support for the full specification as part of a project last year, we unfortunately do not currently have the time to clean up and support the rest of the renderer in order to provide it here. As such we have only provided support for SVG polygons in this sample - though we may address this at a later time. 

Implementing SVG primitives (Rect, Line, Circle, Ellipse..) is less complex than implementing the SVG Polygon class, and so it is possible to do this fairly easily using the SVG Polygon class as an example. However it is significantly more difficult to support paths and strokes. The stroke solution provided in this implementation is for convex polygons only - though full stroke support can easily be created for the SVG primitives if you do choose to implement these. In our own implementation, we chose to use shaders when we realized the difficulty of calculating stroke meshes for complex shapes. 

The project uses the HexConverter and Traingulator classes from the Unify Wiki, which are included in the sample and can be found on the following links: 
http://wiki.unity3d.com/index.php?title=HexConverter
http://wiki.unity3d.com/index.php?title=Triangulator 

Authors:
Dillon Keith Diep and Jason Haciepiri
