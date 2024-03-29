using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using SagardCL.MapObjectInfo;
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

        protected override string IdAddition { get => base.IdAddition + "Controllable"; } 

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
            return NowBalance.Stamina.WalkUseStamina > NowBalance.Stamina.Value;
            //OnDistance
            //return NowBalance.WalkDistance + 0.5f >= Checkers.Distance(new Checkers(this.position), position); 
        }    
    
    #endregion

    [Space(5)]
    [Header(" ==== Controller settings ==== ")]
    [SerializeField] Color Theme;
    override public MeshRenderer[] MustChangeColor { get => transform.parent.GetComponentsInChildren<MeshRenderer>(); }

    protected override void Start()
    {
        base.Start();

        MouseControlEvents.MouseController.AddListener((id, b) => 
        { 
            if(id != MPlaner.Planer | !(IsAlive & CanWalk & CanActing)) { MouseTest = 0; return; }
            MouseTest = b;
            switch(MouseTest)
            {
                default: StandingIn(); return;
                case 1: if(CanWalk) MovePlaningIn(); return;
                case 2: if(CanActing) AttackPlaningIn(); return;
            }
        });
        CursorController.ChangePosOnMap.AddListener(async(a)=>
        { 
            if(MouseTest == 2) {
                AttackTarget = a;
                AddActionToPlan(ActionOnIndex(SkillIndex), "UnitActing"); }

            if(MouseTest == 1) { 
                await Task.Delay(2);
                SetWayToTarget(a); 
                AddActionToPlan(ActionOnIndex(SkillIndex), "UnitActing");

                MPlaner.LineRenderer.positionCount = WalkWay.Count;
                MPlaner.LineRenderer.SetPositions(WalkWay.ConvertAll(new Converter<Checkers, Vector3>(a=>a.ToVector3())).ToArray()); 

                MPlaner.Renderer.enabled = a != new Checkers(this.position);
        } });
        CursorController.MouseWheelTurn.AddListener((a)=>
        {
            if(this.MouseTest == 2) {
                
                this.SkillIndex = Mathf.Clamp((int)Mathf.Round(SkillIndex + a * 12), 0, NowBalance.Skills.Count);
                AttackTarget = CursorController.position;
                AddActionToPlan(ActionOnIndex(SkillIndex), "UnitActing");
            }
        });
        Session.StepEnd.AddListener(()=>SkillIndex = 0);
    
        //Session.thisPlayerFigures.Add(transform.parent.name, this);
    }
    protected override void OnDestroy()
    {
        Session.thisPlayerFigures.Remove(transform.parent.name);
        base.OnDestroy();
    }
    
    private int MouseTest = 0;
    
    void StandingIn() {
        //Attack planner
        //if(!SkillRealizer.Check()) APlaner.position = MPlaner.position;

        
        Session.MapUpdate.Invoke();
        MapUpdate();

        MPlaner.Collider.enabled = true;
    }
    void MovePlaningIn() {
        Session.MapUpdate.Invoke();
    }
    void AttackPlaningIn()
    {
        SkillIndex = 0;
        AttackTarget = CursorController.position;
        AddActionToPlan(ActionOnIndex(SkillIndex), "UnitActing");
        
        Session.MapUpdate.Invoke();

        //APlaner.Renderer.material.color = (!SkillRealizer.Check())? Color.green : Color.red;
        //await AttackPlannerUpdate();
    }
}