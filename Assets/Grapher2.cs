using UnityEngine;
using System.Collections;

public class Grapher2 : MonoBehaviour {

	public int resolution = 10;
	private int currentResolution;
	private ParticleSystem.Particle[] points;

	void Start () {
		CreatePoints();
	}
	
	private void CreatePoints () {
		if(resolution < 2){
			resolution = 2;
		}
		currentResolution = resolution;
		points = new ParticleSystem.Particle[resolution * resolution];
		float increment = 1f / (resolution - 1);
		int i = 0;
		for(int x = 0; x < resolution; x++){
			for(int z = 0; z < resolution; z++){
				Vector3 p = new Vector3(x * increment, 0f, z * increment);
				points[i].position = p;
				points[i].color = new Color(p.x, 0f, p.z);
				points[i++].size = 0.1f;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(currentResolution != resolution){
			CreatePoints();
		}
		
		for(int i = 0; i < resolution; i++){
			Vector3 p = points[i].position;
			p.y = p.x;
			points[i].position = p;
			// fade from black to yellow depending on y position
			Color c = points[i].color;
			c.g = p.y;
			points[i].color = c;
		}
		
		particleSystem.SetParticles(points, points.Length);
	}
}
