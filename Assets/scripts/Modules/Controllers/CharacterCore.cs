using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using SagardCL.MapObjectInfo;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Reflection;
using Random = UnityEngine.Random;
using UnityAsync;
using System.Threading;

public abstract class CharacterCore : MonoBehaviour, IObjectOnMap, HaveID {
    
    #region // ============================================================= Useful stuff =============================================================================================
        
        protected Vector3 position{ get => transform.position; set => transform.position = value; }

        public void ChangeFigureColor(Color color, float speed, params Material[] material)
        { 
            if(material == null){
                List<Material> check = new List<Material>();
                foreach(MeshRenderer renderer in MustChangeColor)
                    check.AddRange(renderer.materials);
                
                material = check.ToArray();}

            LastChangeColor.ForEach(a=>StopCoroutine(a));
            LastChangeColor.Clear();
            foreach (Material mat in material) LastChangeColor.Add(ChangeMaterialColor(mat, color, speed));
            LastChangeColor.ForEach(a=>StartCoroutine(a)); 
        }
        public void ChangeFigureColorWave(Color color, float speed, Material[] material = null)
        { 
            if(material == null){
                List<Material> check = new List<Material>();
                foreach(MeshRenderer renderer in MustChangeColor)
                    check.AddRange(renderer.materials);
                
                material = check.ToArray();}
            
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

        public abstract MeshRenderer[] MustChangeColor { get; }

    #endregion
    
    public static readonly Checkers BufferLocation = new Checkers(3324, 5981);
    #region // =========================================================== All parameters =================================================================================================
        
        protected virtual string IdAddition{ get => ""; }
        void SetRegister()
        {
            transform.parent.name = HaveID.GetName() + " " + IdAddition;
            name += $"({transform.parent.name})";

            Session.CharacterRegister.Add(transform.parent.name, this);
            Session.ObjectRegister.Add(transform.parent.name, this);
        }

        protected virtual async void Start()
        {
            TakeDamageList.Clear();

            Session.StepSystem.Add(FindStepStage);
            Session.AttackTransporter.AddListener((layer, a) => 
            { 
                foreach(Attack attack in a.FindAll((a) => a.position == nowPosition & a.position.layer == nowPosition.layer)) { 
                    TakeDamageList.Add(attack); }
            });
            Session.StepEnd.AddListener(EveryStepEnd);
            
            AfterInventoryUpdate();

            await Task.Delay(5);
            AttackTarget = MoveTarget = position = new Checkers(position);

            SetRegister();
        }
        protected virtual void OnDestroy() {
            Session.CharacterRegister.Remove(transform.parent.name);
            Session.ObjectRegister.Remove(transform.parent.name);
            Session.StepSystem.Remove(FindStepStage);
        }

        #region // =============================== Parameters
            [field : SerializeField] public List<string> Tag { get; set; } = new List<string>();

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

                public Checkers nowPosition { get => this.position.ToCheckers(); }
                
                [SerializeField] public bool CanWalk = true;
                [SerializeField] public bool CanActing = true;

                public virtual Checkers AttackTarget { get; protected set; }
                public virtual Checkers MoveTarget { get; protected set; }
                public virtual Checkers DashTarget { get; set; }
                public virtual Checkers LateDashTarget { get; set; }
                public List<Checkers> WalkWay { get; set; } = new List<Checkers>();
                Dictionary<string, Skill> PlanedAction = new Dictionary<string, Skill>();
                
                public Skill ActionOnIndex(int index){ return index == 0? Skill.Empty() : NowBalance.Skills[index - 1]; } 
                
                protected virtual GameObject[] WalkBlackList { get => new GameObject[] { gameObject }; }


                public void AddActionToPlan(Skill Action, string Sorting)
                {
                    Action.Plan(this);
                    if(PlanedAction.ContainsKey(Sorting)) PlanedAction[Sorting] = Action;
                    else PlanedAction.Add(Sorting, Action);
                }
                public void RemovePlan(string Sorting)
                {
                    if(PlanedAction.ContainsKey(Sorting)) PlanedAction.Remove(Sorting);
                }
                public async void SetWayToTarget(Checkers position)
                {
                    foreach(var Actions in PlanedAction)
                        if(Actions.Value.NoWalk) PlanedAction.Remove(Actions.Key);
                    
                    if(position != this.position.ToCheckers() & CanWalk) {
                        WalkWay = await Checkers.PatchWay.WayTo(new Checkers(this.position), position, NowBalance.WalkDistance, 0.2f, WalkBlackList); 
                        
                        WalkWay[0] = WalkWay[0].Up(0); 
                        WalkWay[WalkWay.Count - 1] = WalkWay[WalkWay.Count - 1].Up(0); 
                        MoveTarget = WalkWay.Last().Up(0); }

                    else {WalkWay.Clear(); MoveTarget = this.position.ToCheckers(); }
                }
                public void SetLateWalking(params Checkers[] positions)
                {
                    LateDashTarget = new Checkers(positions.Sum(a=>a.x) / positions.Length, positions.Sum(a=>a.z) / positions.Length);
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

            #endregion
        
        #endregion

        #region // =============================== Update methods

            public void LostHealth()
            {
                if(!IsAlive & this.NowBalance.Health.Value <= 0) { Session.ObjectRegister.Remove(transform.parent.name); Destroy(transform.parent.gameObject); }

                IsAlive = false;
                
                Effects.RemoveAll(a=>a is OneUse | !a.Workable());
                Effects.Add(Decomposition.Base(this));

                // ChangeFigureColor(new Color(0.5f, 0.5f, 0.5f), 0.2f);

                this.BaseBalance.Health.Value = this.BaseBalance.Health.Max + this.BaseBalance.Health.Value;
                
                if(!IsAlive & this.NowBalance.Health.Value <= 0) { Session.ObjectRegister.Remove(transform.parent.name); Destroy(transform.parent.gameObject); }
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

                return Method != null? Find(Method) : null;

                async Task Find(MethodInfo task) 
                {
                    CancellationTokenSource Block = new CancellationTokenSource();

                    try { Block.CancelAfter(3500); await (Task)Method?.Invoke((this), parameters: null); }
                    
                    catch (OperationCanceledException){ return; }
                    catch (Exception a) { Debug.LogError($"{transform.parent.name}\n{a}"); return;  }
                }
            }   

            protected virtual async Task BotLogic() { await Task.Run(()=>{}); }
            
            async Task Walking()
            {
                LateDashTarget = MoveTarget;
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
                LateDashTarget = MoveTarget;
            }  
            async Task Action()
            {
                if(!CanActing) return;
                WillRest = false;
                
                await Task.Delay(Random.Range(300, 1600));

                foreach(var target in PlanedAction)
                    if(target.Value != null)
                        target.Value.Complete(this);
                
                AttackTarget = nowPosition;
            }
            async Task EffectUpdate()
            {
                await Task.Delay(10);                 
                
                InvokeEffects("Update");
                if(TakeDamageList.Contains) InvokeEffects("DamageReaction");
            }
            async Task LateWalking()
            {
                if(LateDashTarget == new Checkers(position)) return;
                
                await LateTransport();

                async Task LateTransport()
                {
                    while(position != LateDashTarget.ToVector3())
                    {
                        position = Vector3.MoveTowards(position, LateDashTarget.ToVector3(), 0.3f);
                        await new WaitForFixedUpdate();
                    }
                    LateDashTarget = position;
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
                    foreach(var otherState in NowBalance.AdditionState) UpdateStateBar(otherState);

                Effects.RemoveAll(a=>a is OneUse);

                TakeDamageList.Clear();
            }

        #endregion
    
    #endregion
}