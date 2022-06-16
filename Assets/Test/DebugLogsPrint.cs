using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SagardCL;
using UnityEngine.Events;

public class DebugLogsPrint : MonoBehaviour
{
    public static UnityEvent LogEvent = new UnityEvent();
    
    public Text DidActive, fpsText;
    float deltaTime;
 
    void Update () {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = Mathf.Ceil (fps).ToString ();
    }
}
