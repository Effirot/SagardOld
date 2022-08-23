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

    CharacterCore Target;

    protected override async Task BotLogic()
    {
        await Task.Delay(0);

        if(Target != null && Checkers.Distance(Target.nowPosition, nowPosition) > ViewDistance)
            Target = null;

        if(Target == null)
            foreach(var obj in Session.CharacterRegister)
                if(Checkers.Distance(obj.Value.nowPosition, nowPosition) < ViewDistance & obj.Value.nowPosition != nowPosition)
                    Target = obj.Value;

        

        SetWayToTarget(Target?.nowPosition ?? nowPosition);
        AttackTarget = position.ToCheckers() + new Checkers(0, 1);
        AddActionToPlan(ActionOnIndex(Target == null?0:1).Plan(this), "UnitActing");
    }
}