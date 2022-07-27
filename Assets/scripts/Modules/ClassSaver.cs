using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System;
using UnityEngine.Events;
using SagardCL.IParameterManipulate;
using System.Reflection;
using System.Linq;

namespace SagardCL //Class library
{
    public enum DamageType
    {
        Melee,
        Range,
        Rezo,
        Sanity,
        Pure,
        Heal,
        Repair,
        Effect,
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
                
    public enum Race
    {
        Human,
        Robot,
        Lubiak,
        Draif,
        HalfVampire,
        Vampire,
        Brassy,
        LivingArtifact,
        Foctotoum,
    }


    public struct Attack
    {
        #region // Constructor parameters

            public CharacterCore Sender;
            public Checkers Position;
            public int Damage;
            public DamageType DamageType;
            public Effect[] Effects;

        #endregion
        #region // Overloads

            public Attack(CharacterCore Who, Checkers where, int Dam, DamageType Type, params Effect[] debuff)
            {
                Sender = Who;
                Position = where;
                Damage = Mathf.Clamp(Dam, 0, 10000);

                DamageType = Type;
                Effects = debuff;
            }
            public Attack(int Dam, DamageType Type, params Effect[] debuff)
            {
                Sender = null;
                Position = new Checkers();
                Damage = Mathf.Clamp(Dam, 0, 10000);

                DamageType = Type;
                Effects = debuff;
            }

        #endregion

        public Color Color() 
        { 
            switch (DamageType)
            {
                default: return UnityEngine.Color.HSVToRGB(0.02f, 1, 1);
                case DamageType.Melee: return UnityEngine.Color.HSVToRGB(0.02f, 1, 1);
                case DamageType.Repair: goto case DamageType.Heal; 
                case DamageType.Heal: return UnityEngine.Color.HSVToRGB(0.42f, 1, 1);
                case DamageType.Rezo: return UnityEngine.Color.HSVToRGB(67f / 360f, 1, 1);
                case DamageType.Pure: return UnityEngine.Color.HSVToRGB(274f / 360f, 1, 1);
            }
        }
    
        public struct AttackCombiner
        {
            public Dictionary<DamageType, List<Attack>> Sorter;
            public HashSet<CharacterCore> Senders;


            public AttackCombiner(params Attack[] attacks) 
            {   
                Sorter = new Dictionary<DamageType, List<Attack>>(); 
                Senders = new HashSet<CharacterCore>();

                foreach(DamageType type in Enum.GetValues(typeof(DamageType))) { Sorter.Add(type, new List<Attack>()); }
                foreach(Attack attack in attacks) { Add(attack); } 
            }

            public AttackCombiner Add(Attack attack)
            {
                if(attack.Sender != null) Senders.Add(attack.Sender);
                Sorter[attack.DamageType].Add(attack);

                return this;
            }
            public void Clear()
            {
                Sorter = new Dictionary<DamageType, List<Attack>>();
                foreach(DamageType type in Enum.GetValues(typeof(DamageType))) { Sorter.Add(type, new List<Attack>()); }

                Senders = new HashSet<CharacterCore>();
            }

            public List<Attack> Combine()
            {
                List<Attack> result = new List<Attack>();
                foreach (var attacks in Sorter)
                {
                    List<Effect> effects = new List<Effect>();
                    if(attacks.Value.Count > 0) foreach (Attack attack in attacks.Value) { effects.AddRange(attack.Effects); }
                    result.Add(new Attack(attacks.Value.Sum(a=>a.Damage), attacks.Key, effects.ToArray()));
                }
                return result;
            }
            public List<Attack> Combine(Checkers pos)
            {
                List<Attack> result = new List<Attack>();
                foreach (var attacks in Sorter)
                {
                    result.Add(new Attack(null, pos, attacks.Value.Sum(a=>a.Damage), attacks.Key));
                }
                return result;
            }
            
