using System;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEngine;

public static class ChunkMeshBuilder
{
    private static bool IsInsideChunk(Vector3Int pos)
    {
        if (
            pos.x >= 0
            && pos.x < Constants.CHUNK_SIZE
            && pos.y >= 0
            && pos.y < Constants.CHUNK_SIZE
            && pos.z >= 0
            && pos.z < Constants.CHUNK_SIZE
        )
        {
            return true;
        }
        return false;
    }

    private static bool IsVoid(Vector3Int pos, byte[] voxels)
    {
        if (!IsInsideChunk(pos))
            return true;

        if (voxels[pos.x + (pos.z * Constants.CHUNK_SIZE) + (pos.y * Constants.CHUNK_AREA)] == 0)
        {
            return true;
        }
        return false;
    }

    public static ChunkMeshData BuildChunkMesh(
        byte[] voxels,
        Vector3Int chunkPos,
        byte[][] worldVoxels
    )
    {
        ChunkMeshData meshData = new ChunkMeshData();
        meshData.ClearData();

        int counter = 0;

        // Iterate through all voxels in the chunk
        // We start from -1 in order to also check the quads on the edges of the chunk
        for (int x = -1; x < Constants.CHUNK_SIZE + 1; x++)
        {
            for (int y = -1; y < Constants.CHUNK_SIZE + 1; y++)
            {
                for (int z = -1; z < Constants.CHUNK_SIZE + 1; z++)
                {
                    Vector3Int localPos = new Vector3Int(x, y, z);

                    // TODO? Instead of passing all this data, we can make the method local
                    CreateSurfaceQuads(localPos, voxels, meshData, ref counter);
                }
            }
        }

        return meshData;
    }

    // This function will iterate through all the positive axis directions of a voxel
    // If it finds a voxel that is not the same as the current voxel, it will create a quad
    private static void CreateSurfaceQuads(
        Vector3Int localPos,
        byte[] voxels,
        ChunkMeshData meshData,
        ref int counter
    )
    {
        for (int i = 0; i < voxelAxis.Length; i++)
        {
            Vector3Int axis = voxelAxis[i];
            Vector3Int neighborPos = localPos + axis;

            bool voxelExists = !IsVoid(localPos, voxels);
            bool neighboorExists = !IsVoid(neighborPos, voxels);

            // We need to handle quad direction differently for each case
            if ((!voxelExists && neighboorExists) || (voxelExists && !neighboorExists))
            {
                Vector3[] quadVertices = AddQuad(localPos, i, voxels);

                // Add vertices and triangles to the mesh data
                for (int j = 0; j < 6; j++)
                {
                    // Split the quad into two triangles
                    if (voxelExists)
                        meshData.vertices.Add(quadVertices[voxelTriangles[j]]);
                    else
                        meshData.vertices.Add(quadVertices[voxelTrianglesReverse[j]]);

                    meshData.uvs.Add(voxelUVs[voxelTriangles[j]]);
                    meshData.colors.Add(Color.white);
                    meshData.triangles.Add(counter++);
                }
            }
        }
    }

    private static Vector3[] AddQuad(Vector3Int localPos, int axis, byte[] voxels)
    {
        Vector3[] vertices = new Vector3[4];

        for (int i = 0; i < voxelQuads[axis].Length; i++)
        {
            Vector3Int quadVertexPos = localPos + voxelQuads[axis][i];
            vertices[i] = AverageVertex(quadVertexPos, voxels);

            // To disable smoothing, comment the line above and add this one:
            // vertices[i] = quadVertexPos;
        }

        return vertices;
    }

    // Naive surface net implementation
    private static Vector3 AverageVertex(Vector3Int pos, byte[] voxels)
    {
        Vector3 total = Vector3.zero;
        int count = 0;

        // Iterate over the edges of the current voxel
        // For each edge we will check if the voxel at the end of the edge is filled or not
        // We will calculate the average of all these values to estimate the position of the vertex
        // As a result, the final mesh will be smoothed
        for (int i = 0; i < 12; i++)
        {
            Vector3Int edgePos = pos + edgeOffsets[i, 0];
            Vector3Int edgeNeighborPos = pos + edgeOffsets[i, 1];

            if (IsVoid(edgePos, voxels) != IsVoid(edgeNeighborPos, voxels))
            {
                total += (Vector3)(edgePos + edgeNeighborPos) / 2f;
                count++;
            }
        }

        return total / count;
    }

    #region Voxel Statics
    // Constant for the positive axis directions
    static readonly Vector3Int[] voxelAxis = new Vector3Int[3]
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1),
    };

    // Vertex index lookup table for forming quads
    static readonly Vector3Int[][] voxelQuads = new Vector3Int[3][]
    {
        new Vector3Int[4]
        {
            new Vector3Int(0, 0, -1),
            new Vector3Int(0, -1, -1),
            new Vector3Int(0, -1, 0),
            new Vector3Int(0, 0, 0)
        },
        new Vector3Int[4]
        {
            new Vector3Int(0, 0, -1),
            new Vector3Int(0, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(-1, 0, -1)
        },
        new Vector3Int[4]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(-1, 0, 0)
        },
    };

    static readonly Vector3Int[,] edgeOffsets = new Vector3Int[12,2]
    {
        // Edges on min Z axis
        { new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0) },
        { new Vector3Int(1, 0, 0), new Vector3Int(1, 1, 0) },
        { new Vector3Int(1, 1, 0), new Vector3Int(0, 1, 0) },
        { new Vector3Int(0, 1, 0), new Vector3Int(0, 0, 0) },
        // Edges on max Z axis
        { new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 1) },
        { new Vector3Int(1, 0, 1), new Vector3Int(1, 1, 1) },
        { new Vector3Int(1, 1, 1), new Vector3Int(0, 1, 1) },
        { new Vector3Int(0, 1, 1), new Vector3Int(0, 0, 1) },
        // Edges connecting min Z to max Z
        { new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 1) },
        { new Vector3Int(1, 0, 0), new Vector3Int(1, 0, 1) },
        { new Vector3Int(1, 1, 0), new Vector3Int(1, 1, 1) },
        { new Vector3Int(0, 1, 0), new Vector3Int(0, 1, 1) }
    };

    static readonly Vector2[] voxelUVs = new Vector2[4]
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(1, 1),
        new Vector2(1, 0)
    };

    static readonly int[] voxelTriangles = new int[] { 0, 3, 2, 0, 2, 1 };
    static readonly int[] voxelTrianglesReverse = new int[] { 0, 2, 3, 0, 1, 2 };
    #endregion
}
