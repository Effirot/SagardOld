using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using SagardCL.MapObjectInfo;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Reflection;
using Random = UnityEngine.Random;
using UnityAsync;

public abstract class CharacterCoreVisualized : CharacterCore
{
    public int SkillIndex = 0;

    [field: Space(3)]
    [field: Header(" ==== Visualizer settings ====")]
    [field: SerializeField] public AllInOne MPlaner { get; set; }
    [field: SerializeField] public AllInOne APlaner { get; set; }
    protected override GameObject[] WalkBlackList { get { 
            List<GameObject> list = new List<GameObject>() { MPlaner.Planer, APlaner.Planer };
            list.AddRange(base.WalkBlackList); 
            return list.ToArray();
        }
    } 

    protected override string IdAddition { get => base.IdAddition + " PlanVisible-"; } 

    public override Checkers AttackTarget { get{ return APlaner.position; } protected set { APlaner.position = value; } }
    public override Checkers MoveTarget { get { return MPlaner.position; } protected set { MPlaner.position = value; } }

    protected override void Start()
    {
        base.Start();

        Map.MapUpdate.AddListener(MapUpdate);
    }
    public void MapUpdate()
    {
        SetWayToTarget(MoveTarget);
        AddActionToPlan(ActionOnIndex(SkillIndex), "UnitActing");

        MPlaner.LineRenderer.positionCount = WalkWay.Count;
        MPlaner.LineRenderer.SetPositions(Checkers.ToVector3List(WalkWay).ToArray()); 

        MPlaner.Renderer.enabled = new Checkers(MPlaner.position) != new Checkers(this.position);
    }
}