using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL.Usabless;

namespace SagardCL //Class library
{
    [System.Serializable]
    public class ParameterList
    {   
        [Header("Can Controll?")]
        public bool CanControll = true;
        public bool IsDead = false;

        [Space]

        public string ClassTAG = "";

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

        private void Awake()
        {
            Stamina = MaxStamina;
            HP = MaxHP;
            Sanity = MaxSanity;
        }
        public void Rest(int StaminaAdd)
        {
            Stamina = Mathf.Clamp(Stamina + StaminaAdd, 0, MaxStamina);
        }
        
        public void CompleteAllEffects()
        {
            foreach(Effect Effect in Debuffs)
            {

            }
        }

        public void SetMax(int Stamina, int HP, int Sanity)
        {
            MaxStamina = Stamina;
            MaxHP = HP;
            MaxSanity = Sanity;
        }
        public void SetBase(int Stamina, int HP, int Sanity)
        {
            MaxStamina = Stamina;
            MaxHP = HP;
            MaxSanity = Sanity;
        }
        public void SetProtection(int Close, int Balistic, int Sanity)
        {
            ArmoreClose = Stamina;
            ArmoreBalistic = HP;
            SanityShield = Sanity;
        }

        public void AddSkill(GameObject from, string name, HitType type, uint level, uint damage)
        {
            AvailableSkills.Add(new Skill(from, name, type, level, damage));
        }
        public void AddSkill(Skill skill)
        {
            AvailableSkills.Add(skill);
        }

        public void RemoveSkill(GameObject from, string name, HitType type, uint level, uint damage)
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



    namespace Usabless{

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
            SwordSwing,
            Shot,
            Volley,
            Dash,
            
        }
        
        [System.Serializable]
        public class Skill
        {
            public GameObject From;
            public string Name;
            public string Description;
            Texture2D image;
            public HitType Type;
            public uint Level;
            public uint DamageModifier;
            public bool NoWalking = false;

            private Vector3 startPos;

            public Skill(GameObject from, string name, HitType type, uint level, uint damage, bool noWlaking = false)
            { From = from; Name = name; Type = type; Level = level; DamageModifier = damage; NoWalking = noWlaking; 
                startPos = From.transform.position;
            }

            public override string ToString()
            { return "Skill:" + Name + " Type:" + Type + "(" + Level + ":" + DamageModifier + ":" + (NoWalking?":No" : ":Yes") + " walk)"; }
            
            LayerMask Mask = LayerMask.GetMask(new string[] {"Map", "Object"});
            
            private Vector3 ToPoint(Vector3 f, Vector3 t, float Distance)
            {
                if(Physics.Raycast(f, t - f, out RaycastHit hit, Distance, Mask))
                { 
                    return new Checkers(hit.point, 0.3f);
                }
                else return t; 
            }

            public void Complete(Vector3 to)
            {
                switch(Type)
                {
                    case HitType.SwordSwing:
                    {
                        
                        break;
                    }
                    case HitType.Shot:
                    {
                        float Distance = 5.5f + (2 * Level);
                        
                        Debug.DrawLine(startPos, to, Color.yellow);
                        Debug.DrawLine(startPos, ToPoint(startPos, to, Distance), Color.red);
                        break;
                    }
                    case HitType.Volley:
                    {

                        break;
                    }
                }
            }
            
            public bool Check(Vector3 to)
            {
                bool result = false;
                switch(Type)
                {
                    case HitType.SwordSwing:
                    {
                        
                        break;
                    }
                    case HitType.Shot:
                    {
                        float Distance = 5.5f + (2 * Level);

                        result = Vector3.Distance(startPos, to) < Distance & !(startPos.x == to.x && startPos.z == to.z);
                        break;
                    }
                    case HitType.Volley:
                    {

                        break;
                    }
                }
                
                return result;
            }

