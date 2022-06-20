using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SagardCL //Class library
{
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
        Shot, //
        InvertShot, //
        ShotgunShot,
        Volley, // 
        Dash, 
    }

    public struct CustomBar
    {
        string BarName;
        int MaxParam;
        int Param;
    }

    [System.Serializable]
    public class LifeParameters
    {
        [Space, Header("Base Parameters")]
        public Color Team;        
        [Space] // health parameters
        public int MaxHP;
        public int HP;
        public int ArmorMelee;
        public int ArmorRange;
        
        public void GetDamage(Attack attack)
        {
            switch(attack.damageType)
            {
                case DamageType.Pure: HP -= attack.damage; break;
                case DamageType.Melee: HP -= attack.damage - ArmorMelee; break;
                case DamageType.Range: HP -= attack.damage - ArmorRange; break;
                case DamageType.Rezo: HP -= attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f); break;
                case DamageType.Terra: HP -= attack.damage / 4; break;

                case DamageType.Sanity: Sanity -= attack.damage - SanityShield; break;

                case DamageType.Heal: HP = Mathf.Clamp(HP + attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.2f), 0, MaxHP); break;
            }

            if(attack.Debuff != null) { foreach(Effect effect in attack.Debuff) { if(Resists.Find((a) => a == effect) != effect) Debuff.Add(effect); } }
        }

        [Space] // sanity parameters
        public int MaxSanity;
        public int Sanity;
        public int SanityShield;

        [Space] // Stamina parameters
        public int MaxStamina;
        public int Stamina;
        [Range(0, 30)] public int RestEffectivity;
        public void Rest(){ if(RestEffectivity == 0){ Stamina = MaxStamina; return; } Stamina = Mathf.Clamp(Stamina + RestEffectivity, 0, MaxStamina); } 
        [SerializeField] int WalkUseStamina;

        public bool CanControl = true;
        public int WalkDistance;

        [Space] // Debuff's parameters
        public List<Effect> Resists;
        public List<Effect> Debuff;

        // Skills parameters
        [Space]
        public Skill SkillRealizer;
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