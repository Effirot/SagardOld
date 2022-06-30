using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnUi : MonoBehaviour
{
    [SerializeField]private Transform _Target;
    public Transform Target { get{ return _Target; } set{ _Target = value; transform.position = Camera.main.WorldToScreenPoint(Target.position + new Vector3(0, UpDistance, 0)); transform.SetParent(GameObject.Find("UIrenderer/GameUI").transform); } }
    [SerializeField][Range(-2f, 5f)] float UpDistance;

    [SerializeField] private bool DestroyWhenDestroyed = false;

    Vector2 position{ get{ return transform.position; } set{ transform.position = value; } }
    Vector2 scale { get{ return transform.localScale; } set{ transform.localScale = value; } }
    
    void OnEnable()
    {   
        transform.SetParent(GameObject.Find("UIrenderer/GameUI").transform);
    }

    void FixedUpdate()
    {
        if(!Target & DestroyWhenDestroyed) Destroy(gameObject);
        if(Target) position = Vector2.Lerp(position, Camera.main.WorldToScreenPoint(Target.position + new Vector3(0, UpDistance, 0)), 10);
        
    }



}
