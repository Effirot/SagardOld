using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using SagardCL;
using System;
using System.Reflection;
using TMPro;

public class InGameEvents : MonoBehaviour
{
    public static int StepNumber = 0;
    public TextMeshProUGUI StepEndPanelLink;
    public static TextMeshProUGUI StepEndPanel;

    internal static List<TaskStepStage> StepSystem = new List<TaskStepStage>();
    public delegate Task TaskStepStage(string StepStage);
    
    public static UnityEvent StepEnd = new UnityEvent();

    public static UnityEvent MapUpdate = new UnityEvent();
    public static UnityEvent<GameObject, int> MouseController = new UnityEvent<GameObject, int>();
    
    public static UnityEvent<List<SagardCL.Attack>> AttackTransporter = new UnityEvent<List<SagardCL.Attack>>();
    
    private static bool _enabledAttack = false;
    private static bool _Controllable = true;
    public static bool Controllable { get { return _Controllable; } set { _Controllable = value; } }

    void Start() { StepEndPanel = StepEndPanelLink; StepEnd.AddListener(()=> {StepEndPanel.text = StepNumber.ToString();}); }
    void Update(){
        if(EventSystem.current.IsPointerOverGameObject()) return;

        if(Controllable) MouseControl();
    }

    GameObject TargetObject = null;
    void MouseControl()
    {
        if(Input.GetMouseButtonDown(0)) 
        {        
            
            if(CursorController.ObjectOnMap) TargetObject = CursorController.ObjectOnMap;
            if(TargetObject == null) { _enabledAttack = false; return; } 

            _enabledAttack = !_enabledAttack; 
            if(_enabledAttack) 
                {MouseController.Invoke(TargetObject, 2); return; } 
            MouseController.Invoke(TargetObject, 0); TargetObject = null;
            
        }
        if (Input.GetMouseButtonDown(1))
        {
            if(CursorController.ObjectOnMap) TargetObject = CursorController.ObjectOnMap;
            if(CursorController.ObjectOnMap)
            MouseController.Invoke(TargetObject, 1);
            
            _enabledAttack = false;
        }

        if (Input.GetMouseButtonUp(1) ) {MouseController.Invoke(TargetObject, 0); TargetObject = null; }
    }

    enum Step : int
    {
        Walking,
        EffectUpdate,
        Attacking,
        DamageMath,
        Dead,
        Rest
    }
    public static async void CompleteModeSwitch()
    {
        if(!Controllable) return;
        Controllable = false;   

        Debug.ClearDeveloperConsole();
        
        for(int i = 0; i < Enum.GetNames(typeof(Step)).Length; i++){
            Debug.Log($"Now step: {(Step)i}");
            MapUpdate.Invoke();
            List<Task> task = new List<Task>();

            Step step = (Step)i;

            foreach(TaskStepStage summon in StepSystem) { task.Add(summon(step.ToString())); }
            try{ await Task.WhenAll(task.ToArray()); } catch(Exception e) { Debug.LogError(e); }
        }
        StepEnd.Invoke();
        
        StepNumber++;
        
        Controllable = true;
    }
}
