// Authors: Dillon Keith Diep and Jason Haciepiri
// Class for parsing SVG files and generating meshes from the extracted properties

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SVGtoMesh : MonoBehaviour
{   
    // Default point scalar
    float scale = 0.01f;
    // Mesh materials
    public Material mat;
    public Material matNoAlpha;

    // bounds variables
    float lowX = 1000.0f, highX = -1000.0f, lowY = 1000.0f, highY = -1000.0f;

    // Create SVG type selector
    enum SVGType
    {
        polygon,
        path,
        line,
        text,
        rect,
        circle,
        ellipse,
        none
    };
    SVGType type = SVGType.none;


    // Utility function to remove unnecessary characters from the SVG file
    string cleanString(string str)
    {
        str = str.Replace (" \n", "");
        str = str.Replace ("\n", "");
        str = str.Replace ("\t", "");
        str = str.Replace ("\r", "");
        
        // Removes double spaces twice for all extra spaces
        str = str.Replace("  "," ");
        str = str.Replace("  "," ");
        return str;
    }

    // Parsed the file specified by the path parameter
    void importSVG(string path)
    {
        SVGElement element = new SVGElement();
        TextAsset svgTextAsset = (TextAsset)Resources.Load("SVG/"+path); //load svg file as textasset
        string SVGData = svgTextAsset.text;

        // Remove superfluos characters
        SVGData = cleanString (SVGData);
        
        // Extract individual elements
        string[] SVGElements = SVGData.Split('<');
        // Parse each element according to its type
        for (int i = 0; i < SVGElements.Length - 1; i++)
        {
            // Extract element type
            string e = SVGElements[i].Split (' ')[0];
            // Ignore empty strings and end tags
            if(e.Length > 0 && e[0] == '/')
                continue;

            switch(e)
            {
            case "polygon":
                element = new SVGPolygon(SVGElements[i]);
                type = SVGType.polygon;
                break;
            case "path":
                element = new SVGPath(SVGElements[i]);
                type = SVGType.path;
                break;
            case "circle":
                element = new SVGCircle(SVGElements[i]);
                type = SVGType.circle;
                break;
            case "ellipse":
                element = new SVGEllipse(SVGElements[i]);
                type = SVGType.ellipse;
                break;
            case "rect":
                element = new SVGRect(SVGElements[i]);
                type = SVGType.rect;
                break;
            case "line":
                element = new SVGLine(SVGElements[i]);
                type = SVGType.line;
                break;
            case "text":
                element = new SVGText(SVGElements[i]);
                type = SVGType.text;
                break;
            default:
                if (!(String.IsNullOrEmpty(e) || e != "svg")) break;
                    Debug.Log ("Element '" + SVGElements[i] + "' not recognised");
                break;
            }

            // Access element points
            List<Vector2> points = element.points;

            // Move on if there is no mesh to generate
            if ((points.Count == 0 && type != SVGType.text) || type == SVGType.none)
            {
                continue;
            }

            // Call mesh creation functions

            // Text mesh
            if(type == SVGType.text)
            {
                createText (element.id, element.singlePoint, element.fill, element.opacity, element.textString, element.fontSize, element.fontType, i);
            }
            // Line mesh
            else if(type == SVGType.line) 
            {
                createMesh (element.strokePoints, element.stroke, element.opacity, i, element.id);
            }
            // All others
            else
            {
                // Create primary mesh
                createMesh (element.points, element.fill, element.opacity, i+1, element.id);
                // Create stroke mesh only if there are enough points
                if(element.strokePoints.Count > 2) 
                    createMesh (element.strokePoints, element.stroke, element.opacity, i, element.id);
            }
            // reset type to default
            type = SVGType.none;
        }
    }

    void createText(string id, Vector2 pos, Color col, float opacity, string text, int fontSize, string fontType, int i)
    {
        // Instantiate a new object into the scene
        GameObject t = new GameObject("Text Object");

        if (id != "") 
            t.name = id;
        // calculate depth from current iteration (later meshes will be placed further back)
        float depth = (-i*1.0f)/40;
        // Set object position and scale
        t.transform.position += this.transform.position;
        t.transform.localScale *= 0.1f;
        t.transform.position += new Vector3(scale*pos.x, -scale*pos.y, depth);
        t.transform.parent = this.transform;
        txtObj.anchor = TextAnchor.LowerLeft;

        // Add TextMesh component to object
        TextMesh txtObj = t.AddComponent<TextMesh>();
        
        // Set and format text from parsed svg file information
        txtObj.text = text;
        txtObj.fontSize = fontSize;
        Font f = (Font)Resources.Load ("Fonts/"+fontType);
        // If the font used in the SVG file has been found in the Resources folder, set it
        if(f != null) 
        {
            txtObj.font = f;
        }
        // Otherwise default to using Arial
        else 
        {
            f = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            Debug.LogWarning("Font '"+fontType+"' not found. Defaulting to Arial.");
        }

        // Set material, color and lighting
        txtObj.renderer.sharedMaterial = f.material;
        col.a = opacity;
        txtObj.color = col;
    }

    void createMesh(List<Vector2> points, Color colorRGB, float opacity, int i, string id)
    {

        // Use triangulator utility script to triangulate the points
        Triangulator triangulator = new Triangulator(points.ToArray());
        int[] triangles = triangulator.Triangulate ("revert");
        
        // Initialise vertex list
        Vector3[] vertices = new Vector3[points.Count];

        // Find centroid for anchor
        float totalX = 0.0f;
        float totalY = 0.0f;

        for (int j = 0; j < points.Count; j++)
        {
            if (points[j].x > highX)
                highX = points[j].x;
            if (points[j].x < lowX)
                lowX = points[j].x;
            if (points[j].y > highY)
                highY = points[j].y;
            if (points[j].y < lowY)
                lowY = points[j].y;

            totalX += points[j].x;
            totalY += points[j].y;
        }

        float offsetX = 0.0f;
        float offsetY = 0.0f;

        if (points.Count > 1) 
        {
            offsetX = totalX / points.Count;
            offsetY = totalY / points.Count;
        }

        // Convert 2D point list into scaled, 3D space, according to offsets
        for (int j = 0; j < points.Count; j++)
        {
            vertices[j] = new Vector3(scale*(points[j].x-offsetX), -scale*(points[j].y-offsetY), 0);
        }
        
        // Create and set Mesh object properties
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.uv = points.ToArray();
        
        // Create a new GameObject for the mesh
        GameObject meshObject = new GameObject(id);
        
        // Find depth for layers in SVG image
        float depth = (-i*1.0f)/40;

        // Set object transformations
        meshObject.transform.position += this.transform.position;
        meshObject.transform.position += new Vector3(scale*offsetX, -scale*offsetY, depth);
        meshObject.transform.parent = this.transform;
        
        // Add mesh components to the game object to render the generated mesh
        meshObject.AddComponent("MeshFilter");
        meshObject.AddComponent("MeshRenderer");
        meshObject.GetComponent<MeshFilter>().mesh = mesh;

        // Instantiate a material around the mesh to visualise the planes     
        Material meshMat = Instantiate( mat ) as Material;

        // Set color and apply lighting
        colorRGB.a = opacity;
        meshMat.SetColor("_Color", colorRGB);
        meshMat.SetColor("_Emission", colorRGB);
        meshObject.renderer.material = meshMat;
    }
}