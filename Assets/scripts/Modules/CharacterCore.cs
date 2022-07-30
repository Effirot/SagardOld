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

public class CharacterCore : MonoBehaviour, Killable, GetableCrazy, Tiredable, Storage, Attacker, HaveName {
    #region // ============================================================= Useful stuff =============================================================================================
        protected Vector3 position{ get{ return this.transform.position; } set{ this.transform.position = value; } }

        private protected AllInOne MPlaner { get{ return SkillRealizer.From; } set { SkillRealizer.From = value; } }
        private protected AllInOne APlaner { get{ return SkillRealizer.To; } set { SkillRealizer.To = value; } }
                
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
    #endregion
    #region // =========================================================== All parameters =================================================================================================
    
        protected virtual async void Start()
        {
            TakeDamageList.Clear();

            transform.parent.name = HaveName.GetName();
            name += $"({transform.parent.name})";

            BaseHealth = _Health.Clone() as IHealthBar;
            BaseStamina = _Stamina.Clone() as IStaminaBar;
            BaseSanity = _Sanity.Clone() as ISanityBar;
            
            InGameEvents.MapUpdate.AddListener(async() => {
                await MovePlannerSet(MPlaner.position, MPlaner.Renderer.enabled);
                await AttackPlannerSet(APlaner.position, MPlaner.Renderer.enabled);
            });
            InGameEvents.StepSystem.Add(FindStepStage);
            InGameEvents.AttackTransporter.AddListener((a) => { 
                List<Attack> find = a.FindAll((a) => a.Position == new Checkers(position));
                foreach(Attack attack in find) if(attack.Position == new Checkers(position)) AddDamage(attack);
            });
            InGameEvents.StepEnd.AddListener(EveryStepEnd);
            
            AfterInventoryUpdate();

            await Task.Delay(10);
            position = new Checkers(position);
            MPlaner.position = new Checkers(position);
        }

        public Race _Race;
        Race RaceName { get{ return _Race; } set{ _Race = value; } }

        [SerializeField] bool _Corpse = false;
        internal bool Corpse { get { return _Corpse; }
        set { 
                if(_Corpse != value)
                    if(value) 
                    {
                        Effects.RemoveAll(a=>a is OneUse | !a.Workable());
                        Effects.Add(Decomposition.Base(this));

                        ChangeFigureColor(new Color(0.5f, 0.5f, 0.5f), 0.2f);
                    }
                    else 
                    {
                        Effects.Remove(Decomposition.Base(this));

                        ChangeFigureColor(new Color(1f, 1f, 1f), 0.2f);
                    }
                _Corpse = value;
            } 
        } 
        [SerializeField] internal int WalkDistance = 5;

        #region // ================================== parameters

            public const int maxVisibleDistance = 10;
            public float visibleCoefficient = 1;
            [SerializeField] bool AlwaysVisible = false;
            [SerializeField] bool WallIgnoreVisible = false;

            public bool nowVisible(CharacterCore Object) { return (Checkers.Distance(this.position, Object.position) <= maxVisibleDistance * visibleCoefficient &
                                                                WallIgnoreVisible? Physics.Raycast(this.position, Object.position - this.position, Checkers.Distance(this.position, Object.position), LayerMask.NameToLayer("Object")) : true) | 
                                                                AlwaysVisible; }
            
            [SerializeField, SerializeReference]protected IHealthBar BaseHealth;
            [SerializeReference, SubclassSelector] IHealthBar _Health;
            public IHealthBar Health { get { return _Health; } set { _Health = value; } } 

            protected ISanityBar BaseSanity;
            [SerializeReference, SubclassSelector] ISanityBar _Sanity;
            public ISanityBar Sanity { get { return _Sanity; } set { _Sanity = value; } }

            protected IStaminaBar BaseStamina;
            [SerializeReference, SubclassSelector] IStaminaBar _Stamina;
            public IStaminaBar Stamina { get { return _Stamina; } set { _Stamina = value; } }
            
            
            [SerializeReference, SubclassSelector] List<IOtherBar> _OtherStates;
            public List<IOtherBar> OtherStates { get { return FieldManipulate.CombineLists<IOtherBar>(_OtherStates, AllBalanceChanges.AdditionState); } set{ _OtherStates = value; } }

        #endregion
        #region // ================================== effects

            public RacePassiveEffect RaceEffect { get; private set; } 

