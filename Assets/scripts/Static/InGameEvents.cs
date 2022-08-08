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
    
    public static Dictionary<IObjectOnMap, List<IObjectOnMap>> WhoAttackToWho = new Dictionary<IObjectOnMap, List<IObjectOnMap>>();


    private static bool enabledAttack = false;
    public static bool Controllable { get; private set; } = true;

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

    enum Step : int
    {
        Walking,
        Attacking,
        EffectUpdate,
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
        
        WhoAttackToWho.Clear();
        StepNumber++;
        
        Controllable = true;
    }
}
