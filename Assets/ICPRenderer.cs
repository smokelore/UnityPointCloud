using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ICPRenderer : MonoBehaviour {
	// WILL NEED TO ATTACH THE "ParticleSystem" PREFAB TO "Particle Prefab" IN THE INSPECTOR
	public GameObject ParticlePrefab;
	private GameObject ps1;
	//private GameObject ps2;
	private List<PointCloud> cloudList;
	public Vector3 negaXYZ;
	public float scaleFactor;
	public CloudSystem newSystem;
	private int i = 0;
	//private int j = 0;
	public bool slow;

	// Use this for initialization
	void Start () 
	{
		GameObject ps1 = Instantiate(ParticlePrefab, Vector3.zero, Vector3.zero) as GameObject;
		//GameObject ps2 = Instantiate(ParticlePrefab, Vector3.zero, Vector3.zero) as GameObject;
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (i < cloudList.Count) {
			// update parameters when they have not bin set and the cloudList isn't empty
			if (negaXYZ == NULL || scaleFactor == NULL) {
				//getNega();
				//getScaleFactor();
				getParams();

				newSystem = new CloudSystem(ps1, cloudList[i], scaleFactor, negaXYZ);
				//CloudSystem newSystem2 = new CloudSystem(ps2, cloudList[i+1], scaleFactor, negaXYZ);
			} else {
				newSystem.SetPoints(cloudList[i].PointList);
			}

			i++;

		} else {
			i = 0;
		}
	}

	// get the amount you should translate every point cloud to avoid negative coordinates.
	public void getNega() 
	{
		Vector3 negaXYZ = Vector3.zero;
		foreach (PointCloud pc in cloudList)
		{
			foreach (CloudPoint cp in pc)
			{
				if (cp.location[0] < negaXYZ.x)   negaXYZ.x = cp.location[0];
				if (cp.location[1] < negaXYZ.y)   negaXYZ.y = cp.location[1];
				if (cp.location[2] < negaXYZ.z)   negaXYZ.z = cp.location[2];
			}
		}
	}

	// get the amount you should scale every point cloud coordinate by.
	private void getScaleFactor()
	{
		float max = -999999999;

		// Search through points[] array and return the max recorded value within the array.
		foreach (PointCloud pc in cloudList)
			foreach (CloudPoint cp in pc)
				for (int j = 0; j < 3; j++) 
					if (cp.location[j] > max)
						max = cp.location[j];

	}

	// combines the getScaleFactor() and getNega() functions in one ugly monstrosity.
	private void getParams()
	{
		max = -999999999;
		negaXYZ = Vector3.zero;

		foreach (PointCloud pc in cloudList)
		{	
			foreach (CloudPoint cp in pc)
			{
				for (int j = 0; j < 3; j++) 
				{
					if (cp.location[j] > max)
						max = cp.location[j];

					if (j==0 && cp.location[j] < negaXYZ.x) 		negaXYZ.x = cp.location[j];
					else if (j==1 && cp.location[j] < negaXYZ.y) 	negaXYZ.y = cp.location[j];
					else if (j==2 && cp.location[j] < negaXYZ.z) 	negaXYZ.z = cp.location[j];
				}
			}
		}
	}

}