            [SerializeReference, SubclassSelector] List<Effect> _Effects;
            public List<Effect> Effects { get { return _Effects; } set { _Effects = value; } }
            [SerializeReference, SubclassSelector] List<Type> _Resists;
            public List<Type> Resists { get { return _Resists; } set { _Resists = value; } }

        #endregion
        #region // ================================== inventory
        
            [SerializeField] List<Item> _Inventory;
            public int InventorySize = 1;

            [SerializeField] public List<Item> _ArtifacerInventory;
            public int ArtifacerInventorySize = 0;

            public List<Item> Inventory { get { return FieldManipulate.CombineLists<Item>(_Inventory, _ArtifacerInventory); } 
            set
            { 
                foreach(Item item in value) {
                    if(item.Artifacer) _ArtifacerInventory.Add(item);
                    else _Inventory.Add(item);
                }
            } }

            public ReBalancer AllBalanceChanges;
            public List<ReBalancer> PermanentsEffects = new List<ReBalancer>();

        #endregion
        #region // ================================== Skills

            public SkillCombiner SkillRealizer { get{ return _SkillRealizer; } set { _SkillRealizer = value; } }
            [SerializeField] SkillCombiner _SkillRealizer = new SkillCombiner();

            [SerializeField] public int Strength;
            [SerializeField] public int Accuracy;
            [SerializeField] public int RezoOverclocking;
            [SerializeField] public int Healing;
            [SerializeField] public int Repairing;

            [SerializeField] public int DamagePure = 8;
            [SerializeField] public int DamageRange = 9;

        #endregion

        protected List<Attack> AttackZone = new List<Attack>();
        protected List<Checkers> WalkWay = new List<Checkers>();

        public Attack.AttackCombiner TakeDamageList = new Attack.AttackCombiner();

        public void AddDamage(Attack attack)
        {
            if(attack.DamageType != DamageType.Heal | !Effects.Exists(a=>a.GetType() == typeof(Decomposition)))
                TakeDamageList.Add(attack);
        }
        public void AddSanity(int damage)
        {
            if(Sanity!=null) Sanity.Value = Mathf.Clamp(damage >= 0? damage : -(int)(Mathf.Clamp(MathF.Abs(damage) - Sanity.SanityShield, 0, 1000)) + Sanity.Value, 0, Sanity.Max);
        }
        public void AddStamina(int damage)
        {
            Stamina.Value = Mathf.Clamp(damage + Stamina.Value, 0, Stamina.Max);
        }
        public void DrainOtherState<IOtherBar>(int value)
        {

        }
        
        public void AddEffect(params Effect[] Effect) {
            foreach(Effect effect in Effect) { effect.Target = this; if(!effect.Workable() ) continue; effect.InvokeMethod("WhenAdded"); Effects.Add(effect); }
        }
        public void RemoveEffect() {
            List<Effect> Effect = Effects.FindAll(a=>!a.Workable());
            foreach(Effect effect in Effect) { effect.InvokeMethod("WhenRemoved"); Effects.Remove(effect); }
        }
        public void RemoveEffect(params Effect[] Effect) {
            foreach(Effect effect in Effect) { effect.InvokeMethod("WhenRemoved"); Effects.Remove(effect); }
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

                if(SkillRealizer.ThisSkill.NoWalking & position == new Checkers(this.position)) 
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

                if(SkillRealizer.ThisSkill.NoWalking) 
                    await MovePlannerSet(this.position, false);
                
                if(!CustomZone) {
                    AttackZone.Clear();
                    AttackZone = await SkillRealizer.Realize();
                }
                if(Draw) {
                    Generation.DrawAttack(AttackZone, this);
                }
            }

