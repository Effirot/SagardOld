using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATestGeneration : Generation
{   
    [SerializeField] Map.PlatformVisual[] PlatformPreset;

    public Checkers posForWay;

    void Start()
    {
        Regenerate();

    }
    public void Regenerate()
    {
        Map map = new Map(

            (x, z, key) => 
            { return Mathf.PerlinNoise(x * 0.3f + key / 100, z * 0.3f + key / 100) * 1.1f; },
            (x, z, key) => 
            { return (int)Mathf.Round(Mathf.PerlinNoise(x * 0.3f + key / 100, z * 0.3f + key / 100) * 3f) % 3; },
            (x, z, key) => 
            { return (x % 5 == 0 | z % 5 == 0)? 1 : 0; },

            PlatformPreset);

        LetsGenerate(map);

        GetComponent<MeshFilter>().sharedMesh = map.visibleMesh(out Material[] materials);
        GetComponent<MeshCollider>().sharedMesh = map.colliderMesh();

        GetComponent<MeshRenderer>().sharedMaterials = materials;
    }




}