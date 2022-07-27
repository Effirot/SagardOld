using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using SagardCL.IParameterManipulate;
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

        [SerializeField] internal bool Corpse = false;
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
            public List<IOtherBar> OtherStates { get { return FieldManipulate.CombineLists<IOtherBar>(_OtherStates, AllItemStats.AdditionState); } set{ _OtherStates = value; } }

        #endregion
        #region // ================================== effects

            public RacePassiveEffect RaceEffect { get; private set; } 

            [SerializeReference, SubclassSelector] List<Effect> _Effects;
            public List<Effect> Effects { get { return _Effects; } set { _Effects = value; } }
            [SerializeReference, SubclassSelector] List<Effect> _Resists;
            public List<Effect> Resists { get { return _Resists; } set { _Resists = value; } }

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

            public BalanceChanger AllItemStats;

            
        #endregion
        #region // ================================== Skills

            public SkillCombiner SkillRealizer { get{ return _SkillRealizer; } set { _SkillRealizer = value; } }
            [SerializeField] SkillCombiner _SkillRealizer = new SkillCombiner();

        #endregion

        protected List<Attack> AttackZone = new List<Attack>();
        protected List<Checkers> WalkWay = new List<Checkers>();

        public Attack.AttackCombiner TakeDamageList = new Attack.AttackCombiner();

        public void AddDamage(Attack attack) {
            if(attack.DamageType != DamageType.Heal | !Effects.Exists(a=>a.GetType() == typeof(Decomposition)))
                TakeDamageList.Add(attack);
        }
        public void AddEffect(params Effect[] Effect) {
            foreach(Effect effect in Effect) { effect.GetMethod("WhenAdded"); Effects.Add(effect); }
        }

        #region // =============================== Update methods
            
            internal virtual void ParametersUpdate() {}
            internal virtual void DamageReaction(Attack attack) {}
            internal virtual void LostHealth()
            {
                if(Health is HealthCorpse) { Destroy(transform.parent.gameObject); }
                else {
                    Corpse = true;
                    this.Health.Value = this.Health.Max;
                    _Effects.Add(new Decomposition() { Target = this });

                    ChangeFigureColor(new Color(0.6f, 0.6f, 0.6f), 0.2f);
                }
            }
            void AfterInventoryUpdate()
            {
                if(AllItemStats == BalanceChanger.CompoundParameters(Inventory.ToArray())) return;
                AllItemStats = BalanceChanger.CompoundParameters(Inventory.ToArray()); 

                #region // health
                {
                    BalanceChanger ReplaceHealthBar = null;
                    if(Inventory.Find(a => a.ThisItem.ReplaceHealthBar)) ReplaceHealthBar = Inventory.Find(a => a.ThisItem.ReplaceHealthBar);
                    if(ReplaceHealthBar != null){
                        IHealthBar healthBar = ReplaceHealthBar.Health.Clone() as IHealthBar;
                        healthBar.Value = Health.Value;
                        Health = healthBar;
                    }
                    else
                    {
                        IHealthBar healthBar = BaseHealth.Clone() as IHealthBar;
                        healthBar.Value = Health.Value;
                        Health = healthBar;
                    }
                    Health.ArmorMelee = BaseHealth.ArmorMelee + AllItemStats.Health.ArmorMelee;
                    Health.ArmorRange = BaseHealth.ArmorRange + AllItemStats.Health.ArmorRange;
                    Health.Immunity = BaseHealth.Immunity + AllItemStats.Health.Immunity;
                    
                    Health.Max = BaseHealth.Max + AllItemStats.Health.Max;
                }
                #endregion
                #region // Stamina
                {
                    BalanceChanger ReplaceStaminaBar = null;
                    if(Inventory.Find(a => a.ThisItem.ReplaceStaminaBar)) ReplaceStaminaBar = Inventory.Find(a => a.ThisItem.ReplaceStaminaBar);

                    if(ReplaceStaminaBar != null){
                        IStaminaBar staminaBar = ReplaceStaminaBar.Stamina.Clone() as IStaminaBar;
                        staminaBar.Value = Stamina.Value;
                        Stamina = staminaBar;
                    }
                    else
                    {
                        IStaminaBar staminaBar = BaseStamina.Clone() as IStaminaBar;
                        staminaBar.Value = Stamina.Value;
                        Stamina = staminaBar;
                    }
                    Stamina.RestEffectivity = BaseStamina.RestEffectivity + AllItemStats.Stamina.RestEffectivity;
                    Stamina.WalkUseStamina = BaseStamina.WalkUseStamina + AllItemStats.Stamina.WalkUseStamina;
                    Stamina.Max = BaseStamina.Max + AllItemStats.Stamina.Max;
                }
                #endregion
                #region // Sanity
                {
                    BalanceChanger ReplaceSanityBar = null;
                    if(Inventory.Find(a => a.ThisItem.ReplaceSanityBar)) ReplaceSanityBar = Inventory.Find(a => a.ThisItem.ReplaceSanityBar);

                    if(ReplaceSanityBar != null){
                        ISanityBar sanityBar = ReplaceSanityBar.Stamina.Clone() as ISanityBar;
                        sanityBar.Value = Sanity.Value;
                        Sanity = sanityBar;
                    }
                    else
                    {
                        ISanityBar sanityBar = BaseSanity.Clone() as ISanityBar;
                        sanityBar.Value = Sanity.Value;
                        Sanity = sanityBar;
                    }
                    Sanity.SanityShield = BaseSanity.SanityShield + AllItemStats.Sanity.SanityShield;
                    Sanity.Max = BaseSanity.Max + AllItemStats.Sanity.Max;
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
            void UpdateEffects(string Method)
            {
                foreach(Effect effect in Effects)
                {
                    if(effect.Target == null) effect.Target = this;
                    effect.GetMethod(Method);
                }
                Effects.RemoveAll(a=>!a.ExistReasons());
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
                await MPlanerMove();

                ParametersUpdate();

                async Task MPlanerMove()
                {   
                    int PointNum = 1;
                    for(float i = 0.0001f; position != WalkWay[WalkWay.Count - 1].ToVector3(); i *= 1.3f)
                    {
                        await Await.Updates(7);
                        position = Vector3.MoveTowards(position, WalkWay[PointNum], i);
                        if(position == WalkWay[PointNum].ToVector3() & position != WalkWay[WalkWay.Count - 1].ToVector3()){ PointNum++; }
                    }
                }
            }     
            protected virtual async Task Attacking()
            {
                if(AttackZone.Count == 0) return;
                WillRest = false;
                await Task.Delay(Random.Range(900, 2700));

                Stamina.GetTired(SkillRealizer.StaminaWaste());
                InGameEvents.AttackTransporter.Invoke(AttackZone);

                APlaner.position = MPlaner.position;
                MPlaner.Renderer.enabled = false;
            }
            async Task EffectUpdate()
            {
                await Task.Delay(10);                 
                
                UpdateEffects("Update");
            }
            async Task DamageMath()
            {
                await Task.Delay(100);

                foreach(Attack attack in TakeDamageList.Combine()) 
                { 
                    Health.Damage(attack); 
                    
                    AddEffect(attack.Effects);
                }

                if(TakeDamageList.Combine().Sum(a=>a.Damage) > 0) ChangeFigureColorWave(TakeDamageList.CombinedColor(), 1);

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
            
            public bool WillRest = true;

            void EveryStepEnd()
            {
                AfterInventoryUpdate();

                UpdateParameter(Health);
                UpdateParameter(Stamina);
                UpdateParameter(Sanity);
                foreach(IOtherBar otherState in OtherStates) UpdateParameter(otherState);
            }
        
        #endregion
    #endregion
}