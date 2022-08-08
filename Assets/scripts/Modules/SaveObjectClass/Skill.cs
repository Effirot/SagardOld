using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using SagardCL.ParameterManipulate;
using System.Threading.Tasks;
using UnityEditor;
using System.ComponentModel;
using System;
using System.Linq;

[Serializable]public struct Skill
{
    public static Skill Empty(){
        return new Skill()
        {
            Name = "Empty",
            Description = "Empty Skill Description",
            NoWalking = false,
            Realizations = new List<AttacksPlacer>(),
        };}
    

    [Space, Header("Description")]
    public string Name;
    public string Description;
    public string BigDescription;
    public Sprite image;
    [Space(2)]
    [Header("Parameters")]
    public bool NoWalking;
    [SerializeReference, SubclassSelector] public List<AttacksPlacer> Realizations;

    [Space(3)]
    [Header("Skill Price")]
    [SerializeReference, SubclassSelector] public List<SkillAction> Actions;

    public bool isEmpty => Realizations != null ? Realizations.Count == 0 : true |
                           Actions != null ? Actions.Count == 0 : true;

    
    public async Task Complete(Checkers from, Checkers to, IObjectOnMap target)
    {
        InGameEvents.AttackTransporter.Invoke(await GetAttacks(from, to, (IAttacker)target));

        foreach(SkillAction action in Actions) action.Action(target);
    }

    public async Task<List<Attack>> GetAttacks(Checkers from, Checkers to, IAttacker target)
    {
        List<Attack> attackList = new List<Attack>();
        List<Checkers> Overrides = new List<Checkers>();

        foreach(AttacksPlacer HitZone in this.Realizations) 
            await foreach(Attack attack in HitZone.GetAttackList(from, to, target)){ 
                if(!Overrides.Exists(a=>a==attack.Position)) {
                    attackList.Add(attack); 
                    
                    if(HitZone.Override) 
                        Overrides.Add(attack.Position); } }

        return attackList;
    }
}
public interface SkillAction
{
    public void Action(IObjectOnMap target);
}

public interface AttacksPlacer
{
    IAsyncEnumerable<Attack> GetAttackList(Checkers from, Checkers to, IAttacker Target);
    
    float DamagePercent { get; set; }
    DamageType DamageType { get; }

    bool Override { get; }

    protected static Checkers PointTargeting(Checkers from, Checkers to, TargetPointGuidanceType PointType, int MaxDistance, int MinDistance = 0) { switch (PointType)
        {
            default: return from;

            case TargetPointGuidanceType.ToCursor: return MovementTo(from, to, false, false);
            case TargetPointGuidanceType.ToCursorMaximum: return MovementTo(from, to, true, false);

            case TargetPointGuidanceType.ToCursorWithWalls: return MovementTo(from, to, false, true);
            case TargetPointGuidanceType.ToCursorWithWallsMaximum: return MovementTo(from, to, true, true);

            case TargetPointGuidanceType.ToFromPoint: return from;
        }

        Checkers MovementTo(Checkers from, Checkers to, bool farthest, bool WithWalls){
            Checkers vector = to - from;
            List<Checkers> points = Checkers.Line(from, from + vector * 10);

            if(WithWalls) points.RemoveAll(a=>Checkers.Distance(from, a) >= MaxDistance | Checkers.Distance(from, a) <= MinDistance | Checkers.Distance(from, a) > Checkers.Distance(from, ToPoint(from.Up(0.6f), from + vector * 10)));
            else points.RemoveAll(a=>Checkers.Distance(from, a) >= MaxDistance | Checkers.Distance(from, a) <= MinDistance);

            try{
                if(farthest) return points[points.Count - 1];
                else {
                    Checkers nearestPoint = new Checkers(20000, 20000);

                    foreach(Checkers checker in points) {
                        if(Checkers.Distance(to, checker) < Checkers.Distance(to, nearestPoint))
                            nearestPoint = checker;
                    }
                    return nearestPoint;
                }
            }
            catch
            {
                if(WithWalls) return ToPoint(from, to);
                else return to;
            }
        }
    }
    protected static Checkers ToPoint(Vector3 from, Vector3 to, float Up = 0.6f)
    {
        if(Physics.Raycast(from, to - from, out RaycastHit hit, Vector3.Distance(from, to), LayerMask.GetMask("Object", "Map")))
            return new Checkers(hit.point, Up); 
        return new Checkers(to, Up); 
    }

