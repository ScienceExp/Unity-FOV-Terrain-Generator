using System;
using System.Collections;
using UnityEngine;

public class TerrainStreamer : MonoBehaviour
{
    #region inspector
    [Header("Generation Settings")]
    [Tooltip("Camera that will be used for calculations.")]
    public Camera cam;
    [Tooltip("Angle that is added to the FOV to expand it. (Todo: Figure out how to calculate)")]
    public int addAngleToFOV = 10;
    [Tooltip ("Terrain Object")]
    public GameObject terrain;
    [Tooltip("Number of terrains to use as a buffer around fov")]
    public int terrainPadding = 2;
    [Tooltip("Maximum rotation in degrees allowed before recalculating terrains")]
    public int maxRotation = 5;
    [Tooltip("Maximum distance allowed in world units(meters) before recalculating terrains")]
    public float maxMoveDistance = 1f;
    [Tooltip("Use if pivot point is in the center")]
    public bool centerObject = false;
    public bool showDebug = false;
    #endregion
    #region private decelerations
    /// <summary>Parent object to keep things neat</summary>
    GameObject parent;
    /// <summary>Hashtable to hold loaded terrain tiles</summary>
    Hashtable tiles = new Hashtable();
    /// <summary>Used to find all the terrains in the current FOV</summary>
    TerrainsInTriangle triTerrains;
    /// <summary>Used to check how far player moved</summary>
    Vector3 startPos;
    /// <summary>Used to check how far player rotated</summary>
    float cameraRotationY = 0f;
    /// <summary>Used to reposition terrain if pivot is in the center</summary>
    float halfMeshSize = 0f;
    #endregion
    #region Awake + Start
    private void Awake()
    {
        Vector3 meshSize;
        #region get terrain size
        if (terrain.GetComponent<MeshFilter>() != null)                     //if it has a meshFilter it must be a mesh...
        {
            Mesh mesh = terrain.GetComponent<MeshFilter>().mesh;
            Bounds bounds = mesh.bounds;
            meshSize.x = bounds.size.x * terrain.transform.localScale.x;
            meshSize.y = bounds.size.y * terrain.transform.localScale.y;
            meshSize.z = bounds.size.z * terrain.transform.localScale.z;
        }
        else                                                                //if it has no meshFilter it probably is a terrain
        {
            Terrain t = terrain.GetComponent<Terrain>();
            meshSize = t.terrainData.size;
        }
        #endregion
        #region make sure terrain is square
        if (meshSize.x != meshSize.z)
        {
            throw new Exception("Terrain size must be square. X=" + meshSize.x + " Z=" + meshSize.z);
        }
        #endregion

        triTerrains = new TerrainsInTriangle((int)meshSize.x, cam, terrainPadding,addAngleToFOV);

        if (centerObject)
            halfMeshSize = (int)(meshSize.x / 2f);
    }

    void Start()
    {
        parent = new GameObject("Terrains");                                                        //setup parent object to keep things neat
        startPos = cam.transform.position;                                                          //initialize startPos
        cameraRotationY = cam.transform.rotation.eulerAngles.y;                                     //initialize cameraRotationY
        UpdateTerrain();                                                                            //load in visible terrains
    }
    #endregion

    void Update()
    {
        #region update distance and rotation
        float xPlayerMove = Mathf.Abs(cam.transform.position.x - startPos.x);                       //how far player moved in X direction
        float zPlayerMove = Mathf.Abs(cam.transform.position.z - startPos.z);                       //how far player moved in Z direction
        float rotation = Mathf.Abs(cameraRotationY - cam.transform.rotation.eulerAngles.y);         //how far player rotated in Y axis
        float distance = Mathf.Max(xPlayerMove, zPlayerMove);                                       //get max distance to prevent one more conditional statement below
        #endregion

        if (rotation > maxRotation || distance > maxMoveDistance)                                   //check if max conditions exceeded
        {
            startPos = cam.transform.position;                                                      //reset startPos
            cameraRotationY = cam.transform.rotation.eulerAngles.y;                                 //reset cameraRotationY
            UpdateTerrain();
        }
        if (showDebug)
            triTerrains.ShowDebug();                                                                //draws calculated fov triangle
    }

    void UpdateTerrain()
    {
        float updateTime = Time.realtimeSinceStartup;                                               //get time
        triTerrains.UpdateTerrainList();                                                            //update list of terrain positions in FOV

        for (int i = 0; i < triTerrains.TerrainPositions.Count; i++)
        {
            string tileName = triTerrains.TerrainPositions[i].x.ToString() + "_" + triTerrains.TerrainPositions[i].y.ToString(); //slow?
            if (!tiles.ContainsKey(tileName))                                                       //if terrain tile does not exist
            {
                GameObject t = Instantiate(terrain.gameObject, new Vector3(triTerrains.TerrainPositions[i].x -halfMeshSize , 0, triTerrains.TerrainPositions[i].y - halfMeshSize), Quaternion.identity, parent.transform);

                t.name = tileName;
                Tile tile = new Tile(t, updateTime);
                tiles.Add(tileName, tile);
            }
            else                                                                                    //terrain tile exists
            {
                (tiles[tileName] as Tile).creationTime = updateTime;                                //update time so it does not get deleted
            }
        }

        Hashtable newTerrain = new Hashtable();
        foreach (Tile tls in tiles.Values)
        {
            if (tls.creationTime != updateTime)
            {
                Destroy(tls.theTile);
            }
            else
            {
                newTerrain.Add(tls.theTile.name, tls);
            }
        }
        tiles = newTerrain;
    }
}
