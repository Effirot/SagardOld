using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Threading.Tasks;

public class HumanStandardController : UnitController
{    
    List<Attack> AttackZone = new List<Attack>();
    List<Checkers> WalkWay = new List<Checkers>();

    async void Start(){ await Task.Delay(1); position = new Checkers(position); }

    protected override void StandingUpd() // Calling(void Update), when you no planing
    {
        //Base model
        MPlaner.Renderer.enabled = MPlanerChecker();
        
        //Move planner
        MPlaner.Collider.enabled = true;
        if(!MPlanerChecker() & InGameEvents.canControl) MPlaner.position = position;
    
        //Attack planner
        if(!NowUsingSkill.Check()) APlaner.position = MPlaner.position;
        APlaner.Renderer.enabled = NowUsingSkill.Check();
    }
    protected override void MovePlaningUpd() // Calling(void Update), when you planing your moving
    {
        MPlaner.Renderer.enabled = true;
        
        if(NowUsingSkill.NowUsing.NoWalking) APlaner.position = new Checkers(MPlaner.position);

        //Move planner
        MPlaner.position = new Checkers(CursorPos);
        MPlaner.Collider.enabled = false;
        
    }
    protected override void AttackPlaningUpd() // Calling(void Update), when you planing your attacks
    {

        //Move planner
        if(NowUsingSkill.NowUsing.NoWalking)
        {
            MPlaner.position = position;
            MPlaner.Renderer.enabled = false;
            MPlaner.Collider.enabled = true;
        }

        //Attack planner
        APlaner.position = new Checkers(CursorPos);

        //Mouse Scroll
        CurrentSkillIndex = Mathf.Clamp(CurrentSkillIndex + (int)(Input.GetAxis("Mouse ScrollWheel") * 10), 0, Parameters.SkillRealizer.AvailbleBaseSkills.Count - 1);
    }

    protected override void StandingIn()
    {
        ParametersUpdate();
    }
    protected async override void MovePlaningIn()
    {
        await MovePlannerUpdate();
    }
    protected async override void AttackPlaningIn()
    {
        SummonMenu();
        await AttackPlannerUpdate();
    }



    protected async override void MouseWheelTurn(){ await AttackPlannerUpdate();  }
    protected async override void ChangePos() {  if(MouseTest == 2) await AttackPlannerUpdate(); if(MouseTest == 1) ParametersUpdate(); }
    
    protected async override void ParametersUpdate()
    {
        await MovePlannerUpdate();
        await AttackPlannerUpdate();
    }
    
    private async Task MovePlannerUpdate()
    {
        await Task.Delay(1);

        // Move planner
        if(!MPlanerChecker()) { MPlaner.LineRenderer.enabled = false; return;}
        MPlaner.LineRenderer.enabled = true;
        WalkWay.Clear();
        if (MPlanerChecker()){
            await foreach(Checkers step in Checkers.PatchWay.WayTo(new Checkers(position), new Checkers(MPlaner.position))){
                WalkWay.Add(step);
            }
            MPlaner.LineRenderer.positionCount = WalkWay.Count;
            MPlaner.LineRenderer.SetPositions(Checkers.ToVector3List(WalkWay).ToArray()); 
        }
    }
    private async Task AttackPlannerUpdate()
    {
        await Task.Delay(1);
        APlaner.position = new Checkers(APlaner.position);
        // Attack planner
        AttackZone.Clear();
        await foreach(Attack attack in NowUsingSkill.Realize())
        {
            AttackZone.Add(attack);
        }
        AttackVsualizationClear();
        AttackVisualization(AttackZone);

        APlaner.Renderer.enabled = APlaner.position != position & APlaner.position !=  MPlaner.position & NowUsingSkill.NowUsing.Type != (HitType.Empty & HitType.OnSelf & HitType.SwordSwing & HitType.Constant);
        APlaner.Renderer.material.color = (!NowUsingSkill.Check())? Color.green : Color.red;
        NowUsingSkill.Graphics(); 
    }

    protected async override Task Walking()
    {
        await Task.Delay(10);

        IEnumerator MPlanerMove()
        {
            int PointNum = 1;
            for(float i = 0.00001f; position != MPlaner.position; i *= 1.025f)
            {
                position = Vector3.MoveTowards(position, WalkWay[PointNum], i);
                MPlaner.LineRenderer.SetPosition(0, position);
                if(position.x == WalkWay[PointNum].ToVector3().x & position.y == WalkWay[PointNum].ToVector3().y){ PointNum++; }
                yield return null;
            }
            yield break;
        }
        StartCoroutine(MPlanerMove());
        await Task.Run(MPlanerMove);
        ParametersUpdate();
    }

    protected async override Task Attacking()
    {
        await Task.Delay(Random.Range(0, 2700));

        InGameEvents.AttackTransporter.Invoke(AttackZone);
        AttackZone.Clear();
        APlaner.position = position;
        await AttackPlannerUpdate();
    }
    protected override void GetDamage(List<Attack> attacks) 
    {
        Attack thisAttack = attacks.Find((a) => a.Where == new Checkers(position));
        if(attacks.Find((a) => a.Where == new Checkers(position)).Where == new Checkers(position))
        {
            Parameters.GetDamage(thisAttack);
        }
    }


    private void SummonMenu(){
        GameObject obj = Instantiate(UiPreset, GameObject.Find("GameUI").transform);
        obj.GetComponent<UnitUIController>().LifeParams = Parameters;
        obj.GetComponent<MoveOnUi>().Target = MPlaner.Planer.transform;
    }

}