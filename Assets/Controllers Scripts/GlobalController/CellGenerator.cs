using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CellGenerator : MonoBehaviour
{
    private MapGenerator GlobController;
    public string Biome;

    [Header("Pre sets")] 
    public GameObject Cell;
    public GameObject Hole;
    public GameObject[] let;

    int key;


    void Start()
    {
        GlobController = GameObject.Find("MapController").GetComponent<MapGenerator>();        

        switch (Biome)
        {
            default:
                Sands();
            break;     
            case "Sands":
                Sands();
            break; 
            case "Weathered sands":
                WeatheredSands();
            break;
            case "Empty":
                Empty();
            break;
        }
    }

    void Sands()
    {
        float UpMultiplyer = GlobController.CellHeihtMultiplyer;
        key = GlobController.key;

        for (int i = 0; i < 7; i++)
        {
            for(int j = 0; j < 7; j++)
            {
                GameObject obj = Instantiate(Cell, transform);


                obj.transform.localPosition = new Vector3(3 - i, 0, 3 - j);
                            
                int x = Convert.ToInt32(obj.transform.position.x);
                int z = Convert.ToInt32(obj.transform.position.z);

                int UpIndex;

                UpIndex = (int)Mathf.Abs(Mathf.Sin(((x + 1) * (z + 1) + key) + 31) * 5) % 3;

            
                obj.transform.position += new Vector3(0, (UpIndex) * UpMultiplyer + 0.4f, 0);
                obj.name = x + " | " + z + "      " + UpIndex;
                GlobController.Cells[x, z] = obj;
            }
        }
    }

    void WeatheredSands()
    {
        float UpMultiplyer = GlobController.CellHeihtMultiplyer;
        key = GlobController.key;

        for (int i = 0; i < 7; i++)
        {
            for(int j = 0; j < 7; j++)
            {
                GameObject obj = Instantiate(Cell, transform);


                obj.transform.localPosition = new Vector3(3 - i, 0, 3 - j);
                            
                int x = Convert.ToInt32(obj.transform.position.x);
                int z = Convert.ToInt32(obj.transform.position.z);


                int UpIndex = (int)Mathf.Abs(Mathf.Sin(((x + 1) * (z + 1) + key) + 31) * 5) % 4;
                float Column = Mathf.Abs(Mathf.Sin(((x + 11) * (z + 3) * key) + 29) * 5) % 6;
                
                obj.transform.position += new Vector3(0, (Column >= 4.93f ? 10 + UpIndex : UpIndex) * UpMultiplyer + 0.4f, 0);
                obj.name = x + " | " + z + "      " + UpIndex;
                GlobController.Cells[x, z] = obj;
            }
        }
    }
    void Empty()
    {
        enabled = false;
        GetComponent<Renderer>().enabled = false;
    }


}
