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
    public List<HitBoxParameters> Realizations;

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


[System.Serializable] public class SkillCombiner{
    
    //--------------------------------------------------------------------------------------- All Parameters ----------------------------------------------------------------------------------------------------------   
    [Header("Controllers")]  
    public CharacterCore Target;
    [Space]
    public AllInOne From;
    public AllInOne To;

    [NonSerialized] public int SkillIndex = 0;
    [SerializeField] private List<Skill> AvailbleBaseSkills;
    public List<Skill> AvailbleSkills => FieldManipulate.CombineLists<Skill>( AvailbleBaseSkills, Target.AllBalanceChanges.Skills );

    public Skill ThisSkill { get{ try { return AvailbleSkills[Mathf.Clamp(SkillIndex, 0, AvailbleSkills.Count - 1)]; } catch { return new Skill(); } } }
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

    
    public async Task<List<Attack>> Realize()
    {
        List<Attack> attackList = new List<Attack>();
        await foreach(Attack attack in _Realize()) attackList.Add(attack);
        return attackList;

        async IAsyncEnumerable<Attack> _Realize()
        {
            if(!Check()) yield break;
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
                int Damage()
                {
                    switch(NowHit.DamageType)
                    {
                        default: return 0;
                        case DamageType.Melee: return (int)Mathf.Round(Target.Strength * NowHit.DamagePercent);
                        case DamageType.Range: return (int)Mathf.Round(Target.DamageRange * NowHit.DamagePercent);
                        case DamageType.Rezo: return (int)Mathf.Round(Target.RezoOverclocking * NowHit.DamagePercent);
                        case DamageType.Pure: return (int)Mathf.Round(Target.DamagePure * NowHit.DamagePercent);

                        case DamageType.Heal: return (int)Mathf.Round(Target.Healing * NowHit.DamagePercent);
                        case DamageType.Repair: return (int)Mathf.Round(Target.Repairing * NowHit.DamagePercent);
                    }   
                }

                Checkers FinalPoint = this.FinalPoint(NowHit.PointType);

                switch(NowHit.HitBox)
                {
                    default: continue;
                    case HitType.Arc:{
                        for(int x = -NowHit.MaxDistance - 2; x <= NowHit.MaxDistance + 2; x++)
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

                            yield return(new Attack(Target, 
                            NowChecking, 
                            Damage() - DamageScalingDmg(dist), 
                            NowHit.DamageType,
                            NowHit.Debuff));
                            
                        }
                        break;
                    }
                    case HitType.Line:{
                        foreach(Checkers NowChecking in Checkers.Line(startPos, FinalPoint))
                        {
                            if(Checkers.Distance(NowChecking, startPos) < NowHit.MinDistance) 
                                continue;
                            if(Checkers.Distance(NowChecking, startPos) > NowHit.MaxDistance)
                                continue;
                            if(NowChecking != startPos)
                                yield return(new Attack(Target, NowChecking, Damage() - DamageScalingDmg(Checkers.Distance(startPos, NowChecking)), NowHit.DamageType, NowHit.Debuff));
                        }
                        yield return(new Attack(Target, FinalPoint, Damage(), NowHit.DamageType, NowHit.Debuff));

                        break;
                    }
                    case HitType.Sphere:{
                        for(int x = -NowHit.MaxDistance - 1; x < NowHit.MaxDistance + 1; x++)
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

                            yield return new Attack(Target, NowChecking, Damage() - DamageScalingDmg(dist), NowHit.DamageType, NowHit.Debuff);
                            
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