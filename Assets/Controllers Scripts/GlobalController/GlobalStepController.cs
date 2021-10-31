using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GlobalStepController : MonoBehaviour
{
    public bool StepActive = false;
    int StepEndNum = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) StepActive = true; StepEndNum = 0;

        if (StepActive)
        {
            GameObject[] Figures = GameObject.FindGameObjectsWithTag("Figure");
            
            Debug.Log("Выполнене хода, c " + Figures.Length + " объектами");


            if (StepEndNum < Figures.Length)
            {
                foreach (GameObject fig in Figures)
                    if (fig.GetComponent<PlayerController>().SteppedEnd)
                    {
                        StepEndNum++;
                        fig.GetComponent<PlayerController>().active = 3;
                    }
            }
            else
            {
                foreach (GameObject fig in Figures)
                {
                    fig.GetComponent<PlayerController>().active = 0;
                }


                StepActive = false;
            }
        } 
    }
}
