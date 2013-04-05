using UnityEngine;
using System.Collections;

public class Grapher1 : MonoBehaviour {

	public int resolution = 10;
	private int currentResolution;
	private ParticleSystem.Particle[] points;

	void Start () {
		CreatePoints();
	}
	
	private void CreatePoints () {
		if(resolution < 2){
			resolution = 2;
		} else if(resolution > 100){
			resolution = 100;
		}
		currentResolution = resolution;
		points = new ParticleSystem.Particle[resolution];
		float increment = 1f / (resolution - 1);
		for(int i = 0; i < resolution; i++){
			float x = i * increment;
			points[i].position = new Vector3(x, 0f, 0f);
			points[i].color = new Color(x, 0f, 0f);
			points[i].size = 0.1f;
		}
	}
	
	
	void Update () {
		if(currentResolution != resolution){
			CreatePoints();
		}
		
		for(int i = 0; i < resolution; i++){
			// assign point positions
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
