using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public Material worldMaterial;
    private World world;
    public GameObject grassPrefab;

    void Start()
    {
        if (_instance != null)
        {
            if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            _instance = this;
        }

        world = new World();
        world.BuildChunks();
        world.BuildChunkMeshes();
        //world.SpawnGrass(grassPrefab);
    }

    private static WorldManager _instance;

    public static WorldManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<WorldManager>();

            return _instance;
        }
    }
}
