using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnUi : MonoBehaviour
{
    private Transform _Target;
    public Transform Target { get{ return _Target; } set{ _Target = value; transform.position = Camera.main.WorldToScreenPoint(Target.position + new Vector3(0, UpDistance, 0)); } }
    [Range(-2f, 5f)] float UpDistance;

    Vector2 position{ get{ return transform.position; } set{ transform.position = value; } }
    
    void OnEnable()
    {

    }

    void Update()
    {
        if(Target) position = Vector2.Lerp(position, Camera.main.WorldToScreenPoint(Target.position + new Vector3(0, UpDistance, 0)), 10 * Time.deltaTime);
    }
}
