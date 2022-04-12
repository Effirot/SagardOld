using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class MapGenerator : MonoBehaviour
{
    public bool RegenerateMap = false;
    string[] MapVariants =
    { 
        "Desert", 
        "Weathered desert", 
        "Stone wasteland", 
        "Magnetic anomaly" 
    };

    public string MapGeneratorType;

    public int Players = 1, startedPoles, MapX, MapY;

    [SerializeField] GameObject PolePreset;
    [SerializeField] GameObject CellPreset;

    public Map Generation;


    void Start()
    {
        Generation = new Map(PolePreset, CellPreset);
        Generate(UnityEngine.Random.Range(1, 10000000));
    }

    public void Generate(int key)
    {

        MapX = startedPoles + Players * 2 + ((key + 3) % 3);  
        MapY = startedPoles + Players * 2 + ((key - 88) % 3);  

        MapGeneratorType = MapVariants[(key % MapVariants.Length)];

        Generation.key = key;

        switch (MapGeneratorType)
        {
            default:
                Generation.GenerateMap(MapX, MapY);                
            break;
            case "Desert":
                MapX = (int)(MapX * 1.3f);
                MapY = (int)(MapY * 1.3f);

                Generation.GenerateMap(MapX, MapY);
            break;
        }

        Debug.Log("Generated poles: " + MapX + "-" + MapY + "   Pole generation type: " + MapGeneratorType + "   Key: " + key);
    }

    void Update()
    {
        if(RegenerateMap)
        {
            Map.Delete();
            Generation = new Map(PolePreset, CellPreset);
            Generate(UnityEngine.Random.Range(1, 10000000));
            RegenerateMap = false;
        }
    }
}
