using UnityEngine;
using System.Collections.Generic;

public class Edge
{
	private Vector2 v0;
	private Vector2 v1;

	public Edge (Vector2 v0, Vector2 v1)
	{
		this.v0 = v0;
		this.v1 = v1;
	}

	public Vector2 getV0()
	{
		return this.v0;
	}

	public Vector2 getV1()
	{
		return this.v1;
	}
}
