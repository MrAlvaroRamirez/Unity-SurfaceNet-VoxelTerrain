using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkMeshData
{
    public Mesh mesh;
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Color> colors;
    public List<Vector2> uvs;

    public bool initialized;

    public void ClearData()
    {
        if (!initialized)
        {
            vertices = new List<Vector3>();
            triangles = new List<int>();
            uvs = new List<Vector2>();
            colors = new List<Color>();

            initialized = true;
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }
        else
        {
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();
            colors.Clear();
            mesh.Clear();
        }
    }

    public void UploadMesh(bool sharedVertices = false)
    {
        if (sharedVertices)
        {
            Dictionary<Vector3, int> vertexIndices =
                new Dictionary<Vector3, int>();
            List<int> sharedTriangles = new List<int>();

            for (int i = 0; i < vertices.Count; i++)
            {
                var key = vertices[i];
                if (!vertexIndices.ContainsKey(key))
                {
                    vertexIndices.Add(key, vertexIndices.Count);
                }
                sharedTriangles.Add(vertexIndices[key]);
            }

            mesh.SetVertices(vertexIndices.Keys.ToList());
            mesh.SetTriangles(sharedTriangles, 0, false);
        }
        else
        {
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, false);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
        }
        mesh.Optimize();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.UploadMeshData(false);
    }
}
