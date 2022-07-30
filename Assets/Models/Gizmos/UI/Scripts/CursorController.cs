using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using UnityEngine.Events;

public class CursorController : MonoBehaviour
{
    public static GameObject ObjectOnMap;
    static Checkers _Pos;
    static Checkers LastPos;
    public static Checkers position { get { return _Pos; } private set { 
        if(value != LastPos){
            LastPos = value;
            ChangePosOnMap.Invoke(value);
        }
        
        _Pos = value; } }


    public static UnityEvent<Checkers> ChangePosOnMap = new UnityEvent<Checkers>();

    void Start()
    {
        //ChangePosOnMap.AddListener();
    }

    void Update()
    {
        transform.position = position;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Map"))) position = new Checkers(hit.point);

        float Distance = Vector3.Distance(transform.position, position) / 10;
        position = Vector3.MoveTowards(transform.position, new Checkers(position, Input.GetMouseButton(0) | Input.GetMouseButton(1)? 0.1f : 0.4f), 0.001f + Distance * 9.8f);
    }
    
    void OnTriggerEnter(Collider collider) { if(collider.gameObject.layer == LayerMask.NameToLayer("Object")) ObjectOnMap = collider.gameObject; }
    void OnTriggerExit(Collider collider) { ObjectOnMap = null; }

}
