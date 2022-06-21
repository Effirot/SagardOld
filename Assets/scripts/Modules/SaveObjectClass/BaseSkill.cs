using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Threading.Tasks;


[CreateAssetMenu(fileName = "BaseSkill", menuName = "SagardCL objects/Base Skill", order = 51)]
public class BaseSkill : Descript
{
    [Space(2)]
    [Header("Parameters")]
    public HitType Type;
    [SerializeField]public DamageType damageType;
    [SerializeField]public DamageType secondaryDamageType;
    [Space]
    [Range(0, 40)]public int Distance;
    [Range(0, 20)]public int Damage;
    [SerializeField]public bool Piercing;
    [Range(0, 15)]public int Exploding;
    
    public Effect[] Debuff;
    public bool NoWalking;
    public bool HitSelf;

    public int UsingStamina;

    public bool DeleteWhenLowAmmo = false;
}

[System.Serializable]
public class Skill{
    
    //--------------------------------------------------------------------------------------- All Parameters ----------------------------------------------------------------------------------------------------------
    [Header("Controllers")]  
    public AllInOne From;
    GameObject FatherObj{ get{ return From.Planer.transform.parent.gameObject; } }
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
                case HitType.OnSelf:
                {
                    FinalPoint = startPos;
                    break;
                }
                case HitType.SwordSwing:
                {
                    

                    break;
                }
                case HitType.Shot:
                {
                    foreach(RaycastHit hit in Physics.RaycastAll(
                        new Checkers(startPos, -4), 
                        new Checkers(FinalPoint, -4) - new Checkers(startPos, -4f), 
                        Checkers.Distance(startPos, FinalPoint), 
                        LayerMask.GetMask("Map")))
                    {
                        
                        yield return(new Attack(FatherObj, new Checkers(hit.point), (int)Mathf.Round(NowUsing.Damage / (Checkers.Distance(startPos, new Checkers(hit.point)) * 0.08f)), NowUsing.damageType));
                    }
                    yield return(new Attack(FatherObj, FinalPoint, NowUsing.Damage, NowUsing.damageType));

                    break;
                }
                case HitType.InvertShot:
                {
                    foreach(RaycastHit hit in Physics.RaycastAll(
                        new Checkers(startPos, -4), 
                        new Checkers(FinalPoint, -4) - new Checkers(startPos, -4f), 
                        Checkers.Distance(startPos, FinalPoint), 
                        LayerMask.GetMask("Map")))
                    {
                        yield return(new Attack(FatherObj, new Checkers(hit.point), (int)Mathf.Round(NowUsing.Damage * (Checkers.Distance(startPos, new Checkers(hit.point)) / 4)), NowUsing.damageType));
                    } 
                    yield return(new Attack(FatherObj, FinalPoint, 1 + NowUsing.Damage + (int)Mathf.Round(NowUsing.Damage * (Checkers.Distance(startPos, FinalPoint) / 4)), NowUsing.damageType));
                    
                    break;
                }
                case HitType.ConstantShot:
                {
                    foreach(RaycastHit hit in Physics.RaycastAll(
                        new Checkers(startPos, -4), 
                        new Checkers(FinalPoint, -4) - new Checkers(startPos, -4f), 
                        Checkers.Distance(startPos, FinalPoint), 
                        LayerMask.GetMask("Map")))
                    {
                        yield return(new Attack(FatherObj, new Checkers(hit.point), NowUsing.Damage, NowUsing.damageType));
                    } 
                    yield return(new Attack(FatherObj, FinalPoint, 1 + NowUsing.Damage, NowUsing.damageType));
                    
                    break;
                }
                case HitType.Volley:
                {
                    yield return(new Attack(FatherObj, FinalPoint, NowUsing.Damage, NowUsing.damageType));
                    
                    break;
                }
            }

            if(NowUsing.HitSelf) yield return(new Attack(FatherObj, new Checkers(startPos), NowUsing.Damage, NowUsing.damageType));
            if(NowUsing.Exploding == 0) yield break;
            
            // --------------------Explode------------------
            for(int x = -NowUsing.Exploding - 1; x < NowUsing.Exploding + 1; x++)
            {
                for(int z = -NowUsing.Exploding - 1; z <= NowUsing.Exploding + 1; z++)
                {
                    if(FinalPoint == FinalPoint + new Checkers(x, z))
                        continue;
                    if(Checkers.Distance(FinalPoint, FinalPoint + new Checkers(x, z)) > Mathf.Abs(NowUsing.Exploding + 1) - 0.9f)
                        continue;
                    if(Physics.Raycast(new Checkers(FinalPoint, 0.2f), FinalPoint + new Checkers(x, z) - FinalPoint, Checkers.Distance(FinalPoint, FinalPoint + new Checkers(x, z)), LayerMask.GetMask("Map")))
                        continue;


                    yield return(new Attack(FatherObj, 
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
        
        if(NowUsing.Type == (HitType.Empty & HitType.OnSelf & HitType.SwordSwing & HitType.Constant)) 
        { renderer.enabled = false; return; }
        
        renderer.enabled = true;
        renderer.endColor = (Check())? Color.red : Color.grey;

        Checkers FinalPoint = (NowUsing.Piercing)? endPos : ToPoint(startPos, endPos);

        renderer.positionCount = 2;
        renderer.SetPositions(new Vector3[] {startPos, FinalPoint});
        
    }
    public bool Check(){ return Checkers.Distance(startPos, endPos) < NowUsing.Distance & !(startPos == endPos); }
}

public class SkillBuff
{
    [Header("Buff by ")]
    public DamageType ByDamageType;
    public HitType ByHitType;
}