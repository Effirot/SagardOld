using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using SagardCL;
using SagardCL.MapObjectInfo;
using System.Threading.Tasks;
using Random = UnityEngine.Random;

public class ZombieAI : CharacterCore
{
    public override MeshRenderer[] MustChangeColor { get => GetComponents<MeshRenderer>(); }
    protected override string IdAddition { get => base.IdAddition + "AI(Zombie)"; } 
    [Space(5)]
    [Header(" ==== Controller settings ==== ")]
    [Range(1, 20)]public int ViewDistance;

    protected override async Task BotLogic()
    {
        await Task.Delay(0);
        Checkers Target = Map.Current.ObjectRegister[0].nowPosition;

        foreach(IObjectOnMap obj in Map.Current.ObjectRegister)
        {
            if(Checkers.Distance(obj.nowPosition, nowPosition) < ViewDistance & obj.nowPosition != nowPosition){
                Target = obj.nowPosition;
                break;
            }
            else Target = nowPosition;
        }

        SetWayToTarget(Target);
        AttackTarget = position.ToCheckers() + new Checkers(0, 1);
        AddActionToPlan(ActionOnIndex(1).Plan(this), "UnitActing");
    }
}