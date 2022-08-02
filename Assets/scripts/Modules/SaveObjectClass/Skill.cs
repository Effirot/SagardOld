using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using SagardCL.ParameterManipulate;
using System.Threading.Tasks;
using UnityEditor;
using System.ComponentModel;
using System;

[Serializable]public struct Skill
{
    [Space, Header("Description")]
    public string Name;
    public string Description;
    public string BigDescription;
    public Sprite image;

    [Space(2)]
    [Header("Parameters")]
    public bool NoWalking;
    public int MaxCursorDistance;
    [SerializeReference, SubclassSelector] public List<ZonePlacer> Realizations;

    [Space(3)]
    [Header("Skill Price")]
    public int UsingStamina;

    [Serializable]public struct HitBoxParameters
    {
        [Header("Hit box")]
        public HitType HitBox;
        public TargetPointGuidanceType PointType;
        [Range(0, 40)]public int MaxDistance;
        [Range(0, 10)]public int MinDistance;
        [Header("Damage")]
        [Range(0.1f, 5f)]public float DamagePercent;
        public DamageScaling DamageScalingType;
        public DamageType DamageType;
        [Space]
        [SerializeReference, SubclassSelector] public Effect[] Debuff;
    }
    public bool isEmpty => Realizations != null ? Realizations.Count == 0 : true;
}

public interface ZonePlacer
{
    IAsyncEnumerable<Attack> GetAttackList(Checkers from, Checkers to, IAttacker Target);
    
    float DamagePercent { get; set; }
    DamageType DamageType { get; }

    bool Override { get; }

    

