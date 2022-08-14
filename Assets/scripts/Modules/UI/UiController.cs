using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using SagardCL.ParameterManipulate;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Reflection;
using Random = UnityEngine.Random;
using UnityAsync;
using UnityEngine.UI;
using TMPro;
public class UiController : MonoBehaviour
{

}

public class StateBarLink : ScriptableObject
{
    [SerializeField] GameObject Target;

    [SerializeField] GameObject ValueLink;
    [SerializeField] TextMeshProUGUI ValueTextLink;
    [SerializeField] TextMeshProUGUI MaxValueTextLink;
    [SerializeField] GameObject ActualValueLink;

    Slider ValueSlider => Target.GetComponent<Slider>();
    Slider ActualValueSlider => Target.transform.Find("Value").GetComponent<Slider>();

    public int Value { get{ return (int)Mathf.Round(ValueSlider.value); } set{ ValueSlider.value = value; } }
    public int ActualValue { get{ return (int)Mathf.Round(ValueSlider.value); } set{ ValueSlider.value = value; ValueTextLink.text = value.ToString(); } }
    public int MaxValue { get{ return (int)Mathf.Round(ValueSlider.maxValue); } set{ ValueSlider.maxValue = value; ActualValueSlider.maxValue = value; MaxValueTextLink.text = value.ToString(); } }

    public Color ValueColor { get{ return ValueLink.GetComponent<Image>().color; } set{  ValueLink.GetComponent<Image>().color = value; ActualValueLink.GetComponent<Image>().color = value; } }
}