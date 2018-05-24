//Reference: https://youtu.be/dycHQFEz8VI?t=271
using UnityEngine;

public class PerlinNoise: MonoBehaviour {

    public int heightScale = 10;
    public float DetailSize = 0.9f;

    void Start () {
        Mesh mesh = this.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        for (int v = 0; v < vertices.Length; v++)
        {
            float x = Mathf.Round(vertices[v].x);
            float z = Mathf.Round(vertices[v].z);

            vertices[v].y = Mathf.PerlinNoise(
               ((x * this.transform.localScale.x) + this.transform.position.x) * DetailSize,
               ((z * this.transform.localScale.z) + this.transform.position.z) * DetailSize) * heightScale;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        this.gameObject.AddComponent<MeshCollider>();

    }
    

}
