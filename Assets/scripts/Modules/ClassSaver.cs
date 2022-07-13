using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System;
using UnityEngine.Events;

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
        Arc, //
        Constant, 
        Line, //
        CompoundLine,
        Sphere, // 
    }
    public enum TargetPointGuidanceType
    {
        ToCursor,
        ToCursorWithWalls,
        ToFromPoint,
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
        public static UnityEvent StateList = new UnityEvent();

        abstract bool Updatable { get; set; }
    }
    
    public interface IStateBar : IStepEndUpdate
    {
        object Clone();
        
        Color BarColor{ get; }

        int Value { get; set; }
        int Max { get; set; }  
    }

    public interface IHealthBar : IStateBar
    {
        int ArmorMelee { get; set; } 
        int ArmorRange { get; set; }

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

    public interface IOtherBar : IStateBar
    {

    }


    public interface IObjectOnMap
    {
        IHealthBar Health{ get; set; }

        List<Effect> Resists{ get; set; }
        List<Effect> Debuff{ get; set; }

        void DamageReaction(Attack attack);
    }
    public interface IPlayerStats : IObjectOnMap
    {
        Color Team{ get; set; }

        IStaminaBar Stamina{ get; set; }
        ISanityBar Sanity{ get; set; }

        bool CanControl { get; set; }
        bool Corpse { get; set; }
        int WalkDistance { get; set; }

        List<IOtherBar> OtherStates{ get; set; }

        SkillCombiner SkillRealizer{ get; set; }
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

        public Color DamageColor() 
        { 
            switch (damageType)
            {
                default: return Color.HSVToRGB(0.02f, 1, 1);
                case DamageType.Melee: return Color.HSVToRGB(0.02f, 1, 1);
                case DamageType.MetalHeal: goto case DamageType.Heal; 
                case DamageType.Heal: return Color.HSVToRGB(0.42f, 1, 1);
                case DamageType.Rezo: return Color.HSVToRGB(67f / 360f, 1, 1);
                case DamageType.Pure: return Color.HSVToRGB(274f / 360f, 1, 1);
            }
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

        
        public Material Material => Model.GetComponent<Material>() ?? null;
        public Collider Collider => Planer.GetComponent<MeshCollider>() ?? null;
        public Renderer Renderer => Model.GetComponent<Renderer>() ?? null;

        public LineRenderer LineRenderer => Planer.GetComponent<LineRenderer>() ?? null;
    }




    public static class FieldManipulate
    {
        
        public static List<T> CombineLists<T>(List<T> a, List<T> b) 
        {
            List<T> result = new List<T>();
            result.AddRange(a);
            result.AddRange(b);
            return result;
        }
        public static List<T> CombineLists<T>(List<List<T>> a)
        {
            List<T> result = new List<T>();
            foreach(List<T> b in a)
            {
                result.AddRange(b);
            }
            return result;
        }
    }



    // [System.Serializable]public class Perishable<T> where T : Sendable
    // {
    //     private T Target;
        
    //     enum DisappearanceCondition
    //     {
    //         Timer,
    //         LowAmmo,
    //     }
    //     DisappearanceCondition Condition;
        
    //     public int HideTimer;
    //     public IStateBar AmmoLink;

    //     public Perishable(T target, int Timer) { Target = target; HideTimer = Timer; 
    //         Condition = DisappearanceCondition.Timer; 
    //         IStepEndUpdate.StateList.AddListener(Update);
    //     }
    //     public void Update()
    //     {
    //         if(HideTimer <= 0) Target = default(T);
    //         this.HideTimer--;
    //     }


    //     public Perishable(T target, IStateBar ammoLink) { Target = target; AmmoLink = ammoLink; Condition = DisappearanceCondition.LowAmmo; }

    //     public static implicit operator T (Perishable<T> a) { return a.Target; }
    // }
}