            public void DrawLine(LineRenderer lnRenderer, Vector3 to)
            {
                switch(Type)
                {
                    case HitType.SwordSwing:
                    {
                        
                        break;
                    }
                    case HitType.Shot:
                    {   
                        float Distance = 5.5f + (2 * Level);
                        Vector3 toPoint = to;

                        if (Physics.Raycast(startPos, to - startPos, out RaycastHit hit, Distance, Mask))
                        { toPoint = new Checkers(hit.point, 0.3f); }
                        else toPoint = to;

                        lnRenderer.positionCount = 2;
                        lnRenderer.SetPositions(new Vector3[] {startPos, toPoint});
                        break;
                    }
                    case HitType.Volley:
                    {

                        break;
                    }
                }
            }

            public void ResetLine(LineRenderer lnRenderer)
            { lnRenderer.positionCount = 0; }
        }

        [System.Serializable]
        public class Item
        {
            string Name = "";
            string Description = "";
            Texture2D texture = new Texture2D(256, 256);
        }

        [System.Serializable]
        public class Effect
        {
            public string Name;
            public string Description;
            public Texture2D image;
        }


        [System.Serializable]
        public class Attack
        {
            public GameObject WhoAttack;
            public Checkers WhereAttack;
            public int Damage;
            DamageType damageType;
            public Effect[] Debuff;

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
        }
    }
}

[System.Serializable]
public struct Checkers
{
    [SerializeField] static int X, Z;
    [SerializeField] static float UP;
    
    void Update()
    {
        UP = YUpPos();
    }

    private float YUpPos()
    {
        RaycastHit hit;
        Physics.Raycast(new Vector3(X, 1000, Z), -Vector3.up, out hit, Mathf.Infinity, LayerMask.GetMask("Map"));
        return hit.point.y;
    }

    public Checkers(float Xadd, float Zadd, float UPadd = 0) 
    { 
        X = (int)Mathf.Round(Xadd); Z = (int)Mathf.Round(Zadd); UP = YUpPos() + UPadd;
    }
    public Checkers(Vector3 Vector3add, float UPadd = 0) 
    { 
        X = (int)Mathf.Round(Vector3add.x); Z = (int)Mathf.Round(Vector3add.z); UP = YUpPos() + UPadd;
    }
    public Checkers(Vector2 Vector2add, float UPadd = 0) 
    { 
        X = (int)Mathf.Round(Vector2add.x); Z = (int)Mathf.Round(Vector2add.y); UP = YUpPos() + UPadd;
    }
    public Checkers(Transform Transformadd, float UPadd = 0) 
    { 
        X = (int)Mathf.Round(Transformadd.position.x); Z = (int)Mathf.Round(Transformadd.position.z); UP = YUpPos() + UPadd;
    }

    public static implicit operator Vector3(Checkers a) { return new Vector3(a.x, a.up, a.z); }
    public static implicit operator Checkers(Vector3 a) { return new Checkers(a.x, a.z); }
    
    //public Vector3() { return new Vector3(X, UP, Z); }

    public int x { get{ return X; } }
    public int z { get{ return Z; } }
    public float up { get{ return UP; } }

    public static Checkers operator +(Checkers a, Checkers b)
    {
        int X = a.x + b.z;
        int Y = a.z + b.z;

        return new Checkers(X, Y, a.up);
    }
    public static Checkers operator -(Checkers a, Checkers b)
    {
        int X = a.x - b.z;
        int Y = a.z - b.z;

        return new Checkers(X, Y, a.up);
    }

    public Vector3 ToVector3{ get{ return new Vector3(X, UP, Z);} }
}


[System.Serializable]
public class Map
{
    public enum MapGeneratorType
    {
        Desert,
        WeatheredDesert
    }


    static Mesh map;
    static Mesh mapCollider;
    int scaleX, scaleY;
    [SerializeField] static uint key;
    [SerializeField] MapGeneratorType type;

    public Map(uint Key, int[] size, MapGeneratorType Type)
    {
        scaleX = size[0]; scaleY = size[1]; key = Key; type = Type;

        GenerateMap();    
    }

    public void GenerateMap()
    {

    }
    

    private static class mapScale
    {
        public static int[] Size(int xSize, int ySize)
        {
            return new int[2] { xSize, ySize };
        }
    }
}