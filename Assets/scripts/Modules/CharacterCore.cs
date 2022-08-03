using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using SagardCL.ParameterManipulate;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Reflection;
using Random = UnityEngine.Random;
using UnityAsync;

public class CharacterCore : MonoBehaviour, IDeadable, IGetableCrazy, ITiredable, IStorage, IAttacker, IWalk, HaveName {
    #region // ============================================================= Useful stuff =============================================================================================
        protected Vector3 position{ get{ return this.transform.position; } set{ this.transform.position = value; } }

        [field: SerializeField] private protected AllInOne MPlaner { get; set; }
        [field: SerializeField] private protected AllInOne APlaner { get; set; }

        IAttacker Attacker => (IAttacker)IObjectOnMap.objectClassTo<IAttacker>(this);
                
        public void ChangeFigureColor(Color color, float speed, Material material) { StartCoroutine(ChangeMaterialColor(material, color, speed)); }
        public void ChangeFigureColor(Color color, float speed, Material[] material = null)
        { 
            if(material == null) material = new Material[] { transform.parent.Find("MPlaner/Platform").GetComponent<Renderer>().material, 
                                                            transform.parent.Find("MPlaner/Platform/Figure").GetComponent<Renderer>().material };
            foreach (Material mat in material) StartCoroutine(ChangeMaterialColor(mat, color, speed)); 
        }
        public void ChangeFigureColorWave(Color color, float speed, Material[] material = null)
        { 
            if(material == null) material = new Material[] { transform.parent.Find("MPlaner/Platform").GetComponent<Renderer>().material, 
                                                            transform.parent.Find("MPlaner/Platform/Figure").GetComponent<Renderer>().material };
            
            List<Task> tasks = new List<Task>();
            foreach (Material mat in material) { StartCoroutine(Wave(mat, color, speed)); }

            IEnumerator Wave(Material material, Color color, float speed)
            { 
                Color Save = material.color;
                yield return ChangeMaterialColor(material, color, speed); 
                yield return ChangeMaterialColor(material, Save, speed); 
            }
        }
        static protected IEnumerator ChangeMaterialColor(Material material, Color color, float speed)
        {
            while(material.color != color)
            {
                material.color = Color.Lerp(material.color, color, speed);
                speed *= 1.1f;
                yield return new WaitForFixedUpdate();
            }
        }

        public virtual Type type{ get{ return typeof(CharacterCore); } }

        public static List<T> CombineLists<T>(params List<T>[] a)
        {
            List<T> result = new List<T>();
            foreach(List<T> b in a)
            {
                result.AddRange(b);
            }
            return result;
        }
    #endregion
    #region // =========================================================== All parameters =================================================================================================
    
        protected virtual async void Start()
        {
            TakeDamageList.Clear();

            transform.parent.name = HaveName.GetName();
            name += $"({transform.parent.name})";

            BaseHealth = Health;
            BaseStamina = Stamina;
            BaseSanity = Sanity;
            
            InGameEvents.MapUpdate.AddListener(async() => {
                await MovePlannerSet(MPlaner.position, MPlaner.Renderer.enabled);
                await AttackPlannerSet(APlaner.position, MPlaner.Renderer.enabled);
            });
            InGameEvents.StepSystem.Add(FindStepStage);
            InGameEvents.AttackTransporter.AddListener((a) => { 
                List<Attack> find = a.FindAll((a) => a.Position == new Checkers(position));
                foreach(Attack attack in find) if(attack.Position == new Checkers(position)) IObjectOnMap.objectClassTo<IAttacker>(this).AddDamage(attack);
            });
            InGameEvents.StepEnd.AddListener(EveryStepEnd);
            
            AfterInventoryUpdate();

            await Task.Delay(10);
            position = new Checkers(position);
            MPlaner.position = new Checkers(position);
        }

        [field: SerializeField] public Race Race { get; private set; }

