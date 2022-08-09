using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System;
using SagardCL.ParameterManipulate;

[CreateAssetMenu(fileName = "Item", menuName = "SagardCL objects/Standard Item", order = 51)]
public class Item : Descript
{
    [field: SerializeField] public ReBalancer Stats { get; [SerializeField]private set; }
}


