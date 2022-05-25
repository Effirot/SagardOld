using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debuger : MonoBehaviour
{
    public static event System.Action<string> DebuggerLog;


    void Start(){
        DebuggerLog += AddStringToLog;
    }

    void AddStringToLog(string Log)
    {
        GetComponent<Text>().text = Log + GetComponent<Text>().text;
    }






}
