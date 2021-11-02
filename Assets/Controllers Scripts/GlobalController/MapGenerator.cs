using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public string PoleGeneratorType;
    // Desrt 
    // 
    //
    //
    //
    //
    //
    //
    //
    //
    // 

    public int Players = 1, startedPoles = 20, polesMultiply = 6;
    private int Poles;

    // Start is called before the first frame update
    void Start()
    {
        Poles = startedPoles + polesMultiply * Players;
        Debug.Log("Назначено "+ Poles + " поля");
    }

    // Update is called once per frame
    void Update()
    {
        switch (PoleGeneratorType)
        {
            default:

            case "Desert": 
                break;




        }
    }
}
