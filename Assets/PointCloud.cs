using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KdTree;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;


public class PointCloud
{    
    public short[] rawDepth { get; private set; }
    public int depthX { get; private set; }
    public int depthY { get; private set; }
    public Color32[] colorizedDepth { get; private set; } // also has feature points marked in red

    public Color32[] rawColor { get; private set; }
    public int colorX { get; private set; }
    public int colorY { get; private set; }
    
    public List<CloudPoint> PointList { get; private set; }

    public KdTree<CloudPoint> FeatureTree { get; private set; }

    public Matrix<int> FeatureIndices;

    public float[] depthHistogramMap;
    public const int MaxDepth = 10000; // this is something that the sensor knows too, don't mess!
    Color32[] depthToColor;
    public Color32 BaseColor = Color.yellow;

    public Matrix R { get; private set; }
    public Vector T { get; private set; }
    
    public PointCloud(short[] depth, int dWidth, int dHeight,
                      Color32[] color, int cWidth, int cHeight)
    {
        PointList = new List<CloudPoint>();

        // these are different sizes. use the depth image as the native index
        // we want to get as much depth data as possible
        this.rawColor = color;
        this.rawDepth = depth;

        this.depthX = dWidth;
        this.depthY = dHeight;
        this.colorX = cWidth;
        this.colorY = cHeight;

        // express scaling factors to take x,y positions in depth to color
        // also we are assuming that the color image is larger than the depth one
        int factorX = cWidth / dWidth;
        int factorY = cHeight / dHeight;

        for (int i = 0; i < depth.Length; i++)
        {
            // the transform that the Zig thing provides expects coordinates in the image plane though
            if ((depth[i] == 0) || (depth[i] == -1))
                continue; // this is a garbage point

            int x = (i % dWidth) * factorX;
            int y = (i / dWidth) * factorY;
            int z = depth[i];

            Vector3 v = new Vector3(x, y, z);
            v = ZigInput.ConvertImageToWorldSpace(v);

            int cIndex = x + y * cWidth;
            
            PointList.Add(new CloudPoint(v.ToVector(), color[cIndex], Vector.Zeros(3)));
        }

        depthHistogramMap = new float[MaxDepth];
        depthToColor = new Color32[MaxDepth];
        colorizedDepth = new Color32[depthX * depthY];
		
        
		UpdateHistogram();

        this.R = Matrix.Create(new double[,]{{1, 0, 0},
                                            {0, 1, 0},
                                            {0, 0, 1}}); // identity rotation

        this.T = Vector.Zeros(3); // zero translation

       

       

        //Transform = new Matrix(new double[,]{{1, 0, 0, 0},
        //                                     {0, 1, 0, 0},
        //                                     {0, 0, 1, 0},
        //                                     {0, 0, 0, 1}}); // homogeneous identity


    }

