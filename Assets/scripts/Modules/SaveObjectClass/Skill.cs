using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Threading.Tasks;
using UnityEditor;
using System.ComponentModel;
using System;

// old version
// [CreateAssetMenu(fileName = "BaseSkill", menuName = "SagardCL objects/Base Skill", order = 51)]
// public class Skill : Descript, Sendable
// {
//     [Space(2)]
//     [Header("Parameters")]
//     public bool PriorityAttacking;
    
//     public HitType HitBox;
//     [SerializeField]public DamageType damageType;
//     [Space]
//     [Range(0, 40)]public int Distance;
//     [Range(0, 20)]public int Damage;
//     public DamageScaling DamageScalingType;
//     [SerializeField]public bool Piercing;
//     [Range(0, 10)]public int AttackStartDistance;
//     public Effect[] Debuff;

//     [Range(0, 15)]public int Exploding;
//     public bool NoWalking;
//     public bool HitSelf;
//     [Range(0, 10)]public int SelfDamage;
//     [Space(3)]
//     [Header("Skill Price")]
//     public int UsingStamina;
//     [Space]
//     public IStateBar Ammo;
//     public bool DeleteWhenLowAmmo = false;

// }

[CreateAssetMenu(fileName = "BaseSkill", menuName = "SagardCL objects/Base Skill", order = 51)]
public class Skill : Descript, Sendable
{
    [Space(2)]
    [Header("Parameters")]
    public bool PriorityAttacking;
    public bool NoWalking;
    public int MaxCursorDistance;
    public List<HitBoxParameters> Realizations;

    [Space(3)]
    [Header("Skill Price")]
    public int UsingStamina;
    [Space]
    public IStateBar Ammo;
    public bool DeleteWhenLowAmmo = false;

    [Serializable]public class HitBoxParameters
    {
        [Header("Hit box")]
        public HitType HitBox;
        public TargetPointGuidanceType PointType;
        [Range(0, 40)]public int MaxDistance;
        [Range(0, 10)]public int MinDistance;
        [Header("Damage")]
        [Range(0, 20)]public int Damage;
        public DamageScaling DamageScalingType;
        public DamageType damageType;
        [Space]
        public Effect[] Debuff;
    }
    public bool isEmpty => Realizations.Count == 0;
    
}




