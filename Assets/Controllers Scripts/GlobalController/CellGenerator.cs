using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CellGenerator : MonoBehaviour
{
    private MapGenerator GlobController;
    public string cellGenerator;
    public GameObject Cell;
    private GameObject[,] Cells = new GameObject[7, 7];
    int key;


    void Start()
    {
        GlobController = GameObject.Find("MapController").GetComponent<MapGenerator>();
        float UpMultiplyer = GlobController.CellHeihtMultiplyer;
        key = GlobController.GeneratorKey;

        switch (cellGenerator)
        {
            default:
                for (int i = 0; i < 7; i++)
                {
                    for(int j = 0; j < 7; j++)
                    {
                        GameObject obj = Instantiate(Cell, transform);


                        obj.transform.localPosition = new Vector3(3 - i, 0.2f, 3 - j);
                                    
                        int x = Convert.ToInt32(obj.transform.position.x);
                        int z = Convert.ToInt32(obj.transform.position.z); 
                        float UpIndex = (key * ((x * 3 + 1) + (z * 12 + 1)) * 23) % 2 + 0.8f;
                    
                        obj.transform.position = new Vector3(x, UpIndex * UpMultiplyer, z);
                        

                        obj.name = obj.transform.position.x + " | " + obj.transform.position.z;

                        GlobController.Cells[Convert.ToInt32(obj.transform.position.x), Convert.ToInt32(obj.transform.position.z)] = obj;
                    }
                }
            break;            
        }
    }

    void Update()
    {
        foreach(GameObject obj in Cells)
        {

        }
    }

}
