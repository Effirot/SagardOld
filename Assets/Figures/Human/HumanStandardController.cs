using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using SagardCL;


public class HumanStandardController : UnitController
{
    public override void GetDamage(Attack attack) 
    { 
        Health.GetDamage(attack); 
        if(attack.damage > 0) ChangeFigureColorWave(attack.DamageColor(), 0.2f);
    }

}