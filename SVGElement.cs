// Authors: Dillon Keith Diep and Jason Haciepiri
// Base SVG Element class, used to parse, calculate and store SVG element variables for mesh generation.

using UnityEngine;
using System.Collections.Generic;

public class SVGElement
{
    // Colour conversion utility object - rgb to hex
    HexConverter HexConv = new HexConverter();
    // General SVG properties
    public string id;
    public Color fill;
    public Color stroke;
    public float opacity = 1.0f;
    public bool hasStroke = false;
    public float strokeWidth =1, strokeMiterlimit;
    // Point lists specifying the shapes
    public List<Vector2> strokePoints = new List<Vector2>();
    public List<Vector2> points = new List<Vector2>();
    // Used for path calculation
    public List<string> ss = new List<string>();
    // Variables unique to text meshes
    public int fontSize = 24;
    public string textString = "";
    public string fontType = "";
    public Vector2 singlePoint;
    // Quality settings (the higher the better. 
    public int subdivisions = 4;
    public int circleDiv = 64; // Sets number of points used when drawing circles and ellipses
    // Used for a number of primitives
    public float width = 0.0f, height = 0.0f;

    // Parse and set rgb fill
    public Color SetFill(string element)
    {
        // parse hex value
        string colorHex = getColorHex(element, "fill", 6);
        // Convert hex to rgb and store
        fill = HexConv.HexToColor(colorHex);
        return fill;
    }

    public string getColorHex(string element, string property, int propertyLen)
    {
        string colorHex = "";
        int colorPos = element.IndexOf (property+"=");
        if (colorPos != -1)
        {
            colorPos += propertyLen;
            if (element [colorPos] == '#')
            {
                // Extract hex value from expected location
                colorHex = element.Substring (colorPos+1,6);
            }
            else if (element.Substring (colorPos,4) == "none")
            {
                // Set fill to be transparent
                opacity = 0.0f;
            }
            else
            {
                // Assume of form fill:# (one character earlier)
                colorHex = element.Substring (colorPos,6);
            }
        }
        // If hex colour has been found
        if (colorHex != "")
        {
            // Check if hex is in shortform (if " occurs)
            int isShort = colorHex.IndexOf ('"');
            if (isShort != -1)
            {
                string notShort = "";
                notShort = notShort + colorHex[0];
                notShort = notShort + colorHex[0];
                notShort = notShort + colorHex[1];
                notShort = notShort + colorHex[1];
                notShort = notShort + colorHex[2];
                notShort = notShort + colorHex[2];
                colorHex = notShort;
            }
        }
        return colorHex;
    }

    // Parse and set rgb fill and stroke width
    public Color SetStroke(string element)
    {
        int strokePos = element.IndexOf("stroke=");

        if(strokePos != -1)
        {
            hasStroke = true;
            // parse hex value
            string strokeHex = getColorHex(element, "stroke", 8);
            // convert hex to rgb and store
            stroke = HexConv.HexToColor(strokeHex);

            //get stroke width
            int strokeWidthPos = element.IndexOf("stroke-width=");
            if(strokeWidthPos != -1)
            {
                strokeWidthPos += 14;
                int length = element.Substring(strokeWidthPos).IndexOf('"');
                strokeWidth = float.Parse (element.Substring(strokeWidthPos, length));
            }
            else
            {
                strokeWidth = 1;
            }
        }
        return stroke;
    }
    
    // set fill transparency level
    public float SetOpacity(string element)
    {
        int opacityPos = element.IndexOf("opacity=");
        if (opacityPos != -1)
        {
            opacityPos += 10;
            opacity = float.Parse (element.Substring (opacityPos).Split ("\""[0])[0]);
        }
        return opacity;
    }

    // Virtual functions - optional for child classes to define (eg text does not use these)
    public virtual List<Vector2> SetPoints(string str)
    {
        return null;
    }
    public virtual List<Vector2> SetStrokePoints()
    {
        return null;
    }

    // checks for id tag and returns parsed string if found
    public string setId(string str)
    {
        if (str.Contains("id="))
        {
            int xPos = str.IndexOf("id=") + 4;
            int len = str.Substring(xPos).IndexOf('"');
            return str.Substring(xPos, len);
        }
        else
        {
            return "no_id";
        }
    }
}