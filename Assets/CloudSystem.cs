using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudSystem
{
	private ParticleSystem.Particle[] particleCloud;
	private ParticleSystem particleSystem;
	// The maximum x-value, y-value, and z-value among the points. This is used for coloring.
	private float scaleFactor = 1.0f;
	private Vector3 negaXYZ = Vector3.zero;
	public float particleSize = 0.004f;

	public CloudSystem(ParticleSystem ps, PointCloud pc, float scaleFactor, Vector3 negaXYZ) 
	{
		this.particleSystem = ps;
		this.scaleFactor = scaleFactor;
		this.transFactor = transFactor;
		this.particleCloud = new ParticleSystem.Particle[pc.PointList.Length];
		SetPoints(pc);
	}

	// Set particle positions according to point coordinates using single PointCloud parameter.
	public void SetPoints(PointCloud pc)
	{
		for (int i = 0; i < pc.PointList.Length; ++i)
		{
			// Set position of particles to match those of the 3-D points.
			Vector3 newPos = new Vector3(pc.PointList[i].location[0], 
								pc.PointList[i].location[1], 
								pc.PointList[i].location[2]);
			newPos -= transFactor;
			newPos /= scaleFactor;
			particleCloud[i].position = newPos; 
			// Color points according to Color[] array.
			particleCloud[i].color = pc.PointList[i].color;
			// Static size.
			particleCloud[i].size = particleSize;
			//print("{ x: " + cloud[i].position.x + " // y: " + cloud[i].position.y + " // z: " + cloud[i].position.z + " }");
		}

		updatePoints();
	}

	// Replace single particle in particleCloud with single cloud point at the inputted index within the inputted PointCloud
	public void SetPoint(PointCloud pc, int index) 
	{
		// Set position of particles to match those of the 3-D points.
		Vector3 newPos = new Vector3(pc.PointList[index].location[0], 
							pc.PointList[index].location[1], 
							pc.PointList[index].location[2]);
		newPos -= negaXYZ;
		newPos /= scaleFactor;
		particleCloud[index].position = newPos; 
		// Color points according to Color[] array.
		particleCloud[index].color = pc.PointList[index].color;
		// Static size.
		particleCloud[index].size = particleSize;

		updatePoints();
	}

	public void updatePoints() 
	{
		// Redraw the points.
		particleSystem.SetParticles(particleCloud, particleCloud.Length);
	}
}