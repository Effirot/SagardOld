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
            public GameObject To;

            public string Name;
            public string Description;
            Texture2D image;
            public HitType Type;
            public uint Level;
            public uint DamageModifier;
            public bool NoWalking = false;

            private Vector3 startPos{ get{ return From.transform.position; } }
            private Vector3 endPos{ get{ return To.transform.position; } }
            
            // Overloads
            public Skill(GameObject from, string name, HitType type, uint level, uint damage, bool noWlaking = false)
            { From = from; Name = name; Type = type; Level = level; DamageModifier = damage; NoWalking = noWlaking; }

            // ToString
            public override string ToString()
            { return "Skill:" + Name + " Type:" + Type + "(" + Level + ":" + DamageModifier + ":" + (NoWalking?":No" : ":Yes") + " walk)"; }
            
            LayerMask Mask = LayerMask.GetMask(new string[] {"Map", "Object"});
            
            private Vector3 ToPoint(Vector3 f, Vector3 t, float Distance)
            {
                if(Physics.Raycast(f, t - f, out RaycastHit hit, Distance, Mask))
                { 
                    return hit.point;
                }
                return t; 
            }

            public void Complete()
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
                        
                        Debug.DrawLine(startPos, endPos, Color.yellow);
                        Debug.DrawLine(startPos, ToPoint(startPos, endPos, Distance), Color.red);
                        break;
                    }
                    case HitType.Volley:
                    {

                        break;
                    }
                }
            }
            public bool Check()
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

                        return Vector3.Distance(startPos, endPos) < Distance & !(startPos.x == endPos.x && startPos.z == endPos.z);
                    }
                    case HitType.Volley:
                    {

                        break;
                    }
                }
                return false;
            }
            public Vector3[] Line()
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

                        return new Vector3[] {From.transform.position, ToPoint(From.transform.position, To.transform.position, Distance)};
                    }
                    case HitType.Volley:
                    {

                        break;
                    }
                }
                return new Vector3[] {};
            }
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
        

        
        
        }
    }
}




[System.Serializable]
public class Map
{
    public enum MapGeneratorType
    {
        Desert = 1,
        WeatheredDesert = 2,
        SwampedDesert = 3,
        MagnetAnomaly = 4
    }


    static Mesh map;
    static int[,] PlatformModifiers;
    static float[,] PlatformUp;
    int scaleX, scaleY;
    [SerializeField] static uint key;
    [SerializeField] MapGeneratorType type;

    public Map(uint Key, Vector2 Scale, MapGeneratorType Type)
    {
        scaleX = Scale.X; scaleY = Scale.Y; key = Key; type = Type;

        GenerateMap();    
    }

    public void GenerateMap()
    {
        for(int x = 0; x < scaleX; x++)
        {
            for(int z = 0; z < scaleY; z++)
            {
                PlatformUp[x, z] = 
            }
        }
    }



    
    

    private static class mapScale
    {

    }
}