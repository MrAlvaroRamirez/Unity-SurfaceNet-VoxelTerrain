using System;
using UnityEngine;

public class Chunk
{
    internal byte[] voxels;
    public ChunkMesh mesh;
    public World world;
    public Vector3Int position;
    private bool isEmpty = true;

    public Chunk(World world, Vector3Int position)
    {
        this.world = world;
        this.position = position;
    }

    public byte[] BuildVoxels()
    {
        Vector3Int chunkPos = position * Constants.CHUNK_SIZE;
        byte[] voxels = new byte[Constants.CHUNK_VOL];
        for (byte x = 0; x < Constants.CHUNK_SIZE; x++)
        {
            for (byte z = 0; z < Constants.CHUNK_SIZE; z++)
            {
                //int worldX = chunkPos.x + x;
                //int worldZ = chunkPos.z + z;
                //int worldHeight = (int)(Perlin.Noise(new Vector2(worldX, worldZ) * 0.03f) * 20 + 32);
                //int localHeight = Math.Min(worldHeight - chunkPos.y, Constants.CHUNK_SIZE);
                for (byte y = 0; y < Constants.CHUNK_SIZE; y++)
                {
                    //int worldY = chunkPos.y + y;
                    int voxelIndex = x + (z * Constants.CHUNK_SIZE) + (y * Constants.CHUNK_AREA);
                    //voxels[voxelIndex] = (byte)(worldY + 1);
                    voxels[voxelIndex] = Perlin.Noise(new Vector3(x, y, z) * 0.1f) + 1 > 1f ? (byte)1 : (byte)0;

                    if (voxels[voxelIndex] > 0)
                        isEmpty = false;
                }
            }
        }

        return voxels;
    }

    public void BuildMesh()
    {
        if (isEmpty)
            return;
        
        mesh = new GameObject("Chunk Mesh").AddComponent<ChunkMesh>();
        mesh.Initialize(this, WorldManager.Instance.worldMaterial);
        mesh.BuildMesh();
    }
}
