using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InGameEvents : MonoBehaviour
{
    public static List<TaskStepStage> StepSystem = new List<TaskStepStage>();
    public delegate Task TaskStepStage(int StepStage);
    
    public static UnityEvent StepEnd = new UnityEvent();

    public static UnityEvent MapUpdate = new UnityEvent();
    public static UnityEvent<GameObject, int> MouseController = new UnityEvent<GameObject, int>();
    
    public static UnityEvent<List<SagardCL.Attack>> AttackTransporter = new UnityEvent<List<SagardCL.Attack>>();
    
    private static bool _enabledAttack = false;
    private static bool _Controllable = true;
    public static bool Controllable { get { return _Controllable; } set { _Controllable = value; } }

    void Update(){
        if(EventSystem.current.IsPointerOverGameObject()) return;

        if(Controllable) MouseControl();
        if(Controllable & Input.GetKeyDown(KeyCode.Return)) CompleteModeSwitch(); 
    }

    GameObject ID = null;
    void MouseControl()
    {
        if(Input.GetMouseButtonDown(0)) 
        {        
            if(CursorController.ObjectOnMap) ID = CursorController.ObjectOnMap;
            if(ID == null) { _enabledAttack = false; return; } 

            _enabledAttack = !_enabledAttack; 
            if(_enabledAttack) 
                {MouseController.Invoke(ID, 2); return; } 
            MouseController.Invoke(ID, 0); ID = null;
            
        }
        if (Input.GetMouseButtonDown(1))
        {
            if(CursorController.ObjectOnMap) ID = CursorController.ObjectOnMap;
            if(CursorController.ObjectOnMap)
            MouseController.Invoke(ID, 1);
            
            _enabledAttack = false;
        }

        if (Input.GetMouseButtonUp(1) ) {MouseController.Invoke(ID, 0); ID = null; }
    }

    static async void CompleteModeSwitch()
    {
        Controllable = false;   
        
        for(int i = 1; i <= 5; i++){
            MapUpdate.Invoke();
            List<Task> task = new List<Task>();
            foreach(TaskStepStage summon in StepSystem) { task.Add(summon(i)); }

            await Task.WhenAll(task.ToArray());
        }
        StepEnd.Invoke();
        
        Controllable = true;
    }
}
