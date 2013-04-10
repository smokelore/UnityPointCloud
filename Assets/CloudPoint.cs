using System;
using System.Collections.Generic;
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
}

