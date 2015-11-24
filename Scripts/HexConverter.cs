//Author: Danny Lawrence, Unify Wiki http://wiki.unity3d.com/index.php?title=HexConverter
//Licensing under Creative Commons Attribution Share Alike: http://creativecommons.org/licenses/by-sa/3.0/

// Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
using UnityEngine;

public class HexConverter
{
	public string ColorToHex(Color32 color)
	{
		string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		return hex;
	}

	public Color HexToColor(string hex)
	{
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r,g,b, 255);
	}
}