    public double PushOntoCloud(PointCloud other, int iterations, int matchLength, double threshold)
    {
        // we are going to update our transform to place us on the other cloud with the ICP

        double error = 0;

        #region Correspondence
        // make the list of matches
        // NOTE this cannot be a SortedList because that will complain if you give multiple pairs with the same dist
        List<PointMatch> matches = new List<PointMatch>();

        // go over our list of features
        foreach (CloudPoint p in this.FeatureTree)
        {
            // find nearest in their list of features after our transform 
            var pT = p.ApplyTransform(R, T);

            CloudPoint o = other.FeatureTree.FindNearestNeighbor(pT.ColorLocation());

            PointMatch match = new PointMatch(pT, o);

            matches.Add(match);
        }

        // select the top ni matches based on how close they are
        var topMatches = matches.OrderBy(x => x.Distance).Take(matchLength);
        #endregion

        #region find normals for matchpoints

        var ourQueryPoints = topMatches.Select(x => x.A.UnApplyTransform(R, T));
        this.CalculateNormals(ourQueryPoints);

        var theirQueryPoints = topMatches.Select(x => x.B);
        other.CalculateNormals(theirQueryPoints);

        #endregion

        // iterative part
        for (int i = 0; i < iterations; i++)
        {
            
            #region refine ICP estimate
            Matrix cov = new Matrix(6, 6);
            Vector b = new Vector(6);

            var r = Vector.Create(new double[] { R[2, 1], R[0, 2], R[1, 0] });
            foreach (PointMatch pm in topMatches)
            {
                // moving A onto B
                var A = pm.A;
                var B = pm.B;

                Vector ni = B.normal;
                Vector ci = Vector.CrossProduct(A.location, ni);

                Vector CN = Vector.Create(ci.Concat(ni).ToArray());

                cov = cov + (CN.ToColumnMatrix() * CN.ToRowMatrix());

                var diffDot = (A.location - B.location) * ni; // this is a dot product

                b = b - (diffDot * Vector.Ones(6)).ArrayMultiply(CN);

                // also accumulate error for this RT since we're here already
                
                error = error + Math.Pow((diffDot + (T * ni) + (r * ci)), 2);
            }
            // done accumulating cov and B

            // we're done here if our thing put us close enough
            if (error < threshold) break;

            // solve cov * inc_transform = b
            // compute decomp
            var chol = cov.CholeskyDecomposition;

            // solve LL' * inc_transform = b
            var L = chol.TriangularFactor;

            Vector inc_transform = LLTSolve(L, b);

            R = Matrix.Create(new double[,]{{1,     -inc_transform[2], inc_transform[1]},
                                            {inc_transform[2],  1,     -inc_transform[0]},
                                            {-inc_transform[1], inc_transform[0],  1}});

            // last three components are translation
            T = Vector.Create(inc_transform.Skip(3).ToArray());

            #endregion 

        }

        // apply transform to whole cloud
        ApplyTransform();

        return error;
    }

    // this is from https://ece.uwaterloo.ca/~ece204/howtos/forward/ hehehh
    private Vector LLTSolve(Matrix L, Vector b)
    {
        int i = 0;
        int n = b.Count();
        // forward sub to solve Lz = b
        Vector z = Vector.Zeros(n);

        for (i = 0; i < n; i++)
        {
            z[i] = (b[i] - (L.GetRowVector(i) * z)) / L[i, i];
        }

        // back sub to solve L'x = z
        Vector x = Vector.Zeros(n);
        var Lt = Matrix.Transpose(L);

        for (i = n-1; i >= 0; i--)
        {
            x[i] = (z[i] - (Lt.GetRowVector(i) * x)) / Lt[i, i];
        }

        return x;
    }

    private void ApplyTransform()
    {
        PointList = new List<CloudPoint>(PointList.Select(x => x.ApplyTransform(R, T)));
    }

    public void DetectFeatures()
    {
        //const int numFeatures = 15;
        //const double quality = 5000;
        //const double minDistance = 50;
        //const int blockSize = 11;
        
        // note, the histogram is a property of the cloud itself and is updated when the cloud is created
        // load depth image into something that emgucv likes
        Image<Gray, Byte> depthImage = new Image<Gray, byte>(depthX, depthY);

        // have to convert this to gray via luminosity
        byte[] bytes = colorizedDepth.Select(x => (byte)(0.21*x.r + 0.71*x.g + 0.07*x.b)).ToArray();

        depthImage.Bytes = bytes;

        // detect features of depth image using the fast detector
        // I don't really feel like implementing a Harris detector

        FastDetector fast = new FastDetector(10, true);

        var keyPoints = fast.DetectKeyPoints(depthImage, null); // no mask because I don't know what that is

        List<CloudPoint> cloudFeatures = new List<CloudPoint>();

        foreach (var p in keyPoints)
        {
            cloudFeatures.Add(findCloudPoint((int)p.Point.X, (int)p.Point.Y));
        }

        FeatureTree = KdTree<CloudPoint>.Construct(4, cloudFeatures, x => x.ColorLocation());
    }

