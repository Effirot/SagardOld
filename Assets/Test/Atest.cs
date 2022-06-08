using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atest : Generation
{   
    [SerializeField] PlatformVisual[] PlatformPreset;
    void Start()
    {
        Letsgenerate(
            new Map((x, z, key) => new Map.MapCell(
            (int)Mathf.Round((Mathf.PerlinNoise(200 / (float)(x + 1) + (float)key / 45, 200 / (float)(z + 1) + (float)key / 31) * 10) / 5),
            Mathf.Round(Mathf.PerlinNoise(200 / (float)(x + 1) + (float)key / 45, 200 / (float)(z + 1) + (float)key / 31) * 10) / 10,
            0)), 
            PlatformPreset);
    }



    


}
