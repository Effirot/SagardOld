using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRayer : MonoBehaviour
{
    Vector3 Pos;
    public float y = 2;
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
            Pos = new Vector3(hit.point.x, hit.collider.transform.position.y, hit.point.z);            
        }

        transform.position = Vector3.MoveTowards(transform.position, new Vector3(Convert.ToInt32(Pos.x), Pos.y + y, Convert.ToInt32(Pos.z)), 0.1f);
    }
}
