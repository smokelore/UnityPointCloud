using UnityEngine;

public class Grapher3 : MonoBehaviour {

	public bool absolute;
	public float threshold = 0.5f;
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
		else if(resolution > 30){
			resolution = 30;
		}
		currentResolution = resolution;
		points = new ParticleSystem.Particle[resolution * resolution * resolution];
		float increment = 1f / (resolution - 1);
		int i = 0;
		for(int x = 0; x < resolution; x++){
			for(int z = 0; z < resolution; z++){
				for(int y = 0; y < resolution; y++){
					Vector3 p = new Vector3(x, y, z) * increment;
					points[i].position = p;
					points[i].color = new Color(p.x, p.y, p.z);
					points[i++].size = 0.1f;
				}
			}
		}
	}

	void Update () {
		if(currentResolution != resolution){
			CreatePoints();
		}
		
		if(absolute){
			for(int i = 0; i < points.Length; i++){
				Color c = points[i].color;
				c.a = 1.0f;
				//c.a = points[i].position >= threshold ? 1f : 0f;
				points[i].color = c;
			}
		} else {
			for(int i = 0; i < points.Length; i++){
				Color c = points[i].color;
				c.a = 1.0f;
				points[i].color = c;
			}
		}
		particleSystem.SetParticles(points, points.Length);
	}
}