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
        Letsgenerate(new Map(), PlatformPreset);
    }
}