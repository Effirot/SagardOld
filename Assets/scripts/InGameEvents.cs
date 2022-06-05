using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class InGameEvents
{
    public static UnityEvent MapUpdate;
    public static UnityEvent<GameObject> MouseController;
    public static UnityEvent<int> StepSystem;
    public static UnityEvent<SagardCL.Attack> OnAttack;




}