            void LostHealth()
            {
                if(Corpse) Destroy(transform.parent.gameObject);
                else { 
                    Corpse = true;
                    this.Health.Value = this.Health.Max + this.Health.Value;
                }
                
            }
            void AfterInventoryUpdate()
            {
                List<ReBalancer> FromItems = new List<ReBalancer>(); foreach(Item item in Inventory) FromItems.Add(item.Stats);
                List<ReBalancer> FromEffects = new List<ReBalancer>(); if(Effects.Count != 0)foreach(Effect effect in Effects) FromEffects.Add(effect.Stats);
                
                ReBalancer result = ReBalancer.Combine(FieldManipulate.CombineLists<ReBalancer>(FromEffects, PermanentsEffects, FromItems).ToArray());

                if(AllBalanceChanges == ReBalancer.Combine(FromItems.ToArray())) return;
                AllBalanceChanges = ReBalancer.Combine(FromItems.ToArray()); 

                #region // health
                {
                    if(AllBalanceChanges.ReplaceHealthBar){
                        IHealthBar healthBar = AllBalanceChanges.Health.Clone() as IHealthBar;
                        healthBar.Value = Health.Value;
                        Health = healthBar;
                    }
                    else
                    {
                        IHealthBar healthBar = BaseHealth.Clone() as IHealthBar;
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
                    if(AllBalanceChanges.ReplaceStaminaBar){
                        IStaminaBar staminaBar = AllBalanceChanges.Stamina.Clone() as IStaminaBar;
                        staminaBar.Value = Stamina.Value;
                        Stamina = staminaBar;
                    }
                    else
                    {
                        IStaminaBar staminaBar = BaseStamina.Clone() as IStaminaBar;
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
                    if(AllBalanceChanges.ReplaceSanityBar){
                        ISanityBar sanityBar = AllBalanceChanges.Stamina.Clone() as ISanityBar;
                        sanityBar.Value = Sanity.Value;
                        Sanity = sanityBar;
                    }
                    else
                    {
                        ISanityBar sanityBar = BaseSanity.Clone() as ISanityBar;
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
                if(!(parameter is IStepEndUpdate)) return;
                
                Type type = typeof(IStepEndUpdate);
                MethodInfo info = type.GetMethod("StepEnd", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if(info != null) info.Invoke(parameter, parameters: null);
            }
            void InvokeEffects(string Method)
            {
                foreach(Effect effect in Effects)
                {
                    if(effect.Target == null) effect.Target = this;
                    effect.InvokeMethod(Method);
                }
                RemoveEffect();
            }
        
        #endregion      
        #region // =============================== Step System
            
            Task FindStepStage(string id){ 
                MethodInfo Method = type.GetMethod(id, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if(Method == null) return new Task(() => { });
                return (Task)Method?.Invoke(this, parameters: null);
            }   

            async Task Walking()
            {
                if(WalkWay.Count == 0) return;

                WillRest = false;
                await Task.Delay(30);

                Stamina.GetTired(Stamina.WalkUseStamina);
                await transport();

                async Task transport() {
                    int PointNum = 1;
                    for(float i = 0.0003f; position != WalkWay[WalkWay.Count - 1].ToVector3(); i *= 1.3f)
                    {
                        await Await.Updates(2);
                        position = Vector3.MoveTowards(position, WalkWay[PointNum], i);
                        if(position == WalkWay[PointNum].ToVector3() & position != WalkWay[WalkWay.Count - 1].ToVector3()){ PointNum++; }
                    }
                    await MovePlannerSet(this.position, false);
                }
                
            }     
            async Task EffectUpdate()
            {
                await Task.Delay(10);                 
                
                InvokeEffects("Update");
                if(TakeDamageList.Checked) InvokeEffects("DamageReaction");
            }
            async Task Attacking()
            {
                if(AttackZone.Count == 0) return;
                WillRest = false;
                await Task.Delay(Random.Range(900, 2700));

                Stamina.GetTired(SkillRealizer.StaminaWaste());
                InGameEvents.AttackTransporter.Invoke(AttackZone);


                await AttackPlannerSet(MPlaner.position, true);
            }
            async Task DamageMath()
            {
                List<Attack> attacks = TakeDamageList.Combine();
                await Task.Delay(100);

                foreach(Attack attack in attacks) 
                { 
                    Health.Damage(attack); 
                    AddEffect(attack.Effects);
                }
                if(attacks.Sum(a=>a.Damage) > 0) ChangeFigureColorWave(TakeDamageList.CombinedColor(), 0.1f);

                TakeDamageList.Clear();
            }
            async Task Dead() 
            { 
                if(Health.Value > 0) return;
                await Task.Delay(Random.Range(10, 100)); 
                try { LostHealth(); } catch {}
            }
            async Task Rest() 
            { 
                if(!WillRest) { WillRest = true; return; }
                await Task.Delay(Random.Range(0, 2300)); 
                Stamina.Rest();
            }
            
            bool WillRest = true;

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