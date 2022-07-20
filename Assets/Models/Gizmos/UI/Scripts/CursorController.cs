using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;

public class CursorController : MonoBehaviour
{
    public static GameObject ObjectOnMap;
    public static Checkers Pos;
    void Update()
    {
        Pos = new Checkers(transform.position);
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) Pos = new Checkers(hit.point);

        float Distance = Vector3.Distance(transform.position, Pos) / 10;
        transform.position = Vector3.MoveTowards(transform.position, new Checkers(Pos, Input.GetMouseButton(0) | Input.GetMouseButton(1)? 0.1f : 0.4f), 0.001f + Distance * 9.8f);
    }
    
    void OnTriggerEnter(Collider collider) { if(collider.gameObject.layer == LayerMask.NameToLayer("Object")) ObjectOnMap = collider.gameObject; }
    void OnTriggerExit(Collider collider) { ObjectOnMap = null; }

}
