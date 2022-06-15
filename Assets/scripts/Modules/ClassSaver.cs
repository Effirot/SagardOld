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
        [Space]
        public int MaxStamina;
        public int MaxHP;
        public int MaxSanity;
        [Space]
        public int ArmoreClose;
        public int ArmoreBalistic;
        public int SanityShield;
        [Space]
        public  int Stamina;
        public int HP;
        public int Sanity;

        public void Rest(int StaminaAdd)
        {
            Stamina = Mathf.Clamp(Stamina + StaminaAdd, 0, MaxStamina);
        }

        public void SetMax(int Stamina, int HP, int Sanity)
        {
            MaxStamina = Stamina;
            MaxHP = HP;
            MaxSanity = Sanity;
        }
        public void SetBase(int stamina, int hp, int sanity)
        {
            Stamina = Mathf.Clamp(stamina, 0, MaxStamina);
            HP = Mathf.Clamp(hp, 0, MaxHP);
            Sanity = Mathf.Clamp(sanity, -5, MaxSanity);
        }
        public void SetProtection(int Close, int Balistic, int sanity)
        {
            ArmoreClose = Close;
            ArmoreBalistic = Balistic;
            SanityShield = sanity;
        }
        public void Damage(string damageType, int damage, Effect debuff)
        {

        }
        public void Damage(Attack attack)
        {

        }
    }

    [System.Serializable]
    public class PlayerControlList : LifeParameters
    {
        [Space, Header("Controll Settings")]
        public bool CanControll = true;
        [Space]        

        public int WalkDistance;
        [Space]

        public List<Skill> AvailableSkills;
        [Space]
        public List<Effect> Resists;
        public List<Effect> Debuffs;

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
        public static PlayerControlList operator +(PlayerControlList a, PlayerControlList b)
        {
            PlayerControlList list = a;
            list.SetMax(a.MaxStamina + b.MaxStamina, a.MaxHP + b.MaxHP, a.MaxSanity + b.MaxSanity);
            list.SetBase(a.Stamina + b.Stamina, a.HP + b.HP, a.Sanity + b.Sanity);
            list.SetProtection(a.ArmoreClose + b.ArmoreClose, a.ArmoreBalistic + b.ArmoreBalistic, a.SanityShield + b.SanityShield);

            if(b.AvailableSkills != null) list.AddRangeSkill(b.AvailableSkills);

            return list;
        }
        public static PlayerControlList operator -(PlayerControlList a, PlayerControlList b)
        {
            PlayerControlList list = a;
            list.SetMax(a.MaxStamina - b.MaxStamina, a.MaxHP - b.MaxHP, a.MaxSanity - b.MaxSanity);
            list.SetBase(a.Stamina - b.Stamina, a.HP - b.HP, a.Sanity - b.Sanity);
            list.SetProtection(a.ArmoreClose - b.ArmoreClose, a.ArmoreBalistic - b.ArmoreBalistic, a.SanityShield - b.SanityShield);

            foreach(Skill skill in b.AvailableSkills)
            {
                list.RemoveSkill(skill);
            }

            return list;
        }

    }

    public enum DamageType
    {
        Melee,
        Range,
        Rezo,
        Terra,
        Pure
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
        
        public override string ToString()
        { return FatherObj?.name + " type: " + Type + " damageMod: " + Damage + " distanceBuff: " + Distance; }

        public static Skill Empty() { return new Skill {From = null, Name = "null", Type = HitType.Empty, Distance = 0, Damage = 0}; }

        private Checkers ToPoint(Vector3 f, Vector3 t, float Up = 0)
        {
            if(Physics.Raycast(f, t - f, out RaycastHit hit, Vector3.Distance(f, t), LayerMask.GetMask("Object", "Map")))
                return new Checkers(hit.point, Up);
            return t; 
        }

        public async Task<List<Attack>> DamageZone()
        {
            if(Check()){
                List<Attack> result = new List<Attack>();
                Checkers FinalPoint = (Piercing)? endPos : ToPoint(startPos, endPos);
                switch(Type)
                {
                    default: return new List<Attack>();
                    case HitType.OnSelf:
                    {
                        FinalPoint = startPos;
                        result.Add(new Attack(FatherObj, startPos, Damage, damageType));
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
                            
                            result.Add(new Attack(FatherObj, new Checkers(hit.point), (int)Mathf.Round(Damage / (Checkers.Distance(startPos, new Checkers(hit.point)) * 0.08f)), damageType));
                        }
                        result.Add(new Attack(FatherObj, FinalPoint, Damage, damageType));
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
                            result.Add(new Attack(FatherObj, new Checkers(hit.point), (int)Mathf.Round(Damage * (Checkers.Distance(startPos, FinalPoint) / 4)), damageType));
                        } 
                        result.Add(new Attack(FatherObj, FinalPoint, 1 + Damage + (int)Mathf.Round(Damage * (Checkers.Distance(startPos, FinalPoint) / 4)), damageType));
                        break;
                    }
                    case HitType.Volley:
                    {
                        result.Add(new Attack(FatherObj, FinalPoint, Damage, damageType));
                        break;
                    }
                }
                if(Exploding == 0) return result;
                
                // --------------------Explode------------------
                for(int x = -Mathf.Abs(Exploding); x < 1 + Mathf.Abs(Exploding); x++)
                {
                    for(int z = -Mathf.Abs(Exploding); z <= Mathf.Abs(Exploding); z++)
                    {
                        if(Checkers.Distance(FinalPoint, FinalPoint + new Checkers(x, z)) > Mathf.Abs(Exploding) - 0.9f)
                            continue;
                        if(Physics.Raycast(new Checkers(FinalPoint, 1), FinalPoint + new Checkers(x, z) - FinalPoint, Checkers.Distance(FinalPoint, FinalPoint + new Checkers(x, z)), LayerMask.GetMask("Map")))
                            continue;
                        if(result.Find((a) => a.Where == FinalPoint + new Checkers(x, z)).Where == FinalPoint + new Checkers(x, z))
                            continue;

                        result.Add(new Attack(FatherObj, 
                        new Checkers(FinalPoint + new Checkers(x, z)), 
                        (int)Mathf.Round(Damage / (Checkers.Distance(FinalPoint, FinalPoint + new Checkers(x, z), Checkers.mode.Height) * 0.5f)), 
                        damageType));
                    }
                }
                
                return result;
            }
            return new List<Attack>();
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

        [SerializeField]DamageType damageType;
        [SerializeField]Effect[] Debuff;



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

        public string InString()
        { 
            string effects = "";
            foreach(Effect effect in Debuff)
            {
                effects += effect.Name + ", ";
            }
            return "Attack from " + WhoAttack.transform.parent.name + 
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