using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATestGeneration : Generation
{   
    [SerializeField] PlatformVisual[] PlatformPreset;

    public Checkers posForWay;

    void Start()
    {
        Regenerate();
    }
    public async void Regenerate()
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
        
        await System.Threading.Tasks.Task.Delay(2000);

        Debug.Log("Deformed");
        map.ChangeHeigh(new Checkers(1, 1, 2), new Checkers(1, 2, 2), 
                        new Checkers(2, 2, 2.7f), new Checkers(2, 1, 2.5f));

        GetComponent<MeshCollider>().sharedMesh = map.MapCollider;
    }




}