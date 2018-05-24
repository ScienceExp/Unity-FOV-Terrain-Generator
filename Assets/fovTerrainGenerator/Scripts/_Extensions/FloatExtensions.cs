using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloatExtensions
{

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static int Snap2Grid(this float v, int gridSpacing)
    {
        return (int)Mathf.Round(v / gridSpacing) * gridSpacing;
    }
}

