using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    Checkers LastPos;
    Checkers Pos;

    void LateUpdate()
    {
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, LayerMask.GetMask("Map"))) Pos = new Checkers(hit.point);
        if(Pos != LastPos) { InGameEvents.MapUpdate.Invoke(); LastPos = Pos;  }

        float Distance = Vector3.Distance(transform.position, Pos) / 10;
        transform.position = Vector3.MoveTowards(transform.position, new Checkers(Pos, 0.4f), 0.4f + Distance * 10.8f * Time.deltaTime);
    }
}
