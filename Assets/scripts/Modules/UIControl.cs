using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIControl : MonoBehaviour
{
    public GameObject Target;
    public GameObject Ray;

    Vector2 position{ get{ return transform.position; } set{ transform.position = value; } }

    Vector3 TranslateToFollowing(Vector3 position) => Camera.main.WorldToScreenPoint(position);
    
    void Awake()
    {
        position = TranslateToFollowing(Target.transform.position + new Vector3(0, 1.1f, 0));
    }


    void Update()
    {
        Ray.transform.position = Vector2.Lerp(Ray.transform.position, TranslateToFollowing(Target.transform.position + new Vector3(0, 1.5f, 0)), 4);
    }
}
