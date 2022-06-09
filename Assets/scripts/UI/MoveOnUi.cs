using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnUi : MonoBehaviour
{
    public Transform Target;
    [Range(-2f, 5f)] float UpDistance;

    Vector2 position{ get{ return transform.position; } set{ transform.position = value; } }
    
    void Update()
    {
        position = Vector2.Lerp(position, Camera.main.WorldToScreenPoint(Target.position + new Vector3(0, UpDistance, 0)), 4);
    }
}
