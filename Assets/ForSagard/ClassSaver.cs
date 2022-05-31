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

            [SerializeField] string Name;
            [SerializeField] string Description;
            [SerializeField] Texture2D image;
            [SerializeField] DamageType damageType;
            [SerializeField] HitType Type;
            [SerializeField] uint Level;
            [SerializeField] uint DamageModifier;
            public bool NoWalking = false;
            public float distanceBuff = 0.0f;

            private Checkers startPos{ get{ return new Checkers(From.transform.position, 0.3f); } }
            private Checkers endPos{ get{ return new Checkers(To.transform.position, 0.3f); } }
            
            // Overloads
            public Skill(GameObject from, string name, HitType type, uint level, uint damage, bool noWlaking = false)
            { From = from; Name = name; Type = type; Level = level; DamageModifier = damage; NoWalking = noWlaking; }
            
            private Vector3 ToPoint(Vector3 f, Vector3 t)
            {
                if(Physics.Raycast(f, t - f, out RaycastHit hit, Vector3.Distance(f, t), LayerMask.GetMask("Object", "Map")))
                { 
                    return new Checkers(hit.point);
                }
                return t; 
            }

            public List<Attack> DamageZone()
            {
                switch(Type)
                {
                    default: return new List<Attack>();
                    case HitType.SwordSwing:
                    {
                        break;
                    }
                    case HitType.Shot:
                    {
                        int damage = 4 + (int)Mathf.Round(DamageModifier * 1.5f + Level * 0.3f);
                        int distanceDamage(int dist) { return (int)Mathf.Round(damage - dist * 0.5f); }

                        List<Attack> attackList = new List<Attack>();
                        foreach(RaycastHit hit in Physics.RaycastAll(
                            new Checkers(startPos, -1), ToPoint(new Checkers(startPos, -1), 
                            new Checkers(endPos, -1)) - startPos.ToVector3, 
                            Checkers.Distance(startPos, new Checkers(ToPoint(startPos, endPos))), 
                            LayerMask.GetMask("Map")))
                        {
                            if(new Checkers(ToPoint(startPos, endPos)) == endPos)
                            {
                                attackList.Add(new Attack(From, new Checkers(hit.point), distanceDamage((int)Checkers.Distance(startPos, endPos)) + 2, damageType));
                                continue;
                            }
                            attackList.Add(new Attack(From, new Checkers(hit.point), distanceDamage((int)Checkers.Distance(startPos, endPos)), damageType));
                        }
                        return attackList;
                    }
                    case HitType.Volley:
                    {
                        int damage = 5 + (int)Mathf.Round(DamageModifier * 1.5f + Level * 0.4f);

                        List<Attack> attackList = new List<Attack>();
                        attackList.Add(new Attack(From, endPos, damage, damageType));
                        if(Level >= 2)
                        {
                            attackList.Add(new Attack(From, new Checkers(endPos.x + 1, endPos.z + 1), damage - 2, damageType));
                            attackList.Add(new Attack(From, new Checkers(endPos.x + 1, endPos.z - 1), damage - 2, damageType));
                            attackList.Add(new Attack(From, new Checkers(endPos.x - 1, endPos.z + 1), damage - 2, damageType));
                            attackList.Add(new Attack(From, new Checkers(endPos.x - 1, endPos.z - 1), damage - 2, damageType));
                        }

                        return attackList;
                    }
                }
                return new List<Attack>();
            }
            public bool Check()
            {
                switch(Type)
                {
                    default: return true;
                    case HitType.Shot:
                    {
                        float Distance = 5.5f + (2 * Level) + distanceBuff;
                        return Vector3.Distance(startPos, endPos) < Distance & !(startPos.x == endPos.x && startPos.z == endPos.z);
                    }
                    case HitType.Volley:
                    {
                        float Distance = 9.5f + (1.5f * Level) + distanceBuff;
                        return Vector3.Distance(startPos, endPos) < Distance & !(startPos.x == endPos.x && startPos.z == endPos.z);
                    }
                }
            }
            public Vector3[] Line()
            {
                switch(Type)
                {
                    default: return new Vector3[] { };
                    case HitType.Shot:
                    {   
                        Debug.DrawLine(startPos, endPos, Color.blue);
                        Debug.DrawLine(startPos, ToPoint(startPos, endPos), Color.red);
                        return new Vector3[] {startPos, ToPoint(startPos, endPos)};
                    }
                    case HitType.Volley:
                    {   
                        Debug.DrawLine(startPos, endPos, Color.red);
                        return new Vector3[] {startPos, endPos};
                    }
                }
                
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

            public string InString()
            { return "Attack from " + WhoAttack.transform.parent.name + " to " + WhereAttack.x + ":" + WhereAttack.z + " (" + damageType.ToString() + " - " + Damage + ")"; }

            public static Attack Empty{ get{ return new Attack(null, new Checkers(), 0, DamageType.Pure);}}
        
        }
    }
}

public class GameLog 
{

}