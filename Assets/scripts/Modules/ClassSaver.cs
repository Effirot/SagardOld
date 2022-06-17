using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace SagardCL //Class library
{
    public class Descript
    {
        [Space, Header("Description")]
        public string Name;
        public string Description;
        public string BigDescription;
        public Texture2D image;
    }
    public class LifeParameters : Descript
    {
        [Space, Header("Base Parameters")]
        public Color Team;        
        [Space] // health parameters
        public int MaxHP;
        public int HP;
        public int ArmorMelee;
        public int ArmorRange;
        


        public void Damage(Attack attack)
        {
            switch(attack.damageType)
            {
                case DamageType.Pure: HP -= attack.damage; break;
                case DamageType.Melee: HP -= attack.damage - ArmorMelee; break;
                case DamageType.Range: HP -= attack.damage - ArmorRange; break;
                case DamageType.Rezo: HP -= attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f); break;
                case DamageType.Terra: HP -= attack.damage / 4; break;

                case DamageType.Sanity: Sanity -= attack.damage - SanityShield; break;

                case DamageType.Heal: HP = Mathf.Clamp(HP + attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.1f), 0, MaxHP); break;
            }

            if(attack.Debuff != null) { foreach(Effect effect in attack.Debuff) { if(Resists.Find((a) => a == effect) != effect) Debuff.Add(effect); } }
        }

        [Space] // sanity parameters
        public int MaxSanity;
        public int Sanity;
        public int SanityShield;

        [Space] // Stamina parameters
        public int MaxStamina;
        public  int Stamina;
        [SerializeField] int RestEffectivity;
        [SerializeField] int WalkUseStamina;
        
        [Space] // Debuff's parameters
        public List<Effect> Resists;
        public List<Effect> Debuff;

        public void Rest(){ if(RestEffectivity == 0){ Stamina = MaxStamina; return; } Stamina += Mathf.Clamp(Stamina + RestEffectivity * 2, 0, MaxStamina); } 
    }

    [System.Serializable]
    public class PlayerControl : LifeParameters
    {
        [Space, Header("Control Settings")]
        public bool CanControl = true;
        [Space]        

        public int WalkDistance;
        [Space]

        public List<Skill> AvailableSkills;

        public void AddSkill(Skill skill)
        {
            AvailableSkills.Add(skill);
        }
        public void RemoveSkill(Skill skill)
        {
            AvailableSkills.Remove(skill);
        }
        public void AddRangeSkill(List<Skill> skills)
        {
            AvailableSkills.AddRange(skills);
        }
    }

    public enum DamageType
    {
        Melee,
        Range,
        Rezo,
        Sanity,
        Terra,
        Pure,
        Heal,
    }        
    public enum HitType
    {
        Empty, //
        OnSelf, //
        SwordSwing, 
        Constant, 
        BraveSwordSwing, 
        Shot, //
        InvertShot, //
        ShotgunShot,
        Volley, // 
        Dash, 
    }

    [System.Serializable]
    public class Skill : Descript
    {
        //--------------------------------------------------------------------------------------- All Parameters ----------------------------------------------------------------------------------------------------------
        [Header("Contollers")]
        public AllInOne From;
        GameObject FatherObj{ get{ return From.Planer.transform.parent.gameObject; } }
        public AllInOne To;
        
        [Space(2)]
        [Header("Parameters")]
        public HitType Type;
        [SerializeField] DamageType damageType;
        [SerializeField] DamageType secondaryDamageType;
        [Space]
        [Range(0, 40)]public int Distance;
        [Range(0, 20)]public int Damage;
        [SerializeField] bool Piercing;
        [Range(0, 15)]public int Exploding;
        
        public Effect[] Debuff;
        public bool NoWalking;

        public object Other1;
        public object Other2;

        private Checkers startPos{ get{ return new Checkers(From.position, 0.8f); } set { From.position = value; } }
        private Checkers endPos{ get{ return new Checkers(To.position, 0f); } }
        private int endPosRotate{ get { return (int)Mathf.Round(Quaternion.LookRotation(cursor.transform.InverseTransformPoint(From.position)).eulerAngles.y) / 90; } }

        private Checkers cursorPos{ get { return new Checkers(GameObject.Find("3DCursor").transform.position); }}
        private GameObject cursor{ get { return GameObject.Find("3DCursor"); }}
        //--------------------------------------------------------------------------------------- All Parameters ----------------------------------------------------------------------------------------------------------

        public static Skill Empty() { return new Skill {From = null, Name = "null", Type = HitType.Empty, Distance = 0, Damage = 0}; }

        private Checkers ToPoint(Vector3 f, Vector3 t, float Up = 0)
        {
            if(Physics.Raycast(f, t - f, out RaycastHit hit, Vector3.Distance(f, t), LayerMask.GetMask("Object", "Map")))
                return new Checkers(hit.point, Up);
            return t; 
        }

        public async IAsyncEnumerable<Attack> DamageZone()
        {
            if(Check()){
                await Task.Delay(0);
                Checkers FinalPoint = (Piercing)? endPos : ToPoint(startPos, endPos);
                switch(Type)
                {
                    default: yield break;
                    case HitType.OnSelf:
                    {
                        FinalPoint = startPos;
                        yield return(new Attack(FatherObj, startPos, Damage, damageType));
                        break;
                    }
                    case HitType.SwordSwing:
                    {
                        

                        break;
                    }
                    case HitType.BraveSwordSwing:
                    {

                        break;
                    }
                    case HitType.Shot:
                    {
                        foreach(RaycastHit hit in Physics.RaycastAll(
                            new Checkers(startPos, -4), 
                            ToPoint(startPos, endPos, -4f) - new Checkers(startPos, -4f), 
                            Checkers.Distance(startPos, FinalPoint), 
                            LayerMask.GetMask("Map")))
                        {
                            
                            yield return(new Attack(FatherObj, new Checkers(hit.point), (int)Mathf.Round(Damage / (Checkers.Distance(startPos, new Checkers(hit.point)) * 0.08f)), damageType));
                        }
                        yield return(new Attack(FatherObj, FinalPoint, Damage, damageType));

                        break;
                    }
                    case HitType.InvertShot:
                    {
                        foreach(RaycastHit hit in Physics.RaycastAll(
                            new Checkers(startPos, -4), 
                            ToPoint(startPos, endPos, -4f) - new Checkers(startPos, -4f), 
                            Checkers.Distance(startPos, FinalPoint), 
                            LayerMask.GetMask("Map")))
                        {
                            yield return(new Attack(FatherObj, new Checkers(hit.point), (int)Mathf.Round(Damage * (Checkers.Distance(startPos, FinalPoint) / 4)), damageType));
                        } 
                        yield return(new Attack(FatherObj, FinalPoint, 1 + Damage + (int)Mathf.Round(Damage * (Checkers.Distance(startPos, FinalPoint) / 4)), damageType));
                        
                        break;
                    }
                    case HitType.Volley:
                    {
                        yield return(new Attack(FatherObj, FinalPoint, Damage, damageType));
                        
                        break;
                    }
                }
                if(Exploding == 0) yield break;
                
                // --------------------Explode------------------
                for(int x = -Mathf.Abs(Exploding); x < 1 + Mathf.Abs(Exploding); x++)
                {
                    for(int z = -Mathf.Abs(Exploding); z <= Mathf.Abs(Exploding); z++)
                    {
                        if(Checkers.Distance(FinalPoint, FinalPoint + new Checkers(x, z)) > Mathf.Abs(Exploding) - 0.9f)
                            continue;
                        if(Physics.Raycast(new Checkers(FinalPoint, 0.2f), FinalPoint + new Checkers(x, z) - FinalPoint, Checkers.Distance(FinalPoint, FinalPoint + new Checkers(x, z)), LayerMask.GetMask("Map")))
                            continue;


                        yield return(new Attack(FatherObj, 
                        new Checkers(FinalPoint + new Checkers(x, z)), 
                        (int)Mathf.Round(Damage / (Checkers.Distance(FinalPoint, FinalPoint + new Checkers(x, z), Checkers.mode.Height) * 0.5f)), 
                        damageType));
                    }
                }
                
               yield break;
            }
            yield return new Attack();
        }
        public void Graphics()
        {
            LineRenderer renderer = To.LineRenderer;
            
            if(Type == (HitType.Empty & HitType.OnSelf & HitType.SwordSwing & HitType.Constant)) 
            { renderer.enabled = false; return; }
            
            renderer.enabled = true;
            renderer.endColor = (Check())? Color.red : Color.grey;

            Checkers FinalPoint = (Piercing)? endPos : ToPoint(startPos, endPos);

            renderer.positionCount = 2;
            renderer.SetPositions(new Vector3[] {startPos, ToPoint(startPos, FinalPoint)});
            
        }
        public bool Check(){ return Checkers.Distance(startPos, endPos) < Distance & !(startPos == endPos); }
    }

    [System.Serializable]
    public class Item : Descript
    {
    }

    [System.Serializable]
    public class Effect : Descript
    {

    }

    [System.Serializable]
    public struct Attack
    {
        public GameObject WhoAttack;

        [SerializeField]Checkers WhereAttack;
        public Checkers Where { get { return WhereAttack; } }

        [SerializeField]int Damage;
        public int damage { get { return Damage; } }

        public DamageType damageType;
        public Effect[] Debuff;



        // Overloads
        public Attack(GameObject Who, Checkers Where, int Dam, DamageType Type, Effect[] debuff)
        {
            WhoAttack = Who;
            WhereAttack = Where;
            Damage = Dam;

            damageType = Type;
            Debuff = debuff;
        }
        public Attack(GameObject Who, Checkers Where, int Dam, DamageType Type, Effect debuff)
        {
            WhoAttack = Who;
            WhereAttack = Where;
            Damage = Dam;

            damageType = Type;
            Debuff = new Effect[] { debuff };
        }
        public Attack(GameObject Who, Checkers Where, int Dam, DamageType Type)
        {
            WhoAttack = Who;
            WhereAttack = Where;
            Damage = Dam;

            damageType = Type;
            Debuff = new Effect[] { };
        }

        public override string ToString()
        { 
            string effects = "";
            foreach(Effect effect in Debuff)
            {
                effects += effect.Name + ", ";
            }
            return "Attack from " + WhoAttack?.transform.parent.name + 
                    " to " + WhereAttack.x + 
                    ":" + WhereAttack.z + 
                    " (" + damageType.ToString() +
                    " - " + Damage + ")" +
                    effects; }
    
    }
    
    [System.Serializable]
    public class AllInOne
    {
        public GameObject Planer;
        public GameObject Model;
        public AllInOne(GameObject planer) { Planer = planer; if(Model == null) Model = Planer.transform.Find("Model").gameObject; }
        



        public Vector3 position{ get{ return Planer.transform.position; } set{ Planer.transform.position = value; } }
        public Vector3 localPosition{ get{ return Planer.transform.localPosition; } set{ Planer.transform.localPosition = value; } }
        public Transform Parent => Planer.transform.parent;

        public static implicit operator GameObject(AllInOne a) { return a.Planer; }

        
        public Material Material => Model.GetComponent<Material>();
        public Collider Collider => Planer.GetComponent<MeshCollider>();
        public Renderer Renderer => Model.GetComponent<Renderer>();

        public LineRenderer LineRenderer { get{ return Planer.GetComponent<LineRenderer>(); } }
    }
}