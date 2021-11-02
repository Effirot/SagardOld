using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GlobalStepController : MonoBehaviour
{
    public bool StepActive = false;

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

            Figures = GameObject.FindGameObjectsWithTag("Figure");
            
            Debug.Log("Выполнене хода, c " + Figures.Length + " объектами");
            foreach (GameObject fig in Figures)
                if (fig.GetComponent<PlayerController>().Stepped)
                {
                    fig.GetComponent<PlayerController>().active = 3;
                }

            
            StartCoroutine(StepTest());



        }
    }

    private IEnumerator StepTest()
    {
        yield return new WaitForSeconds(0.9f);

        foreach (GameObject fig in Figures) fig.GetComponent<PlayerController>().active = 0;
        StepActive = false;

        yield break;
    }
}
