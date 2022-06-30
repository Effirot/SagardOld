using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System;

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
        MetalHeal,
    }       
    public enum HitType
    {
        Empty, //
        OnSelfPoint, //
        Arc, //
        Constant, 
        Line, //
        InvertLine, //
        ConstantLine, //
        ShotgunShot,
        Point, // 
        Dash, 
    }
    public enum DamageScaling 
    {
        Constant,
        Descending,
        Addition,
    }

    // All Interfaces 
    public interface Sendable
    {

    }
    public interface IStepEndUpdate
    {
        void Update();
    }
    
    public interface IStateBar : IStepEndUpdate
    {
        Color BarColor{ get; }

        int Value { get; }
        int Max { get; set; }  

        public static List<IStepEndUpdate> StateList = new List<IStepEndUpdate>();
    }

    public interface IHealthBar : IStateBar
    {
        int ArmorMelee { get; set; } 
        int ArmorRange {get; set; }

        void GetDamage(Attack attack);
    }
    public interface IStaminaBar : IStateBar
    {
        void GetTired(int value);
        int RestEffectivity{ get; set; }
        int WalkUseStamina{ get; set; }

        void Rest();
    }
    public interface ISanityBar : IStateBar
    {
        int SanityShield { get; set; } 
    }

    public interface IAmmoBar : IStateBar
{

}







    public struct Attack
    {
        public UnitController WhoAttack;

        [SerializeField]Checkers WhereAttack;
        public Checkers Where { get { return WhereAttack; } }

        [SerializeField]int Damage;
        public int damage { get { return Damage; } }

        public DamageType damageType;
        public Effect[] Debuff;

        // Overloads
        public Attack(UnitController Who, Checkers Where, int Dam, DamageType Type, Effect[] debuff)
        {
            WhoAttack = Who;
            WhereAttack = Where;
            Damage = Dam;

            damageType = Type;
            Debuff = debuff;
        }
        public Attack(UnitController Who, Checkers Where, int Dam, DamageType Type, Effect debuff = null)
        {
            WhoAttack = Who;
            WhereAttack = Where;
            Damage = Dam;

            damageType = Type;
            Debuff = new Effect[] { debuff };
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

    [System.Serializable]public class Perishable<T> where T : Sendable
    {
        private T Target;
        
        enum DisappearanceCondition
        {
            Timer,
            LowAmmo,
        }
        DisappearanceCondition Condition;
        
        public int HideTimer;
        public IStateBar AmmoLink;

        public Perishable(T target, int Timer) { Target = target; HideTimer = Timer; 
            Condition = DisappearanceCondition.Timer; 
            InGameEvents.StepEnd.AddListener(this.TimerTick); 
        }
        public void TimerTick()
        {
            if(HideTimer <= 0) Target = default(T);
            this.HideTimer--;
            Debug.LogWarning("Something");
        }


        public Perishable(T target, IStateBar ammoLink) { Target = target; AmmoLink = ammoLink; Condition = DisappearanceCondition.LowAmmo; }

        public static implicit operator T (Perishable<T> a) { return a.Target; }
    }
}
