using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoleGenerator : MonoBehaviour
{
    int[, ,] PlatformParameters;
    private string PoleGeneratorType = GameObject.Find("GlobalMapController").GetComponent<MapGenerator>().PoleGeneratorType;





    public GameObject panel;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("GlobalMapController").GetComponent<MapGenerator>().Poles--;


        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                GameObject CreatedPole = Instantiate(panel, new Vector3(transform.position.x - 3 + i, transform.position.y + 0.1f, transform.position.z - 3 + j), panel.transform.rotation, gameObject.transform);

                CreatedPole.name = "Pole - " +  (i+1) + ":" + (j+1);
                CreatedPole.GetComponent<CellController>().Upped = 0;
            
            
            }
            
        }
            


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
