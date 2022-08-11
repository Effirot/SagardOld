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

public abstract class CharacterCoreVisualized : CharacterCore
{
    [field: Space(3)]
    [field: Header(" ==== Visualizer settings ====")]
    [field: SerializeField] public AllInOne MPlaner { get; set; }
    [field: SerializeField] public AllInOne APlaner { get; set; }

    public override Checkers AttackTarget { get{ return APlaner.position; } protected set { APlaner.position = value; } }
    public override Checkers MoveTarget { get { return MPlaner.position; } protected set { MPlaner.position = value; } }

    protected override void Start()
    {
        base.Start();

        Map.MapUpdate.AddListener(UpdatePos);
    }
    public void UpdatePos()
    {
        GenerateWayToTarget(MoveTarget, gameObject, MPlaner.Planer, APlaner.Planer);
        SetAttackTarget(AttackTarget);

        MPlaner.LineRenderer.positionCount = WalkWay.Count;
        MPlaner.LineRenderer.SetPositions(Checkers.ToVector3List(WalkWay).ToArray()); 

        MPlaner.Renderer.enabled = new Checkers(MPlaner.position) != new Checkers(this.position);
    }

    public override async void SetAttackTarget(Checkers position)
    {
        base.SetAttackTarget(position);
        if(CurrentSkill.NoWalking){
            MPlaner.LineRenderer.positionCount = 0;
            MPlaner.LineRenderer.enabled = false;}

        Generation.DrawAttack(await CurrentSkill.GetAttacks(MoveTarget, AttackTarget, this), this);
    }
}