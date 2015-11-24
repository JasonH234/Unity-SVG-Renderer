using UnityEngine;
using System.Collections;

public class GenerateSVG : MonoBehaviour {

	public string SVGFilePath;
	// Use this for initialization
	void Start () {
		SVGtoMesh creator = this.gameObject.GetComponent <SVGtoMesh>() as SVGtoMesh;
		creator.importSVG (SVGFilePath);
	}

}