[System.Serializable] public class SkillCombiner{
    
    //--------------------------------------------------------------------------------------- All Parameters ----------------------------------------------------------------------------------------------------------   
    [Header("Controllers")]  
    public UnitController Unit;
    [Space]
    public AllInOne From;
    public AllInOne To;

    [NonSerialized] public int SkillIndex = 0;
    [SerializeField] private List<Skill> AvailbleBaseSkills;
    public List<Skill> AdditionBaseSkills;
    public List<Skill> AvailbleSkills => FieldManipulate.CombineLists<Skill>( AvailbleBaseSkills, AdditionBaseSkills, Unit.AllItemStats.AdditionSkills );

    public Skill ThisSkill => AvailbleSkills[Mathf.Clamp(SkillIndex, 0, AvailbleSkills.Count - 1)];
    private Checkers startPos{ get{ return new Checkers(From.position, 0.8f); } set { From.position = value; } }
    private Checkers endPos => new Checkers(To.position, 0f);
    private Checkers FinalPoint(TargetPointGuidanceType PointType) {switch (PointType)
    {
        default: return new Checkers(From.position);
        case TargetPointGuidanceType.ToCursor: return endPos;
        case TargetPointGuidanceType.ToCursorWithWalls: return ToPoint(startPos, endPos);
        case TargetPointGuidanceType.ToFromPoint: return startPos;
    }}

    private int endPosRotate{ get { return (int)Mathf.Round(Quaternion.LookRotation(cursor.transform.InverseTransformPoint(From.position)).eulerAngles.y) / 90; } }

    private Checkers cursorPos{ get { return new Checkers(GameObject.Find("3DCursor").transform.position); }}
    private GameObject cursor{ get { return GameObject.Find("3DCursor"); }}
    //--------------------------------------------------------------------------------------- All Parameters ----------------------------------------------------------------------------------------------------------

    private Checkers ToPoint(Vector3 f, Vector3 t, float Up = 0)
    {
        if(Physics.Raycast(f, t - f, out RaycastHit hit, Vector3.Distance(f, t), LayerMask.GetMask("Object", "Map")))
            return new Checkers(hit.point, Up); 
        return t; 
    }

    public async IAsyncEnumerable<Attack> Realize()
    {
        if(Check()){
            await Task.Delay(0);

            foreach(Skill.HitBoxParameters NowHit in ThisSkill.Realizations)
            {
                int DamageScalingDmg(float Distance) 
                {
                    switch(NowHit.DamageScalingType)
                    {
                        default: return 0;
                        case DamageScaling.Descending: return (int)Mathf.Round(Distance / 5) - 3;
                        case DamageScaling.Addition: return (int)Mathf.Round(-Distance / 5) + 3;
                    }
                }

                Checkers FinalPoint = this.FinalPoint(NowHit.PointType);

                switch(NowHit.HitBox)
                {
                    default: yield break;
                    case HitType.Arc:{
                        for(int x = -NowHit.MaxDistance - 2; x <= NowHit.MaxDistance + 2; x++)
                        {
                            for(int z = -NowHit.MaxDistance - 2; z <= NowHit.MaxDistance + 2; z++)
                            {
                                Checkers NowChecking = FinalPoint + new Checkers(x, z);
                                float dist = Checkers.Distance(startPos,  NowChecking);

                                if(dist > Checkers.Distance(endPos, startPos))
                                    continue;
                                // if(Checkers.Distance(endPos, NowChecking) > Checkers.Distance(endPos, startPos))
                                //     continue;
                                if(dist < NowHit.MinDistance * ((Checkers.Distance(endPos, startPos) / 8f))) 
                                    continue;
                                if(Physics.Raycast(new Checkers(startPos, 0.2f), NowChecking - startPos, dist, LayerMask.GetMask("Map")))
                                    continue;

                                yield return(new Attack(Unit, 
                                NowChecking, 
                                NowHit.Damage - DamageScalingDmg(dist), 
                                NowHit.damageType));
                            }
                        }
                        break;
                    }
                    case HitType.Line:{
                        foreach(RaycastHit hit in Physics.RaycastAll(
                            new Checkers(startPos, -4), 
                            new Checkers(FinalPoint, -4) - new Checkers(startPos, -4f), 
                            Checkers.Distance(startPos, FinalPoint), 
                            LayerMask.GetMask("Map")))
                        {
                            Checkers NowChecking = new Checkers(hit.point);
                            if(Checkers.Distance(NowChecking, startPos) < NowHit.MinDistance) 
                                continue;
                            if(Checkers.Distance(NowChecking, startPos) > NowHit.MaxDistance)
                                continue;
                            yield return(new Attack(Unit, NowChecking, NowHit.Damage - DamageScalingDmg(Checkers.Distance(startPos, NowChecking)), NowHit.damageType));
                        }
                        yield return(new Attack(Unit, FinalPoint, NowHit.Damage, NowHit.damageType));

                        break;
                    }
                    case HitType.Sphere:{
                        for(int x = -NowHit.MaxDistance - 1; x < NowHit.MaxDistance + 1; x++)
                        {
                            for(int z = -NowHit.MaxDistance - 1; z <= NowHit.MaxDistance + 1; z++)
                            {
                                Checkers NowChecking = FinalPoint + new Checkers(x, z);
                                if(Checkers.Distance(NowChecking, startPos) < NowHit.MinDistance - 0.7f) 
                                    continue;
                                float dist = Checkers.Distance(FinalPoint, NowChecking);
                                if(dist > Mathf.Abs(NowHit.MaxDistance + 1) - 0.9f)
                                    continue;
                                if(Physics.Raycast(new Checkers(FinalPoint, 0.2f), NowChecking - FinalPoint, dist, LayerMask.GetMask("Map")))
                                    continue;

                                yield return new Attack(Unit, NowChecking, NowHit.Damage - DamageScalingDmg(dist), NowHit.damageType);
                            }
                        }
                        break;
                    }
                }
            }
        }
    }
    public void Graphics()
    {
        LineRenderer renderer = To.LineRenderer;
        
        // if(ThisSkill.HitBox == (HitType.Empty | HitType.OnSelfPoint | HitType.Arc | HitType.Constant)) 
        // { renderer.enabled = false; return; }
        
        // renderer.enabled = true;
        // renderer.endColor = (Check())? Color.red : Color.black;

        // renderer.positionCount = 2;

        // renderer.SetPositions(new Vector3[] { startPos, FinalPoint(FirstRealization.PointType) });
    }
    public bool Check(){ 
        //if(NowUsing.Type == (HitType.OnSelfPoint)) return true;
        if(ThisSkill.isEmpty) return false;
        return Checkers.Distance(startPos, endPos) < ThisSkill.MaxCursorDistance & !(startPos == endPos); } 

    public int StaminaWaste()
    {
        return ThisSkill.UsingStamina;

    }





}






public class SkillBuff
{
    [Header("Buff by ")]
    public DamageType ByDamageType;
    public HitType ByHitType;
}