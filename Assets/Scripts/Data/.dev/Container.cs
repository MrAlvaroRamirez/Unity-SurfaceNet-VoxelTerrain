using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Container : MonoBehaviour
{
    public Vector3 containerPosition;

    // TODO: Change this to an array
    private Dictionary<Vector3, Voxel> data;
    private MeshData meshData = new MeshData();

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    public void Initialize(Material mat, Vector3 position)
    {
        ConfigureComponents();
        data = new Dictionary<Vector3, Voxel>();
        meshRenderer.sharedMaterial = mat;
        containerPosition = position;
    }

    public void ClearData()
    {
        data.Clear();
    }

    private void ConfigureComponents()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void GenerateMesh()
    {
        meshData.ClearData();

        Vector3 blockPos;
        Voxel block;

        int counter = 0;
        Vector3[] faceVertices = new Vector3[4];
        Vector2[] faceUVs = new Vector2[4];

        foreach (KeyValuePair<Vector3, Voxel> kvp in data)
        {
            if (kvp.Value.ID == 0)
                continue;

            blockPos = kvp.Key;
            block = kvp.Value;

            // Iterate through each face
            for (int i = 0; i < 6; i++)
            {
                if (this[blockPos + voxelFaceChecks[i]].isSolid)
                    continue;
                    
                // Draw the face

                // Collect the vertices and UVs for the face
                for (int j = 0; j < 4; j++ )
                {
                    faceVertices[j] = voxelVertices[voxelVertexIndex[i, j]] + blockPos;
                    faceUVs[j] = voxelUVs[j];
                }

                for (int j = 0; j < 6; j++)
                {
                    meshData.vertices.Add(faceVertices[voxelTriangles[i, j]]);
                    meshData.uvs.Add(faceUVs[voxelTriangles[i, j]]);
                    meshData.triangles.Add(counter++);
                }
            }
        }
    }

    public void UploadMesh()
    {
        meshData.UploadMesh();

        if (meshRenderer == null)
            ConfigureComponents();

        meshFilter.mesh = meshData.mesh;
        if (meshData.vertices.Count > 3)
            meshCollider.sharedMesh = meshData.mesh;
    }

    public Voxel this[Vector3 index]
    {
        get
        {
            if (data.ContainsKey(index))
                return data[index];
            else
                return emptyVoxel;
        }
        set
        {
            if (data.ContainsKey(index))
                data[index] = value;
            else
                data.Add(index, value);
        }
    }

    public static Voxel emptyVoxel = new Voxel() { ID = 0 };

    #region Mesh Data
    public struct MeshData
    {
        public Mesh mesh;
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector2> uvs;

        public bool initialized;

        public void ClearData()
        {
            if (!initialized)
            {
                vertices = new List<Vector3>();
                triangles = new List<int>();
                uvs = new List<Vector2>();

                initialized = true;
                mesh = new Mesh();
            }
            else
            {
                vertices.Clear();
                triangles.Clear();
                uvs.Clear();
                mesh.Clear();
            }
        }

        public void UploadMesh(bool sharedVertices = false)
        {
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, false);
            mesh.SetUVs(0, uvs);
            mesh.Optimize();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.UploadMeshData(true);
        }
    }
    #endregion

    
}
