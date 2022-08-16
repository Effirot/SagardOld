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
            
            public bool Checked { get; private set; }

            public static AttackCombiner Empty() {
                Dictionary<DamageType, List<Attack>> Sorter = new Dictionary<DamageType, List<Attack>>();
                foreach(DamageType type in Enum.GetValues(typeof(DamageType))) { Sorter.Add(type, new List<Attack>()); }
                
                return new AttackCombiner(){
                    Sorter = Sorter,
                    Senders = new HashSet<CharacterCore>(),
                    Checked = false
                };
            }

            public AttackCombiner(params Attack[] attacks) 
            {   
                Sorter = new Dictionary<DamageType, List<Attack>>(); 
                Senders = new HashSet<CharacterCore>();
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

            public static bool CheckCoords(Checkers Coordinates, params GameObject[] BlackList) 
            {
                bool result = Physics.Raycast(new Vector3(Coordinates.x, 1000, Coordinates.z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Object"));
                if(BlackList != null && result)BlackList.ToList().ForEach(a=> { if(a == hit.collider.gameObject) result = false;});
                return !result;
            }
            public static bool CheckCoords(int x, int z)
            {
                return Physics.Raycast(new Vector3(x, 1000, z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Map"));
            }
            public static bool CheckFloor(Checkers Coordinates) 
            {
                return Physics.Raycast(new Vector3(Coordinates.x, 1000, Coordinates.z), -Vector3.up, Mathf.Infinity, LayerMask.GetMask("Map"));
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

                static async IAsyncEnumerable<PatchNode> AllWays(Checkers from, Checkers to, int MaxSteps, params GameObject[] BlackListObjects)
                {
                    List<PatchNode> UnChecked = new List<PatchNode>() ;
                    List<PatchNode> Checked = new List<PatchNode>() { new PatchNode(from) }; 

                    Fill(Checked[0]);
                    float DistanceWalking = MaxSteps;

                    while(UnChecked.Count > 0 & DistanceWalking > 0){
                        PatchNode node = UnChecked[0];
                        UnChecked.ForEach(x => { if(Checkers.Distance(x.position, to) < Checkers.Distance(node.position, to)) node = x; } );

                        UnChecked.Remove(node);
                        Checked.Add(node);

                        Fill(node);

                        yield return node;
                        DistanceWalking -= node.DistanceToFrom;
                    }
                    await Task.Delay(0);

                    void Fill(PatchNode target)
                    {
                        for (int x = -1; x <= 1; x++)
                        for (int z = -1; z <= 1; z++) {
                            if(Checkers.CheckCoords(target.position + new Checkers(x, z), BlackListObjects))
                            if(Checkers.CheckFloor(target.position + new Checkers(x, z)))
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

    #region // Balancers

        [Serializable] public class Balancer
        {
            public int WalkDistance;

            public int Visible;

            [SerializeReference, SubclassSelector] public IHealthBar Health;
            [SerializeReference, SubclassSelector] public IStaminaBar Stamina;
            [SerializeReference, SubclassSelector] public ISanityBar Sanity;

            [SerializeReference, SubclassSelector] public Dictionary<string, ICustomBar> AdditionState;
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
                Balancer Result = Current.MemberwiseClone() as Balancer;
                Incoming.Health.Value = Current.Health.Value + (Current.Health.Max - Incoming.Health.Max);
                if(Incoming.ReplaceHealth) Result.Health = Incoming.Health + Current.Health;
                else Result.Health = Current.Health + Incoming.Health;

                Incoming.Sanity.Value = Current.Sanity.Value;
                if(Incoming.ReplaceSanity) Result.Sanity = Incoming.Sanity + Current.Sanity;
                else Result.Sanity = Current.Sanity + Incoming.Sanity;

                Incoming.Stamina.Value = Current.Stamina.Value;
                if(Incoming.ReplaceStamina) Result.Stamina = Incoming.Stamina + Current.Stamina;
                else Result.Stamina = Current.Stamina + Incoming.Stamina;

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

                if(Incoming.AdditionState is not null) foreach(var IncomingState in Incoming.AdditionState) {
                    if(Result.AdditionState.TryGetValue(IncomingState.Key, out ICustomBar Checking)) 
                        Checking.AddDuplicate(IncomingState.Value);
                    else
                        Result.AdditionState.Add(IncomingState.Key, IncomingState.Value); }

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

                    AdditionState = new Dictionary<string, ICustomBar>(),
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

            [SerializeReference, SubclassSelector]public Dictionary<string, ICustomBar> AdditionState;
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

                if(Incoming.AdditionState is not null) foreach(var otherState in Incoming.AdditionState) {
                    if(left.AdditionState.TryGetValue(otherState.Key, out ICustomBar Checking)) 
                        Checking.AddDuplicate(otherState.Value);
                    else
                        left.AdditionState.Add(otherState.Key, otherState.Value); }

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

                    AdditionState = new Dictionary<string, ICustomBar>(),
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
    
    namespace MapObjectInfo
    {        
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
                Checkers nowPosition { get; }

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
                
                public bool IsAlive { get; }

                public void AddDamage(params Attack[] attack);
                public void AddSanity(int damage);
                public void AddStamina(int damage);

                public void AddEffect(params Effect[] Effect);
                public void RemoveEffect(params Effect[] Effect);
                public void RemoveEffect(Predicate<Effect> predicate);
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
