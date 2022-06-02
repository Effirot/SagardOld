using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atest : Generation
{   
    [SerializeField] GameObject PlatformPreset;
    void Start()
    {
        Letsgenerate(new Map(), new PlatformVisual[] { new PlatformVisual(PlatformPreset) });
    }

    


}
