using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Reflection;

public class ATestGeneration : Generation
{   
    [SerializeField] PlatformPreset[] PlatformPreset;

    public Checkers posForWay;

    void Start()
    {
        Regenerate();
    }
    public void Regenerate()
    {
        new Map(
        (x, z, key) => 
        { return Mathf.PerlinNoise(x * 0.3f + key / 100, z * 0.3f + key / 100) * 1.1f; },

        (x, z, key) => 
        { return PlatformPreset[(int)Mathf.Round(Mathf.PerlinNoise(x * 0.3f + key / 100, z * 0.3f + key / 100) * 3f) % 3]; },

        (x, z, key) => 
        { return (x % 5 == 0 | z % 5 == 0)? 1 : 0; },
        
        3);

        GetComponent<MeshRenderer>().sharedMaterials = Map.Current.MaterialsList.ToArray();

        GetComponent<MeshCollider>().sharedMesh = Map.Current.MapCollider;
        GetComponent<MeshFilter>().sharedMesh = Map.Current.MapMesh;
        
        LetsGenerate(Map.Current);
    }




}