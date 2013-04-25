using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class CloudPoint
{
    public Color32 color { get; private set; }
    public Vector location { get; private set; }
    public Vector normal { get; private set; }

    public CloudPoint(Vector location, Color32 color, Vector normal)
    {
        this.location = location;
        this.color = color;
        this.normal = normal;
    }

    // clone constructor so that we can do the iterative stuff without messing up the underlying cloud
    public CloudPoint(CloudPoint o)
    {
        this.color = o.color;
        this.location = o.location;
        this.normal = o.normal;
    }

    public CloudPoint ApplyTransform(Matrix R, Vector T)
    {
        // I can guarantee that this will work correctly so we index blindly
        var newLocation = (R * location.ToColumnMatrix() + T.ToColumnMatrix()).GetColumnVector(0);

        return new CloudPoint(newLocation, this.color, this.normal);
    }

    public CloudPoint UnApplyTransform(Matrix R, Vector T)
    {
        // R is trivially invertible because it is a rotation matrix
        var Rt = new Matrix(R);
        Rt.Transpose(); // yo this language is pass by ref so be careful
        var newLocation = (R * (location - T).ToColumnMatrix()).GetColumnVector(0);

        return new CloudPoint(newLocation, this.color, this.normal); 
    }

    internal void CalculateNormal(C5.IPriorityQueue<CloudPoint> neighbors)
    {
        Vector avg = neighbors.Aggregate(new Vector(3), (sum, val) => sum + val.location) / (double)neighbors.Count;

        Matrix cov = ((double)1 / neighbors.Count) *
                            neighbors.Aggregate(new Matrix(3, 3),
                                                        (sum, val) => sum +
                                                                    (val.location - avg).ToColumnMatrix() *
                                                                    (val.location - avg).ToRowMatrix());

        // that dude says the smallest eigenvalue is the normal
		// first column of eigenvectors is the smallest eigenvalued one
        normal = cov.EigenVectors.GetColumnVector(0);
		
		// orient normal toward viewpoint
		
		// to do this we have to push the origin into the world frame
		 //v = ZigInput.ConvertImageToWorldSpace(v);
		var viewp = ZigInput.ConvertImageToWorldSpace(Vector3.zero);
		
		normal = ((normal * (viewp.ToVector() - this.location)) > 0 ? normal : -normal);
		normal.Normalize();
    }
}

public class PointMatch : IComparable
{
    public CloudPoint A { get; private set; }
    public CloudPoint B { get; private set; }
    public double Distance {get; private set;}

    public PointMatch(CloudPoint A, CloudPoint B)
    {
        this.A = A;
        this.B = B;

        var D = A.ColorLocation() - B.ColorLocation();

        Distance = D.Norm();
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        PointMatch otherMatch = obj as PointMatch;

        if (otherMatch != null)
        {
            return this.Distance.CompareTo(otherMatch.Distance);
        }
        else
        {
            throw new ArgumentException("obj is not a PointMatch");
        }
    }
}