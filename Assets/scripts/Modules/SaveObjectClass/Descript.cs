using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Descript", menuName = "SagardCL objects/Description", order = 51)]
public class Descript : ScriptableObject
{
    [Space, Header("Description")]
    public string Name;
    public string Description;
    public string BigDescription;
    public Sprite image;
}
