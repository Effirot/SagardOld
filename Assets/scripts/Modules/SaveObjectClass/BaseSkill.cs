using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Threading.Tasks;
using UnityEditor;

[CreateAssetMenu(fileName = "BaseSkill", menuName = "SagardCL objects/Base Skill", order = 51)]
public class BaseSkill : Descript
{
    [Space(2)]
    [Header("Parameters")]
    public bool PriorityAttacking;
    public HitType Type;
    [SerializeField]public DamageType damageType;
    [SerializeField]public DamageType secondaryDamageType;
    [Space]
    [Range(0, 40)]public int Distance;
    [Range(0, 20)]public int Damage;
    [SerializeField]public bool Piercing;
    [Range(0, 15)]public int Exploding;
    [Range(0, 10)]public int AttackStartDistance;
    
    public Effect[] Debuff;
    public bool NoWalking;
    public bool HitSelf;
    [Range(0, 10)]public int SelfDamage;
    [Space(3)]
    [Header("Skill Price")]
    public int UsingStamina;
    [Space]
    public StateBar Ammo;
    public bool DeleteWhenLowAmmo = false;
}

[System.Serializable]
public class Skill{
    
    //--------------------------------------------------------------------------------------- All Parameters ----------------------------------------------------------------------------------------------------------
    [Header("Controllers")]  
    public UnitController Unit;
    [Space]
    public AllInOne From;
    public AllInOne To;

    public int SkillIndex = 0;
    [SerializeField] private List<BaseSkill> AvailbleBaseSkills;
    public List<BaseSkill> AvailbleSkills => AvailbleBaseSkills;

    public BaseSkill NowUsing => AvailbleSkills[Mathf.Clamp(SkillIndex, 0, AvailbleSkills.Count - 1)];

    private Checkers startPos{ get{ return new Checkers(From.position, 0.8f); } set { From.position = value; } }
    private Checkers endPos{ get{ return new Checkers(To.position, 0f); } }
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
            Checkers FinalPoint = (NowUsing.Piercing)? endPos : ToPoint(startPos, endPos);
            switch(NowUsing.Type)
            {
                default: yield break;
                case HitType.OnSelfPoint:
                {
                    FinalPoint = startPos;
                    break;
                }
                case HitType.Arc:
                {
                    NowUsing.Exploding = 0;
                    FinalPoint = cursorPos - startPos;
                    
                    for(int x = -NowUsing.Distance; x < NowUsing.Distance; x++)
                    {
                        for(int z = -NowUsing.Distance; z <= NowUsing.Distance; z++)
                        {
                            if(Checkers.Distance(startPos, startPos + new Checkers(x, z)) > Mathf.Abs(NowUsing.Distance / 2) - 0.8f)
                                continue;
                            if(Checkers.Distance(cursorPos, startPos + new Checkers(x, z)) > Checkers.Distance(cursorPos, startPos))
                                continue;
                            if(startPos == startPos + new Checkers(x, z))
                                continue;
                            if(Checkers.Distance(startPos + new Checkers(x, z), startPos) < NowUsing.AttackStartDistance + 0.5f + ((Checkers.Distance(cursorPos, startPos) / 3) - 3f)) 
                                continue;
                            if(!NowUsing.Piercing & Physics.Raycast(new Checkers(startPos, 0.2f), startPos + new Checkers(x, z) - startPos, Checkers.Distance(startPos, startPos + new Checkers(x, z)), LayerMask.GetMask("Map")))
                                continue;

                            yield return(new Attack(Unit, 
                            new Checkers(startPos + new Checkers(x, z)), 
                            (int)Mathf.Round(NowUsing.Damage + (Checkers.Distance(startPos, startPos + new Checkers(x, z), Checkers.mode.Height) * 0.5f)), 
                            NowUsing.damageType));
                        }
                    }
                    break;
                }
                case HitType.Line:
                {
                    
                    foreach(RaycastHit hit in Physics.RaycastAll(
                        new Checkers(startPos, -4), 
                        new Checkers(FinalPoint, -4) - new Checkers(startPos, -4f), 
                        Checkers.Distance(startPos, FinalPoint), 
                        LayerMask.GetMask("Map")))
                    {
                        if(Checkers.Distance(new Checkers(hit.point), startPos) < NowUsing.AttackStartDistance) continue;
                        yield return(new Attack(Unit, new Checkers(hit.point), (int)Mathf.Round(NowUsing.Damage / (Checkers.Distance(startPos, new Checkers(hit.point)) * 0.08f)), NowUsing.damageType));
                    }
                    yield return(new Attack(Unit, FinalPoint, NowUsing.Damage, NowUsing.damageType));

                    break;
                }
                case HitType.InvertLine:
                {
                    foreach(RaycastHit hit in Physics.RaycastAll(
                        new Checkers(startPos, -4), 
                        new Checkers(FinalPoint, -4) - new Checkers(startPos, -4f), 
                        Checkers.Distance(startPos, FinalPoint), 
                        LayerMask.GetMask("Map")))
                    {
                        if(Checkers.Distance(new Checkers(hit.point), startPos) < NowUsing.AttackStartDistance) continue;
                        yield return(new Attack(Unit, new Checkers(hit.point), (int)Mathf.Round(NowUsing.Damage * (Checkers.Distance(startPos, new Checkers(hit.point)) / 4)), NowUsing.damageType));
                    } 
                    yield return(new Attack(Unit, FinalPoint, 1 + NowUsing.Damage + (int)Mathf.Round(NowUsing.Damage * (Checkers.Distance(startPos, FinalPoint) / 4)), NowUsing.damageType));
                    
                    break;
                }
                case HitType.ConstantLine:
                {
                    foreach(RaycastHit hit in Physics.RaycastAll(
                        new Checkers(startPos, -4), 
                        new Checkers(FinalPoint, -4) - new Checkers(startPos, -4f), 
                        Checkers.Distance(startPos, FinalPoint), 
                        LayerMask.GetMask("Map")))
                    {
                        if(Checkers.Distance(new Checkers(hit.point), startPos) < NowUsing.AttackStartDistance) continue;
                        yield return(new Attack(Unit, new Checkers(hit.point), NowUsing.Damage, NowUsing.damageType));
                    } 
                    yield return(new Attack(Unit, FinalPoint, 1 + NowUsing.Damage, NowUsing.damageType));
                    
                    break;
                }
                case HitType.Point:
                {
                    yield return(new Attack(Unit, FinalPoint, NowUsing.Damage, NowUsing.damageType));
                    
                    break;
                }
            }

