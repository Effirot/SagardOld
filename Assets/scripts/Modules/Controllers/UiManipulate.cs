using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using SagardCL.MapObjectInfo;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Reflection;
using Random = UnityEngine.Random;
using UnityAsync;
using System.Threading;

public class UiManipulate : MonoBehaviour
{
    CharacterCore ParameterLink;

    void Start()
    {
        ParameterLink = GetComponent<CharacterCore>();
    }

    bool _MiniOpened = false;
    bool MiniOpened { get { return _MiniOpened; } set { _MiniOpened = value; } }

    public void ViewMini()
    {
        
    }
    public void ViewFull()
    {

    }
    public void Close()
    {
        
    }
}