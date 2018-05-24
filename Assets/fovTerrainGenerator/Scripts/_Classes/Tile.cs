//reference: https://www.youtube.com/watch?v=dycHQFEz8VI
using UnityEngine;
/// <summary>Class to hold terrain tile object and it's creation time.</summary>
class Tile
{
    /// <summary>The terrain object</summary>
    public GameObject theTile;
    /// <summary>Time used for deleting terrain when not needed.</summary>
    public float creationTime;
    /// <summary>Class to hold terrain tile object and it's creation time.</summary>
    public Tile(GameObject t, float ct)
    {
        theTile = t;
        creationTime = ct;
    }
}
