using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extensions{

    /// <summary>get bounds from triangle vector positions</summary>
    /// <returns>new Rect(minx, miny, maxx, maxy)</returns>
    public static Rect getTriangleBounds(this Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float minx = Mathf.Min(Mathf.Min(v1.x, v2.x), v3.x);
        float maxx = Mathf.Max(Mathf.Max(v1.x, v2.x), v3.x);

        float miny = Mathf.Min(Mathf.Min(v1.y, v2.y), v3.y);
        float maxy = Mathf.Max(Mathf.Max(v1.y, v2.y), v3.y);

        return new Rect(minx, miny, maxx - minx, maxy - miny);
    }
}
