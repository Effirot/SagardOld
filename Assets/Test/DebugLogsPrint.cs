using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SagardCL;

public class DebugLogsPrint : MonoBehaviour
{
    public Text DidActive, FigureZones;
    void LateUpdate()
    {
        FigureZones.text = FigureAttacksAndWalkZones();
    }

    string FigureAttacksAndWalkZones()
    {
        string result = "";
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Figure"))
        {
            result += "--------------[" + obj.name+ "] --------------\n";
            result += " moves to position - " + obj.transform.Find("MovePlaner").transform.position.x + ":" + obj.transform.Find("MovePlaner").transform.position.z + "\n";
            foreach(Attack attack in obj.GetComponent<UnitController>().AttackList)
            {
                result += " - " + attack.InString() + "\n";
            }
        }
        return result;
    }
}
