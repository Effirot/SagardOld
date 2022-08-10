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

public abstract class CharacterCore : MonoBehaviour, IObjectOnMap, HaveID {
    
    #region // ============================================================= Useful stuff =============================================================================================
        
        protected Vector3 position{ get => transform.position; set => transform.position = value; }

        public void ChangeFigureColor(Color color, float speed, params Material[] material)
        { 
            if(material == null){
                List<Material> materials = new List<Material>();
                transform.parent.GetComponentsInChildren<MeshRenderer>().ToList().ForEach(a=>materials.AddRange(a.materials));
                material = materials.ToArray();}

            LastChangeColor.ForEach(a=>StopCoroutine(a));
            LastChangeColor.Clear();
            foreach (Material mat in material) LastChangeColor.Add(ChangeMaterialColor(mat, color, speed));
            LastChangeColor.ForEach(a=>StartCoroutine(a)); 
        }
        public void ChangeFigureColorWave(Color color, float speed, Material[] material = null)
        { 
            if(material == null){
                List<Material> materials = new List<Material>();
                transform.parent.GetComponentsInChildren<MeshRenderer>().ToList().ForEach(a=>materials.AddRange(a.materials));
                material = materials.ToArray();}
            
            LastChangeColor.ForEach(a=>StopCoroutine(a));
            LastChangeColor.Clear();
            foreach (Material mat in material) LastChangeColor.Add(Wave(mat, color, speed));
            LastChangeColor.ForEach(a=>StartCoroutine(a)); 

            IEnumerator Wave(Material material, Color color, float speed)
            { 
                Color Save = material.color;
                yield return ChangeMaterialColor(material, color, speed); 
                yield return ChangeMaterialColor(material, Save, speed); 
            }
        }
        static IEnumerator ChangeMaterialColor(Material material, Color color, float speed)
        {
            while(material.color != color)
            {
                material.color = Color.Lerp(material.color, color, speed);
                speed *= 1.1f;
                yield return new WaitForFixedUpdate();
            }
        }
    
        List<IEnumerator> LastChangeColor; 

        public abstract Material[] MustChangeColor { get; set; }

    #endregion
    
    public static readonly Checkers BufferLocation = new Checkers(3324, 5981);
    #region // =========================================================== All parameters =================================================================================================
        protected virtual void SetName()
        {
            transform.parent.name = HaveID.GetName();
            name += $"({transform.parent.name})";
        }
        protected virtual async void Start()
        {
            TakeDamageList.Clear();

            SetName();

            InGameEvents.StepSystem.Add(FindStepStage);
            InGameEvents.AttackTransporter.AddListener((a) => 
            { 
                foreach(Attack attack in a.FindAll((a) => a.Position == new Checkers(position))) { 
                    TakeDamageList.Add(attack); }
            });
            InGameEvents.StepEnd.AddListener(EveryStepEnd);
            
            AfterInventoryUpdate();

            await Task.Delay(10);
            position = new Checkers(position);
        }

        #region // =============================== Parameters

            public RacePassiveEffect RaceEffect { get; private set; } 

            [field: SerializeField] public Race Race { get; private set; }
            [field: SerializeField] public bool IsAlive { get; set; } = true;

            [field : SerializeField] public Balancer NowBalance { get; private set; }
            [field : SerializeField] public Balancer BaseBalance { get; private set; }
            ReBalancer AllBalanceChanges;
            
            [field: SerializeReference, SubclassSelector] public List<Effect> Effects { get; set; } = new List<Effect>();

            public Attack.AttackCombiner TakeDamageList { get; set; } = Attack.AttackCombiner.Empty();

        
            void AutoRemoveEffect() {
                List<Effect> Effect = Effects.FindAll(a=>!a.Workable());

                RemoveEffect(Effect.ToArray());
            }
            public void RemoveEffect(params Effect[] Effect) {
                foreach(Effect effect in Effect) { 
                    effect.InvokeMethod("WhenRemoved"); 
                    Effects.Remove(effect); 
                }
            }
            
            void InvokeEffects(string Method)
            {
                foreach(Effect effect in Effects)
                {
                    effect.Target = this;
                    effect.InvokeMethod(Method);
                }
                AutoRemoveEffect();
            }

            #region // ================================== inventory
            
                [SerializeField] List<Item> _Inventory;
                public int InventorySize = 1;

                public List<Item> Inventory { get { return _Inventory; } set { _Inventory = value; } }

            #endregion
            #region // ================================== controlling

                public virtual Checkers AttackTarget { get; set; }
                public virtual Checkers MoveTarget { get; set; }
                public List<Checkers> WalkWay { get; set; } = new List<Checkers>();

                public int SkillIndex { get; set; } = 0;
                public Skill CurrentSkill { get { return NowBalance.Skills[SkillIndex]; } } 

                public virtual void SetAttackTarget(Checkers position)
                {
                    if(CurrentSkill.NoWalking) {
                        GenerateWayToTarget(this.position); }

                    AttackTarget = position;
                }

