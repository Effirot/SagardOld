using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Reflection;

public class ATestGeneration : MonoBehaviour

{
    public void StartStep(int repeats = 0)
    {
        Session.StartStepTasks(repeats);
    }

    [SerializeField] PlatformPreset[] PlatformPreset;

    public Checkers posForWay;

    void Start()
    {
        new Session(40, 40, 2,

            (x, z, layer, key) => 
            { return Mathf.PerlinNoise(x * 0.3f + key / 100, z * 0.3f + key / 100) * 1.1f; },

            (x, z, layer, key) => 
            { return PlatformPreset[layer % (PlatformPreset.Length - 1)]; },

            (x, z, layer, key) => 
            { return (x % 5 == 0 | z % 5 == 0)? 1 : 0; }
        );
    }
}