    // this reconstructs the cloudpoint from its location in the depth image, unwinding the constructor
    private CloudPoint findCloudPoint(int xD, int yD)
    {
        // find depth, native indexing
        int dIndex = yD * depthX + xD;
        int z = rawDepth[dIndex];

        // note that the location in the CloudPoint corresponds to the location in the image plane

        // find color, scale indexing
        int factorX = colorX / depthX;
        int factorY = colorY / depthY;

        int cIndex = xD * factorX + yD * factorY * colorX;

        int y = yD * factorY;
        int x = xD * factorX;

        Color32 color = rawColor[cIndex];

        return new CloudPoint(new Vector(new double[] { x, y, z }), color, Vector.Zeros(3));
    }

    // adapted from ZigDepthViewer. this cleans up the depth image so the corner detector can get it easier.
    private void UpdateHistogram()
    {
        int i, numOfPoints = 0;

        System.Array.Clear(depthHistogramMap, 0, depthHistogramMap.Length);

        int depthIndex = 0;

        // the textureSize below is the size of whatever thing you are trying to draw on
        // since this is for our own use this can be the same size

        // assume only downscaling
        // calculate the amount of source pixels to move per column and row in
        // output pixels
        //int factorX = depth.xres / textureSize.Width;
        //int factorY = ((depth.yres / textureSize.Height) - 1) * depth.xres;

        int factorX = 1;
        int factorY = 0; // this is weird, possibly supposed to be 1
        for (int y = 0; y < depthY; ++y, depthIndex += factorY)
        {
            for (int x = 0; x < depthX; ++x, depthIndex += factorX)
            {
                short pixel = rawDepth[depthIndex];
                if (pixel != 0)
                {
                    depthHistogramMap[pixel]++;
                    numOfPoints++;
                }
            }
        }
        depthHistogramMap[0] = 0;
        if (numOfPoints > 0)
        {
            for (i = 1; i < depthHistogramMap.Length; i++)
            {
                depthHistogramMap[i] += depthHistogramMap[i - 1];
            }
            depthToColor[0] = Color.black;
            for (i = 1; i < depthHistogramMap.Length; i++)
            {
                float intensity = (1.0f - (depthHistogramMap[i] / numOfPoints));
                //depthHistogramMap[i] = intensity * 255;
                depthToColor[i].r = (byte)(BaseColor.r * intensity);
                depthToColor[i].g = (byte)(BaseColor.g * intensity);
                depthToColor[i].b = (byte)(BaseColor.b * intensity);
                depthToColor[i].a = 255;//(byte)(BaseColor.a * intensity);
            }
        }
		depthIndex = 0;
        // now remap the depth into colors (from depthviewer)
        for (int y = depthY - 1; y >= 0; --y, depthIndex += factorY)
        {
            int outputIndex = y * depthX;
            for (int x = 0; x < depthX; ++x, depthIndex += factorX, ++outputIndex)
            {
                colorizedDepth[outputIndex] = depthToColor[rawDepth[depthIndex]];
            }
        }
    }

    // adds the whole cloud to a tree, then calculate normals on the querypoints
    private void CalculateNormals(IEnumerable<CloudPoint> cloudPoints)
    {
        // somewhat slow, but faster than a brute-force search
        KdTree<CloudPoint> tree = KdTree<CloudPoint>.Construct(3, PointList, x => x.location);

        foreach (CloudPoint cp in cloudPoints)
        {
            // find nearest 4 neighbors and do the thing here: http://pointclouds.org/documentation/tutorials/normal_estimation.php
            C5.IPriorityQueue<CloudPoint> neighbors = tree.FindNearestNNeighbors(cp.location, 4);

            cp.CalculateNormal(neighbors);
        }
    }
}