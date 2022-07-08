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
        DrawWay();
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



    public async void DrawWay()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        
        List<Vector3> checkers = new List<Vector3>();
        await foreach(Checkers a in Checkers.PatchWay.WayTo(new Checkers(0, 0), posForWay, 10))
        {
            checkers.Insert(0, a);
        }

        if(lineRenderer == null) return;
        lineRenderer.positionCount = checkers.Count;
        lineRenderer.SetPositions(checkers.ToArray());
    }
}