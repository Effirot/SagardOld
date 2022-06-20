using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Descript : ScriptableObject
{
    [Space, Header("Description")]
    public string Name;
    public string Description;
    public string BigDescription;
    public Sprite image;
}
