using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour
{
    string[] PoleVariants = { "Desert", "Weathered desert", "Stone wasteland", "Magnetic anomaly" };
    public string PoleGeneratorType;
    // Desert 
    // Weathered desert
    // Stone wasteland
    // 
    //
    //
    //
    //
    //
    //
    // 

    public int Players = 1, startedPoles = 20, polesMultiply = 6, Poles;

    // Start is called before the first frame update
    void Start()
    {
        PoleGeneratorType = PoleVariants[UnityEngine.Random.Range(0, PoleVariants.Length - 1)];

        Poles = Convert.ToInt32(startedPoles + (polesMultiply + UnityEngine.Random.Range(1, 3)) * (Players));
        Debug.Log("��������� "+ Poles + " ����, � ���������: " + PoleGeneratorType);


        switch (PoleGeneratorType)
        {
            default:
                Desert(new int[3] { 4, 1, 1 });
                break;

        }
    }

    // Update is called once per frame
    void Update()
    {

    }


    void Desert(int[] RatyoPoleType)
    {



    }




}