            if(NowUsing.HitSelf) yield return(new Attack(Unit, new Checkers(startPos), NowUsing.SelfDamage, DamageType.Pure));
            if(NowUsing.Exploding == 0) yield break;
            
            // --------------------Explode------------------
            for(int x = -NowUsing.Exploding - 1; x < NowUsing.Exploding + 1; x++)
            {
                for(int z = -NowUsing.Exploding - 1; z <= NowUsing.Exploding + 1; z++)
                {
                    if(Checkers.Distance(FinalPoint + new Checkers(x, z), startPos) < NowUsing.AttackStartDistance - 0.7f & NowUsing.Type == HitType.OnSelfPoint) 
                        continue;
                    if(FinalPoint == FinalPoint + new Checkers(x, z))
                        continue;
                    if(Checkers.Distance(FinalPoint, FinalPoint + new Checkers(x, z)) > Mathf.Abs(NowUsing.Exploding + 1) - 0.9f)
                        continue;
                    if(Physics.Raycast(new Checkers(FinalPoint, 0.2f), FinalPoint + new Checkers(x, z) - FinalPoint, Checkers.Distance(FinalPoint, FinalPoint + new Checkers(x, z)), LayerMask.GetMask("Map")))
                        continue;


                    yield return(new Attack(Unit, 
                    new Checkers(FinalPoint + new Checkers(x, z)), 
                    (int)Mathf.Round(NowUsing.Damage / (Checkers.Distance(FinalPoint, FinalPoint + new Checkers(x, z), Checkers.mode.Height) * 0.5f)), 
                    NowUsing.damageType));
                }
            }
        }
        yield break;
    }
    public void Graphics()
    {
        LineRenderer renderer = To.LineRenderer;
        
        if(NowUsing.Type == (HitType.Empty | HitType.OnSelfPoint | HitType.Arc | HitType.Constant)) 
        { renderer.enabled = false; return; }
        
        renderer.enabled = true;
        renderer.endColor = (Check())? Color.red : Color.black;

        Checkers FinalPoint = (NowUsing.Piercing)? endPos : ToPoint(startPos, endPos);

        renderer.positionCount = 2;
        
        List<Vector3> points = new List<Vector3>();
        foreach(Vector3 point in EvaluateSlerpPoints(startPos, FinalPoint, CenterOfPoints(startPos,FinalPoint)))
        {
            points.Add(point);
        }

        renderer.SetPositions(points.ToArray());
        
    }
    public bool Check(){ 
        if(NowUsing.Type == (HitType.OnSelfPoint)) return true;
        return Checkers.Distance(startPos, endPos) < NowUsing.Distance & 
               !(startPos == endPos) & 
               (NowUsing.Type != (HitType.Arc))? Checkers.Distance(startPos, endPos) > NowUsing.AttackStartDistance - 0.7f : true; }

    public int StaminaWaste()
    {
        switch(NowUsing.Type)
        {
            default: return NowUsing.UsingStamina;
            // case HitType.Arc: return ;
        }
    }


    public Vector3 CenterOfPoints(Vector3 a, Vector3 b)
    {
        var Sqrt = Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow( a.y - b.y, 2) + Mathf.Pow(a.z - b.z, 2));

        var x = a.x + (b.x - a.x) * 3 /  Sqrt;
        var y = a.y + (b.y - a.y) * 3 /  Sqrt;
        var z = a.z + (b.z - a.z) * 3 /  Sqrt;

        return new Vector3(x, y, z);
    }

    IEnumerable<Vector3> EvaluateSlerpPoints(Vector3 start, Vector3 end, Vector3 center, int count = 10) {
        
        var startRelativeCenter = start - center;
        var endRelativeCenter = end - center;

        var f = 1f / count;

        for (var i = 0f; i < 1 + f; i += f) {
            yield return Vector3.Slerp(startRelativeCenter, endRelativeCenter, i) + center;
        }
    }
}

public class SkillBuff
{
    [Header("Buff by ")]
    public DamageType ByDamageType;
    public HitType ByHitType;
}