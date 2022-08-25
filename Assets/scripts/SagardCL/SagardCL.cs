using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System;
using UnityEngine.Events;
using SagardCL.MapObjectInfo;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using UnityAsync;
using SagardCL.Actions;
using Action = SagardCL.Actions.Action;

namespace SagardCL //Class library
{
    public delegate T Argument<out T>(object value);
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

    public enum Resist 
    {
        Man, Woman, BattleHelicopter, // Gender

        Persistent, // EffectBlocking
        Miasm,
        StrongImmunity,
        Bloodless,
        NonCombustible,
        HardenedSkin,
        Armless, Legless,  

        YoursAmongStrangers, YoursAmongMph, // UI blocking

        NoRezo, NoHeal, NoRepair, NoPure // Damage blocking
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

            public CharacterCore Sender;
            public Checkers position;
            public int Damage;
            public DamageType DamageType;
            public Effect[] Effects;
            public MapEffect[] MapEffects;

        #endregion
        #region // Overloads

            public Attack(CharacterCore Who, Checkers where, int Dam, DamageType Type, params Effect[] effects)
            {
                Sender = Who;
                position = where;
                Damage = Mathf.Clamp(Dam, 0, 10000);

                DamageType = Type;
                Effects = effects;
                MapEffects = null;
            }
            public Attack(CharacterCore Who, Checkers where, int Dam, DamageType Type, MapEffect[] mapEffects, params Effect[] effects)
            {
                Sender = Who;
                position = where;
                Damage = Mathf.Clamp(Dam, 0, 10000);

                DamageType = Type;
                Effects = effects;
                MapEffects = mapEffects;
            }
            public Attack(int Dam, DamageType Type, params Effect[] effects)
            {
                Sender = null;
                position = new Checkers();
                Damage = Mathf.Clamp(Dam, 0, 10000);

                DamageType = Type;
                Effects = effects;
                MapEffects = null;
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
    
        public class AttackCombiner
        {
            public Dictionary<DamageType, List<Attack>> Sorter = new Dictionary<DamageType, List<Attack>>();
            public HashSet<CharacterCore> Senders = new HashSet<CharacterCore>();
            
            public bool Contains => Sorter.Count != 0;

            public static AttackCombiner Empty() {
                Dictionary<DamageType, List<Attack>> Sorter = new Dictionary<DamageType, List<Attack>>();
                foreach(DamageType type in Enum.GetValues(typeof(DamageType))) { Sorter.Add(type, new List<Attack>()); }
                
                return new AttackCombiner(){
                    Sorter = Sorter,
                    Senders = new HashSet<CharacterCore>(),
                };
            }

            public AttackCombiner(params Attack[] attacks) 
            {
                foreach(DamageType type in Enum.GetValues(typeof(DamageType))) { Sorter.Add(type, new List<Attack>()); }
                foreach(Attack attack in attacks) { Add(attack); } 
            }

            public AttackCombiner Add(params Attack[] attacks){
                
                foreach(Attack attack in attacks){
                    if(attack.Sender is not null) Senders.Add(attack.Sender);
                    Sorter[attack.DamageType].Add(attack);}

                return this;
            }
            public AttackCombiner Remove(Attack attack){
                Sorter[attack.DamageType].Remove(attack);
                return this;
            }
            public AttackCombiner RemoveAll(Predicate<Attack> math){
                foreach(var list in Sorter)
                    list.Value.RemoveAll(math);
                return this;
            }

            public void Clear()
            {
                Sorter.Clear(); 
                Senders.Clear();

                foreach(DamageType type in Enum.GetValues(typeof(DamageType))) 
                    { Sorter.Add(type, new List<Attack>()); }
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
                foreach (Attack attacks in Combine())
                    result += attacks.Color();
                
                result.r -= 1 / (float)Combine().Sum(a=>(float)a.Damage / 3);
                result.g -= 1 / (float)Combine().Sum(a=>(float)a.Damage / 3);
                result.b -= 1 / (float)Combine().Sum(a=>(float)a.Damage / 3);
                
                return result;
            }
        }
    }
    [Serializable] public struct Checkers : IEquatable<Checkers>
    {
        [field: SerializeField] public int x { get; private set; }
        [field: SerializeField] public int z { get; private set; }
        [field: SerializeField] private float UP { get; set; }
        [field: SerializeField] public byte layer { get; private set; }

        public LayerMask CurrentLayer => LayerMask.GetMask("Map" + layer.ToString());

        public float up => this.UP + YUpPos();
        public float clearUp => UP;

        float YUpPos()
        {
            if (Physics.Raycast(new Vector3(x, 1000, z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, CurrentLayer))
                return hit.point.y;
            return 0;
        }

        #region // =============================== Realizations

            public Checkers(float x, float z, float UPadd = 0) { 
                this.x = (int)Mathf.Round(x); 
                this.z = (int)Mathf.Round(z); 
                this.UP = UPadd; 
                layer = 0; }
            public Checkers(float x, float z, byte Layer, float UPadd = 0) { 
                this.x = (int)Mathf.Round(x); 
                this.z = (int)Mathf.Round(z); 
                this.UP = UPadd; 
                layer = Layer; }
            public Checkers(Vector3 Vector, float UPadd = 0) { 
                this.x = (int)Mathf.Round(Vector.x); 
                this.z = (int)Mathf.Round(Vector.z); 
                this.UP = UPadd; 
                layer = 0; }
            public Checkers(Vector3 Vector, byte Layer, float UPadd = 0) { 
                this.x = (int)Mathf.Round(Vector.x); 
                this.z = (int)Mathf.Round(Vector.z); 
                this.UP = UPadd; 
                layer = Layer; }

            public static implicit operator Vector3(Checkers a) { return new Vector3(a.x, a.up, a.z); }
            public static implicit operator Checkers(Vector3 a) { return new Checkers(a.x, a.z); }

            public static Checkers operator +(Checkers a, Checkers b) { return new Checkers(a.x + b.x, a.z + b.z, a.layer, a.up); }
            public static Checkers operator -(Checkers a, Checkers b) { return new Checkers(a.x - b.x, a.z - b.z, a.layer, a.up); }
            public static Checkers operator *(Checkers a, float b) { return new Checkers(a.x * b, a.z * b, a.layer, a.up); }
            public static Checkers operator *(float b, Checkers a) { return new Checkers(a.x * b, a.z * b, a.layer, a.up); }
            public static bool operator ==(Checkers a, Checkers b) { return a.x == b.x & a.z == b.z; }
            public static bool operator !=(Checkers a, Checkers b) { return !(a.x == b.x & a.z == b.z); }
            public bool Equals(Checkers other) { return this == other; }
            
            public override int GetHashCode() { return 0; }  
            public override bool Equals(object o) { return true; } 

            public Checkers Up(float a) => new Checkers(x, z, layer, a); 
            public Checkers Layer(byte a) => new Checkers(x, z, a, UP); 

        #endregion // =============================== Realizations
        #region // =============================== Math

            public override string ToString() { return $"{x}:{z}-{layer} {up.ToString()}"; }

            public enum CheckersDistMode{ NoHeight, Height, OnlyHeight, }
            public static float Distance(Checkers a, Checkers b, CheckersDistMode Mode = CheckersDistMode.NoHeight)
            {
                if(Mode == CheckersDistMode.OnlyHeight) return Mathf.Abs(a.up - b.up);
                if(Mode == CheckersDistMode.Height) return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.ToVector3().y - b.ToVector3().y, 2) + Mathf.Pow(a.z - b.z, 2));
                return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.z - b.z, 2));
            }
            public static float Distance(Vector3 a, Vector3 b, CheckersDistMode Mode = CheckersDistMode.NoHeight)
            {
                if(Mode == CheckersDistMode.Height) return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2) + Mathf.Pow(a.z - b.z, 2));
                return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.z - b.z, 2));
            }

            public Vector3 ToVector3(bool UseClearUp = false){ return new Vector3(x, UseClearUp? clearUp : up, z); }
            public static List<Vector3> ToVector3List(params Checkers[] checkers) 
            { 
                List<Vector3> list = new List<Vector3>();
                foreach(Checkers checker in checkers){ list.Add(checker.ToVector3()); }
                return list;        
            }
            public static List<Checkers> ToCheckersList(float up = 0, params Vector3[] vector3) 
            { 
                List<Checkers> list = new List<Checkers>();
                foreach(Checkers vector in vector3){ list.Add(new Checkers(vector, up)); }
                return list;        
            }

            public bool CheckCoords(params GameObject[] BlackList) 
            {
                bool result = Physics.Raycast(new Vector3(x, 1000, z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("FactualObject", "PlanedObject"));
                if(BlackList != null && result)BlackList.ToList().ForEach(a=> { if(a == hit.collider.gameObject) result = false;});
                return !result;
            }
            public bool CheckFloor() => Physics.Raycast(new Vector3(x, 1000, z), -Vector3.up, Mathf.Infinity, CurrentLayer);

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
            public Checkers Normalize() 
            { 
                float distance = Mathf.Sqrt(x * x + z * z);
                return new Checkers(x / distance, z / distance); 
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
                        pos1.x += signX;
                    }
                    if(error2 < deltaX) 
                    {
                        error += deltaX;
                        pos1.z += signZ;
                    }
                    result.Add(pos1);
                }
                

                

                return result;

            }
            

            public static class PatchWay
            {
                class PatchNode
                {
                    public Checkers position;
                    public PatchNode from;

                    public float DistanceToFrom => Checkers.Distance(position, from.position, CheckersDistMode.Height);

                    public PatchNode(Checkers position, PatchNode from = null) { this.position = position; this.from = from;  }
                }
                public static async Task<List<Checkers>> WayTo(Checkers a, Checkers b, int MaxSteps, float CheckersUp, params GameObject[] BlackListObjects) 
                {   
                    List<PatchNode> Nodes = new List<PatchNode>();
                    await foreach(PatchNode node in AllWays(a, b, MaxSteps, BlackListObjects)) Nodes.Add(node);

                    PatchNode CheckingPoint = Nodes[0];
                    foreach(PatchNode checker in Nodes)
                        if(Checkers.Distance(b, checker.position) < Checkers.Distance(b, CheckingPoint.position))
                            CheckingPoint = checker;
                    
                    List<Checkers> result = new List<Checkers>();
                    
                    while(CheckingPoint != null){
                        result.Add(CheckingPoint.position.Up(CheckersUp));
                        CheckingPoint = CheckingPoint.from;
                    }
                    result.Add(a.Up(CheckersUp));
                    result.Reverse();
                    //result.Add(b.Up(CheckersUp));

                    return result;
                }

                static async IAsyncEnumerable<PatchNode> AllWays(Checkers from, Checkers to, float MaxSteps, params GameObject[] BlackListObjects)
                {
                    List<PatchNode> UnChecked = new List<PatchNode>() ;
                    List<PatchNode> Checked = new List<PatchNode>() { new PatchNode(from) }; 

                    Fill(Checked[0]);

                    while(UnChecked.Count > 0 & MaxSteps > 0){
                        PatchNode node = UnChecked[0];
                        UnChecked.ForEach(x => { if(Checkers.Distance(x.position, to) < Checkers.Distance(node.position, to)) node = x; } );

                        UnChecked.Remove(node);
                        Checked.Add(node);

                        Fill(node);

                        yield return node;
                        MaxSteps -= node.DistanceToFrom;
                    }
                    await Task.Delay(0);

                    void Fill(PatchNode target)
                    {
                        for (int x = -1; x <= 1; x++)
                        for (int z = -1; z <= 1; z++) {
                            if((target.position + new Checkers(x, z)).CheckCoords(BlackListObjects))
                            if((target.position + new Checkers(x, z)).CheckFloor())
                            if(Checkers.Distance(target.position + new Checkers(x, z), target.position, CheckersDistMode.OnlyHeight) < 1.3f)
                            
                            if(!UnChecked.Exists(a=>a.position == target.position + new Checkers(x, z)))
                            if(!Checked.Exists(a=>a.position == target.position + new Checkers(x, z)))
                                UnChecked.Add(new PatchNode(target.position + new Checkers(x, z), target));
                        }
                    }
                }
            }
        #endregion // =============================== Math 
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




    
    namespace MapObjectInfo
    {        
        #region // Balancers

            [Serializable] public class Balancer
            {
                public int WalkDistance;

                public int Visible;

                [SerializeReference, SubclassSelector] public IHealthBar Health;
                [SerializeReference, SubclassSelector] public IStaminaBar Stamina;
                [SerializeReference, SubclassSelector] public ISanityBar Sanity;

                [SerializeReference, SubclassSelector] public List<ICustomBar> AdditionState;
                public List<Type> Resists;
                [Space]
                public List<Action> Skills;

                [field: SerializeField] public int Strength { get; set; }
                [field: SerializeField] public int Accuracy { get; set; }
                [field: SerializeField] public int RezoOverclocking { get; set; }
                [field: SerializeField] public int Healing { get; set; }
                [field: SerializeField] public int Repairing { get; set; }

                [field: SerializeField] public int DamagePure { get; set; }
                [field: SerializeField] public int DamageRange { get; set; }
                
                public static Balancer operator +(Balancer Current, ReBalancer Incoming)
                {
                    Balancer Result = Current.MemberwiseClone() as Balancer;
                    
                    if(Incoming.ReplaceHealth) Result.Health = Incoming.Health + Current.Health;
                    else Result.Health = Current.Health + Incoming.Health;
                    Result.Health.Value = Current.Health.Value + Incoming.Health.Max;

                    if(Incoming.ReplaceSanity) Result.Sanity = Incoming.Sanity + Current.Sanity;
                    else Result.Sanity = Current.Sanity + Incoming.Sanity;
                    Result.Sanity.Value = Current.Sanity.Value + Incoming.Sanity.Max;

                    if(Incoming.ReplaceStamina) Result.Stamina = Incoming.Stamina + Current.Stamina;
                    else Result.Stamina = Current.Stamina + Incoming.Stamina;
                    Result.Stamina.Value = Current.Stamina.Value + Incoming.Stamina.Max;

                    Result.Visible = Current.Visible + Incoming.Visible;
                    Result.WalkDistance = Current.WalkDistance + Incoming.WalkDistance;

                    Result.Strength = Current.Strength + Incoming.Strength;
                    Result.Accuracy = Current.Accuracy + Incoming.Accuracy;
                    Result.RezoOverclocking = Current.RezoOverclocking + Incoming.RezoOverclocking;
                    Result.Healing = Current.Healing + Incoming.Healing;
                    Result.Repairing = Current.Repairing + Incoming.Repairing;
                    Result.DamagePure = Current.DamagePure + Incoming.DamagePure;
                    Result.DamageRange = Current.DamageRange + Incoming.DamageRange;

                    if(Incoming.Resists is not null & Incoming.Resists.Count > 0) { 
                        Result.Resists.AddRange(Incoming.Resists); 
                        Result.Resists = Current.Resists.Distinct().ToList(); 
                        Result.Resists.Sort(); }

                    // if(Incoming.AdditionState is not null) foreach(ICustomBar IncomingState in Incoming.AdditionState) {
                    //     if(Result.AdditionState.Exists(a=>a.GetType() == IncomingState.GetType())) 
                    //         Checking.AddDuplicate(IncomingState.Value);
                    //     else
                    //         Result.AdditionState.Add(IncomingState.Key, IncomingState.Value); }

                    if(Incoming.Skills is not null) 
                        Result.Skills.AddRange(Incoming.Skills);

                    return Result;
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
                        Skills = new List<Action>() { },

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
                public List<Action> Skills;

                [field: SerializeField] public int Strength { get; set; }
                [field: SerializeField] public int Accuracy { get; set; }
                [field: SerializeField] public int RezoOverclocking { get; set; }
                [field: SerializeField] public int Healing { get; set; }
                [field: SerializeField] public int Repairing { get; set; }

                [field: SerializeField] public int DamagePure { get; set; }
                [field: SerializeField] public int DamageRange { get; set; }

                public static ReBalancer operator +(ReBalancer left, ReBalancer Incoming)
                {
                    if(Incoming.ReplaceHealth) {
                        left.ReplaceHealth = true;
                        
                        left.Health = Incoming.Health + left.Health;
                    }
                    else left.Health += Incoming.Health;
                    if(Incoming.ReplaceSanity) {
                        left.ReplaceSanity = true;

                        left.Sanity = Incoming.Sanity + left.Sanity;
                    }
                    else left.Sanity += Incoming.Sanity;
                    if(Incoming.ReplaceStamina) {
                        left.ReplaceStamina = true;
                        
                        left.Stamina = Incoming.Stamina + left.Stamina;
                    }
                    else left.Stamina += Incoming.Stamina;

                    left.Visible += Incoming.Visible;
                    left.WalkDistance += Incoming.WalkDistance;

                    left.Strength += Incoming.Strength;
                    left.Accuracy += Incoming.Accuracy;
                    left.RezoOverclocking += Incoming.RezoOverclocking;
                    left.Healing += Incoming.Healing;
                    left.Repairing += Incoming.Repairing;
                    left.DamagePure += Incoming.DamagePure;
                    left.DamageRange += Incoming.DamageRange;

                    if(Incoming.Resists is not null) { 
                        left.Resists.AddRange(Incoming.Resists); 
                        left.Resists = left.Resists.Distinct().ToList(); 
                        left.Resists.Sort(); }

                    // if(Incoming.AdditionState is not null) foreach(var otherState in Incoming.AdditionState) {
                    //     if(left.AdditionState.TryGetValue(otherState.Key, out ICustomBar Checking)) 
                    //         Checking.AddDuplicate(otherState.Value);
                    //     else
                    //         left.AdditionState.Add(otherState.Key, otherState.Value); }

                    if(Incoming.Skills is not null) 
                        left.Skills.AddRange(Incoming.Skills);

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
                        Skills = new List<Action>() { },

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

        #region // State management
            
            public interface IStateBar
            {
                Color BarColor{ get; }

                public void Use(int value) {}
                int Value { get; set; }
                int Max { get; set; }  

                object Clone();
            }

            public interface IHealthBar : IStateBar
            {
                public static IHealthBar operator + (IHealthBar a, IHealthBar b) 
                {
                    IHealthBar result = a.Clone() as IHealthBar;

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

                protected int Pure(Attack attack) { return Mathf.Clamp(attack.Damage, 0, 1000); }
                protected int Melee(Attack attack) { return Mathf.Clamp(attack.Damage - ArmorMelee, 0, 1000); }
                protected int Range(Attack attack) { return Mathf.Clamp(attack.Damage - ArmorRange, 0, 1000); }
                protected int Rezo(Attack attack) { return Mathf.Clamp(attack.Damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f), 0, 1000); }

                protected int Heal(Attack attack) { return Mathf.Clamp((int)Mathf.Round((Value + attack.Damage) * ((1 - Immunity) * 0.6f)), 0, Max - Value); }
                protected int Repair(Attack attack) { return -1; }

                protected int Effect(Attack attack) { return Mathf.Clamp((int)Mathf.Round(attack.Damage * (1 - Immunity)), 0, 1000); }

                
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

                public static IStaminaBar operator +(IStaminaBar left, IStaminaBar Incoming)
                {
                    IStaminaBar result = left.Clone() as IStaminaBar;

                    result.Value = left.Value;
                    result.Max = left.Max + Incoming.Max;
                    result.RestEffectivity = left.RestEffectivity + Incoming.RestEffectivity;
                    result.WalkUseStamina = left.WalkUseStamina + Incoming.WalkUseStamina;

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

                public static ISanityBar operator +(ISanityBar left, ISanityBar Incoming)
                {
                    ISanityBar result = left.Clone() as ISanityBar;

                    result.Value = left.Value;
                    result.Max = left.Max + Incoming.Max;
                    result.SanityShield = left.SanityShield + Incoming.SanityShield;

                    return result;
                }

                int SanityShield { get; set; } 
            }
            public interface ICustomBar : IStateBar
            {
                void AddDuplicate(ICustomBar duplicate);
                bool UseChecking(int value);
            }

            public interface RefillType : ICustomBar
            {
                void Refill(int RefillPower);
            }
            public interface MoneyRefill : RefillType
            {
                int RefillCost { get; }
            }

        #endregion
        
        #region // Map Object information's
            
            public interface IObjectOnMap
            {
                Race Race { get; }

                List<string> Tag { get; }
                Checkers nowPosition { get; }
                
                protected static IObjectOnMap objectClassTo<T>(IObjectOnMap classObject) where T : IObjectOnMap
                {
                    lock(classObject)
                    {
                        if(classObject is T) return (T)classObject;
                        else return (IObjectOnMap)classObject; 
                    }
                }
        
                void AddEffect(params Effect[] Effect);
                void RemoveEffect(params Effect[] Effect);
                void RemoveEffect(Predicate<Effect> predicate);            
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
                
                public void InvokeMethod(string name) { MethodInfo info = this.GetType().GetMethod(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public); 
                                                     if(info != null) info.Invoke(this, parameters: null); } 

                ReBalancer Stats { get; }
            }
            
            public interface Effect : IEffect { CharacterCore Target { get; set; } bool Workable(); }
            public interface ICombineWithDuplicates : Effect { void NewEffectAdded(Effect NewEffect); }
            public interface HiddenEffect : Effect { }
            public interface OneUse : Effect { new public Balancer Stats { get{ return null; } set { } } new bool Workable() { return false; } }
            
            public interface RacePassiveEffect : IEffect { CharacterCore Target { get; set; } string RaceDescription { get; } }
        
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

    public static class UsefulExtern
    {
        public static Checkers ToCheckers(this Vector3 position, float Up = 0)
        {
            return new Checkers(position.x, position.z, Up);
        }

        public static T MinBy<T>(this IEnumerable<T> obj, Func<T, float> searchBy)
        {
            T result = default(T);
            foreach(T Object in obj){
                if(searchBy(Object) < searchBy(result))
                    result = Object;
            }
            return result;
        }
        public static T MinBy<T>(this IEnumerable<T> obj, Func<T, double> searchBy)
        {
            T result = default(T);
            foreach(T Object in obj){
                if(searchBy(Object) < searchBy(result))
                    result = Object;
            }
            return result;
        }
        public static T MaxBy<T>(this IEnumerable<T> obj, Func<T, float> searchBy)
        {
            T result = default(T);
            foreach(T Object in obj){
                if(searchBy(Object) > searchBy(result))
                    result = Object;
            }
            return result;
        }
        public static T MaxBy<T>(this IEnumerable<T> obj, Func<T, double> searchBy)
        {
            T result = default(T);
            foreach(T Object in obj){
                if(searchBy(Object) > searchBy(result))
                    result = Object;
            }
            return result;
        }
    }
}