        [SerializeField] bool _Corpse = false;
        public bool Corpse { get { return _Corpse; }
        set { 
                if(_Corpse != value)
                    if(value) 
                    {
                        Effects.RemoveAll(a=>a is OneUse | !a.Workable());
                        //Effects.Add(Decomposition.Base(this));

                        ChangeFigureColor(new Color(0.5f, 0.5f, 0.5f), 0.2f);
                    }
                    else 
                    {
                        Effects.Remove(Effects.Find(a=>a is Decomposition));

                        ChangeFigureColor(new Color(1f, 1f, 1f), 0.2f);
                    }
                _Corpse = value;
            }
        } 
        [field: SerializeField]public int WalkDistance { get; set; } = 5;
        
        #region // ================================== parameters

            public const int maxVisibleDistance = 10;
            public float visibleCoefficient = 1;
            [SerializeField] bool AlwaysVisible = false;
            [SerializeField] bool WallIgnoreVisible = false;

            public bool nowVisible(CharacterCore Object) { return (Checkers.Distance(this.position, Object.position) <= maxVisibleDistance * visibleCoefficient &
                                                                WallIgnoreVisible? Physics.Raycast(this.position, Object.position - this.position, Checkers.Distance(this.position, Object.position), LayerMask.NameToLayer("Object")) : true) | 
                                                                AlwaysVisible; }
            
            public IHealthBar BaseHealth { get; protected set; }
            [field: SerializeReference, SubclassSelector] public IHealthBar Health { get; set; } 

            public ISanityBar BaseSanity { get; protected set; }
            [field: SerializeReference, SubclassSelector] public ISanityBar Sanity { get; set; }

            public IStaminaBar BaseStamina { get; protected set; }
            [field: SerializeReference, SubclassSelector] public IStaminaBar Stamina { get; set; }
            
            
            [SerializeReference, SubclassSelector] List<IOtherBar> _OtherStates;
            public List<IOtherBar> OtherStates { get { return CombineLists<IOtherBar>(_OtherStates, AllBalanceChanges.AdditionState); } set{ _OtherStates = value; } }

        #endregion
        #region // ================================== effects

            public RacePassiveEffect RaceEffect { get; private set; } 

            [field: SerializeReference, SubclassSelector] public List<Effect> Effects { get; set; } 
            [field: SerializeReference, SubclassSelector] public List<Type> Resists { get; set; }

        #endregion
        #region // ================================== inventory
        
            [SerializeField] List<Item> _Inventory;
            public int InventorySize = 1;
            [SerializeField] public List<Item> _ArtifacerInventory;
            public int ArtifacerInventorySize = 0;

            public List<Item> Inventory { get { return CombineLists<Item>(_Inventory, _ArtifacerInventory); } 
            set
            { 
                foreach(Item item in value) {
                    if(item.Artifacer) _ArtifacerInventory.Add(item);
                    else _Inventory.Add(item);
                }
            } }

            Balancer AllBalanceChanges;
            public List<Balancer> PermanentsEffects = new List<Balancer>();

        #endregion
        #region // ================================== Skills

            public int SkillIndex { get; set; }

            [field: SerializeField] public List<Skill> AvailbleBaseSkills { get; [SerializeField]private set; } = new List<Skill>();

            [field: SerializeField] public int Strength { get; set; }
            [field: SerializeField] public int Accuracy { get; set; }
            [field: SerializeField] public int RezoOverclocking { get; set; }
            [field: SerializeField] public int Healing { get; set; }
            [field: SerializeField] public int Repairing { get; set; }

            [field: SerializeField] public int DamagePure { get; set; }
            [field: SerializeField] public int DamageRange { get; set; }
        
        #endregion

        public Attack.AttackCombiner TakeDamageList { get; set; } = Attack.AttackCombiner.Empty();

        public List<Attack> AttackZone { get; set; } = new List<Attack>();
        public List<Checkers> WalkWay { get; set; } = new List<Checkers>();
        

        public void DrainOtherState<IOtherBar>(int value)
        {

        }

        #region // =============================== Update methods
            
