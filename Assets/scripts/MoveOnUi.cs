using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnUi : MonoBehaviour
{
    public Transform Target;

    Vector2 position{ get{ return transform.position; } set{ transform.position = value; } }
    
    void Update()
    {
        transform.position = Vector2.Lerp(transform.position, Camera.main.WorldToScreenPoint(Target.position + new Vector3(0, 1.5f, 0)), 4);
    }
}
