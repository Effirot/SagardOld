using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using SagardCL;
using SagardCL.MapObjectInfo;
using System;
using System.Reflection;
using TMPro;

public class MouseControlEvents : MonoBehaviour
{
    public static TextMeshProUGUI StepEndPanel;
    public TextMeshProUGUI StepEndPanelLink;


    void Start() { StepEndPanel = StepEndPanelLink; Session.StepEnd.AddListener(()=> {StepEndPanel.text = Session.Current.StepNumber.ToString();}); }
    void Update(){
        if(EventSystem.current.IsPointerOverGameObject()) return;

        if(Controllable) MouseControl();
    }
    private static bool enabledAttack = false;
    private static bool enabledMove = false;
    
    public static UnityEvent<GameObject, int> MouseController = new UnityEvent<GameObject, int>();
    public static bool Controllable = true;


    GameObject TargetObject = null;
    void MouseControl()
    {
        if(Input.GetMouseButtonDown(0) & !enabledMove) 
        {   
            // if(TargetObject = CursorController.ObjectOnMap) { 
            //     enabledAttack = false; return; } 

            enabledAttack = !enabledAttack; 
            if(enabledAttack) 
                {MouseController.Invoke(TargetObject = CursorController.ObjectOnMap, 2); enabledAttack = true; }
            else
                {MouseController.Invoke(TargetObject = null, 0); enabledAttack = false; }
        }
        if (Input.GetMouseButtonDown(1) & !enabledAttack)
        {
            MouseController.Invoke(TargetObject = CursorController.ObjectOnMap, 1);
            enabledMove = true;
        }
        if (Input.GetMouseButtonUp(1) & !enabledAttack) 
        { 
            MouseController.Invoke(TargetObject = null, 0); 
            enabledMove = false;
        }
    }


}
