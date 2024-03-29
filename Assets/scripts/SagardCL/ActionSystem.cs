using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using SagardCL.MapObjectInfo;
using System.Threading.Tasks;
using UnityEditor;
using System.ComponentModel;
using System;
using System.Linq;

namespace SagardCL.Actions{

    [Serializable] public class Action
    {
        public static Action Empty(){
            return new Action()
            {
                Name = "Empty",
                Description = "Empty Skill Description",
                NoWalk = false,
                Realizations = new List<Act>(),
            };}
        
        [Space, Header("Description")]
        [SerializeField]public string Name;
        [SerializeField]public string Description;
        [SerializeField]public string BigDescription;
        [SerializeField]public Sprite image;

        [Space(2)]
        [Header("Parameters")]
        public bool NoWalk;

        [SerializeReference, SubclassSelector] List<Act> Realizations;
        [SerializeReference, SubclassSelector] List<ActionCheck> ActionChecks;

        public string Exception;

        public bool Check(CharacterCore target)
        {
            Exception = "";
            foreach(ActionCheck check in ActionChecks) 
                if(!check.Check(target))
                    Exception+=$" - {check.CheckFalseReasons}\n";
            
            return Exception == "";
        }
        
        public void Complete(CharacterCore target)
        {
            foreach(Act act in Realizations)
                act.Completing(target);
        }
        public Action Plan(CharacterCore target)
        {
            foreach(Act act in Realizations)
                act.Planing(target);
            return this;
        }

        #region // Action Check

            public interface ActionCheck
            {
                bool Check(CharacterCore target);
                string CheckFalseReasons { get; }
            }

            [Serializable] public struct LowBaseParameter : ActionCheck
            {
                enum BaseParameterName
                {
                    Health,
                    Stamina,
                    Sanity,
                }
                [SerializeField] BaseParameterName baseParameterName;
                [SerializeField, Range(0, 10)] int WasteCount;
                public bool Check(CharacterCore target)
                {
                    switch(baseParameterName)
                    {
                        default: return false;
                        case BaseParameterName.Health: return target.NowBalance.Health.Value - WasteCount > 0;
                        case BaseParameterName.Stamina: return target.NowBalance.Stamina.Value - WasteCount > 0;
                        case BaseParameterName.Sanity: return target.NowBalance.Sanity.Value - WasteCount > 0;
                    }
                }
                public string CheckFalseReasons { get { return $"You have not enough {baseParameterName.ToString()}"; } }
            }

        #endregion
        #region // Act
            public interface Act
            {
                void Planing(CharacterCore action);
                void Completing(CharacterCore action);
            }
            [Serializable]public class PlaceAttacksOnMap : Act
            {
                [SerializeReference, SubclassSelector] public List<AttacksPlacer> AttacksPlacers;
                Checkers PlanedTargetPoint;
                Checkers PlanedFromPoint;

                List<Attack> PlannedHitBox = new List<Attack>();

                public async void Planing(CharacterCore action) 
                {
                    PlanedTargetPoint = action.AttackTarget;
                    PlanedFromPoint = action.MoveTarget;

                    Session.Current.DrawAttack(PlannedHitBox = await GetAttacks(PlanedFromPoint, PlanedTargetPoint, action), action.nowPosition.layer, action);
                }

                public void Completing(CharacterCore action)
                {
                    Session.AttackTransporter.Invoke(action.nowPosition.layer, PlannedHitBox);
                    PlannedHitBox.Clear();
                    Session.Current.DrawAttack(PlannedHitBox, action.nowPosition.layer, action);
                }

                public async Task<List<Attack>> GetAttacks(Checkers from, Checkers to, CharacterCore target)
                {
                    List<Attack> attackList = new List<Attack>();
                    List<Checkers> Overrides = new List<Checkers>();

                    foreach(AttacksPlacer HitZone in AttacksPlacers) 
                        await foreach(Attack attack in HitZone.GetAttackList(from, to, target)){ 
                            if(!Overrides.Exists(a=>a==attack.position)) {
                                attackList.Add(attack); 
                                
                                if(HitZone.Override) 
                                    Overrides.Add(attack.position); } }

                    return attackList;
                }
            }
            [Serializable]public class DrainBaseParameter : Act
            {
                enum BaseParameterName
                {
                    Health,
                    Stamina,
                    Sanity,
                }
                [SerializeField] BaseParameterName baseParameterName;
                [SerializeField, Range(0, 20)] int HowMuch;

                public void Planing(CharacterCore action){
                }
                public void Completing(CharacterCore action){
                    switch(baseParameterName)
                    {
                        case BaseParameterName.Health: action.BaseBalance.Health.Value -= HowMuch; break;
                        case BaseParameterName.Sanity: action.BaseBalance.Sanity.Value -= HowMuch; break;
                        case BaseParameterName.Stamina: action.BaseBalance.Stamina.Value -= HowMuch; break;
                    }
                }
            }

        #endregion

        #region // Attack Zones

            public interface AttacksPlacer
            {
                IAsyncEnumerable<Attack> GetAttackList(Checkers from, Checkers to, CharacterCore Target);
                
                float DamagePercent { get; set; }
                DamageType DamageType { get; }

                bool Override { get; }

                protected static Checkers PointTargeting(Checkers from, Checkers to, TargetPointGuidanceType PointType, int MaxDistance, int MinDistance = 0) { 
                    switch (PointType)
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

                protected static int Damage(DamageType damageType, float DamagePercent, CharacterCore Target)
                {
                    switch(damageType)
                    {
                        default: return 0;
                        case DamageType.Melee: return (int)Mathf.Round(Target.NowBalance.Strength * DamagePercent);
                        case DamageType.Range: return (int)Mathf.Round(Target.NowBalance.DamageRange * DamagePercent);
                        case DamageType.Rezo: return (int)Mathf.Round(Target.NowBalance.RezoOverclocking * DamagePercent);
                        case DamageType.Pure: return (int)Mathf.Round(Target.NowBalance.DamagePure * DamagePercent);

                        case DamageType.Heal: return (int)Mathf.Round(Target.NowBalance.Healing * DamagePercent);
                        case DamageType.Repair: return (int)Mathf.Round(Target.NowBalance.Repairing * DamagePercent);
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

                public async IAsyncEnumerable<Attack> GetAttackList(Checkers from, Checkers to, CharacterCore Target)
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

                public async IAsyncEnumerable<Attack> GetAttackList(Checkers from, Checkers to, CharacterCore Target)
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

        #endregion
    }
}