    protected static int Damage(DamageType damageType, float DamagePercent, IAttacker Target)
    {
        switch(damageType)
        {
            default: return 0;
            case DamageType.Melee: return (int)Mathf.Round(Target.Strength * DamagePercent);
            case DamageType.Range: return (int)Mathf.Round(Target.DamageRange * DamagePercent);
            case DamageType.Rezo: return (int)Mathf.Round(Target.RezoOverclocking * DamagePercent);
            case DamageType.Pure: return (int)Mathf.Round(Target.DamagePure * DamagePercent);

            case DamageType.Heal: return (int)Mathf.Round(Target.Healing * DamagePercent);
            case DamageType.Repair: return (int)Mathf.Round(Target.Repairing * DamagePercent);
        }   
    }
    protected static int DamageDistanceScaling(float Distance, DamageScaling Scaling, float ScalingPower)
    {
        switch(Scaling)
        {
            default: return 0;
            case DamageScaling.Constant: goto default;
            case DamageScaling.Descending: return -(int)Mathf.Round(((Distance / 5) * ScalingPower) + 2);
            case DamageScaling.Addition: return (int)Mathf.Round(((-Distance / 5) * ScalingPower) - 2);
        }
    }
}

[Serializable] public struct Sphere : AttacksPlacer
{
    [SerializeField, Range(0, 8)] public int StartDistance;
    [SerializeField, Range(1, 20)] public int AdditionDistance;
    [SerializeField] TargetPointGuidanceType TargetingType;
    [field: Space]
    [field: SerializeField] public float DamagePercent { get; set; }
    [field: SerializeField] public DamageScaling Scaling { get; set; }
    [SerializeField, Range(0, 5)] float ScalingPower;
    [field: SerializeField] public DamageType DamageType { get; set; }
    [Space]
    [SerializeField, Range(0, 5)] int StartRadius;
    [SerializeField, Range(1, 6)] int PlusRadius;
    [SerializeField] bool WallPiercing;
    [field: Space]
    [field: SerializeReference, SubclassSelector] List<Effect> Effects { get; set; } 
    
    [field: Space, SerializeField] public bool Override{ get; set; }

    public async IAsyncEnumerable<Attack> GetAttackList(Checkers from, Checkers to, IAttacker Target)
    {
        await Task.Delay(0);
        Checkers FinalPoint = AttacksPlacer.PointTargeting(from, to, TargetingType, StartDistance + AdditionDistance, StartDistance); 

        int Radius = StartRadius + PlusRadius;
        for(int x = -Radius - 2; x <= Radius + 2; x++)
        for(int z = -Radius - 2; z <= Radius + 2; z++) 
        {
            Checkers NowChecking = FinalPoint + new Checkers(x, z);
            int Damage = (int)Mathf.Round((Radius / Checkers.Distance(NowChecking, FinalPoint)) * 
                                          (AttacksPlacer.Damage(DamageType, DamagePercent, Target) + 
                                           AttacksPlacer.DamageDistanceScaling(Checkers.Distance(from, NowChecking), Scaling, ScalingPower))); 
            
            if(!WallPiercing | Physics.Raycast(from.Up(0.3f), NowChecking - from, LayerMask.GetMask("Object", "Map")))
            if(Checkers.Distance(NowChecking, FinalPoint) + 1f > StartRadius)
            if(Checkers.Distance(NowChecking, FinalPoint) + 0.5f < Radius)
                yield return new Attack(Target, NowChecking, Damage, DamageType);
        }
    }
}
[Serializable] public struct Line : AttacksPlacer 
{
    [SerializeField, Range(0, 10)] int StartDistance;
    [SerializeField, Range(5, 20)] int Distance;
    [SerializeField] TargetPointGuidanceType TargetingType;
    [field: Space]
    [field: SerializeField] public float DamagePercent { get; set; }
    [field: SerializeField] public DamageScaling Scaling { get; set; }
    [SerializeField, Range(0, 5)] float ScalingPower;
    [field: SerializeField] public DamageType DamageType { get; set; }
    [field: Space]
    [field: SerializeReference, SubclassSelector] List<Effect> Effects { get; set; } 
    [field: Space]
    [field: SerializeField]public bool Override { get; set; }

    public async IAsyncEnumerable<Attack> GetAttackList(Checkers from, Checkers to, IAttacker Target)
    {
        Checkers FinalPoint = AttacksPlacer.PointTargeting(from, to, TargetingType, StartDistance + Distance, StartDistance); 
        
        List<Checkers> line = Checkers.Line(from, FinalPoint);

        foreach(Checkers NowChecking in line)
        {
            if(Checkers.Distance(NowChecking, from) > StartDistance) 
            if(Checkers.Distance(NowChecking, from) < StartDistance + Distance)
            if(NowChecking != from)
                yield return new Attack(Target, NowChecking, 
                                        AttacksPlacer.Damage(DamageType, DamagePercent, Target) + AttacksPlacer.DamageDistanceScaling(Checkers.Distance(NowChecking, from), Scaling, ScalingPower),
                                        DamageType, 
                                        Effects.ToArray());
        }
        await Task.Delay(0);
    }
}