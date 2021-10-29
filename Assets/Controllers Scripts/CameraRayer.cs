using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRayer : MonoBehaviour
{
    public Transform PosCursor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        

        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "Map")
        {
            PosCursor.position = Vector3.MoveTowards(PosCursor.position, new Vector3(Convert.ToInt32(hit.point.x), 0, Convert.ToInt32(hit.point.z)), 0.1f);            

            Debug.DrawRay(transform.position, hit.point, Color.red);
        }
    }
}
