using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atest : Generation
{   
    [SerializeField] PlatformVisual[] PlatformPreset;
    void Start()
    {
        Regenerate();
    }
    public void Regenerate()
    {
        LetsGenerate(new Map(), PlatformPreset);
    }
}