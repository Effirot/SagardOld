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
    public void Regenerate()
    {
        LetsGenerate(new Map(
            (x, z, key) => 
            {
                return new Map.MapCell(
                    (int)Mathf.Round(Mathf.PerlinNoise(x * 0.3f + key / 100, z * 0.3f + key / 100) * 3f) % 2,
                    Mathf.PerlinNoise(x * 0.3f + key / 100, z * 0.3f + key / 100) * 1.1f
                );
            }, 
            new Vector2(30, 30)
        ), PlatformPreset);

        GetComponent<MeshCombiner>().CombineMeshes(false);
    }




}