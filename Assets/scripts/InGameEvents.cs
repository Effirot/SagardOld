using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public static List<TaskStepStage> StepSystem = new List<TaskStepStage>();
    public delegate Task TaskStepStage(int StepStage);


    public static UnityEvent MapUpdate = new UnityEvent();
    public static UnityEvent<uint, int> MouseController = new UnityEvent<uint, int>();
    
    public static UnityEvent<List<SagardCL.Attack>> AttackTransporter = new UnityEvent<List<SagardCL.Attack>>();
    
    private bool _enabledAttack = false;
    private static bool _canControl = true;
    public static bool canControl { get { return _canControl; } set { _canControl = value; } }

    void Update(){
        if(canControl) MouseControl();
        if(canControl & Input.GetKeyDown(KeyCode.Return)) WalkModeSwitch(); 
    }

    void MouseControl()
    {
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

    async void WalkModeSwitch()
    {
        canControl = false;   
        
        for(int i = 1; i <= 5; i++){
            MapUpdate.Invoke();
            List<Task> task = new List<Task>();
            foreach(TaskStepStage summon in StepSystem) { task.Add(summon(i)); }

            await Task.WhenAll(task.ToArray());
            await Task.Delay(500);
        }
        
        canControl = true;
    }
}