                public async void GenerateWayToTarget(Checkers position)
                {
                    await Task.Delay(2);
                    
                    // MPlaner.LineRenderer.enabled = CheckPosition(position) & Draw;
                    // MPlaner.Renderer.enabled = CheckPosition(position) & Draw;

                    if(CurrentSkill.NoWalking & position == new Checkers(this.position)) AttackTarget = this.position;

                    MoveTarget = position;
                    
                    WalkWay = Checkers.PatchWay.WayTo(new Checkers(this.position), new Checkers(position), 20);                
                }

                public void AddDamage(params Attack[] attacks) {
                    foreach(Attack attack in attacks)
                    {
                        if(attack.DamageType == DamageType.Heal & !IsAlive) return;
                        TakeDamageList.Add(attack);
                    }
                }
                
                public void AddSanity(int Value) { BaseBalance.Sanity.Value += Value >= 0? Value : -Mathf.Clamp(Value - NowBalance.Sanity.SanityShield, 0, 1000); }

                public void AddEffect(params Effect[] Effect) {
                    foreach(Effect effect in Effect) { 
                        effect.Target = this; 
                        if(!effect.Workable()) continue; 

                        effect.InvokeMethod("WhenAdded"); 
                        Effects.Add(effect); 
                    }
                }
            #endregion
        #endregion

        #region // =============================== Update methods



            public void LostHealth()
            {
                if(!IsAlive) Destroy(transform.parent.gameObject);
                else { 
                    IsAlive = false;
                    
                    Effects.RemoveAll(a=>a is OneUse | !a.Workable());
                    Effects.Add(Decomposition.Base(this));

                    ChangeFigureColor(new Color(0.5f, 0.5f, 0.5f), 0.2f);

                    this.BaseBalance.Health.Value = this.BaseBalance.Health.Max + this.BaseBalance.Health.Value;
                }
            }
            void AfterInventoryUpdate()
            {
                List<ReBalancer> BalanceChanges = new List<ReBalancer>();
                Effects.ForEach(a=>BalanceChanges.Add(a.Stats));
                Inventory.ForEach(a=>BalanceChanges.Add(a.Stats));

                AllBalanceChanges = ReBalancer.Empty();
                BalanceChanges.ForEach(a=>AllBalanceChanges += a);
                
                NowBalance = BaseBalance + AllBalanceChanges;
            } 
            void UpdateStateBar(IStateBar parameter)
            {        
                MethodInfo info = parameter.GetType().GetMethod("StepEnd", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if(info != null) info.Invoke(parameter, parameters: null);
            }
        
        #endregion 
        #region // =============================== Step System

            public bool WillRest { get; set; } = true;

            Task FindStepStage(string id){ 
                MethodInfo Method = typeof(CharacterCore).GetMethod(id, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if(Method == null) return Empty();
                return (Task)Method?.Invoke((this), parameters: null);

                async Task Empty() { await Task.Delay(0); }
            }   

            async Task Walking()
            {
                if(WalkWay.Count == 0) return;

                WillRest = false;

                NowBalance.Stamina.GetTired(NowBalance.Stamina.WalkUseStamina);
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
                if(CurrentSkill.Equals(Skill.Empty())) return;
                WillRest = false;
                //await Task.Delay(Random.Range(900, 2700));

                await CurrentSkill.Complete(MoveTarget, AttackTarget, this);
                AttackTarget = MoveTarget;
                SkillIndex = 0;
            }
            async Task EffectUpdate()
            {
                await Task.Delay(10);                 
                
                InvokeEffects("Update");
                if(TakeDamageList.Checked) InvokeEffects("DamageReaction");
            }
            async Task DamageMath()
            {
                await Task.Delay(30);
                
                TakeDamageList.Combine().ForEach(a=>{BaseBalance.Health.Value += NowBalance.Health.GetDamage(a); });

                foreach(Attack attack in TakeDamageList.Combine()) 
                    AddEffect(attack.Effects);
                
                if(TakeDamageList.Combine().Sum(a=>a.Damage) > 0) ChangeFigureColorWave(TakeDamageList.CombinedColor(), 0.1f, MustChangeColor);
            }       
            async Task Dead()
            { 
                if(NowBalance.Health.Value > 0) return;
                await Task.Delay(Random.Range(10, 100)); 
                LostHealth();
            }
            async Task Rest()
            { 
                if(!WillRest) { WillRest = true; return; }
                await Task.Delay(Random.Range(0, 2300)); 
                NowBalance.Stamina.Rest();
                WillRest = true;
            }

            void EveryStepEnd()
            {
                AfterInventoryUpdate();

                UpdateStateBar(NowBalance.Health);
                UpdateStateBar(NowBalance.Stamina);
                UpdateStateBar(NowBalance.Sanity);
                if(NowBalance.AdditionState is not null && NowBalance.AdditionState.Count > 0) 
                    foreach(ICustomBar otherState in NowBalance.AdditionState) UpdateStateBar(otherState);

                Effects.RemoveAll(a=>a is OneUse);

                TakeDamageList.Clear();
            }

        #endregion
    
    #endregion
}