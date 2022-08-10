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


[Serializable] public sealed class CharacterCoreController : CharacterCoreVisualized
{

    #region // ============================================================ Useful Stuff ==================================================================================================
    
        new public int SkillIndex { get { return base.SkillIndex; }  set { SetAttackTarget(CursorController.position); base.SkillIndex = value; } }

        protected override void SetName() {
            transform.parent.name = HaveID.GetName() + " - ControllableUnit";
            name += $"({transform.parent.name})";
        }

        bool CheckPosition(Checkers position, bool Other = true)
        {        
            if(!Other) return false;
            
            //OnOtherPlaner
            foreach (RaycastHit hit in Physics.RaycastAll(MoveTarget.Up(100), -Vector3.up, 105, LayerMask.GetMask("Object"))) 
                if(hit.collider.gameObject != MPlaner.Planer) return false; 
            
            
            //OnSelf
            if(new Checkers(position) == new Checkers(this.position))
                return false;
            
            //OnStamina
            if(NowBalance.Stamina.WalkUseStamina > NowBalance.Stamina.Value) return false;
            //OnDistance
            return NowBalance.WalkDistance + 0.5f >= Checkers.Distance(new Checkers(this.position), position); 
        }    
    
    #endregion

    [Space(5)]
    [Header(" ==== Controller settings ==== ")]
    [SerializeField] Color Theme;

    [SerializeField] bool CanControl = true;

    [field : SerializeField] override public Material[] MustChangeColor { get; set; }

    protected override void Start()
    {
        base.Start();

        InGameEvents.MouseController.AddListener((id, b) => 
        { 
            if(id != MPlaner.Planer | !(IsAlive & CanControl)) { MouseTest = 0; return; }
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
            if(MouseTest == 2) {
                SetAttackTarget(a); }

            if(MouseTest == 1) { 
                GenerateWayToTarget(CheckPosition(a)? a : this.position); 
                Generation.DrawAttack(await CurrentSkill.GetAttacks(MoveTarget, AttackTarget, this), this);

                MPlaner.LineRenderer.positionCount = WalkWay.Count;
                MPlaner.LineRenderer.SetPositions(Checkers.ToVector3List(WalkWay).ToArray()); 

                MPlaner.Renderer.enabled = CheckPosition(a);
        } });
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
        UnitUIController.UiEvent.Invoke("CloseForPlayer", gameObject, this);
    }
    void AttackPlaningIn()
    {
        
        SkillIndex = 0;
        UnitUIController.UiEvent.Invoke("OpenForPlayer", MPlaner.Planer, this);

        //APlaner.Renderer.material.color = (!SkillRealizer.Check())? Color.green : Color.red;
        //await AttackPlannerUpdate();
    }

}