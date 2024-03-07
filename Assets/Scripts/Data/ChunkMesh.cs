using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkMesh : MonoBehaviour
{
    private Chunk chunk;
    private MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    private MeshCollider meshCollider;

    public void Initialize(Chunk chunk, Material mat)
    {
        this.chunk = chunk;
        ConfigureComponents();
        meshRenderer.sharedMaterial = mat;
        transform.position = chunk.position * Constants.CHUNK_SIZE;
    }

    private void ConfigureComponents()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void BuildMesh()
    {
        ChunkMeshData meshData = ChunkMeshBuilder.BuildChunkMesh(
            chunk.voxels,
            chunk.position,
            chunk.world.voxels
        );
        meshData.UploadMesh(true);
        meshFilter.mesh = meshData.mesh;
        if (meshData.vertices.Count > 3)
            meshCollider.sharedMesh = meshData.mesh;
    }
}
