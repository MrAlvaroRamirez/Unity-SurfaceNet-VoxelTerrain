using System;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkMeshBuilder
{
    public static int GetChunkCoord(int worldCoord)
    {
        return (int)Math.Floor((float)worldCoord / Constants.CHUNK_SIZE);
    }

    public static int GetChunkIndex(Vector3Int worldPos)
    {
        int chunkX = GetChunkCoord(worldPos.x);
        int chunkY = GetChunkCoord(worldPos.y);
        int chunkZ = GetChunkCoord(worldPos.z);

        if (
            chunkX >= 0
            && chunkX < Constants.WORLD_WIDTH
            && chunkY >= 0
            && chunkY < Constants.WORLD_HEIGHT
            && chunkZ >= 0
            && chunkZ < Constants.WORLD_DEPTH
        )
        {
            return chunkX + (chunkZ * Constants.WORLD_WIDTH) + (chunkY * Constants.WORLD_AREA);
        }
        else
            return -1;
    }

    private static int Modulo(int x, int m)
    {
        return (x % m + m) % m;
    }

    private static bool IsVoid(Vector3Int localPos, Vector3Int worldPos, byte[][] worldVoxels)
    {
        int chunkIndex = GetChunkIndex(worldPos);
        if (chunkIndex == -1)
            return false;

        byte[] chunkVoxels = worldVoxels[chunkIndex];

        int voxelIndex =
            Modulo(localPos.x, Constants.CHUNK_SIZE)
            + (Modulo(localPos.z, Constants.CHUNK_SIZE) * Constants.CHUNK_SIZE)
            + (Modulo(localPos.y, Constants.CHUNK_SIZE) * Constants.CHUNK_AREA);

        if (chunkVoxels[voxelIndex] > 0)
            return false;
        else
            return true;
    }

    public static ChunkMeshData BuildChunkMesh(
        byte[] voxels,
        Vector3Int chunkPos,
        byte[][] worldVoxels
    )
    {
        ChunkMeshData meshData = new ChunkMeshData();
        meshData.ClearData();

        Vector3[] faceVertices = new Vector3[4];

        int counter = 0; // For triangles

        for (int x = 0; x < Constants.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Constants.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                {
                    byte voxelId = voxels[
                        x + (z * Constants.CHUNK_SIZE) + (y * Constants.CHUNK_AREA)
                    ];

                    if (voxelId == 0)
                        continue;

                    Vector3Int localPos = new Vector3Int(x, y, z);

                    Vector3Int worldPos = chunkPos * Constants.CHUNK_SIZE + localPos;

                    // Generate a color based on the voxel's height
                    float normalizedY =
                        (float)voxelId / (Constants.WORLD_HEIGHT * Constants.CHUNK_SIZE);
                    Color voxelColor = new Color(
                        normalizedY,
                        Mathf.Sin(normalizedY * Mathf.PI),
                        Mathf.Cos(normalizedY * Mathf.PI)
                    );

                    // Iterate through each face
                    for (int i = 0; i < 6; i++)
                    {
                        if (
                            !IsVoid(
                                localPos + voxelFaceChecks[i],
                                worldPos + voxelFaceChecks[i],
                                worldVoxels
                            )
                        )
                            continue;

                        // Iterate through each vertex
                        for (int j = 0; j < 4; j++)
                        {
                            faceVertices[j] = voxelVertices[voxelVertexIndex[i, j]] + localPos;
                        }

                        for (int j = 0; j < 6; j++)
                        {
                            meshData.vertices.Add(faceVertices[voxelTriangles[j]]); // Split the quad into two triangles
                            meshData.uvs.Add(voxelUVs[voxelTriangles[j]]);
                            meshData.colors.Add(voxelColor);
                            meshData.triangles.Add(counter++);
                        }
                    }
                }
            }
        }

        return meshData;
    }

    #region Voxel Statics
    // This defines all the vertex positions for a voxel
    static readonly Vector3[] voxelVertices = new Vector3[8]
    {
        new Vector3(0, 1, 0),
        new Vector3(1, 1, 0),
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1),
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(1, 0, 1),
        new Vector3(0, 0, 1),
    };

    // This is a lookup table for the vertex indices of each face of a voxel
    static readonly int[,] voxelVertexIndex = new int[6, 4]
    {
        { 0, 1, 2, 3 }, // Top Face
        { 7, 6, 5, 4 }, // Bottom Face
        { 7, 4, 0, 3 }, // Left Face
        { 5, 6, 2, 1 }, // Right Face
        { 6, 7, 3, 2 }, // Front Face
        { 4, 5, 1, 0 } // Back Face
    };

    // This defines the direction of each face of a voxel
    static readonly Vector3Int[] voxelFaceChecks = new Vector3Int[6]
    {
        new Vector3Int(0, 1, 0), // Top Face
        new Vector3Int(0, -1, 0), // Bottom Face
        new Vector3Int(-1, 0, 0), // Left Face
        new Vector3Int(1, 0, 0), // Right Face
        new Vector3Int(0, 0, 1), // Front Face
        new Vector3Int(0, 0, -1) // Back Face
    };

    static readonly Vector2[] voxelUVs = new Vector2[4]
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(1, 1),
        new Vector2(1, 0)
    };

    static readonly int[] voxelTriangles = new int[] { 0, 3, 2, 0, 2, 1 };
    #endregion

    public void UploadMesh(bool sharedVertices = false)
    {
        if (sharedVertices)
        {
            Dictionary<(Vector3, Vector2, Color), int> vertexIndices =
                new Dictionary<(Vector3, Vector2, Color), int>();
            List<int> sharedTriangles = new List<int>();

            for (int i = 0; i < vertices.Count; i++)
            {
                var key = (vertices[i], uvs[i], colors[i]);
                if (!vertexIndices.ContainsKey(key))
                {
                    vertexIndices.Add(key, vertexIndices.Count);
                }
                sharedTriangles.Add(vertexIndices[key]);
            }

            mesh.SetVertices(vertexIndices.Keys.Select(k => k.Item1).ToList());
            mesh.SetUVs(0, vertexIndices.Keys.Select(k => k.Item2).ToList());
            mesh.SetColors(vertexIndices.Keys.Select(k => k.Item3).ToList());
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
        mesh.UploadMeshData(true);
    }
}
