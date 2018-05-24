//Reference: http://www.sunshine2k.de/coding/java/TriangleRasterization/TriangleRasterization.html
using System.Collections.Generic;
using UnityEngine;

/// <summary>Loads all possible terrains into a coordinate list in TerrainPositions</summary>
public class TerrainsInTriangle
{
    #region public decelerations
    /// <summary>List of possible terrain positions in a grid based on terrain size.</summary>
    public List<Vector2> TerrainPositions;
    #endregion
    #region private decelerations
    /// <summary>Camera that will be used for calculations</summary>
    Camera cam;
    /// <summary>Triangle coordinate 1</summary>
    Vector2 v1;
    /// <summary>Triangle coordinate 2</summary>
    Vector2 v2;
    /// <summary>Triangle coordinate 3</summary>
    Vector2 v3;
    /// <summary>Square terrain size in world units. (meters)</summary>
    int terrainSize;
    /// <summary>angle added to camera FOV. TODO.. Find out how to calculate this better so no user value is needed.</summary>
    int angleAdd2FOV;
    /// <summary>value used to add terrains outside of triangle to pad movement and rotation. (TerrainPadding * 2)</summary>
    int terrainPadding;
    #endregion
    #region constructor
    /// <summary>Loads all possible terrains into a coordinate list in TerrainPositions</summary>
    /// <param name="TerrainSize">Square terrain size in world units. (meters)</param>
    /// <param name="Cam">Camera that will be used for calculations</param>
    /// <param name="TerrainPadding">Number of terrains around triangle to add for padding</param>
    /// <param name="AddAngleToFOV">Angle added to camera FOV. TODO (figure out how to calculate)</param>
    public TerrainsInTriangle(int TerrainSize, Camera Cam, int TerrainPadding = 2, int AddAngleToFOV = 10)
    {
        terrainSize = TerrainSize;
        cam = Cam;
        angleAdd2FOV = AddAngleToFOV;
        terrainPadding = TerrainPadding * 2;
    }
    #endregion
    #region debug
    public void ShowDebug()
    {
        Vector3 vv1 = new Vector3(v1.x, 0, v1.y);
        Vector3 vv2 = new Vector3(v2.x, 0, v2.y);
        Vector3 vv3 = new Vector3(v3.x, 0, v3.y);
        Debug.DrawLine(vv1, vv2, Color.blue);
        Debug.DrawLine(vv1, vv3, Color.blue);
        Debug.DrawLine(vv2, vv3, Color.blue);
    }
    #endregion
    #region private functions
    /// <summary>Set v1, v2, v3 to scaled fov triangle coordinates</summary>
    void calculateVectors()
    {
        #region calculate fov triangle world points
        float halfFOV = cam.fieldOfView * 0.5f + angleAdd2FOV;                  //FOV does not match calculated fov so angle is added. TODO: figure out how to calculate this.

        float adj = cam.farClipPlane + terrainSize;                             //triangle side straight out from camera
        float hyp = adj / Mathf.Cos(Mathf.Deg2Rad * halfFOV);                   //hypotenuse of right or left fov
        float opp = hyp * Mathf.Sin(Mathf.Deg2Rad * halfFOV);                   //span across 1/2 back distance.

        float angleL = cam.transform.rotation.eulerAngles.y - halfFOV;          //angle in degrees to project vL 
        Vector3 vL = Vector3.forward.Rotate(new Vector3(0f, angleL, 0f)) * hyp; //Rotate world forward vector by angle and project out by "hyp" distance
        float angleR = cam.transform.rotation.eulerAngles.y + halfFOV;          //angle in degrees to project vR
        Vector3 vR = Vector3.forward.Rotate(new Vector3(0f, angleR, 0f)) * hyp; //Rotate world forward vector by angle and project out by "hyp" distance

        v1 = new Vector2(cam.transform.position.x, cam.transform.position.z);               //v1 = cam position
        v2 = new Vector2(cam.transform.position.x + vL.x, cam.transform.position.z + vL.z); //v2 = vL + cam position
        v3 = new Vector2(cam.transform.position.x + vR.x, cam.transform.position.z + vR.z); //v3 = vR + cam position
        #endregion
        #region scale triangle based in terrainPadding.
        float terrainsWideOrig = adj / terrainSize;
        float terrainsWide = adj / terrainSize + terrainPadding;
        float scale = terrainsWide/terrainsWideOrig;
        float centerx = (v1.x + v2.x + v3.x) / 3f;
        float centery = (v1.y + v2.y + v3.y) / 3f;

        v1 = new Vector2(centerx + (v1.x - centerx) * scale, centery + (v1.y - centery) * scale);
        v2 = new Vector2(centerx + (v2.x - centerx) * scale, centery + (v2.y - centery) * scale);
        v3 = new Vector2(centerx + (v3.x - centerx) * scale, centery + (v3.y - centery) * scale);
        #endregion
    }
    /// <summary>Sort v1.y > v2.y > v3.y</summary>
    void sortVerticesAscendingByY()
    {
        Vector2 tmp;
        //after v1.y > v2.y
        if (v2.y > v1.y)
        {
            tmp = v1;
            v1 = v2;
            v2 = tmp;
        }

        //after v1.y > v3.y && v1.y > v2.y
        if (v3.y > v1.y)
        {
            tmp = v1;
            v1 = v3;
            v3 = tmp;
        }
        //after v1.y  > v2.y > v3.y 
        if (v3.y > v2.y)
        {
            tmp = v2;
            v2 = v3;
            v3 = tmp;
        }
    }
    /// <summary>fills a bottom-flat triangle line by line</summary>
    void fillBottomFlatTriangle(Vector2 V1, Vector2 V2, Vector2 V3)
    {
        #region reorder V2.x < V3.x
        if (V2.x > V3.x)
        {
            Vector2 tmp;
            tmp = V2;
            V2 = V3;
            V3 = tmp;
        }
        #endregion
        //v1.y  > v2.y == v3.y && v2.x < v3.x
        #region initialize variables
        float invslope1 = (V2.x - V1.x) / (V1.y - V2.y);    //slope of left edge
        float invslope2 = (V3.x - V1.x) / (V1.y - V3.y);    //slope of right edge

        float curx1 = V1.x;                                 //set to top point vertex
        float curx2 = V1.x;                                 //set to top point vertex

        int sy = V1.y.Snap2Grid(terrainSize);               //start y position snapped to grid
        int ey = V2.y.Snap2Grid(terrainSize);               //  end y position snapped to grid

        Rect bounds = v1.getTriangleBounds(v2, v3);         //TODO: figure out a way to not have to check bounds
        #endregion

        for (int y = sy; y >= ey; y -= terrainSize)
        {
            int sx = curx1.Snap2Grid(terrainSize);          //start x position snapped to grid
            int ex = curx2.Snap2Grid(terrainSize);          //  end x position snapped to grid

            for (int x = sx; x <= ex; x += terrainSize)
            {
                Vector2 n = new Vector2(x, y);
                if (bounds.Contains(n))                     //need this because of very narrow slopes. Todo: figure out how to remove
                    TerrainPositions.Add(n);                //add terrain position
            }
            curx1 += invslope1 * terrainSize;               //move down the left edge
            curx2 += invslope2 * terrainSize;               //move down the right edge
        }
    }
    /// <summary>fills a top-flat triangle line by line</summary>
    void fillTopFlatTriangle(Vector2 V1, Vector2 V2, Vector2 V3)
    {
        #region reorder V1.x < V2.x
        if (V1.x > V2.x)
        {
            Vector2 tmp;
            tmp = V1;
            V1 = V2;
            V2 = tmp;
        }
        #endregion
        //v1.y == v2.y > v3.y && v1.x < v2.x
        #region initialize variables
        float invslope1 = (V3.x - V1.x) / (V3.y - V1.y);    //slope of left edge
        float invslope2 = (V3.x - V2.x) / (V3.y - V2.y);    //slope of right edge

        float curx1 = V3.x;                                 //set to bottom point vertex
        float curx2 = V3.x;                                 //set to bottom point vertex

        int sy = V3.y.Snap2Grid(terrainSize);               //start y position snapped to grid
        int ey = V1.y.Snap2Grid(terrainSize);               //end   y position snapped to grid

        Rect bounds = v1.getTriangleBounds(v2, v3);         //TODO: figure out a way to not have to check bounds
        #endregion

        for (int y = sy; y <= ey; y += terrainSize)
        {
            int sx = curx1.Snap2Grid(terrainSize);          //start x position snapped to grid
            int ex = curx2.Snap2Grid(terrainSize);          //  end x position snapped to grid

            for (int x = sx; x <= ex; x += terrainSize)
            {
                Vector2 n = new Vector2(x, y);
                if (bounds.Contains(n))                     //need this because of very narrow slopes. Todo: figure out how to remove
                    TerrainPositions.Add(n);                //add terrain position
            }

            curx1 += invslope1 * terrainSize;               //move up the left edge
            curx2 += invslope2 * terrainSize;               //move up the right edge
        }
    }
    #endregion

    public void UpdateTerrainList()
    {
        TerrainPositions = new List<Vector2>(); //reinitialize (TODO is there a faster way to add remove?)
        calculateVectors();                     //set v1, v2, v3
        sortVerticesAscendingByY();             //v1.y > v2.y > v3.y

        if (v2.y == v3.y)                       //check for trivial case of bottom-flat triangle
            fillBottomFlatTriangle(v1, v2, v3);

        else if (v1.y == v2.y)                  //check for trivial case of top-flat triangle
            fillTopFlatTriangle(v1, v2, v3);

        else                                    //general case - split the triangle in a top-flat and bottom-flat one
        {
            Vector2 v4 = new Vector2(v1.x + ((v2.y - v1.y) / (v3.y - v1.y)) * (v3.x - v1.x), v2.y);
            fillBottomFlatTriangle(v1, v2, v4);
            fillTopFlatTriangle(v2, v4, v3);
        }
    }
}
