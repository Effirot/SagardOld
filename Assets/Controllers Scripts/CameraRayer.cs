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

    void Update()
    {


        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, LayerMask.GetMask("Founded")) && hit.collider.gameObject.tag == "Map")
        {
            Pos = hit.collider.transform.position;
            SelectedCell = hit.collider.gameObject;
        }

        float Distance = Vector3.Distance(transform.position, Pos) / 10;
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(Convert.ToInt32(Pos.x), Pos.y + y, Convert.ToInt32(Pos.z)), 0.1f + Distance);

    }
}
