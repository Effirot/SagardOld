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
    
        public int CurrentSkillIndex { get { return SkillIndex; }  set { new Action(async () => await AttackPlannerRender(CursorPos)).Invoke(); SkillIndex = value; } }
    
    #endregion
    #region // ================================== controlling

        public override Checkers AttackPose { get{ return APlaner.position; } set { APlaner.position = value; } }

        [SerializeField] Color Theme;

        [SerializeField] bool CanControl = true;
            
    #endregion

    #region // ============================================================ Methods ========================================================================================================
        
        
        protected override void Start()
        {
            base.Start();

            InGameEvents.MouseController.AddListener((id, b) => 
            { 
                if(id != MPlaner.Planer | !(!Alive & CanControl)) { MouseTest = 0; return; }
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
                    await AttackPlannerRender(a); 

                if(MouseTest == 1) { 
                    await MovePlannerSet(a); 
                    await AttackPlannerRender(CursorController.position);  
            } }
            );
        }
        
        private int MouseTest = 0;
        
        void StandingIn() {
            //Attack planner
            //if(!SkillRealizer.Check()) APlaner.position = MPlaner.position;

            UnitUIController.UiEvent.Invoke("CloseForPlayer", gameObject, this);
            
            InGameEvents.MapUpdate.Invoke();

            MPlaner.Collider.enabled = true;
        }
        void MovePlaningIn() {

            APlaner.position = new Checkers(APlaner.position);
            UnitUIController.UiEvent.Invoke("CloseForPlayer", gameObject, this);
        }
        void AttackPlaningIn()
        {
            
            CurrentSkillIndex = 0;
            UnitUIController.UiEvent.Invoke("OpenForPlayer", MPlaner.Planer, this);

            //APlaner.Renderer.material.color = (!SkillRealizer.Check())? Color.green : Color.red;
            //await AttackPlannerUpdate();
        }
    
    #endregion
}