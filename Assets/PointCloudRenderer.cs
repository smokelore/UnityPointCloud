using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;

public class PointCloudRenderer : MonoBehaviour
{
  private Camera cam;
  private ParticleSystem.Particle[] cloud;
  // Enable this in realtime if you want to re-render the points and camera background.
  public bool updatePoints = false;
  // The 2-D array of all points to plot.
  private float[,] p;
  // The 1-D array of the color of all points to plot.
  private Color[] c;
  // The maximum x-value, y-value, and z-value among the points. This is used for coloring.
  public float max = 1.0f;
  public Color backgroundRGBA = Color.white/4;
  public float particleSize = 0.004f;


  void Start ()
  {
    // Set the control camera to be the Main Camera.
    cam = Camera.mainCamera.camera;

    // Get new CloudPoint data.
    CloudPoint[] cloudP = new CloudPoint[4];
    cloudP[0] = new CloudPoint(new Vector(new double[] {-562.1205,562.1205,-2230}), Color.red, new Vector(new double[] {0,0,0}));
    cloudP[1] = new CloudPoint(new Vector(new double[] {-549.9371,565.6496,-2244}), Color.red, new Vector(new double[] {0,0,0}));
    cloudP[2] = new CloudPoint(new Vector(new double[] {-534.2246,565.6496,-2244}), Color.red, new Vector(new double[] {0,0,0}));
    cloudP[3] = new CloudPoint(new Vector(new double[] {-518.5121,565.6496,-2244}), Color.red, new Vector(new double[] {0,0,0}));
    
    // Convert the CloudPoint data and fill the p locations matrix and c colors matrix.
    getCloudPoints(cloudP);
    // Fix all negative points and force them into the unit cube.
    normalizePoints();
    // Reset Camera to focus at center of unit cube with original viewing angle.
    //resetCamera();
    // Initial point cloud rendering.
    updatePoints = true;
  }

  void resetCamera() {
    // Update points' Max.
    max = Max(p);
    // Reset Camera distance according to coordinate range;
    cam.transform.position = new Vector3(0, max/2, 0);
    cam.transform.LookAt(Vector3.one * max/2);
    cam.transform.position -= cam.transform.forward*2*max;
  }

  void Update () 
  {
    if (updatePoints)
    {
      // RETRIEVE UPDATED POINTS HERE IF YOU WANT TO

      // Fix all negative points and force them into the unit cube.
      normalizePoints();
      // Create new particles and set at new positions.
      SetPoints(p,c);
      // Redraw the points.
      particleSystem.SetParticles(cloud, cloud.Length);
      
      // Don't redraw the points until SetPoints() is called again.
      updatePoints = false;
    }
  }

  // Set particle positions according to point coordinates using one argument. Color points according to position.
  public void SetPoints(float[,] points)
  {
    Color[] rgba = new Color[points.GetLength(0)];
    
    // Find the max value of any coordinate and use it for normalizing the automatic coloring of the points.
    max = Max(p);
    for (int i = 0; i < rgba.Length; i++)
    {
      // Color points according to xyz location.
      rgba[i] = new Color(points[i,0], points[i,1], points[i,2]);
      // Normalize with max coordinate value.
      //rgba[i] /= max;
      // Retain complete opacity.
      rgba[i].a = 255;
    }
    SetPoints(points,rgba);
  } 

  // Set particle positions according to point coordinates using two arguments. Color points according to inputted Color[] array.
  public void SetPoints(float[,] points, Color[] rgba)
  {
    cloud = new ParticleSystem.Particle[points.GetLength(0)];

    // Find the max value of any coordinate and use it for normalizing the automatic coloring of the points.
    max = Max(p);

    for (int i = 0; i < points.GetLength(0); ++i)
    {
      // Set position of particles to match those of the 3-D points.
      cloud[i].position = new Vector3(points[i,0], points[i,1], points[i,2]);       
      // Color points according to Color[] array.
      cloud[i].color = rgba[i];
      // Static size.
      cloud[i].size = particleSize;
      //print("{ x: " + cloud[i].position.x + " // y: " + cloud[i].position.y + " // z: " + cloud[i].position.z + " }");
    }
    
    // Every time the points are set, redraw them.
    updatePoints = true;
  }

  private float Max (float[,] points) {
    float max = points[0,0];

    // Search through points[] array and return the max recorded value within the array.
    for (int i = 1; i < points.GetLength(0); i++)
      for (int j = 0; j < points.GetLength(1); j++) 
        if (points[i,j] > max)
          max = points[i,j];

    return max;
  }

  private void correctNegativePoints() {
    // Find the most negative value for each coordinate and subtract it from every one of those resepective coordinates.
    // For example, if -300 was the most negative x-coordinate in p[], this would subtract -300 from every x-coordinate in p[].
    Vector3 negaXYZ = Vector3.zero;
    for (int i = 0; i < p.GetLength(0); i++) {
      if (p[i,0] < negaXYZ.x)   negaXYZ.x = p[i,0];
      if (p[i,1] < negaXYZ.y)   negaXYZ.y = p[i,1];
      if (p[i,2] < negaXYZ.z)   negaXYZ.z = p[i,2];
    }

    for (int i = 0; i < p.GetLength(0); i++) {
      p[i,0] -= negaXYZ.x;
      p[i,1] -= negaXYZ.y;
      p[i,2] -= negaXYZ.z;
    }
  }

  private void normalizePoints() {
    correctNegativePoints();
    max = Max(p);

    // Force every point to be in the unit cube.
    for (int i = 0; i < p.GetLength(0); i++) {
      p[i,0] /= max;
      p[i,1] /= max;
      p[i,2] /= max;
    }

  }

  public void getCloudPoints(CloudPoint[] cloudPoints) {
    p = new float[cloudPoints.Length,3];
    c = new Color[cloudPoints.Length];
    // Convert CloudPoint array to 2-D location array and 1-D color array.
    for (int i = 0; i < cloudPoints.Length; i++) {
      p[i,0] = (float) cloudPoints[i].location[0];
      p[i,1] = (float) cloudPoints[i].location[1];
      p[i,2] = (float) cloudPoints[i].location[2];
      c[i] = cloudPoints[i].color;
    }

    updatePoints = true;

  }
}