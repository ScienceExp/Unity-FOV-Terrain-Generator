using UnityEngine;

public static class BoundsExtensions
{
    public static bool IsVisibleFrom(this Bounds bounds, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    public static bool IsVisibleFrom(this Bounds bounds, Plane[] planes)
    {
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }
}
