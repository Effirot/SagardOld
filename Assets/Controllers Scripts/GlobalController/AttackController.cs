using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public GameObject WhoAttack;
    public GameObject WhereAttack;
    public int Damage;
    public string DamageType;
    public string Debuff;

    
        

    public override string ToString()
    {
        return "NAME: " + WhoAttack.name + " attacks on " + WhereAttack.name + "   Damage: " + Damage + "   DamageType: " + DamageType + "   Debuff: " + Debuff == ""? "None" : Debuff;
    }
}
public class AttackController : MonoBehaviour
{

    void Update()
    {
        
    }
}
