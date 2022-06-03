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
    
    void LateUpdate()
    {
        FigureZones.text = FigureAttacksAndWalkZones();
    }

    string FigureAttacksAndWalkZones()
    {
        string result = "";

        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Figure"))
        {
            result += "--------------[" + obj.name + "]--------------\n";
            result += " moves to position - " + obj.transform.Find("MovePlaner").transform.position.x + ":" + obj.transform.Find("MovePlaner").transform.position.z + "   \n";
            if(obj.GetComponent<UnitController>().NowUsingSkill != null) result += " Use skills" + obj.GetComponent<UnitController>().NowUsingSkill.ToString() + "\n";
            foreach(Attack attack in obj.GetComponent<UnitController>().NowUsingSkill.DamageZone())
            {
                result += " - " + attack.InString() + "\n";

                Color DebugAttackColor()
                {
                    if(attack.damage >= 0) return new Color(attack.damage * 0.15f, 0, 0);
                    return new Color(0, Mathf.Abs(attack.damage) * 0.15f, 0);
                }

                Debug.DrawLine(attack.Where, new Checkers(attack.Where, 1f), DebugAttackColor());
            }
        }
        return result;
    }
}
