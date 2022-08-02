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
        Checkers CursorPos { get { return CursorController.position; } }
    
        public int CurrentSkillIndex { get { return SkillIndex; }  set { new Action(async () => await AttackPlannerSet(CursorPos)).Invoke(); SkillIndex = value; } }
    
    #endregion
    #region // ================================== controlling
    
        [SerializeField] Color Theme;

        [SerializeField] bool CanControl = true;
            
    #endregion

    #region // ============================================================ Methods ========================================================================================================
        
        
        protected override void Start()
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
            CursorController.ChangePosOnMap.AddListener(async(a)=>
            { 
                if(MouseTest == 2) 
                    await AttackPlannerSet(a); 

                if(MouseTest == 1) { 
                    await MovePlannerSet(a); 
                    await AttackPlannerSet(APlaner.position);  
            } }
            );
        }
        
        private int MouseTest = 0;
        
        // Standing methods
        void StandingIn() {
            //Attack planner
            //if(!SkillRealizer.Check()) APlaner.position = MPlaner.position;

            UnitUIController.UiEvent.Invoke("CloseForPlayer", gameObject, this);
            
            InGameEvents.MapUpdate.Invoke();

            MPlaner.Collider.enabled = true;
        }
        // Move planning methods
        // void MovePlaningUpd() // Calling(void Update), when you planing your moving
        // {
            
        //     // MPlaner.Renderer.enabled = true;
            
        //     // if(SkillRealizer.ThisSkill.NoWalking) APlaner.position = new Checkers(MPlaner.position);

        //     // //Move planner
        //     // MPlaner.position = new Checkers(CursorPos);
        //     // MPlaner.Collider.enabled = false;
        // }
        void MovePlaningIn() {

            APlaner.position = new Checkers(APlaner.position);
            UnitUIController.UiEvent.Invoke("CloseForPlayer", gameObject, this);
            //await MovePlannerUpdate();
        }
        // Attack planning methods
        // void AttackPlaningUpd() // Calling(void Update), when you planing your attacks
        // {
            
        //     // //Move planner
        //     // if(SkillRealizer.ThisSkill.NoWalking)
        //     // {
        //     //     MPlaner.position = position;
        //     //     MPlaner.Collider.enabled = true;
        //     // }

        //     // //Attack planner
        //     //APlanerPlane = new Checkers(CursorPos);
        // }
        void AttackPlaningIn()
        {
            
            CurrentSkillIndex = 0;
            UnitUIController.UiEvent.Invoke("OpenForPlayer", MPlaner.Planer, this);

            //APlaner.Renderer.material.color = (!SkillRealizer.Check())? Color.green : Color.red;
            //await AttackPlannerUpdate();
        }
    
    #endregion
}