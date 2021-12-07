using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CellController : MonoBehaviour
{
    public int Upped = 0;
    float StartPosY;

    public List<int> DamageOn = new List<int> {};
    public List<string> DamageTypeOn = new List<string> {};
    public List<string> DebuffOn = new List<string> {};
    public List<string> WhoAttackOn = new List<string> {};





    


    void Start()
    {
        StartPosY = transform.position.y;
        
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, StartPosY + Upped * 0.2f, transform.position.z);

        if(DamageOn.Sum() > 0){GetComponent<Renderer>().material.color = Color.red;}
        else{GetComponent<Renderer>().material.color = Color.white;}
    }


    public void Founded(GameObject Who, int Damage, string Type = "Phys", string Debuff = "")
    {
        GetComponent<Renderer>().material.color = Color.red;
    }
    public void Losted (GameObject Who, int Damage, string Type = "Phys", string Debuff = "")
    {

    }
}
