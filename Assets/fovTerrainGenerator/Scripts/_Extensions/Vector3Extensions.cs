using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions  {

    public static Vector3 Rotate(this Vector3 v, Vector3 rotation)
    {
        return Quaternion.Euler(rotation) * v;
    }
}
