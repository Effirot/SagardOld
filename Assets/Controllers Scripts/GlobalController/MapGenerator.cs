using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Attack : MonoBehaviour
{
    public GameObject WhoAttack;
    public GameObject WhereAttack;
    public int Damage;
    public string DamageType;
    public string Debuff;

    
        

    public override string ToString()
    {
        return "NAME: " + WhoAttack.name + " attacks on " + WhereAttack.name + "   Damage: " + Damage + "   DamageType: " + DamageType + "   Debuff: " + Debuff == ""? "None" : Debuff;
    }
}




public class MapGenerator : MonoBehaviour
{
    [SerializeField] private bool RegenerateMap = false;
    public int GeneratorKey = 0;
    string[] PoleVariants =
    { 
        "Desert", 
        "Weathered desert", 
        "Stone wasteland", 
        "Magnetic anomaly" 
    };

    public string PoleGeneratorType;
    
    public GameObject PolePreset = null;


    List<Attack> Attacks = new List<Attack>();
    

    public GameObject[,] Poles;
    public GameObject[,] Cells;




    public int Players = 1, startedPoles = 20, polesMultiply = 6, PolesCount;

    [Range(0, 1)] public float CellHeihtMultiplyer = 0.1f;




    void Start()
    {
        Generate();
    }
    void Update()
    {
        if(RegenerateMap)
        {
            GeneratorKey = 0;
            Delete();
            Generate();
            RegenerateMap = false;
        }
    }


    private void Generate()
    {
        if(GeneratorKey == 0) 
        {
            for(int i = 0; i < UnityEngine.Random.Range(1, 30); i++)
            {
                GeneratorKey += UnityEngine.Random.Range(1, 10000000);
            }
        }


        PoleGeneratorType = PoleVariants[(GeneratorKey % PoleVariants.Length)];

        PolesCount = Convert.ToInt32(startedPoles + (polesMultiply + (GeneratorKey % 3)) * (Players));
        


        switch (PoleGeneratorType)
        {
            default:
                PolesCount = Convert.ToInt32(PolesCount * 1.2f);

                Poles = new GameObject[5, 5];
                Cells = new GameObject[5 * 7, 5 * 7];

                for(int i = 0; i < 5; i++)
                {
                    for(int j = 0; j < 5; j++)
                    {
                        Poles[i, j] = Instantiate(PolePreset, transform);
                        Poles[i, j].name = "Pole: " + i + " | " + j;
                        
                        Poles[i, j].transform.position = new Vector3(3 + i * 7, transform.position.y, 3 + j * 7);
                        
                        
                    }
                }

            break;

        }

        Debug.Log("Generated poles: " + PolesCount + "   Pole generation type: " + PoleGeneratorType + "   Key: " + GeneratorKey);
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




}
