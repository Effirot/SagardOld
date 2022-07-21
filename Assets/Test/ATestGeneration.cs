using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Reflection;

public class ATestGeneration : Generation
{   
    [SerializeField] PlatformPresets[] PlatformPreset;

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
        
        3,
        PlatformPreset);

        GetComponent<MeshRenderer>().sharedMaterials = map.MaterialsList.ToArray();

        GetComponent<MeshCollider>().sharedMesh = map.MapCollider;
        GetComponent<MeshFilter>().sharedMesh = map.MapMesh;
        
        LetsGenerate(map);
    }




}