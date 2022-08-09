using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System;
using UnityEngine.Events;
using SagardCL.ParameterManipulate;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using UnityAsync;

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
        ToCursorMaximum,
        ToCursorWithWalls,
        ToCursorWithWallsMaximum,
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

    [Serializable] public struct Attack
    {
        #region // Constructor parameters

            public IObjectOnMap Sender;
            public Checkers Position;
            public int Damage;
            public DamageType DamageType;
            public Effect[] Effects;

        #endregion
        #region // Overloads

            public Attack(IObjectOnMap Who, Checkers where, int Dam, DamageType Type, params Effect[] debuff)
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
            public HashSet<IObjectOnMap> Senders;
            
            public bool Checked { get; private set; }

            public static AttackCombiner Empty() {             
                Dictionary<DamageType, List<Attack>> Sorter = new Dictionary<DamageType, List<Attack>>();
                foreach(DamageType type in Enum.GetValues(typeof(DamageType))) { Sorter.Add(type, new List<Attack>()); }
                
                return new AttackCombiner(){
                    Sorter = Sorter,
                    Senders = new HashSet<IObjectOnMap>(),
                    Checked = false
                };
            }

            public AttackCombiner(params Attack[] attacks) 
            {   
                Sorter = new Dictionary<DamageType, List<Attack>>(); 
                Senders = new HashSet<IObjectOnMap>();
                Checked = attacks.Sum(a=>a.Damage) > 0;

                foreach(DamageType type in Enum.GetValues(typeof(DamageType))) { Sorter.Add(type, new List<Attack>()); }
                foreach(Attack attack in attacks) { Add(attack); } 
            }

            public AttackCombiner Add(Attack attack)
            {
                if(attack.Sender is not null) Senders.Add(attack.Sender);
                Sorter[attack.DamageType].Add(attack);

                if(attack.Damage > 0) Checked = true;

                return this;
            }
            public void Clear()
            {
                Sorter.Clear(); 
                Senders.Clear();

                foreach(DamageType type in Enum.GetValues(typeof(DamageType))) 
                    { Sorter.Add(type, new List<Attack>()); }

                Checked = false;
            }

            public List<Attack> Combine()
            {
                List<Attack> result = new List<Attack>();
                foreach (var attacks in Sorter)
                {
                    if(attacks.Value.Count == 0) continue;
                    List<Effect> effects = new List<Effect>();
                    foreach (Attack attack in attacks.Value) { effects.AddRange(attack.Effects); }
                    
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
    [Serializable] public struct Checkers
    {
        [SerializeField] int X, Z;
        [SerializeField] float UP;

        public int x => X;
        public int z => Z;
        public float up => this.UP + YUpPos();
        public float clearUp => UP;

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

            public Checkers Up(float a){ return new Checkers(this, a); }

        #endregion // =============================== Realizations
        #region // =============================== Math

            public override string ToString() { return $"{x}:{z}"; }

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

                
                
                result.Add(pos1);
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

            private PathPoint NewPathPoint(Checkers point, float pathLenghtFromStart, float heuristicEstimatePathLenght, EMoveAction moveAction, PathPoint pPoint)
            {
                PathPoint a = new PathPoint();
                a.point = point;
                a.pathLenghtFromStart = pathLenghtFromStart;
                a.heuristicEstimatePathLenght = heuristicEstimatePathLenght;
                a.moveAction = moveAction;
                a.cameFrom = pPoint;
                return a;
            }
        }

    }
    [Serializable] public class AllInOne
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

    #region // Balancers

        [Serializable] public struct Balancer
        {
            public int WalkDistance;

            public int Visible;

            [SerializeReference, SubclassSelector] public IHealthBar Health;
            [SerializeReference, SubclassSelector] public IStaminaBar Stamina;
            [SerializeReference, SubclassSelector] public ISanityBar Sanity;

            [SerializeReference, SubclassSelector] public List<ICustomBar> AdditionState;
            public List<Type> Resists;
            [Space]
            public List<Skill> Skills;

            [field: SerializeField] public int Strength { get; set; }
            [field: SerializeField] public int Accuracy { get; set; }
            [field: SerializeField] public int RezoOverclocking { get; set; }
            [field: SerializeField] public int Healing { get; set; }
            [field: SerializeField] public int Repairing { get; set; }

            [field: SerializeField] public int DamagePure { get; set; }
            [field: SerializeField] public int DamageRange { get; set; }
            
            public static Balancer operator +(Balancer Current, ReBalancer Incoming)
            {
                Incoming.Health.Value = Current.Health.Value + (Current.Health.Max - Incoming.Health.Max);
                if(Incoming.ReplaceHealth) Current.Health = Incoming.Health + Current.Health;
                else Current.Health += Incoming.Health;

                Incoming.Sanity.Value = Current.Sanity.Value;
                if(Incoming.ReplaceSanity) Current.Sanity = Incoming.Sanity + Current.Sanity;
                else Current.Sanity += Incoming.Sanity;

                Incoming.Stamina.Value = Current.Stamina.Value;
                if(Incoming.ReplaceStamina) Current.Stamina = Incoming.Stamina + Current.Stamina;
                else Current.Stamina += Incoming.Stamina;

                Current.Visible += Incoming.Visible;
                Current.WalkDistance += Incoming.WalkDistance;

                Current.Strength += Incoming.Strength;
                Current.Accuracy += Incoming.Accuracy;
                Current.RezoOverclocking += Incoming.RezoOverclocking;
                Current.Healing += Incoming.Healing;
                Current.Repairing += Incoming.Repairing;
                Current.DamagePure += Incoming.DamagePure;
                Current.DamageRange += Incoming.DamageRange;

                if(Incoming.Resists is not null & Incoming.Resists.Count > 0) { 
                    Current.Resists.AddRange(Incoming.Resists); 
                    Current.Resists = Current.Resists.Distinct().ToList(); 
                    Current.Resists.Sort(); }

                if(Incoming.AdditionState is not null) foreach(ICustomBar otherState in Incoming.AdditionState) {
                    if(Current.AdditionState.Contains(otherState)) 
                        Current.AdditionState.Find(a=>a==otherState).AddDuplicate(otherState);

                    else
                        Current.AdditionState.Add(otherState); }

                if(Incoming.Skills is not null) 
                    Current.Skills.AddRange(Incoming.Skills);

                return Current;
            }

            public static Balancer Empty() {
                return new Balancer()
                {
                    WalkDistance = 0,
                    Visible = 0,

                    Health = new Health() { Max = 0 },
                    Stamina = new Stamina() { Max = 0 },
                    Sanity = new Sanity() { Max = 0 },

                    AdditionState = new List<ICustomBar>(),
                    Resists = new List<Type>(),
                    Skills = new List<Skill>() { },

                    Strength = 0,
                    Accuracy = 0,
                    RezoOverclocking = 0,
                    Healing = 0,
                    Repairing = 0,

                    DamagePure = 0,
                    DamageRange = 0,
                };
            }
        }
        [Serializable] public struct ReBalancer
        {
            public int WalkDistance;

            public int Visible;

            [SerializeReference, SubclassSelector]public IHealthBar Health;
            public bool ReplaceHealth;
            [SerializeReference, SubclassSelector]public IStaminaBar Stamina;
            public bool ReplaceStamina;
            [SerializeReference, SubclassSelector]public ISanityBar Sanity;
            public bool ReplaceSanity;

            [SerializeReference, SubclassSelector]public List<ICustomBar> AdditionState;
            public List<Type> Resists;
            [Space]
            public List<Skill> Skills;

            [field: SerializeField] public int Strength { get; set; }
            [field: SerializeField] public int Accuracy { get; set; }
            [field: SerializeField] public int RezoOverclocking { get; set; }
            [field: SerializeField] public int Healing { get; set; }
            [field: SerializeField] public int Repairing { get; set; }

            [field: SerializeField] public int DamagePure { get; set; }
            [field: SerializeField] public int DamageRange { get; set; }

            public static ReBalancer operator +(ReBalancer left, ReBalancer right)
            {
                if(right.ReplaceHealth) {
                    left.ReplaceHealth = true;
                    
                    left.Health = right.Health + left.Health;
                }
                else left.Health += right.Health;
                if(right.ReplaceSanity) {
                    left.ReplaceSanity = true;

                    left.Sanity = right.Sanity + left.Sanity;
                }
                else left.Sanity += right.Sanity;
                if(right.ReplaceStamina) {
                    left.ReplaceStamina = true;
                    
                    left.Stamina = right.Stamina + left.Stamina;
                }
                else left.Stamina += right.Stamina;

                left.Visible += right.Visible;
                left.WalkDistance += right.WalkDistance;

                left.Strength += right.Strength;
                left.Accuracy += right.Accuracy;
                left.RezoOverclocking += right.RezoOverclocking;
                left.Healing += right.Healing;
                left.Repairing += right.Repairing;
                left.DamagePure += right.DamagePure;
                left.DamageRange += right.DamageRange;

                if(right.Resists is not null) { 
                    left.Resists.AddRange(right.Resists); 
                    left.Resists = left.Resists.Distinct().ToList(); 
                    left.Resists.Sort(); }

                if(right.AdditionState is not null) foreach(ICustomBar otherState in right.AdditionState) {
                    if(left.AdditionState.Contains(otherState)) 
                        left.AdditionState.Find(a=>a==otherState).AddDuplicate(otherState);

                    else
                        left.AdditionState.Add(otherState); }

                if(right.Skills is not null) 
                    left.Skills.AddRange(right.Skills);

                return left;
            }

            public static ReBalancer Empty() {
                return new ReBalancer()
                {
                    WalkDistance = 0,
                    Visible = 0,

                    Health = new Health() { Max = 0 },
                    ReplaceHealth = false,

                    Stamina = new Stamina() { Max = 0 },
                    ReplaceStamina = false,

                    Sanity = new Sanity() { Max = 0 },
                    ReplaceSanity = false,

                    AdditionState = new List<ICustomBar>(),
                    Resists = new List<Type>(),
                    Skills = new List<Skill>() { },

                    Strength = 0,
                    Accuracy = 0,
                    RezoOverclocking = 0,
                    Healing = 0,
                    Repairing = 0,

                    DamagePure = 0,
                    DamageRange = 0,
                };
            }
        }

    #endregion
    
    namespace ParameterManipulate
    {        
        #region // State management
            
            public interface IStateBar
            {
                
                Color BarColor{ get; }

                int Value { get; set; }
                int Max { get; set; }  

                public void Use(int value) 
                {
                    Value += value;
                }
            }

            public interface IHealthBar : IStateBar
            {
                public static IHealthBar operator + (IHealthBar a, IHealthBar b) 
                {
                    IHealthBar result = Activator.CreateInstance(a.GetType()) as IHealthBar;

                    result.Value = a.Value;
                    result.Max = a.Max + b.Max;
                    result.ArmorMelee = a.ArmorMelee + b.ArmorMelee;
                    result.ArmorRange = a.ArmorRange + b.ArmorRange;
                    result.Immunity = a.Immunity + b.Immunity;

                    return result;
                }
                
                void IStateBar.Use(int value) { }

                int ArmorMelee { get; set; } 
                int ArmorRange { get; set; }

                float Immunity { get; set; }

                public void Damage(Attack attack)
                {
                    switch(attack.DamageType)
                    {
                        case DamageType.Pure: Value -= Pure(attack); break;
                        case DamageType.Melee: Value -= Melee(attack); break;
                        case DamageType.Range: Value -= Range(attack); break;
                        case DamageType.Rezo: Value -= Rezo(attack); break;
            
                        case DamageType.Heal: Value += Heal(attack); break;
                        case DamageType.Repair: Value += Repair(attack); break;

                        case DamageType.Effect: Value -= Effect(attack); break;
                    }
                }
                public void Damage(Attack.AttackCombiner attackCombiner)
                {
                    List<Attack> attackList = attackCombiner.Combine();
                    foreach(Attack attack in attackList)
                    {
                        switch(attack.DamageType)
                        {
                            case DamageType.Pure: Value -= Pure(attack); break;
                            case DamageType.Melee: Value -= Melee(attack); break;
                            case DamageType.Range: Value -= Range(attack); break;
                            case DamageType.Rezo: Value -= Rezo(attack); break;
                
                            case DamageType.Heal: Value += Heal(attack); break;
                            case DamageType.Repair: Value += Repair(attack); break;

                            case DamageType.Effect: Value -= Effect(attack); break;
                        }
                    }
                }
                public int GetDamage(Attack attack)
                {
                    switch(attack.DamageType)
                    {
                        case DamageType.Pure: return -Pure(attack); 
                        case DamageType.Melee: return -Melee(attack); 
                        case DamageType.Range: return -Range(attack); 
                        case DamageType.Rezo: return -Rezo(attack); 
            
                        case DamageType.Heal: return +Heal(attack); 
                        case DamageType.Repair: return +Repair(attack); 

                        case DamageType.Effect: return -Effect(attack); 
                    }
                    return 0;
                }

                protected virtual int Pure(Attack attack) { return Mathf.Clamp(attack.Damage, 0, 1000); }
                protected virtual int Melee(Attack attack) { return Mathf.Clamp(attack.Damage - ArmorMelee, 0, 1000); }
                protected virtual int Range(Attack attack) { return Mathf.Clamp(attack.Damage - ArmorRange, 0, 1000); }
                protected virtual int Rezo(Attack attack) { return Mathf.Clamp(attack.Damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f), 0, 1000); }

                protected virtual int Heal(Attack attack) { return Mathf.Clamp((int)Mathf.Round((Value + attack.Damage) * ((1 - Immunity) * 0.5f)), 0, Max - Value); }
                protected virtual int Repair(Attack attack) { return -1; }

                protected virtual int Effect(Attack attack) { return Mathf.Clamp((int)Mathf.Round(attack.Damage * (1 - Immunity)), 0, 1000); }

                
            }
            public interface IStaminaBar : IStateBar
            {
                void GetTired(int value);
                void IStateBar.Use(int value)
                {
                    Value = Mathf.Clamp(Value + value, 0, Max);
                }
                int RestEffectivity{ get; set; }
                int WalkUseStamina{ get; set; }

                public static IStaminaBar operator +(IStaminaBar left, IStaminaBar right)
                {
                    IStaminaBar result = Activator.CreateInstance(left.GetType()) as IStaminaBar;

                    result.Value = left.Value;
                    result.Max = left.Max + right.Max;
                    result.RestEffectivity = left.RestEffectivity + right.RestEffectivity;
                    result.WalkUseStamina = left.WalkUseStamina + right.WalkUseStamina;

                    return result;
                }

                void Rest();
            }
            public interface ISanityBar : IStateBar
            {
                void IStateBar.Use(int value)
                {
                    Value = value<0? 
                        Value - Mathf.Clamp(Mathf.Abs(value) - SanityShield, 0, 1000) : 
                        Mathf.Clamp(Value + value, 0, Max);
                }

                public static ISanityBar operator +(ISanityBar left, ISanityBar right)
                {
                    ISanityBar result = Activator.CreateInstance(left.GetType()) as ISanityBar;

                    result.Value = left.Value;
                    result.Max = left.Max + right.Max;
                    result.SanityShield = left.SanityShield + right.SanityShield;

                    return result;
                }

                int SanityShield { get; set; } 
            }
            public interface ICustomBar : IStateBar
            {
                void IStateBar.Use(int value) { UseMath(value); }
                void UseMath(int value);

                void AddDuplicate(ICustomBar duplicate);
            }

        #endregion
        
        #region // Map Object information's
            
            public interface IObjectOnMap
            {
                Balancer BaseBalance { get; }
                Balancer NowBalance { get; }

                protected static IObjectOnMap objectClassTo<T>(IObjectOnMap classObject) where T : IObjectOnMap
                {
                    lock(classObject)
                    {
                        if(classObject is T) return (T)classObject;
                        else return (IObjectOnMap)classObject; 
                    }
                }

                public Attack.AttackCombiner TakeDamageList { get; set; }
                
                public bool Alive { get; }

                public void AddDamage(params Attack[] attack) { }

                public void AddSanity(int damage) { }

                public void AddStamina(int damage) { }

                public void AddState(ICustomBar state) { }

                public void AddEffect(params Effect[] Effect) { }
                public void AutoRemoveEffect() { }
                public void RemoveEffect(params Effect[] Effect) { }
                public void InvokeEffects(string Method) { }
            }

            public interface IDeadable : IObjectOnMap
            {               
                new void AddDamage(params Attack[] attacks);

                void LostHealth();
            }
            public interface IGetableCrazy : IObjectOnMap
            {

            }
            public interface ITiredable : IObjectOnMap
            {
                bool WillRest{ get; set; }
            }
            public interface IAnotherBars : IObjectOnMap
            {

            }
            
            public interface IInvisible : IObjectOnMap
            {

            }

            public interface IStorage : IObjectOnMap
            {
                public List<Item> Inventory { get; set; }
            }
            public interface IEffector : IObjectOnMap
            {
                List<Effect> Effects { get; set; }

                protected delegate Type IEffect<T>() where T : IEffect;

                new void AddEffect(params Effect[] Effect);
                new void AutoRemoveEffect();
                new void RemoveEffect(params Effect[] Effect);

                new void InvokeEffects(string Method);
            }

            public interface IAttacker : IObjectOnMap
            {
                int SkillIndex { get; set; }
                public Skill CurrentSkill { get; }
            }
            public interface IWalk : IObjectOnMap, ITiredable
            {
                List<Checkers> WalkWay { get; set; } 
            }
            
            public interface HaveID : IObjectOnMap
            {
                private static int LastID = 1;
                protected static string GetName(){ LastID++; return $"{LastID.ToString()}:unit"; }
            }
        
        #endregion
        
        #region // Effects
            public interface IEffect
            {
                string Name { get; } 
                Sprite Icon { get; } 
                string Description { get; }

                IObjectOnMap Target { get; set; }
                
                public void InvokeMethod(string name) { MethodInfo info = this.GetType().GetMethod(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public); 
                                                     if(info != null) info.Invoke(this, parameters: null); } 

                ReBalancer Stats { get; }
            }
            
            public interface Effect : IEffect { bool Workable(); }
            public interface ICombineWithDuplicates : Effect { void NewEffectAdded(Effect NewEffect); }
            public interface HiddenEffect : Effect { }
            public interface OneUse : Effect { new public Balancer? Stats { get{ return null; } set { } } new bool Workable() { return false; } }
            
            interface MapEffectTransporter : OneUse 
            { 
                new public IObjectOnMap Target { get { return null; } set { } }

                MapEffect Effect { get; }
            }
            
            public interface RacePassiveEffect : IEffect { string RaceDescription { get; } }
        
        #endregion
        #region // Map Effects
            public interface MapEffect
            {
                Checkers Where { get; set; }
                int Level{ set; }
            }
            
            interface LandscapeDeform : MapEffect { bool DestroyWhenZero { get; } }
        #endregion
    }
}
