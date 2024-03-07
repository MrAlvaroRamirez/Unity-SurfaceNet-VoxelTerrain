using UnityEngine;

public class World
{
    private Chunk[] chunks = new Chunk[Constants.WORLD_VOL];
    public byte[][] voxels = new byte[Constants.WORLD_VOL][];

    public void BuildChunks()
    {
        for (int x = 0; x < Constants.WORLD_WIDTH; x++)
        {
            for (int y = 0; y < Constants.WORLD_HEIGHT; y++)
            {
                for (int z = 0; z < Constants.WORLD_DEPTH; z++)
                {
                    long chunkIndex = x + (z * Constants.WORLD_WIDTH) + (y * Constants.WORLD_AREA);
                    Chunk chunk = new Chunk(this, new Vector3Int(x, y, z));
                    chunks[chunkIndex] = chunk;
                    voxels[chunkIndex] = chunk.BuildVoxels();
                    chunk.voxels = voxels[chunkIndex];
                }
            }
        }
    }

    public void BuildChunkMeshes()
    {
        foreach (Chunk chunk in chunks)
        {
            chunk.BuildMesh();
        }
    }

    // Experimental grass spawner
    // Horrible performance, but it works as a preview
    public void SpawnGrass(GameObject prefab)
    {
        foreach (Chunk chunk in chunks)
        {
            var mesh = chunk.mesh.meshFilter.sharedMesh;
            var polygons = mesh.triangles;

            // Iterate thorugh all the triangles in the mesh
            for (int i = 0; i < polygons.Length; i += 9)
            {
                // Get the vertices of the triangle
                Vector3 v1 = mesh.vertices[polygons[i]];
                Vector3 v2 = mesh.vertices[polygons[i + 1]];
                Vector3 v3 = mesh.vertices[polygons[i + 2]];

                // Calculate the center of the triangle
                Vector3 center = (v1 + v2 + v3) / 3;

                // Get the normal of the triangle
                Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

                // If normal is pointing up
                if (normal.y > 0.5f)
                {
                    GameObject gameObject = GameObject.Instantiate(prefab);
                    gameObject.transform.position = center;
                    // Rotate grass randomly
                    gameObject.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));
                }
            }
        }
    }
}
