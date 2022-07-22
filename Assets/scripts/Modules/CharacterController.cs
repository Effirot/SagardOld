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


[Serializable] public abstract class CharacterController : CharacterCore
{

    #region // ============================================================ Useful Stuff ==================================================================================================


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
            
    #endregion

    #region // ============================================================ Methods ========================================================================================================
        
        private int MouseTest = 0;
        protected override void Start()
        {
            base.Start();
            InGameEvents.MapUpdate.AddListener(ParametersUpdate);

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
        async void ChangePos() { if(MouseTest == 2) await AttackPlannerUpdate(); if(MouseTest == 1) ParametersUpdate(); }
        
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


    #region // =============================== Update methods

        
        internal override async void ParametersUpdate()
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
            await foreach(Attack attack in SkillRealizer.Realize()) { AttackZone.Add(attack); }
            
            Generation.DrawAttack(AttackZone, this);
            
            SkillRealizer.Graphics(); 
        }

    #endregion
    
    #endregion
}