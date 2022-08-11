using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using SagardCL;
using SagardCL.ParameterManipulate;
using System;
using System.Reflection;
using TMPro;

public class MouseControlEvents : MonoBehaviour
{
    public static TextMeshProUGUI StepEndPanel;
    public TextMeshProUGUI StepEndPanelLink;


    void Start() { StepEndPanel = StepEndPanelLink; Map.StepEnd.AddListener(()=> {StepEndPanel.text = Map.StepNumber.ToString();}); }
    void Update(){
        if(EventSystem.current.IsPointerOverGameObject()) return;

        if(Controllable) MouseControl();
    }
    private static bool enabledAttack = false;
    
    public static UnityEvent<GameObject, int> MouseController = new UnityEvent<GameObject, int>();
    public static bool Controllable = true;


    GameObject TargetObject = null;
    void MouseControl()
    {
        if(Input.GetMouseButtonDown(0)) 
        {        
            
            if(CursorController.ObjectOnMap) TargetObject = CursorController.ObjectOnMap;
            if(TargetObject == null) { enabledAttack = false; return; } 

            enabledAttack = !enabledAttack; 
            if(enabledAttack) 
                {MouseController.Invoke(TargetObject, 2); return; } 
            MouseController.Invoke(TargetObject, 0); TargetObject = null;
            
        }
        if (Input.GetMouseButtonDown(1))
        {
            if(CursorController.ObjectOnMap) TargetObject = CursorController.ObjectOnMap;
            if(CursorController.ObjectOnMap)
            MouseController.Invoke(TargetObject, 1);
            
            enabledAttack = false;
        }

        if (Input.GetMouseButtonUp(1) ) {MouseController.Invoke(TargetObject, 0); TargetObject = null; }
    }


}
