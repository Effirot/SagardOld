using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using SagardCL.ParameterManipulate;
using System.Threading.Tasks;
using System;
using Random = UnityEngine.Random;
using UnityAsync;

#if UNITY_EDITOR
using UnityEditor;
#endif


[Serializable] public sealed class CharacterController : CharacterCore
{

    #region // ============================================================ Useful Stuff ==================================================================================================

        Collider Collider => GetComponent<MeshCollider>();

        static Checkers LastPose = new Checkers();
        Checkers CursorPos { get { Checkers pos = CursorController.Pos; if(LastPose != pos) { LastPose = pos; ChangePos(); } return pos; } }
    
        public int CurrentSkillIndex { get { return SkillRealizer.SkillIndex; } set { SkillRealizer.SkillIndex = value; } }
    
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
            }
        }

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
    
    #endregion
}