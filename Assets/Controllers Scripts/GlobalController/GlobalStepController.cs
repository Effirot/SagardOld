using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GlobalStepController : MonoBehaviour
{
    public bool StepActive = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !StepActive) 
        {
            StartCoroutine(Walker());
        }
    }


    private IEnumerator Walker()
    {
        GameObject[] Figures = GameObject.FindGameObjectsWithTag("Figure");

        StepActive = true; 

        bool Sum = false;

        //Walk
        foreach (GameObject fig in Figures) {
            if(fig.GetComponent<PlayerController>().ActionOptions[0]) { 
                fig.GetComponent<PlayerController>().active = 3;
                yield return new WaitForSeconds(0.05f);
                Sum = true;
            }
        }
        yield return new WaitForSeconds(Sum ? 0.3f : 0);
        Sum = false;
        //Attack
        foreach (GameObject fig in Figures) {
            if(fig.GetComponent<PlayerController>().ActionOptions[1]) {
                fig.GetComponent<PlayerController>().active = 4;
                yield return new WaitForSeconds(0.05f);
                Sum = true;
            }
        }
        yield return new WaitForSeconds(Sum ? 0.3f : 0);
        Sum = false;
        //Death
        foreach (GameObject fig in Figures) {
            if(fig.GetComponent<PlayerParameterList>().HP <= 0) {
                fig.GetComponent<PlayerController>().active = 5;
                yield return new WaitForSeconds(0.05f);
                Sum = true;
            }
        }
        yield return new WaitForSeconds(Sum ? 0.3f : 0);
        Sum = false;
        //Rest\Level Ups
        foreach (GameObject fig in Figures) {
            if(!fig.GetComponent<PlayerController>().ActionOptions[0] && !fig.GetComponent<PlayerController>().ActionOptions[1]) {
                fig.GetComponent<PlayerParameterList>().Rest();
                yield return new WaitForSeconds(0.05f);
                Sum = true;
            }
        }
        yield return new WaitForSeconds(Sum ? 0.3f : 0);
        
        
        StepActive = false;
    }


    private IEnumerator StepTest()
    {
        while (true) { }
    }
}
