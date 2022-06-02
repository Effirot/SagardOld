using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SagardCL //Class library
{
    [System.Serializable]
    public class ParameterList
    {   
        public string ClassTAG = "";

        [Space]
        public bool CanControll = true;
        public bool IsDead = false;

        [Space]

        

        public int WalkDistance;
        [Space]

        public List<Skill> AvailableSkills;
        [Space]
        public List<Effect> Resists;
        public List<Effect> Debuffs;
        [Space]


        [Space]
        [Space]
        [Header("Base Parameters")]
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

        public void AddSkill(AllInOne from, string name, HitType type, int level, int damage)
        {
            AvailableSkills.Add(new Skill(from, name, type, level, damage));
        }
        public void AddSkill(Skill skill)
        {
            AvailableSkills.Add(skill);
        }

        public void RemoveSkill(AllInOne from, string name, HitType type, int level, int damage)
        {
            AvailableSkills.Remove(new Skill(from, name, type, level, damage));
        }
        public void RemoveSkill(Skill skill)
        {
            AvailableSkills.Remove(skill);
        }


        public void Damage(string damageType, int damage, Effect debuff)
        {

        }
        public void Damage(Attack attack)
        {

        }

        public void AddRangeSkill(List<Skill> skills)
        {
            AvailableSkills.AddRange(skills);
        }
        public static ParameterList operator +(ParameterList a, ParameterList b)
        {
            ParameterList list = a;
            list.SetMax(a.MaxStamina + b.MaxStamina, a.MaxHP + b.MaxHP, a.MaxSanity + b.MaxSanity);
            list.SetBase(a.Stamina + b.Stamina, a.HP + b.HP, a.Sanity + b.Sanity);
            list.SetProtection(a.ArmoreClose + b.ArmoreClose, a.ArmoreBalistic + b.ArmoreBalistic, a.SanityShield + b.SanityShield);

            if(b.AvailableSkills != null) list.AddRangeSkill(b.AvailableSkills);

            return list;
        }
        public static ParameterList operator -(ParameterList a, ParameterList b)
        {
            ParameterList list = a;
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
        SwordSwing, 
        Constant, 
        BraveSwordSwing, 
        Shot, //
        InvertShot, //
        ExplodeShot, //
        PiercingShot, //
        ShotgunShot,
        Volley, // 
        ObstacleVolley,//
        Dash,
    }
    
    public class Descript
    {
        public string Name;
        public string Description;
        public string VeryBigDescription;
        public Texture2D image;
    }

    [System.Serializable]
    public class Skill : Descript
    {
        //--------------------------------------------------------------------------------------- All Parameters ----------------------------------------------------------------------------------------------------------
        [Header("Contollers")]
        public AllInOne From;
        GameObject FatherObj{ get{ return From.Planer.transform.parent.gameObject; }}
        public AllInOne To;
        
        [Space(2)]
        [Header("Parameters")]
        public HitType Type;
        [SerializeField] DamageType damageType;
        [Space]
        [Range(0, 40)]public int Distance;
        [Range(0, 20)]public int Damage;
        [SerializeField] bool Exploding;
        [Range(0, 15)]public int Level;
        public Effect debuff;
        public bool NoWalking;
        public bool WaitAStep;
        public bool DeleteWhenLowAmmo;        

        private Checkers startPos{ get{ return new Checkers(From.position, 0.3f); } set { From.position = value; } }
        private Checkers endPos{ get{ return new Checkers(To.position, 0f); } }
        private int endPosRotate{ get { return (int)Mathf.Round(Quaternion.LookRotation(cursor.transform.InverseTransformPoint(From.position)).eulerAngles.y) / 90; } }

        private Checkers newEndPos{ get{ return ToPoint(startPos, endPos); } }

        private int distanceDamage(float dist) { return (int)Mathf.Round(Damage - (dist / 4)); }

        private Checkers cursorPos{ get { return new Checkers(GameObject.Find("3DCursor").transform.position); }}
        private GameObject cursor{ get { return GameObject.Find("3DCursor"); }}
        //--------------------------------------------------------------------------------------- All Parameters ----------------------------------------------------------------------------------------------------------

        
        // Overloads
        public Skill(AllInOne from, string name, HitType type, int level, int damage, bool noWlaking = false, bool waitAStep = false)
        { From = from; Name = name; Type = type; Level = level; Damage = damage; NoWalking = noWlaking; WaitAStep = waitAStep; }
        
        public string ToString()
        { return FatherObj?.name + " type: " + Type + " damageMod: " + Damage + " distanceBuff: " + Distance; }

        public static Skill Empty() { return new Skill(null, "null", HitType.Empty, 0, 0); }

        private Checkers ToPoint(Vector3 f, Vector3 t, float Up = 0)
        {
            if(Physics.Raycast(f, t - f, out RaycastHit hit, Vector3.Distance(f, t), LayerMask.GetMask("Object", "Map")))
            { 
                return new Checkers(hit.point, Up);
            }
            return t; 
        }

        public List<Attack> DamageZone()
        {
            if(Check()){
                switch(Type)
                {
                    default: return new List<Attack>();
                    case HitType.SwordSwing:
                    {
                        List<Attack> result = new List<Attack>();

                        return result;
                    }
                    case HitType.Shot:
                    {
                        List<Attack> result = new List<Attack>();
                        foreach(RaycastHit hit in Physics.RaycastAll(
                            new Checkers(startPos, -1), 
                            ToPoint(startPos, endPos, -0.3f) - new Checkers(startPos, -0.3f), 
                            Checkers.Distance(startPos, new Checkers(ToPoint(startPos, endPos))), 
                            LayerMask.GetMask("Map")))
                        {
                            result.Add(new Attack(FatherObj, new Checkers(hit.point), distanceDamage((int)Checkers.Distance(startPos, new Checkers(hit.point))) - 1, damageType));
                        }
                        result.Add(new Attack(FatherObj, new Checkers(ToPoint(startPos, endPos)), Damage - 1, damageType));
                        return result;
                    }
                    case HitType.ExplodeShot:
                    {
                        List<Attack> result = new List<Attack>();
                        foreach(RaycastHit hit in Physics.RaycastAll(
                                new Checkers(startPos, -1), 
                                ToPoint(startPos, endPos, -0.3f) - new Checkers(startPos, -0.3f), 
                                Checkers.Distance(startPos, new Checkers(ToPoint(startPos, endPos))), 
                                LayerMask.GetMask("Map")))
                        {
                            result.Add(new Attack(FatherObj, new Checkers(hit.point), distanceDamage((int)Checkers.Distance(startPos, new Checkers(hit.point))) - 1, damageType));
                        }
                        
                        for(int x = -Level; x < 1 + Level; x++)
                        {
                            for(int z = -Mathf.Abs(Level); z <= Mathf.Abs(Level); z++)
                            {
                                if(Checkers.Distance(newEndPos, newEndPos + new Checkers(x, z)) > Mathf.Abs(Level) - 0.5f)
                                    continue;
                                if(newEndPos == newEndPos + new Checkers(x, z))
                                    continue;
                                result.Add(new Attack(FatherObj, 
                                new Checkers(newEndPos + new Checkers(x, z)), 
                                distanceDamage(Checkers.Distance(newEndPos, newEndPos + new Checkers(x, z))), 
                                damageType));
                            }
                        } 
                        return result;
                    }
                    case HitType.InvertShot:
                    {                    
                        List<Attack> result = new List<Attack>();
                        foreach(RaycastHit hit in Physics.RaycastAll(
                            new Checkers(startPos, -1), 
                            ToPoint(startPos, endPos, -0.3f) - new Checkers(startPos, -0.3f), 
                            Checkers.Distance(startPos, new Checkers(ToPoint(startPos, endPos))), 
                            LayerMask.GetMask("Map")))
                        {
                            result.Add(new Attack(FatherObj, new Checkers(hit.point), distanceDamage((int)Checkers.Distance(startPos, new Checkers(hit.point))) - 1, damageType));
                        }
                        result.Add(new Attack(FatherObj, new Checkers(ToPoint(startPos, endPos)), distanceDamage((int)Checkers.Distance(startPos, ToPoint(startPos, endPos))) + 2, damageType));
                        return result;
                    }
                    case HitType.PiercingShot:
                    {
                        List<Attack> result = new List<Attack>();
                        foreach(RaycastHit hit in Physics.RaycastAll(
                            new Checkers(startPos, -1), 
                            ToPoint(startPos, endPos, -0.3f) - new Checkers(startPos, -0.3f), 
                            Checkers.Distance(startPos, new Checkers(endPos)), 
                            LayerMask.GetMask("Map")))
                        {
                            result.Add(new Attack(FatherObj, new Checkers(hit.point), distanceDamage((int)Checkers.Distance(startPos, new Checkers(hit.point))) - 1, damageType));
                        }
                        result.Add(new Attack(FatherObj, endPos, Damage - 1, damageType));
                        return result;
                    }
                    case HitType.Volley:
                    {
                        List<Attack> result = new List<Attack>();
                        result.Add(new Attack(FatherObj, endPos, Damage, damageType));

                        for(int x = -Mathf.Abs(Level); x < 1 + Mathf.Abs(Level); x++)
                        {
                            for(int z = -Mathf.Abs(Level); z <= Mathf.Abs(Level); z++)
                            {
                                if(Checkers.Distance(endPos, endPos + new Checkers(x, z)) > Mathf.Abs(Level) - 0.5f)
                                    continue;
                                if(endPos == endPos + new Checkers(x, z))
                                    continue;
                                result.Add(new Attack(FatherObj, 
                                new Checkers(endPos + new Checkers(x, z)), 
                                distanceDamage(Checkers.Distance(endPos, endPos + new Checkers(x, z))), 
                                damageType));
                            }
                        } 
                        return result;
                    }
                    case HitType.ObstacleVolley:
                    {
                        List<Attack> result = new List<Attack>();
                        result.Add(new Attack(From, newEndPos, Damage, damageType));

                        for(int x = -Mathf.Abs(Level); x < 1 + Mathf.Abs(Level); x++)
                        {
                            for(int z = -Mathf.Abs(Level); z <= Mathf.Abs(Level); z++)
                            {
                                if(Checkers.Distance(newEndPos, newEndPos + new Checkers(x, z)) > Mathf.Abs(Level) - 0.5f)
                                    continue;
                                if(newEndPos == newEndPos + new Checkers(x, z))
                                    continue;
                                result.Add(new Attack(From, 
                                new Checkers(newEndPos + new Checkers(x, z)), 
                                distanceDamage(Checkers.Distance(newEndPos, newEndPos + new Checkers(x, z))), 
                                damageType));
                            }
                        } 
                        return result;
                    }
                }
            }
            return new List<Attack>();
        }
        public Vector3[] Line()
        {
            switch(Type)
            {
                default: return new Vector3[] { };
                case HitType.Shot:
                {   
                    Debug.DrawLine(startPos, endPos, Color.blue);
                    Debug.DrawLine(startPos, ToPoint(startPos, endPos, 0.3f), Color.red);
                    return new Vector3[] {startPos, ToPoint(startPos, endPos)};
                }
                case HitType.InvertShot:
                {   
                    Debug.DrawLine(startPos, endPos, Color.blue);
                    Debug.DrawLine(startPos, ToPoint(startPos, endPos, 0.3f), Color.red);
                    return new Vector3[] {startPos, ToPoint(startPos, endPos)};
                }
                case HitType.ExplodeShot:
                {   
                    Debug.DrawLine(startPos, endPos, Color.blue);
                    Debug.DrawLine(startPos, ToPoint(startPos, endPos, 0.3f), Color.red);
                    return new Vector3[] {startPos, ToPoint(startPos, endPos)};
                }
                case HitType.PiercingShot:
                {   
                    Debug.DrawLine(startPos, endPos, Color.red);
                    return new Vector3[] {startPos, endPos};
                }
                case HitType.Volley:
                {   
                    Debug.DrawLine(startPos, endPos, Color.red);
                    return new Vector3[] {startPos, endPos};
                }
                case HitType.ObstacleVolley:
                {   
                    Debug.DrawLine(startPos, endPos, Color.blue);
                    Debug.DrawLine(startPos, ToPoint(startPos, endPos, 0.3f), Color.red);
                    return new Vector3[] {startPos, ToPoint(startPos, endPos)};
                }
            }
            
        }
        public bool Check(){ return Checkers.Distance(startPos, endPos) < Distance & !(startPos.x == endPos.x && startPos.z == endPos.z); }
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
        public AllInOne(GameObject planer) { Planer = planer; }

        public Vector3 position{ get{ return Planer.transform.position; } set{ Planer.transform.position = value; } }
        public Vector3 localPosition{ get{ return Planer.transform.localPosition; } set{ Planer.transform.localPosition = value; } }

        public static implicit operator GameObject(AllInOne a) { return a.Planer; }

        public GameObject Model { get{ return Planer.transform.Find("Model").gameObject; } }
        public Material Material { get{ return Model.GetComponent<Material>(); } }
        public Collider Collider { get{  return Planer.GetComponent<MeshCollider>(); } }
        public Renderer Renderer { get{  return Model.GetComponent<Renderer>(); } }

        public LineRenderer LineRenderer { get{ return Planer.GetComponent<LineRenderer>(); } }
    }
}