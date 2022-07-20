using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using SagardCL.IParameterManipulate;
using System.Threading.Tasks;
using System;
using Random = UnityEngine.Random;
using UnityAsync;

#if UNITY_EDITOR
using UnityEditor;
#endif


[Serializable] public abstract class UnitController : Parameters
{
    public override Type type{ get{ return typeof(UnitController); }}

    #region // ============================================================ Useful Stuff ==================================================================================================
        
        private protected AllInOne MPlaner { get{ return SkillRealizer.From; } set { SkillRealizer.From = value; } }
        private protected AllInOne APlaner { get{ return SkillRealizer.To; } set { SkillRealizer.To = value; } }

        Collider Collider => GetComponent<MeshCollider>();

        static Checkers LastPose = new Checkers();
        Checkers CursorPos { get { Checkers pos = CursorController.Pos; if(LastPose != pos) { LastPose = pos; ChangePos(); } return pos; } }

        protected bool WalkChecker(bool Other = true)
        {        
            if(!Other) return false;
            
            //OnOtherPlaner
            foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(0, 100, 0) + MPlaner.position, -Vector3.up, 105, LayerMask.GetMask("Object"))) 
            { 
                if(hit.collider.gameObject != MPlaner.Planer) { return false; }
            }
            
            //OnSelf
            if(new Checkers(position) == new Checkers(MPlaner.position))
                return false;
            
            //OnStamina
            if(Stamina.WalkUseStamina > Stamina.Value) return false;
            //OnDistance
            return WalkDistance + AllItemStats.WalkDistance + 0.5f >= Checkers.Distance(MPlaner.position, position); 
        }
    
        public int CurrentSkillIndex { get { return SkillRealizer.SkillIndex; } set { if(value != SkillRealizer.SkillIndex) MouseWheelTurn(); SkillRealizer.SkillIndex = value; } }
    #endregion

    #region // ================================== controlling
    
        [SerializeField] Color Team;

        [SerializeField] bool CanControl = true;
        [SerializeField] bool Corpse = false;
        [SerializeField] int WalkDistance = 5;
            
    #endregion
    #region // ================================== Skills

    public SkillCombiner SkillRealizer { get{ return _SkillRealizer; } set { _SkillRealizer = value; } }
    [SerializeField] SkillCombiner _SkillRealizer = new SkillCombiner();

    protected List<Attack> AttackZone = new List<Attack>();
    protected List<Checkers> WalkWay = new List<Checkers>();
    
    #endregion
    #region // ================================== inventory
    
        [SerializeField] List<Item> _Inventory;
        public int InventorySize = 1;
        [SerializeField] public List<Item> ArtifacerItems;
        public int ArtifacerInventorySize = 0;
        public List<Item> Inventory { get { return FieldManipulate.CombineLists<Item>(_Inventory, ArtifacerItems); } set{ _Inventory = value; } }

        public ParamsChanger AllItemStats;

        public new List<IOtherBar> OtherStates { get { return FieldManipulate.CombineLists<IOtherBar>(base.OtherStates, AllItemStats.AdditionState); } set{ base.OtherStates = value; } }
    #endregion
    #region // ============================================================ Methods ========================================================================================================
        
        private int MouseTest = 0;
        protected override async void Start()
        {
            base.Start();
            InGameEvents.MouseController.AddListener((id, b) => 
            { 
                if(id != MPlaner.Planer | !(!Corpse & CanControl)) { MouseTest = 0; return; }
                MouseTest = b;
                switch(MouseTest)
                {
                    default: StandingIn(); return;
                    case 1: MovePlaningIn(); return;
                    case 2: AttackPlaningIn(); return;
                }
            });

            InGameEvents.AttackTransporter.AddListener((a) => { 
                Attack find = a.Find((a) => a.Where == new Checkers(position));
                if(find.Where == new Checkers(position)){
                    DamageReaction(find);
                    GetDamage(find);
                }
            });

            InGameEvents.StepEnd.AddListener(EveryStepEnd);
            AfterInventoryUpdate();

            await Task.Delay(10);
            position = new Checkers(position);
            MPlaner.position = new Checkers(MPlaner.position);
        }
        void Update()
        {   
            switch(MouseTest)
            {
                default: return;
                case 1: MovePlaningUpd(); return;
                case 2: AttackPlaningUpd(); return;
                case 4: break;
            }
        }

        // Control use methods   
        async void MouseWheelTurn(){ await AttackPlannerUpdate();  }
        async void ChangePos() {  if(MouseTest == 2) await AttackPlannerUpdate(); if(MouseTest == 1) ParametersUpdate(); }
        
        // Standing methods
        void StandingIn()
        {
            if(!WalkChecker()) MPlaner.position = position;
            MPlaner.Renderer.enabled = WalkChecker();

            //Attack planner
            if(!SkillRealizer.Check()) APlaner.position = MPlaner.position;


            UnitUIController.UiEvent.Invoke("CloseForPlayer", gameObject, this);
            
            InGameEvents.MapUpdate.Invoke();

            if(!WalkChecker() & InGameEvents.Controllable) MPlaner.position = position;
            MPlaner.Collider.enabled = true;
        }
        // Move planning methods
        void MovePlaningUpd() // Calling(void Update), when you planing your moving
        {
            MPlaner.Renderer.enabled = true;
            
            if(SkillRealizer.ThisSkill.NoWalking) APlaner.position = new Checkers(MPlaner.position);

            //Move planner
            MPlaner.position = new Checkers(CursorPos);
            MPlaner.Collider.enabled = false;
        }
        async void MovePlaningIn()
        {
            APlaner.position = new Checkers(APlaner.position);
            UnitUIController.UiEvent.Invoke("CloseForPlayer", gameObject, this);
            await MovePlannerUpdate();
        }
        // Attack planning methods
        void AttackPlaningUpd() // Calling(void Update), when you planing your attacks
        {

            //Move planner
            if(SkillRealizer.ThisSkill.NoWalking)
            {
                MPlaner.position = position;
                MPlaner.Collider.enabled = true;
            }

            //Attack planner
            APlaner.position = new Checkers(CursorPos);

            //Mouse Scroll
            CurrentSkillIndex = Mathf.Clamp(CurrentSkillIndex + (int)(Input.GetAxis("Mouse ScrollWheel") * 10), 0, SkillRealizer.AvailbleSkills.Count - 1);
        }
        async void AttackPlaningIn()
        {
            CurrentSkillIndex = 0;
            UnitUIController.UiEvent.Invoke("OpenForPlayer", MPlaner.Planer, this);

            APlaner.Renderer.material.color = (!SkillRealizer.Check())? Color.green : Color.red;

            await AttackPlannerUpdate();
        }

        #region // =============================== Step System
            async Task Walking()
            {
                if(WalkWay.Count == 0) return;

                MouseTest = 4;
                WillRest = false;
                await Task.Delay(30);

                await MovePlannerUpdate();
                Stamina.GetTired(Stamina.WalkUseStamina);
                await MPlanerMove();

                MPlaner.Renderer.enabled = false;
                MouseTest = 0;

                ParametersUpdate();

                async Task MPlanerMove()
                {   
                    int PointNum = 1;
                    for(float i = 0.0001f; position != WalkWay[WalkWay.Count - 1].ToVector3(); i *= 1.3f)
                    {
                        await Await.Updates(7);
                        position = Vector3.MoveTowards(position, WalkWay[PointNum], i);
                        MPlaner.LineRenderer.SetPosition(0, position);
                        if(position == WalkWay[PointNum].ToVector3() & position != WalkWay[WalkWay.Count - 1].ToVector3()){ PointNum++; }
                    }
                }
            }     
            async Task PriorityAttacking()
            {
                if(AttackZone.Count == 0) return;
                if(!SkillRealizer.ThisSkill.PriorityAttacking) return;
                WillRest = false;
                await Task.Delay(Random.Range(0, 2700));

                InGameEvents.AttackTransporter.Invoke(AttackZone);
                AttackZone.Clear();
                APlaner.position = MPlaner.position;

                await AttackPlannerUpdate();
            }
            async Task Attacking()
            {
                if(AttackZone.Count == 0) return;
                if(SkillRealizer.ThisSkill.PriorityAttacking) return;
                WillRest = false;
                await Task.Delay(Random.Range(900, 2700));

                Stamina.GetTired(SkillRealizer.StaminaWaste());
                InGameEvents.AttackTransporter.Invoke(AttackZone);
                AttackZone.Clear();
                APlaner.position = MPlaner.position;
                
                await AttackPlannerUpdate();
            }
            protected override async Task Dead() 
            { 
                if(Health.Value > 0) return;
                await Task.Delay(Random.Range(10, 100)); 
                Corpse = true;
                try { LostHealth(); } catch {}
            }
            async Task Rest() 
            { 
                if(!WillRest) { WillRest = true; return; }
                await Task.Delay(Random.Range(0, 2300)); 
                AfterInventoryUpdate(); 
                Stamina.Rest();
            }
            public bool WillRest = true;
        #endregion

    #region // =============================== Update methods

        
        protected override async void ParametersUpdate()
        {
            await MovePlannerUpdate();
            await AttackPlannerUpdate();
        }
        protected async Task MovePlannerUpdate()
        {
            await Task.Delay(1);


            // Move planner
            if(!WalkChecker()) { MPlaner.LineRenderer.enabled = false; WalkWay.Clear(); return; }
            MPlaner.LineRenderer.enabled = true;
            WalkWay.Clear();
            if (WalkChecker()){

                WalkWay = Checkers.PatchWay.WayTo(new Checkers(position), new Checkers(MPlaner.position), 20);

                MPlaner.LineRenderer.positionCount = WalkWay.Count;
                MPlaner.LineRenderer.SetPositions(Checkers.ToVector3List(WalkWay).ToArray()); 
            }
        }
        protected async Task AttackPlannerUpdate()
        {
            await Task.Delay(1);
            APlaner.position = new Checkers(APlaner.position);
            // Attack planner
            AttackZone.Clear();
            if(SkillRealizer.ThisSkill.NoWalking) await MovePlannerUpdate();
            await foreach(Attack attack in SkillRealizer.Realize())
            {
                AttackZone.Add(attack);
            }
            Generation.DrawAttack(AttackZone, this);
            
            SkillRealizer.Graphics(); 
        }
        public virtual void AfterInventoryUpdate()
        {
            AllItemStats = Item.CompoundParameters(Inventory);

            Health.Max = BaseHealth.Max + AllItemStats.Health.Max;
            Stamina.Max = BaseStamina.Max + AllItemStats.Stamina.Max;
            Sanity.Max = BaseSanity.Max + AllItemStats.Sanity.Max;
        } 

        private void GetDamage(Attack attack)
        {
            Health.GetDamage(attack); 
            if(attack.damage > 0) ChangeFigureColorWave(attack.DamageColor(), 0.2f);
        }
        public override void LostHealth()
        {
            if(Health is HealthCorpse) { Destroy(transform.parent.gameObject); }
            else {
                Health = new HealthCorpse() { Max = Health.Max, Value = Health.Max, ArmorMelee = Health.ArmorMelee, ArmorRange = Health.ArmorRange };

                ChangeFigureColor(new Color(0.6f, 0.6f, 0.6f), 0.2f);
            }
        }

    #endregion
    
    #endregion
}