using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GlobalStepController : MonoBehaviour
{
    public bool StepActive = false;
    int StepEndNum = 0;
    GameObject[] Figures;
    // Start is called before the first frame update
    void Start()
    {
        
    }

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
        Figures = GameObject.FindGameObjectsWithTag("Figure");

        StepActive = true; 
        StepEndNum = 0;

        int sum = 0;

        foreach (GameObject fig in Figures) {
            if(fig.GetComponent<PlayerController>().ActionOptions[0]) { 
                fig.GetComponent<PlayerController>().active = 3;
                yield return new WaitForSeconds(0.05f);
            }
            sum++;
        
        }
        yield return new WaitForSeconds(sum * 0.05f + 0.3f);

        sum = 0;

        foreach (GameObject fig in Figures) {
            if(fig.GetComponent<PlayerController>().ActionOptions[1]) {
                fig.GetComponent<PlayerController>().active = 4;
                yield return new WaitForSeconds(0.05f);
            }
            sum++;

        }

        yield return new WaitForSeconds(sum * 0.05f + 0.3f);
        StepActive = false;
    }


    private IEnumerator StepTest()
    {
        while (true) { }
    }
}
