using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GlobalStepController : MonoBehaviour
{
    bool StepActive = false;
    bool SteppedEnd = false;
    float Timer = 0;
    public bool[] ActionEnder;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) StepActive = true;
        if (StepActive)
        {

            Debug.Log("Выполнене хода" + Timer);
            GameObject[] Figures = GameObject.FindGameObjectsWithTag("Figure");

            foreach (GameObject fig in Figures) fig.GetComponent<PlayerController>().active = 3;



            if (SteppedEnd)
            {
                foreach (GameObject fig in Figures) fig.GetComponent<PlayerController>().active = 0;
                StepActive = false;
            }
            else
            {
                foreach (GameObject fig in Figures) if(fig.GetComponent<PlayerController>().SteppedEnd) fig.GetComponent<PlayerController>().active = 0;
            }
        }
        else Timer = 0;
    }
}
