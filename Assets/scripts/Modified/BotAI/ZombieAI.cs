using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using SagardCL;
using SagardCL.MapObjectInfo;
using System.Threading.Tasks;

public class ZombieAI : CharacterCore
{
    public override MeshRenderer[] MustChangeColor { get => GetComponents<MeshRenderer>(); }

    protected override async Task BotLogic()
    {
        Checkers Target = this.position;
        if(Map.Current.ObjectRegister.Exists(a=>Checkers.Distance(a.nowPosition, nowPosition) <= 5))
            Target = Map.Current.ObjectRegister.MinBy(a=>Checkers.Distance(a.nowPosition, nowPosition)).nowPosition;

    
        SetWayToTarget(Target);
        SetAttackTarget(position.ToCheckers() + new Checkers(0, 1), 1);

    }
}