    protected static Checkers PointTargeting(Checkers from, Checkers to, TargetPointGuidanceType PointType) {switch (PointType)
    {
        default: return from;
        case TargetPointGuidanceType.ToCursor: return to;
        case TargetPointGuidanceType.ToCursorWithWalls: return ToPoint(from, to);
        case TargetPointGuidanceType.ToFromPoint: return from;
    }}
    protected static Checkers ToPoint(Vector3 from, Vector3 to, float Up = 0.4f)
    {
        if(Physics.Raycast(from, to - from, out RaycastHit hit, Vector3.Distance(from, to), LayerMask.GetMask("Object", "Map")))
            return new Checkers(hit.point, Up); 
        return to; 
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

[Serializable] public struct Sphere : ZonePlacer
{
    [field: SerializeField] public float DamagePercent { get; set; }
    [field: SerializeField] public DamageScaling Scaling { get; set; }
    [SerializeField, Range(0, 5)] float ScalingPower;
    [field: SerializeField] public DamageType DamageType { get; set; }
    [Space]
    [SerializeField] TargetPointGuidanceType TargetingType;
    [SerializeField, Range(0, 5)] int StartRadius;
    [SerializeField, Range(1, 6)] int PlusRadius;
    
    [field: Space, SerializeField] public bool Override{ get; set; }

    public async IAsyncEnumerable<Attack> GetAttackList(Checkers from, Checkers to, IAttacker Target)
    {
        await Task.Delay(0);
        Checkers FinalPoint = ZonePlacer.PointTargeting(from, to, TargetingType); 

        int Radius = StartRadius + PlusRadius;
        for(int x = -Radius - 2; x <= Radius + 2; x++)
        for(int z = -Radius - 2; z <= Radius + 2; z++) 
        {
            Checkers NowChecking = FinalPoint + new Checkers(x, z);
            int Damage = (int)Mathf.Round((Radius / Checkers.Distance(NowChecking, FinalPoint)) * 
            (ZonePlacer.Damage(DamageType, DamagePercent, Target) + DamageScaling(Checkers.Distance()))); 
            
            if(Checkers.Distance(NowChecking, FinalPoint) + 1f > StartRadius)
            if(Checkers.Distance(NowChecking, FinalPoint) + 0.5f < Radius)
                yield return new Attack(Target, NowChecking, Damage, DamageType);
        }
    }
}
[Serializable] public struct Line : ZonePlacer 
{
    [field: SerializeField]public float DamagePercent { get; set; }
    [field: SerializeField] public DamageScaling Scaling { get; set; }
    [SerializeField, Range(0, 5)] float ScalingPower;
    [field: SerializeField]public DamageType DamageType { get; }
    [Space]
    [SerializeField] TargetPointGuidanceType TargetingType;
    [Space]
    [SerializeField, Range(0, 10)] int MinDistance;
    [SerializeField, Range(5, 20)] int MaxDistance;

    [field: SerializeField]public bool Override { get; set; }


    public async IAsyncEnumerable<Attack> GetAttackList(Checkers from, Checkers to, IAttacker Target)
    {
        Checkers FinalPoint = ZonePlacer.PointTargeting(from, to, TargetingType); 

        foreach(Checkers NowChecking in Checkers.Line(from, FinalPoint))
        {
            if(Checkers.Distance(NowChecking, from) > MinDistance) 
            if(Checkers.Distance(NowChecking, from) < MinDistance + MaxDistance)
            if(NowChecking != from)
                yield return(new Attack(Target, NowChecking, Damage() - DamageScalingDmg(Checkers.Distance(startPos, NowChecking)), NowHit.DamageType, NowHit.Debuff));
        }
        await Task.Delay(0);
    }


    
}













    // switch(NowHit.HitBox)
    // {
    //     default: continue;
    //     case HitType.Arc:{
    //         for(int x = -NowHit.MaxDistance - 2; x <= NowHit.MaxDistance + 2; x++)
    //         for(int z = -NowHit.MaxDistance - 2; z <= NowHit.MaxDistance + 2; z++)
    //         {
    //             Checkers NowChecking = FinalPoint + new Checkers(x, z);
    //             float dist = Checkers.Distance(startPos,  NowChecking);

    //             if(dist > Checkers.Distance(endPos, startPos))
    //                 continue;
    //             // if(Checkers.Distance(endPos, NowChecking) > Checkers.Distance(endPos, startPos))
    //             //     continue;
    //             if(dist < NowHit.MinDistance * ((Checkers.Distance(endPos, startPos) / 8f))) 
    //                 continue;
    //             if(Physics.Raycast(new Checkers(startPos, 0.2f), NowChecking - startPos, dist, LayerMask.GetMask("Map")))
    //                 continue;

    //             yield return(new Attack(Target, 
    //             NowChecking, 
    //             Damage() - DamageScalingDmg(dist), 
    //             NowHit.DamageType,
    //             NowHit.Debuff));
                
    //         }
    //         break;
    //     }
    //     case HitType.Line:{
    //         foreach(Checkers NowChecking in Checkers.Line(startPos, FinalPoint))
    //         {
    //             if(Checkers.Distance(NowChecking, startPos) < NowHit.MinDistance) 
    //                 continue;
    //             if(Checkers.Distance(NowChecking, startPos) > NowHit.MaxDistance)
    //                 continue;
    //             if(NowChecking != startPos)
    //                 yield return(new Attack(Target, NowChecking, Damage() - DamageScalingDmg(Checkers.Distance(startPos, NowChecking)), NowHit.DamageType, NowHit.Debuff));
    //         }
    //         yield return(new Attack(Target, FinalPoint, Damage(), NowHit.DamageType, NowHit.Debuff));

    //         break;
    //     }
    //     case HitType.Sphere:{
    //         for(int x = -NowHit.MaxDistance - 1; x < NowHit.MaxDistance + 1; x++)
    //         for(int z = -NowHit.MaxDistance - 1; z <= NowHit.MaxDistance + 1; z++)
    //         {
    //             Checkers NowChecking = FinalPoint + new Checkers(x, z);
    //             if(Checkers.Distance(NowChecking, startPos) < NowHit.MinDistance - 0.7f) 
    //                 continue;
    //             float dist = Checkers.Distance(FinalPoint, NowChecking);
    //             if(dist > Mathf.Abs(NowHit.MaxDistance + 1) - 0.9f)
    //                 continue;
    //             if(Physics.Raycast(new Checkers(FinalPoint, 0.2f), NowChecking - FinalPoint, dist, LayerMask.GetMask("Map")))
    //                 continue;

    //             yield return new Attack(Target, NowChecking, Damage() - DamageScalingDmg(dist), NowHit.DamageType, NowHit.Debuff);
                
    //         }
    //         break;
    //     }
    // }

    
    
    // public bool Check(){ 
    //     //if(NowUsing.Type == (HitType.OnSelfPoint)) return true;
    //     if(ThisSkill.isEmpty) return false;
    //     return Checkers.Distance(startPos, endPos) < ThisSkill.MaxCursorDistance & !(startPos == endPos); } 
