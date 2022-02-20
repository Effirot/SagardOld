using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;







public class MapGenerator : MonoBehaviour
{
    [SerializeField] private bool RegenerateMap = false;
    public int key = 0;
    string[] MapVariants =
    { 
        "Desert", 
        "Weathered desert", 
        "Stone wasteland", 
        "Magnetic anomaly" 
    };

    public string MapGeneratorType;
    
    public GameObject PolePreset = null;
    

    public GameObject[,] Poles;
    public GameObject[,] Cells;




    public int Players = 1, startedPoles = 20, MapX, MapY;

    [Range(0, 1)] public float CellHeihtMultiplyer = 0.1f;




    void Start()
    {
        Generate();
    }


    private void Generate()
    {
        if(key <= 0) key += UnityEngine.Random.Range(1, 10000000);

        MapX = startedPoles + Players * 3 + ((key + 3) % 3);  
        MapY = startedPoles + Players * 3 + ((key - 88) % 3);  

        MapGeneratorType = MapVariants[(key % MapVariants.Length)];

        


        switch (MapGeneratorType)
        {
            default:
                Desert(new string[] {"Sands", "Weathered sands"});                
            break;
            case "Desert":
                MapX *= (int)1.3f;
                MapY *= (int)1.3f;

                Desert(new string[] {"Sands", "Weathered sands", "Empty"});
            break;

        }

        Debug.Log("Generated poles: " + MapX + "-" + MapY + "   Pole generation type: " + MapGeneratorType + "   Key: " + key);
    }

    void Desert(string[] Biomes)
    {
        Poles = new GameObject[MapX, MapY];
        Cells = new GameObject[MapX * 7, MapY * 7];

        for(int i = 0; i < MapX; i++)
        {
            for(int j = 0; j < MapY; j++)
            {
                Poles[i, j] = Instantiate(PolePreset, transform);
                Poles[i, j].name = "Pole: " + (i + 1)  + " | " + (j + 1);
                

                int BiomeID = (int)Mathf.Abs((int)(Mathf.Sin((((3 * i + 1) * (j * 12 + 1)) + 31 * key)) * 5)) % Biomes.Length;

                CellGenerator Generator = Poles[i, j].GetComponent<CellGenerator>();
                Generator.Biome = Biomes[BiomeID];                
                Poles[i, j].transform.position = new Vector3(3 + i * 7, transform.position.y, 3 + j * 7);                
            }
        }
    }


    private void Delete()
    {
        GameObject[] map = GameObject.FindGameObjectsWithTag("Map");
        
        foreach (GameObject mapObject in map)
        {
            Destroy(mapObject);
        }

    }
    
    Vector3 VectorInInt(Vector3 Vector, float Y)
    {
        return new Vector3(Convert.ToInt32(Vector.x), Y, Convert.ToInt32(Vector.z));
    }
    
    
    
    
    void Update()
    {
        if(RegenerateMap)
        {
            key = 0;
            Delete();
            Generate();
            RegenerateMap = false;
        }
    }
}

