using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System;
using SagardCL.IParameterManipulate;

[CreateAssetMenu(fileName = "Item", menuName = "SagardCL objects/Standard Item", order = 51)]
public class Item : Descript, Sendable
{
    public bool Artifacer = false;
    public ParamsChanger ThisItem;
    public static implicit operator ParamsChanger(Item item) { return item.ThisItem; }

}


