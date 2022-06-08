using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atest : Generation
{   
    [SerializeField] GameObject PlatformPreset;
    void Start()
    {
        Letsgenerate(
            new Map((x, z, key) =>
            Mathf.Round(Mathf.PerlinNoise(
            200 / (float)(x + 1) + (float)key / 45,
            200 / (float)(z + 1) + (float)key / 31)
            * 10) / 10
            ), new PlatformVisual[] { new PlatformVisual(PlatformPreset) });
    }



    


}