            public Color CombinedColor()
            {
                Color result = new Color();
                foreach (var attacks in Sorter)
                {
                    int Damage = 0;
                    foreach (Attack attack in attacks.Value) { Damage += attack.Damage; }
                    
                    if(result == new Color()) new Attack(Damage, attacks.Key).Color();
                    else result += new Attack(Damage, attacks.Key).Color();
                }
                return result;
            }
        }
    
    }
    [System.Serializable] public struct Checkers
    {
        [SerializeField] int X, Z;
        [SerializeField] float UP;

        public int x { get{ return X; } }
        public int z { get{ return Z; } }
        public float up { get{ return this.UP + YUpPos(); } }
        public float clearUp { get{ return UP; } }

        float YUpPos()
        {
            if (Physics.Raycast(new Vector3(x, 1000, z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Map")))
                return hit.point.y;
            return 0;
        }

        #region // =============================== Realizations

        public Checkers(float Xadd, float Zadd, float UPadd = 0) { X = (int)Mathf.Round(Xadd); Z = (int)Mathf.Round(Zadd); UP = UPadd; }
        public Checkers(Vector3 Vector3add, float UPadd = 0) { X = (int)Mathf.Round(Vector3add.x); Z = (int)Mathf.Round(Vector3add.z); UP = UPadd; }
        public Checkers(Vector2 Vector2add, float UPadd = 0) { X = (int)Mathf.Round(Vector2add.x); Z = (int)Mathf.Round(Vector2add.y); UP = UPadd; }
        public Checkers(Transform Transformadd, float UPadd = 0) { X = (int)Mathf.Round(Transformadd.position.x); Z = (int)Mathf.Round(Transformadd.position.z); UP = UPadd; }

        public static implicit operator Vector3(Checkers a) { return new Vector3(a.x, a.up, a.z); }
        public static implicit operator Checkers(Vector3 a) { return new Checkers(a.x, a.z); }

        public static Checkers operator +(Checkers a, Checkers b) { return new Checkers(a.x + b.x, a.z + b.z, a.up); }
        public static Checkers operator -(Checkers a, Checkers b) { return new Checkers(a.x - b.x, a.z - b.z, a.up); }
        public static Checkers operator *(Checkers a, float b) { return new Checkers(a.x * b, a.z * b, a.up); }
        public static Checkers operator *(float b, Checkers a) { return new Checkers(a.x * b, a.z * b, a.up); }
        public static bool operator ==(Checkers a, Checkers b) { return a.x == b.x & a.z == b.z; }
        public static bool operator !=(Checkers a, Checkers b) { return !(a.x == b.x & a.z == b.z); }
        
        public override int GetHashCode() { return 0; }  
        public override bool Equals(object o) { return true; } 

        public Checkers WithUp(float a){ return new Checkers(this, a); }

        #endregion // =============================== Realizations
        #region // =============================== Math

        public enum CheckersDistanceMode{ NoHeight, Height, OnlyHeight, }
        public static float Distance(Checkers a, Checkers b, CheckersDistanceMode Mode = CheckersDistanceMode.NoHeight)
        {
            if(Mode == CheckersDistanceMode.OnlyHeight) return Mathf.Abs(a.up - b.up);
            if(Mode == CheckersDistanceMode.Height) return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.ToVector3().y - b.ToVector3().y, 2) + Mathf.Pow(a.z - b.z, 2));
            return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.z - b.z, 2));
        }
        public static float Distance(Vector3 a, Vector3 b, CheckersDistanceMode Mode = CheckersDistanceMode.NoHeight)
        {
            if(Mode == CheckersDistanceMode.Height) return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2) + Mathf.Pow(a.z - b.z, 2));
            return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.z - b.z, 2));
        }

        public Vector3 ToVector3(){ return new Vector3(x, up, z); }
        public static List<Vector3> ToVector3List(List<Checkers> checkers) 
        { 
            List<Vector3> list = new List<Vector3>();
            foreach(Checkers checker in checkers){ list.Add(checker.ToVector3()); }
            return list;        
        }
        public static List<Checkers> ToCheckersList(List<Vector3> vector3, float up = 0) 
        { 
            List<Checkers> list = new List<Checkers>();
            foreach(Checkers vector in vector3){ list.Add(new Checkers(vector, up)); }
            return list;        
        }

        public static bool CheckCoords(Checkers Coordinates) 
        {
            return Physics.Raycast(new Vector3(Coordinates.x, 1000, Coordinates.z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Map"));
        }
        public static bool CheckCoords(int x, int z) 
        {
            return Physics.Raycast(new Vector3(x, 1000, z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Map"));
        }
        
        public static Checkers Lerp(Checkers pos1, Checkers pos2, float StepSize)
        {
            float x = pos1.x + (pos2.x - pos1.x) * (StepSize);
            float z = pos1.z + (pos2.z - pos1.z) * (StepSize);

            return new Checkers(x, z);
        }
        public static Checkers MoveTowards(Checkers pos1, Checkers pos2, float StepSize)
        {
            return Checkers.Lerp(pos1, pos2, Distance(pos1, pos2) / StepSize);
        }

        public static List<Checkers> Line(Checkers pos1, Checkers pos2) {
            List<Checkers> result = new List<Checkers>();
            
            int deltaX = Mathf.Abs(pos2.x - pos1.x);
            int deltaZ = Mathf.Abs(pos2.z - pos1.z);
            
            int signX = pos1.x < pos2.x ? 1 : -1;
            int signZ = pos1.z < pos2.z ? 1 : -1;
            
            int error = deltaX - deltaZ;

            result.Add(pos2);

            while(pos1 != pos2) 
            {
                

                int error2 = error * 2;
                if(error2 > -deltaZ) 
                {
                    error -= deltaZ;
                    pos1.X += signX;
                }
                if(error2 < deltaX) 
                {
                    error += deltaX;
                    pos1.Z += signZ;
                }
                result.Add(pos1);
            }

            return result;

        }
        
        #endregion // =============================== Math

        public static class PatchWay
        {
            public static List<Checkers> WayTo(Checkers a, Checkers b, int MaxSteps, float CheckersUp = 0.1f) 
            {            
                return new List<Checkers>() { a, b };
            }

        }

        public enum EMoveAction { walk, jump, fall, swim };
        
        public class PathPoint
        {
            // текущая точка
            public Checkers point { get; set; }
            // расстояние от старта
            public float pathLenghtFromStart { get; set; }
            // примерное расстояние до цели
            public float heuristicEstimatePathLenght { get; set; }
            // еврестическое расстояние до цели
            public float estimateFullPathLenght
            {
                get
                {
                return this.heuristicEstimatePathLenght + this.pathLenghtFromStart;
                }
            }
            // способ движения
            public EMoveAction moveAction = EMoveAction.walk;
            // точка из которой пришли сюда
            public PathPoint cameFrom;
                private PathPoint NewPathPoint(Checkers point, float pathLenghtFromStart, float heuristicEstimatePathLenght, EMoveAction moveAction)
            {
                PathPoint a = new PathPoint();
                a.point = point;
                a.pathLenghtFromStart = pathLenghtFromStart;
                a.heuristicEstimatePathLenght = heuristicEstimatePathLenght;
                a.moveAction = moveAction;
                return a;
            }

            private PathPoint NewPathPoint(Checkers point, float pathLenghtFromStart, float heuristicEstimatePathLenght, EMoveAction moveAction, PathPoint ppoint)
            {
                PathPoint a = new PathPoint();
                a.point = point;
                a.pathLenghtFromStart = pathLenghtFromStart;
                a.heuristicEstimatePathLenght = heuristicEstimatePathLenght;
                a.moveAction = moveAction;
                a.cameFrom = ppoint;
                return a;
            }
        }

    }
    [System.Serializable] public class AllInOne
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

    [System.Serializable] public class BalanceChanger
    {
        [Space]
        public int WalkDistance = 0;

        [SerializeReference, SubclassSelector]public IHealthBar Health = new Health();
        public bool ReplaceHealthBar = false;
        [SerializeReference, SubclassSelector]public IStaminaBar Stamina = new Stamina();
        public bool ReplaceStaminaBar = false;
        [SerializeReference, SubclassSelector]public ISanityBar Sanity = new Sanity();
        public bool ReplaceSanityBar = false;
        
        public List<IOtherBar> AdditionState;

        public List<IEffect> Resists = new List<IEffect>();
        [Space]
        public List<Skill> AdditionSkills = new List<Skill>();

        
        public static BalanceChanger CompoundParameters(params BalanceChanger[] items) 
        {
            var result = new BalanceChanger();
            var resists = new List<IEffect>();
            var additionStates = new List<IOtherBar>();
            var additionSkills = new List<Skill>();
            foreach(BalanceChanger item in items)
            {
                result.WalkDistance += item.WalkDistance;

                result.Health.Max += item.Health.Max;
                result.Health.ArmorMelee += item.Health.ArmorMelee;
                result.Health.ArmorRange += item.Health.ArmorRange;
                result.Health.Immunity += item.Health.Immunity;

                result.Stamina.Max += item.Stamina.Max;
                result.Stamina.WalkUseStamina += item.Stamina.WalkUseStamina;
                result.Stamina.RestEffectivity += item.Stamina.RestEffectivity;
                
                result.Sanity.Max += item.Sanity.Max;
                result.Sanity.SanityShield += item.Sanity.SanityShield;

                resists.AddRange(item.Resists);
                additionStates.AddRange(item.AdditionState);
                additionSkills.AddRange(item.AdditionSkills);
            }
            result.Resists = resists;
            result.AdditionSkills = additionSkills;
            result.AdditionState = additionStates;

            return result; 
        }
        public static BalanceChanger CompoundParameters(params Item[] items) 
        {
            var result = new BalanceChanger();
            var resists = new List<IEffect>();
            var additionStates = new List<IOtherBar>();
            var additionSkills = new List<Skill>();
            foreach(BalanceChanger item in items)
            {
                result.WalkDistance += item.WalkDistance;

                result.Health.Max += item.Health.Max;
                result.Health.ArmorMelee += item.Health.ArmorMelee;
                result.Health.ArmorRange += item.Health.ArmorRange;

                result.Stamina.Max += item.Stamina.Max;
                result.Stamina.WalkUseStamina += item.Stamina.WalkUseStamina;
                result.Stamina.RestEffectivity += item.Stamina.RestEffectivity;
                
                result.Sanity.Max += item.Sanity.Max;
                result.Sanity.SanityShield += item.Sanity.SanityShield;

                if(item.Resists != null) resists.AddRange(item.Resists);
                if(item.AdditionState != null) additionStates.AddRange(item.AdditionState);
                if(item.AdditionSkills != null) additionSkills.AddRange(item.AdditionSkills);
            }
            result.Resists = resists;
            result.AdditionSkills = additionSkills;
            result.AdditionState = additionStates;

            return result; 
        }
    }
    public static class FieldManipulate
    {
        public static List<T> CombineLists<T>(params List<T>[] a)
        {
            List<T> result = new List<T>();
            foreach(List<T> b in a)
            {
                result.AddRange(b);
            }
            return result;
        }
    }

    namespace IParameterManipulate
    {
        // All Interfaces 
        public interface Sendable
        {

        }
        public interface IStepEndUpdate
        {
            void StepEnd();
        }
        
        #region // State management
            
            public interface IStateBar
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

                float Immunity { get; set; }

                void Damage(Attack attack);
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
        
        #endregion
        #region // Map Object information's
            
            public interface ObjectOnMap
            {
                public const int standardVisibleDistance = 10;
                bool nowVisible(CharacterCore Object);

                List<Effect> Effects { get; set; }
                List<Effect> Resists { get; set; }

                protected delegate Type IEffect<T>() where T : IEffect;
            }

            public interface Killable : ObjectOnMap
            {
                IHealthBar Health { get; set; }
            }
            public interface GetableCrazy : ObjectOnMap
            {
                ISanityBar Sanity { get; set; }
            }
            public interface Tiredable : ObjectOnMap
            {
                IStaminaBar Stamina { get; set; }
            }
            public interface Storage : ObjectOnMap
            {
                public List<Item> Inventory { get; set; }
            }
            public interface Attacker : ObjectOnMap
            {
                public SkillCombiner SkillRealizer { get; set; }
            }

            public interface HaveName : ObjectOnMap
            {
                protected enum Names
                {
                    Jessy,
                    Yohan,
                    Ulfrik,
                    Sakarok,
                    Ung,
                    Shung
                }

                protected static string GetName(){ return ((Names)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Names)).Length)).ToString(); }
            }
        
        #endregion
        #region // Effects

            public interface ICombineWithDuplicates
            {
                IEffect CombineDuplicates(IEffect a, IEffect b);
            }

            public interface IEffect
            {
                string Name { get; } 
                Sprite Icon { get; } 
                string Description { get; }

                CharacterCore Target { get; set; }
                
                public void GetMethod(string name) { MethodInfo info = this.GetType().GetMethod(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public); 
                                                     if(info != null) info.Invoke(this, parameters: null); } 

                BalanceChanger Stats { get; set; }
            }
            public interface Effect : IEffect { bool ExistReasons(); }
            public interface HiddenEffect : Effect { }
            public interface RacePassiveEffect : IEffect { Race RaceName { get; set; } string RaceDescription { get; set; } }
        
        #endregion
    }
}
