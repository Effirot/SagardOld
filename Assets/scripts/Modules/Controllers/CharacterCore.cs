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
        protected virtual string IdAddition{ get => ""; }
        void SetName()
        {
            transform.parent.name = HaveID.GetName() + " " + IdAddition;
            name += $"({transform.parent.name})";
        }
        protected virtual async void Start()
        {
            TakeDamageList.Clear();

            SetName();

            Map.StepSystem.Add(FindStepStage);
            Map.AttackTransporter.AddListener((a) => 
            { 
                foreach(Attack attack in a.FindAll((a) => a.Position == new Checkers(position))) { 
                    TakeDamageList.Add(attack); }
            });
            Map.StepEnd.AddListener(EveryStepEnd);
            
            AfterInventoryUpdate();

            await Task.Delay(5);
            position = new Checkers(position);
            MoveTarget = new Checkers(position);
            AttackTarget = new Checkers(position);
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

            #region // ================================== inventory
            
                [SerializeField] List<Item> _Inventory;
                public int InventorySize = 1;

                public List<Item> Inventory { get { return _Inventory; } set { _Inventory = value; } }

            #endregion
            #region // ================================== controlling
                
                [SerializeField] public bool CanWalk = true;
                [SerializeField] public bool CanAttack = true;

                public virtual Checkers AttackTarget { get; protected set; }
                public virtual Checkers MoveTarget { get; protected set; }
                public virtual Checkers LateMoveTarget { get; protected set; }
                public List<Checkers> WalkWay { get; set; } = new List<Checkers>();

                public int SkillIndex { get; set; } = 0;
                public Skill CurrentSkill { get { return SkillIndex == 0? Skill.Empty() : NowBalance.Skills[SkillIndex - 1]; } } 

                public virtual void SetAttackTarget(Checkers position)
                {
                    if(CurrentSkill.NoWalking) {
                        GenerateWayToTarget(this.position); }

                    AttackTarget = position;
                }
                public async void GenerateWayToTarget(Checkers position, params GameObject[] BlackList)
                {
                    if(CurrentSkill.NoWalking & position == new Checkers(this.position)) {AttackTarget = this.position; SkillIndex = 0; }
                    
                    if(position != new Checkers(this.position)) { 
                        WalkWay = await Checkers.PatchWay.WayTo(new Checkers(this.position), position, NowBalance.WalkDistance, 0.2f, BlackList); 
                        
                        WalkWay[0] = WalkWay[0].Up(0); 
                        WalkWay[WalkWay.Count - 1] = WalkWay[WalkWay.Count - 1].Up(0); 
                        MoveTarget = WalkWay.Last().Up(0); }
                    else {WalkWay.Clear(); MoveTarget = position; }
                }
                public void SetLateWalking(params Checkers[] positions)
                {
                    LateMoveTarget = new Checkers(positions.Sum(a=>a.x) / positions.Length, positions.Sum(a=>a.z) / positions.Length);
                }

                public void AddDamage(params Attack[] attacks) {
                    foreach(Attack attack in attacks)
                    {
                        if(attack.DamageType == DamageType.Heal & !IsAlive) return;
                        TakeDamageList.Add(attack);
                    }
                }
                public void AddSanity(int Value) { BaseBalance.Sanity.Value += Value >= 0? Value : -Mathf.Clamp(Value - NowBalance.Sanity.SanityShield, 0, 1000); }
                public void AddStamina(int Value) { BaseBalance.Stamina.Value += Value; }

                public void AddEffect(params Effect[] Effect) {
                    foreach(Effect effect in Effect) { 
                        effect.Target = this; 
                        if(!effect.Workable()) continue; 

                        effect.InvokeMethod("WhenAdded"); 
                        Effects.Add(effect); 
                    }
                }
                public void RemoveEffect(params Effect[] Effect) {
                    foreach(Effect effect in Effect) { 
                        effect.InvokeMethod("WhenRemoved"); 
                        Effects.Remove(effect); 
                    }
                }
                public void RemoveEffect(Predicate<Effect> predicate) {
                    RemoveEffect(Effects.FindAll(predicate).ToArray());
                }
                void AutoRemoveEffect() {
                    List<Effect> Effect = Effects.FindAll(a=>!a.Workable());

                    RemoveEffect(Effect.ToArray());
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

                public void AddState(params ICustomBar[] state) { }

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

            protected virtual async Task BotLogic() { await Task.Run(()=>{}); }
            async Task Walking()
            {
                LateMoveTarget = position;
                if(WalkWay.Count == 0 | !CanWalk) return;

                WillRest = false;

                NowBalance.Stamina.GetTired(NowBalance.Stamina.WalkUseStamina);
                await Transport();

                async Task Transport() {
                    int PointNum = 1;
                    for(float i = 0.0003f; position != WalkWay.Last().ToVector3(); i *= 1.25f)
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
                if(SkillIndex == 0 | !CanAttack) return;
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
            async Task LateWalking()
            {
                if(LateMoveTarget == new Checkers(position)) return;
                
                await LateTransport();

                async Task LateTransport()
                {
                    while(position != LateMoveTarget.ToVector3())
                    {
                        position = Vector3.MoveTowards(position, LateMoveTarget.ToVector3(), 0.3f);
                        await new WaitForFixedUpdate();
                    }
                    LateMoveTarget = position;
                }
            }
            async Task DamageMath()
            {
                await Task.Delay(30);
                
                TakeDamageList.Combine().ForEach(a=>{BaseBalance.Health.Value += NowBalance.Health.GetDamage(a); });

                foreach(Attack attack in TakeDamageList.Combine()) 
                    AddEffect(attack.Effects);
                
                // if(TakeDamageList.Combine().Sum(a=>a.Damage) > 0) ChangeFigureColorWave(TakeDamageList.CombinedColor(), 0.1f, MustChangeColor);
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