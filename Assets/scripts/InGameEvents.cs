using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InGameEvents : MonoBehaviour
{
    public static UnityEvent MapUpdate = new UnityEvent();
    public static UnityEvent<uint, int> MouseController = new UnityEvent<uint, int>();
    public static UnityEvent<int> StepSystem = new UnityEvent<int>();
    public static UnityEvent<SagardCL.Attack> OnAttack = new UnityEvent<SagardCL.Attack>();

    private bool _enabledAttack = false;
    void Update(){
        if (Input.GetMouseButtonDown(0) | (Input.GetMouseButtonDown(1)))
        {
            if(Input.GetMouseButtonDown(0)& _enabledAttack) { MouseController.Invoke(0, 0); _enabledAttack = false; return; }
            
            MouseController.Invoke(CursorController.ObjectOnMap.GetComponent<IDgenerator>().ID, Input.GetMouseButtonDown(1)? 1:2);
            
            _enabledAttack = !_enabledAttack;
        }

        if (Input.GetMouseButtonUp(1)) MouseController.Invoke(0, 0); 
    }
}
