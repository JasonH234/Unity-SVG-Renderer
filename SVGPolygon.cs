/*
* Parses the polygon pointlist, and is able to calculate inflated/deflacted convex polygons for strokes
*
* @author Jason Haciepiri
* 
*/

using UnityEngine;
using System.Collections.Generic;

public class SVGPolygon: SVGElement
{
	//Set properties inherited by SVGElement.
	public SVGPolygon(string element)
	{
		fill = SetFill (element);
		opacity = SetOpacity (element);
		points = SetPoints(element);
		id = setId (element);
		SetStroke (element);

		if(hasStroke)
			strokePoints = SetStrokePoints ();
	}
    
    //Find polygon points from input string.
	public override List<Vector2> SetPoints(string str)
	{
		int startPos = str.IndexOf("points=") + 8;
		
		// Extract point information.
		string pointString = str.Substring(startPos);
		
		// Remove string after closing quotation mark.
		pointString = pointString.Split("\""[0])[0];
		
		// Store points in a string array.
		string[] positions = pointString.Split(' ');
		List<Vector2> points = new List<Vector2>();
		
		// Parse and set points.
		for(int i = 0; i < positions.Length-1; i++)
		{
			string[] coordinates = positions[i].Split(","[0]);
			float x1, y1;
			float.TryParse(coordinates[0], out x1);
			float.TryParse(coordinates[1], out y1);
			points.Add (new Vector2(x1,y1));
		}
		return points;
	}

	// Legacy. Used for finding the centroid of a polygon.
	public Vector2 getPolyCentre()
	{
			
		float sumX = 0;
		float sumY = 0;
		float sumA = 0;
		//Calculate sums.
		for(int i = 0; i < points.Count; i++)
		{
			int i1 = (i+1)%points.Count;
			float factor2 = (points[i].x * points[i1].y) - (points[i1].x * points[i].y);
			sumX += (points[i].x + points[i1].x) * factor2;
			sumY += (points[i].y + points[i1].y) *factor2;
			sumA += factor2;
		}
		//Divide to find centroid.
		sumA /= 2;
		sumX /= (6*sumA);
		sumY /= (6*sumA);
		
		//if negative, then polygon is counter-clockwise.
		if(sumX < 0)
		{
			sumX = -sumX;
			sumY = -sumY;
		}
		
		return new Vector2(sumX, sumY);
	}

	//Utility function to find whether a polygon's points are going in a clockwise or anti-clockwise direction.
	public bool isPolyClockwise(Vector2 p0, Vector2 p1, Vector2 p2)
	{
		Vector2 edge1 = p1 - p0;
		Vector2 edge2 = p2 - p1;

		Vector2 normal1 = new Vector2(edge1.y, -edge1.x);
		float prod = (edge2.x * normal1.x) + (edge2.y * normal1.y);

		if(prod == 0)
			Debug.LogError("Polygon edge product is 0. Resulting poly stroke may be incorrect.");

		if(prod > 0)
			return true;
		else 
			return false;
	}

	// Offsets each edge by the strokewidth, by creating a normal edge for each pair of vertices, 
	// scaling the normal by the stroke width, and then adding it to the original points to find
	// an offsetted parallel line.
	public Edge[] getStrokeLines(bool isClockwise)
	{
		Edge[] strokeLines = new Edge[points.Count];

		
		for(int i = 0; i < points.Count; i++)
		{
			Vector2 edge = points[(i+1)%points.Count] - points[i];
			if(!isClockwise)
				edge = new Vector2(edge.y, -edge.x);
			else
				edge = new Vector2(-edge.y, edge.x);
			// get unit vector.
			edge = edge.normalized;
            
            // multiply the unit vector by stroke width.
			edge *= strokeWidth;
			// add scaled normal values to original points to get stroke points.
			Vector2 s1 = points[i] + edge;
			Vector2 s2 = points[(i+1)%points.Count] + edge;
			strokeLines[i] = new Edge(s1, s2);
		}
		return strokeLines;
	}

	// Finds the intersections between each pair of lines, which give the points of the
	// inflated polygon.
	public Vector2[] addIntersections(Edge[] edges)
	{
		Vector2[] newPoints = new Vector2[edges.Length];
		for(int i = 0; i < edges.Length; i++)
		{
			// get edge vertices.
			Vector2 v0 = edges[i].getV0();
			Vector2 v1 = edges[i].getV1();
			Vector2 v2 = edges[(i+1)%edges.Length].getV0();
			Vector2 v3 = edges[(i+1)%edges.Length].getV1();
			// find lines.
			Vector2 e1 = v1 - v0;
			Vector2 e2 = v2 - v3;
			Vector2 e3 = v2 - v0;
			// add vertex at intersection.
			float c = ((e2.x * e3.y) - (e2.y * e3.x)) / ((e1.y * e2.x) - (e1.x * e2.y));
			Vector2 s = v0 + (c * (v1 - v0));
			newPoints[i] = s;
        }
		return newPoints;
	}

	// Calls all methods needed to find the points of the inflated polygon, with a fixed depth.
	public override List<Vector2> SetStrokePoints()
	{
		List<Vector2> strokePoints = new List<Vector2> ();
		// returns whether polygon vertex order is clockwise or counter-clockwise.
		bool isClockwise = isPolyClockwise(points[0], points[1], points[2]);
		Edge[] strokeLines = getStrokeLines(isClockwise);
		strokePoints.AddRange(addIntersections(strokeLines));
		return strokePoints;
	}
}