            public bool CheckPosition(Checkers position, bool Other = true)
            {        
                if(!Other) return false;
                
                //OnOtherPlaner
                foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(0, 100, 0) + MPlaner.position, -Vector3.up, 105, LayerMask.GetMask("Object"))) 
                { 
                    if(hit.collider.gameObject != MPlaner.Planer) { return false; }
                }
                
                //OnSelf
                if(new Checkers(position) == new Checkers(this.position))
                    return false;
                
                //OnStamina
                if(Stamina.WalkUseStamina > Stamina.Value) return false;
                //OnDistance
                return WalkDistance + AllBalanceChanges.WalkDistance + 0.5f >= Checkers.Distance(new Checkers(this.position), position); 
            }    

            public async Task MovePlannerSet(Checkers position, bool Draw = true, bool CustomWay = false)
            {
                await Task.Delay(2);
                
                MPlaner.LineRenderer.enabled = CheckPosition(position) & Draw;
                MPlaner.Renderer.enabled = CheckPosition(position) & Draw;

                if(Attacker.CurrentSkill.NoWalking & position == new Checkers(this.position)) 
                    await AttackPlannerSet(position);

                if(!CustomWay) 
                    WalkWay.Clear();

                if (CheckPosition(position)){
                    MPlaner.position = position;
                    
                    if(!CustomWay) 
                        WalkWay = Checkers.PatchWay.WayTo(new Checkers(this.position), new Checkers(position), 20);
                
                    if(Draw) {
                        MPlaner.LineRenderer.positionCount = WalkWay.Count;
                        MPlaner.LineRenderer.SetPositions(Checkers.ToVector3List(WalkWay).ToArray()); 
                    }
                }
                else 
                    MPlaner.position = new Checkers(this.position);
                
            }
            public async Task AttackPlannerSet(Checkers position, bool Draw = true, bool CustomZone = false)
            {
                APlaner.position = new Checkers(position);

                if(Attacker.CurrentSkill.NoWalking) 
                    await MovePlannerSet(this.position, false);
                
                if(!CustomZone) {
                    AttackZone.Clear();
                    if(new Checkers(this.position) != position) AttackZone = await Attacker.Realize(MPlaner.position, APlaner.position, this);
                }
                if(Draw) {
                    Generation.DrawAttack(AttackZone, this);
                }
            }

            public void LostHealth()
            {
                if(Corpse) Destroy(transform.parent.gameObject);
                else { 
                    Corpse = true;
                    this.Health.Value = this.Health.Max + this.Health.Value;
                }
            }
            void AfterInventoryUpdate()
            {
                List<Balancer> FromItems = new List<Balancer>(); foreach(Item item in Inventory) FromItems.Add(item.Stats);
                List<Balancer> FromEffects = new List<Balancer>(); if(Effects.Count != 0)foreach(Effect effect in Effects) FromEffects.Add(effect.Stats);
                
                Balancer result = Balancer.Combine(CombineLists<Balancer>(FromEffects, PermanentsEffects, FromItems).ToArray());

                if(AllBalanceChanges == Balancer.Combine(FromItems.ToArray())) return;
                AllBalanceChanges = Balancer.Combine(FromItems.ToArray()); 

                #region // health
                {
                    if(AllBalanceChanges.ReplaceHealth){
                        IHealthBar healthBar = AllBalanceChanges.Health as IHealthBar;
                        healthBar.Value = Health.Value;
                        Health = healthBar;
                    }
                    else
                    {
                        IHealthBar healthBar = BaseHealth as IHealthBar;
                        healthBar.Value = Health.Value;
                        Health = healthBar;
                    }
                    Health.ArmorMelee = BaseHealth.ArmorMelee + AllBalanceChanges.Health.ArmorMelee;
                    Health.ArmorRange = BaseHealth.ArmorRange + AllBalanceChanges.Health.ArmorRange;
                    Health.Immunity = BaseHealth.Immunity + AllBalanceChanges.Health.Immunity;
                    
                    Health.Max = BaseHealth.Max + AllBalanceChanges.Health.Max;
                }
                #endregion
                #region // Stamina
                {
                    if(AllBalanceChanges.ReplaceStamina){
                        IStaminaBar staminaBar = AllBalanceChanges.Stamina as IStaminaBar;
                        staminaBar.Value = Stamina.Value;
                        Stamina = staminaBar;
                    }
                    else
                    {
                        IStaminaBar staminaBar = BaseStamina as IStaminaBar;
                        staminaBar.Value = Stamina.Value;
                        Stamina = staminaBar;
                    }
                    Stamina.RestEffectivity = BaseStamina.RestEffectivity + AllBalanceChanges.Stamina.RestEffectivity;
                    Stamina.WalkUseStamina = BaseStamina.WalkUseStamina + AllBalanceChanges.Stamina.WalkUseStamina;
                    Stamina.Max = BaseStamina.Max + AllBalanceChanges.Stamina.Max;
                }
                #endregion
                #region // Sanity
                {
                    if(AllBalanceChanges.ReplaceSanity){
                        ISanityBar sanityBar = AllBalanceChanges.Stamina as ISanityBar;
                        sanityBar.Value = Sanity.Value;
                        Sanity = sanityBar;
                    }
                    else
                    {
                        ISanityBar sanityBar = BaseSanity as ISanityBar;
                        sanityBar.Value = Sanity.Value;
                        Sanity = sanityBar;
                    }
                    Sanity.SanityShield = BaseSanity.SanityShield + AllBalanceChanges.Sanity.SanityShield;
                    Sanity.Max = BaseSanity.Max + AllBalanceChanges.Sanity.Max;
                }
                #endregion
            } 
                        
            void UpdateParameter(IStateBar parameter)
            {        
                MethodInfo info = parameter.GetType().GetMethod("StepEnd", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if(info != null) info.Invoke(parameter, parameters: null);
            }
        
        #endregion      
        #region // =============================== Step System
            public bool WillRest { get; set; } = true;

            Task FindStepStage(string id){ 
                MethodInfo Method = typeof(CharacterCore).GetMethod(id, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if(Method == null) return new Task(() => { });
                return (Task)Method?.Invoke((this), parameters: null);
            }   
  
            async Task Walking()
            {
                if(WalkWay.Count == 0) return;

                WillRest = false;

                Stamina.GetTired(Stamina.WalkUseStamina);
                await transport();

                async Task transport() {
                    int PointNum = 1;
                    for(float i = 0.0003f; position != WalkWay.Last().ToVector3(); i *= 1.3f)
                    {
                        await new WaitForFixedUpdate();

                        position = Vector3.MoveTowards(position, WalkWay[PointNum], i);
                        if(position == WalkWay[PointNum].ToVector3() & position != WalkWay[WalkWay.Count - 1].ToVector3()){ PointNum++; }
                    }
                    WalkWay.Clear();
                }
            }  
            async Task Attacking()
            {
                if(AttackZone.Count == 0) return;
                WillRest = false;
                await Task.Delay(Random.Range(900, 2700));

                Stamina.GetTired(Attacker.CurrentSkill.UsingStamina);
                InGameEvents.AttackTransporter.Invoke(AttackZone);

                await AttackPlannerSet(position, true);
            }
            async Task EffectUpdate()
            {
                await Task.Delay(10);                 
                
                ((IObjectOnMap)this).InvokeEffects("Update");
                if(TakeDamageList.Checked) ((IObjectOnMap)this).InvokeEffects("DamageReaction");
            }
            async Task DamageMath()
            {
                List<Attack> attacks = TakeDamageList.Combine();
                await Task.Delay(100);

                foreach(Attack attack in attacks) 
                { 
                    Health.Damage(attack); 
                    ((IObjectOnMap)this).AddEffect(attack.Effects);
                }
                if(attacks.Sum(a=>a.Damage) > 0) ChangeFigureColorWave(TakeDamageList.CombinedColor(), 0.1f);

                TakeDamageList.Clear();
            }       
            async Task Dead() 
            { 
                if(Health.Value > 0) return;
                await Task.Delay(Random.Range(10, 100)); 
                LostHealth();
            }
            async Task Rest() 
            { 
                if(!WillRest) { WillRest = true; return; }
                await Task.Delay(Random.Range(0, 2300)); 
                Stamina.Rest();
                WillRest = true;
            }

            void EveryStepEnd()
            {
                AfterInventoryUpdate();

                UpdateParameter(Health);
                UpdateParameter(Stamina);
                UpdateParameter(Sanity);
                foreach(IOtherBar otherState in OtherStates) UpdateParameter(otherState);

                Effects.RemoveAll(a=>a is OneUse);
            }
        
        #endregion
    
    #endregion
}