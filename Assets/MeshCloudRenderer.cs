using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class MeshCloudRenderer : MonoBehaviour {
	public MeshFilter meshFilter;
	public Mesh mesh;
	private List<Vector3> vertices = new List<Vector3>();
	private List<int> indices = new List<int>();
	private List<Color32> colors = new List<Color32>();
	private List<Vector2> uvs = new List<Vector2>();
	private List<Vector3> normals = new List<Vector3>();
	public bool updateMesh = false;

	// Use this for initialization
	void Start () {
		meshFilter = gameObject.GetComponent<MeshFilter>();
		meshFilter.mesh = mesh = new Mesh();

		// Tetrahedron Test
		/*StartCoroutine(Testrahedron());*/
		CloudPoint p0 = new CloudPoint(new Vector(new double[] {0,0,0}), Color.red, new Vector(new double[] {0,0,0}));
		CloudPoint p1 = new CloudPoint(new Vector(new double[] {1,0,0}), Color.green, new Vector(new double[] {0,0,0}));
		CloudPoint p2 = new CloudPoint(new Vector(new double[] {0.5f,0,Mathf.Sqrt(0.75f)}), Color.blue, new Vector(new double[] {0,0,0}));
		CloudPoint p3 = new CloudPoint(new Vector(new double[] {0.5f,Mathf.Sqrt(0.75f),Mathf.Sqrt(0.75f)/3}), Color.yellow, new Vector(new double[] {0,0,0}));
		addTriangle(p0,p1,p2);
		addTriangle(p0,p2,p3);
		addTriangle(p2,p1,p3);
		addTriangle(p0,p3,p1);
	}

	/*IEnumerator Testrahedron() {
		Vector3 p0 = new Vector3(0,0,0);
		Vector3 p1 = new Vector3(1,0,0);
		Vector3 p2 = new Vector3(0.5f,0,Mathf.Sqrt(0.75f));
		Vector3 p3 = new Vector3(0.5f,Mathf.Sqrt(0.75f),Mathf.Sqrt(0.75f)/3);
		addTriangle(p0,p1,p2);
		yield return StartCoroutine(MyWaitFunction (1.0f));
		addTriangle(p0,p2,p3);
        	yield return StartCoroutine(MyWaitFunction (1.0f));
		addTriangle(p2,p1,p3);
        	yield return StartCoroutine(MyWaitFunction (1.0f));
		addTriangle(p0,p3,p1);
    	}
	
	IEnumerator MyWaitFunction (float delay) {
        	float timer = Time.time + delay;
        	while (Time.time < timer) {
            	yield return null;
        	}
    	}*/
	
	// Update is called once per frame
	void Update () {
		/*// Random Triangle Test
		Vector3 p1 = new Vector3(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
		Vector3 p2 = new Vector3(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
		Vector3 p3 = new Vector3(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
		addTriangle(p1,p2,p3);*/

		// Update the Mesh if flag is set.
		if (updateMesh) {
			// Clear the Mesh.
			mesh.Clear();

			// Update vertices and triangles of the Mesh.
			mesh.vertices = vertices.ToArray();
			mesh.triangles = indices.ToArray();

			// Apply Colors if applicable.
			if (colors.Count == vertices.Count) {
				mesh.colors32 = colors.ToArray();
			}
			// Apply UVs if applicable/
			if (uvs.Count == vertices.Count) {
				mesh.uv = uvs.ToArray();
			}

			// Clean up the updated mesh.
			mesh.RecalculateNormals(); 
			mesh.RecalculateBounds(); 
			mesh.Optimize();
			updateMesh = false;
		}
	}

	private void addTriangle(Vector3 p1, Vector3 p2, Vector3 p3) {
		// Add triangle to the Mesh using inputted Vector3 locations. No colors.
		vertices.Add(p1);
		vertices.Add(p2);
		vertices.Add(p3);
		indices.Add(indices.Count);
		indices.Add(indices.Count);
		indices.Add(indices.Count);
		//uvs.Add(new Vector2((float) p1.x, (float) p1.z));
		//uvs.Add(new Vector2((float) p2.x, (float) p2.z));
		//uvs.Add(new Vector2((float) p3.x, (float) p3.z));
		updateMesh = true;
	}

	private void addTriangle(CloudPoint p1, CloudPoint p2, CloudPoint p3) {
		// Add triangle to the Mesh using inputted CloudPoints to use as colored vertices.
		vertices.Add(new Vector3((float) p1.location[0], (float) p1.location[1], (float) p1.location[2]));
		vertices.Add(new Vector3((float) p2.location[0], (float) p2.location[1], (float) p2.location[2]));
		vertices.Add(new Vector3((float) p3.location[0], (float) p3.location[1], (float) p3.location[2]));
		colors.Add(p1.color);
		colors.Add(p2.color);
		colors.Add(p3.color);
		indices.Add(indices.Count);
		indices.Add(indices.Count);
		indices.Add(indices.Count);
		//uvs.Add(new Vector2((float) p1.location[0], (float) p1.location[2]));
		//uvs.Add(new Vector2((float) p2.location[0], (float) p2.location[2]));
		//uvs.Add(new Vector2((float) p3.location[0], (float) p3.location[2]));

		updateMesh = true;
	}

	private void addTriangle(CloudPoint[] cp, int i1, int i2, int i3) {
		// Add triangle to the Mesh using inputted CloudPoint[] array and indices of points to use as colored vertices.
		vertices.Add(new Vector3((float) cp[i1].location[0], (float) cp[i1].location[1], (float) cp[i1].location[2]));
		vertices.Add(new Vector3((float) cp[i2].location[0], (float) cp[i2].location[1], (float) cp[i2].location[2]));
		vertices.Add(new Vector3((float) cp[i3].location[0], (float) cp[i3].location[1], (float) cp[i3].location[2]));
		colors.Add(cp[i1].color);
		colors.Add(cp[i2].color);
		colors.Add(cp[i3].color);
		indices.Add(indices.Count);
		indices.Add(indices.Count);
		indices.Add(indices.Count);
		//uvs.Add(new Vector2((float) cp[i1].location[0], (float) cp[i1].location[2]));
		//uvs.Add(new Vector2((float) cp[i2].location[0], (float) cp[i2].location[2]));
		//uvs.Add(new Vector2((float) cp[i3].location[0], (float) cp[i3].location[2]));
		//normals.Add()
		updateMesh = true;
	}

	private float Max (CloudPoint[] cp) {
    		float max = (float) cp[0].location[0];

    		// Search through points[] array and return the max recorded value within the array.
    		for (int i = 1; i < cp.Length; i++)
      		for (int j = 0; j < 3; j++) 
      			if (cp[i].location[j] > max)
 	      			max = (float) cp[i].location[j];

    		return max;
  	}


	private void correctNegativePoints(CloudPoint[] cp) {
    		// Find the most negative value for each coordinate and subtract it from every one of those resepective coordinates.
		// For example, if -300 was the most negative x-coordinate in cp[], this would subtract -300 from every x-coordinate in cp[].
	    	Vector3 negaXYZ = Vector3.zero;
	    	for (int i = 0; i < cp.Length; i++) {
	      	if (cp[i].location[0] < negaXYZ.x)   negaXYZ.x = (float) cp[i].location[0];
	      	if (cp[i].location[1] < negaXYZ.y)   negaXYZ.y = (float) cp[i].location[1];
	      	if (cp[i].location[2] < negaXYZ.z)   negaXYZ.z = (float) cp[i].location[2];
	    	}

	    	for (int i = 0; i < cp.Length; i++) {
	      	cp[i].location[0] -= negaXYZ.x;
	      	cp[i].location[1] -= negaXYZ.y;
	      	cp[i].location[2] -= negaXYZ.z;
	    	}
	}

  	private void normalizePoints(CloudPoint[] cp) {
    		correctNegativePoints(cp);
    		float max = Max(cp);

    		// Force every point to be in the unit cube.
    		for (int i = 0; i < cp.Length; i++) {
      		cp[i].location[0] /= max;
      		cp[i].location[1] /= max;
     			cp[i].location[2] /= max;
    		}

  	}
}
