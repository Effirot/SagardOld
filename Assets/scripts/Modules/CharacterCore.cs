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

public class CharacterCore : MonoBehaviour, IObjectOnMap, IDeadable, IGetableCrazy, ITiredable, IStorage, IEffector, IAttacker, IWalk, HaveID {
    
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
    
    #endregion
    
    public static readonly Checkers BufferLocation = new Checkers(3324, 5981);
    #region // =========================================================== All parameters =================================================================================================
    
        protected virtual async void Start()
        {
            TakeDamageList.Clear();

            transform.parent.name = HaveID.GetName();
            name += $"({transform.parent.name})";

            InGameEvents.MapUpdate.AddListener(async() => {
                await MovePlannerSet(MPlaner.position, MPlaner.Renderer.enabled);
                await AttackPlannerRender(AttackPose);
            });
            InGameEvents.StepSystem.Add(FindStepStage);
            InGameEvents.AttackTransporter.AddListener((a) => { 
                foreach(Attack attack in a.FindAll((a) => a.Position == new Checkers(position))) { 
                    TakeDamageList.Add(attack); }
            });
            InGameEvents.StepEnd.AddListener(EveryStepEnd);
            
            AfterInventoryUpdate();

            await Task.Delay(10);
            position = new Checkers(position);
            MPlaner.position = new Checkers(position);
        }

        #region // ================================== parameters

            public RacePassiveEffect RaceEffect { get; private set; } 

            [field: SerializeField] public Race Race { get; private set; }
            [field: SerializeField] public bool Alive { get; set; }

            [field : SerializeField] public Balancer NowBalance { get; private set; }
            [field : SerializeField] public Balancer BaseBalance { get; private set; }
            ReBalancer AllBalanceChanges;
            
            [field: SerializeReference, SubclassSelector] public List<Effect> Effects { get; set; } = new List<Effect>();

            public virtual Checkers AttackPose { get; set; }
            public int SkillIndex { get; set; } = 0;
            public Skill CurrentSkill { get { return NowBalance.Skills[SkillIndex]; } } 


            public Attack.AttackCombiner TakeDamageList { get; set; } = Attack.AttackCombiner.Empty();

            public List<Checkers> WalkWay { get; set; } = new List<Checkers>();


            public void AddDamage(params Attack[] attacks) {
                foreach(Attack attack in attacks)
                {
                    if(attack.DamageType == DamageType.Heal & Alive) return;
                    TakeDamageList.Add(attack);
                }
            }
                        
            public void AddEffect(params Effect[] Effect) {
                foreach(Effect effect in Effect) { 
                    effect.Target = (IObjectOnMap)this; 
                    if(!effect.Workable()) continue; 

                    effect.InvokeMethod("WhenAdded"); 
                    Effects.Add(effect); 
                }
            }
            public void AutoRemoveEffect() {
                List<Effect> Effect = Effects.FindAll(a=>!a.Workable());

                RemoveEffect(Effect.ToArray());
            }
            public void RemoveEffect(params Effect[] Effect) {
                foreach(Effect effect in Effect) { 
                    effect.InvokeMethod("WhenRemoved"); 
                    Effects.Remove(effect); 
                }
            }
            
            public void InvokeEffects(string Method)
            {
                foreach(Effect effect in Effects)
                {
                    effect.Target = this;
                    effect.InvokeMethod(Method);
                }
                AutoRemoveEffect();
            }

        #endregion
        #region // ================================== inventory
        
            [SerializeField] List<Item> _Inventory;
            public int InventorySize = 1;

            public List<Item> Inventory { get { return _Inventory; } set { _Inventory = value; } }

        #endregion

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
                if(NowBalance.Stamina.WalkUseStamina > NowBalance.Stamina.Value) return false;
                //OnDistance
                return NowBalance.WalkDistance + AllBalanceChanges.WalkDistance + 0.5f >= Checkers.Distance(new Checkers(this.position), position); 
            }    

            public async Task MovePlannerSet(Checkers position, bool Draw = true, bool CustomWay = false)
            {
                await Task.Delay(2);
                
                MPlaner.LineRenderer.enabled = CheckPosition(position) & Draw;
                MPlaner.Renderer.enabled = CheckPosition(position) & Draw;

                if(Attacker.CurrentSkill.NoWalking & position == new Checkers(this.position)) 
                    await AttackPlannerRender(position);

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
            public async Task AttackPlannerRender(Checkers position)
            {
                AttackPose = new Checkers(position);

                if(Attacker.CurrentSkill.NoWalking) 
                    await MovePlannerSet(this.position, false);               

                Generation.DrawAttack(await CurrentSkill.GetAttacks(MPlaner.position, AttackPose, this), this);
                
            }

            public void LostHealth()
            {
                if(Alive) Destroy(transform.parent.gameObject);
                else { 
                    Alive = true;
                    
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

                await CurrentSkill.Complete(MPlaner.position, AttackPose, this);

                await AttackPlannerRender(position);
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
                
                TakeDamageList.Combine().ForEach(a=>{BaseBalance.Health.Value += NowBalance.Health.GetDamage(a); Debug.LogWarning($"{transform.parent.name} :- {NowBalance.Health.GetDamage(a)} {TakeDamageList.Combine().Sum(a=>a.Damage)}"); });

                foreach(Attack attack in TakeDamageList.Combine()) 
                    AddEffect(attack.Effects);
                
                if(TakeDamageList.Combine().Sum(a=>a.Damage) > 0) ChangeFigureColorWave(TakeDamageList.CombinedColor(), 0.1f);
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

                UpdateParameter(NowBalance.Health);
                UpdateParameter(NowBalance.Stamina);
                UpdateParameter(NowBalance.Sanity);
                if(NowBalance.AdditionState is not null && NowBalance.AdditionState.Count > 0) 
                    foreach(ICustomBar otherState in NowBalance.AdditionState) UpdateParameter(otherState);

                Effects.RemoveAll(a=>a is OneUse);

                TakeDamageList.Clear();
            }
        
        #endregion
    
    #endregion
}