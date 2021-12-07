using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRayer : MonoBehaviour
{
    Vector3 Pos;
    public GameObject SelectedCell;
    private GameObject LastSelectedCell;

    public float y = 2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Founded")) && hit.collider.gameObject.tag == "Map")
        {
            Pos = new Vector3(hit.point.x, hit.collider.transform.position.y, hit.point.z);
            SelectedCell = hit.collider.gameObject;
        }
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(Convert.ToInt32(Pos.x), Pos.y + y, Convert.ToInt32(Pos.z)), 0.1f);

    }
}
