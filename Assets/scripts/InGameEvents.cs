using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InGameEvents : MonoBehaviour
{
    public static Transform AttackFolders;
    public static Transform Figures;


    void Awake() 
    {
        AttackFolders = GameObject.Find("Attacks").transform;
        Figures = GameObject.Find("Figures").transform;
    }

    public static UnityEvent MapUpdate = new UnityEvent();
    public static UnityEvent<uint, int> MouseController = new UnityEvent<uint, int>();
    public static UnityEvent<int> StepSystem = new UnityEvent<int>();
    public static UnityEvent<SagardCL.Attack> OnAttack = new UnityEvent<SagardCL.Attack>();

    private bool _enabledAttack = false;
    void Update(){

        if(Input.GetMouseButtonDown(0)) 
        {            
            _enabledAttack = !_enabledAttack; 
            if(CursorController.ObjectOnMap & _enabledAttack) 
                {MouseController.Invoke(CursorController.ObjectOnMap.GetComponent<IDgenerator>().ID, 2); return; }
            MouseController.Invoke(0, 0);
            
        }
        if ((Input.GetMouseButtonDown(1)))
        {
            if(CursorController.ObjectOnMap)
            MouseController.Invoke(CursorController.ObjectOnMap.GetComponent<IDgenerator>().ID, 1);
            
            _enabledAttack = false;
        }

        if (Input.GetMouseButtonUp(1)) MouseController.Invoke(0, 0); 
    }
}
