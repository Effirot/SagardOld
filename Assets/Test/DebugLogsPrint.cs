using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SagardCL;

public class DebugLogsPrint : MonoBehaviour
{
    public Text DidActive, FigureZones, fpsText;
    float deltaTime;
 
    void Update () {
         deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
         float fps = 1.0f / deltaTime;
         fpsText.text = Mathf.Ceil (fps).ToString ();
    }
    void Awake () { InGameEvents.MapUpdate.AddListener(() => FigureZones.text = FigureAttacksAndWalkZones() ); }

    
    GameObject[] Figures => GameObject.FindGameObjectsWithTag("Figure");
    
    string FigureAttacksAndWalkZones()
    {
        string result = "";

        foreach(GameObject obj in Figures)
        {
            result += "--------------[" + obj.name + "]--------------\n";
            result += " moves to position - " + obj.transform.Find("MovePlaner").transform.position.x + ":" + obj.transform.Find("MovePlaner").transform.position.z + "   \n";
            if(obj.GetComponent<UnitController>().NowUsingSkill != null) result += " Use skills" + obj.GetComponent<UnitController>().NowUsingSkill.ToString() + "\n";
            foreach(Attack attack in obj.GetComponent<UnitController>().NowUsingSkill.DamageZone())
            {
                result += " - " + attack.InString() + "\n";
            }
        }
        return result;
    }
}
