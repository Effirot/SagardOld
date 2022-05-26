using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRayer : MonoBehaviour
{
    public Vector3 Pos;
    public GameObject SelectedCell;
    private GameObject LastSelectedCell;

    public float y = 2;

    void FixedUpdate()
    {
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, LayerMask.GetMask("Map")))
        {
            Pos = new Checkers(hit.point, y);
            SelectedCell = hit.collider.gameObject;
        }
        float Distance = Vector3.Distance(transform.position, Pos) / 10;
        transform.position = Vector3.MoveTowards(transform.position, Pos, 0.4f + Distance * 1.8f);
    }
}
