using UnityEngine;
using System.Collections;

public class Controls : MonoBehaviour {
	private Camera cam;
	public bool controlsEnabled = true;
  	public bool autoRotateEnabled = false;
	public float rotateSpeed = 50.0f;
  	public float zoomSpeed = 25.0f;
  	public float max = 1.0f;
  	public Color backgroundRGBA = Color.white/4;

	// Use this for initialization
	void Start () {
		cam = Camera.mainCamera.camera;
		// Set the background of the control camera's display.
   		cam.backgroundColor = backgroundRGBA;
	}
	
	// Update is called once per frame
	void Update () {
		if (controlsEnabled)
		{
		      // Horizontal camera rotation controls:
		      if (Input.GetKey("up"))
		        cam.transform.RotateAround(new Vector3(max/2, max/2, max/2), Vector3.right, rotateSpeed*Time.deltaTime);
		      else if (Input.GetKey("down"))
		        cam.transform.RotateAround(new Vector3(max/2, max/2, max/2), -1*Vector3.right, rotateSpeed*Time.deltaTime);

		      // Vertical camera rotation controls:
		      if (Input.GetKey("right"))
		        cam.transform.RotateAround(new Vector3(max/2, max/2, max/2), -1*Vector3.up, rotateSpeed*Time.deltaTime);
		      else if (Input.GetKey("left"))
		        cam.transform.RotateAround(new Vector3(max/2, max/2, max/2), Vector3.up, rotateSpeed*Time.deltaTime);
		    
		      // Scroll camera zoom controls:
		      cam.transform.position += cam.transform.forward * Input.GetAxis("Mouse ScrollWheel")*zoomSpeed*Time.deltaTime;
		}

	    	if (autoRotateEnabled)
	    	{
	      	// Rotate up and to the right around the center of the point cloud.
	      	cam.transform.RotateAround(new Vector3(max/2, max/2, max/2), Vector3.right + Vector3.up, rotateSpeed*Time.deltaTime);
	    	}
	}
}
