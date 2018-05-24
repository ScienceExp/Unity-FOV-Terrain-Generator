using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IntExtensions  {

    public static int Snap2Grid(this int v, int gridSpacing)
    {
        return (int)Mathf.Round(v / gridSpacing) * gridSpacing;
    }
}
