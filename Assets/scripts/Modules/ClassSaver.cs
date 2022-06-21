using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
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
        ConstantShot, //
        ShotgunShot,
        Volley, // 
        Dash, 
    }

    public abstract class StateBar{
        public string BarName;
        public Color Color;
        public int MaxParam;
        public int MinParam;

        private protected int _Param;

        public int State { get{ return _Param; } set{ _Param = Mathf.Clamp(value, MinParam, MaxParam); } }
    }

    [System.Serializable]
    public class LifeParameters
    {
        [Space, Header("Base Parameters")]
        public Color Team;

        [Space] 
        // health parameters

        public HealthBar Health = new Health();
        // sanity parameters
        public SanityBar Sanity;
        // Stamina parameters
        public StaminaBar Stamina;



        public void Rest(){  } 
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