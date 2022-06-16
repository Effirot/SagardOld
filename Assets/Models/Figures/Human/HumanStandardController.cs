using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Threading.Tasks;

public class HumanStandardController : UnitController
{
    List<Attack> AttackZone = new List<Attack>();
    List<Checkers> WalkWay = new List<Checkers>();


    void OnEnable() { ParametersUpdate(); }

    protected override void StandingUpd() // Calling(void Update), when you no planing
    {
        //Base model
        MPlaner.Renderer.enabled = MPlanerChecker();
        
        //Move planner
        MPlaner.Collider.enabled = true;
    
        //Attack planner
        if(!NowUsingSkill.Check()) APlaner.position = MPlaner.position;

        APlaner.Renderer.enabled = NowUsingSkill.Check();
    }
    protected override void MovePlaningUpd() // Calling(void Update), when you planing your moving
    {
        MPlaner.Renderer.enabled = true;
        
        if(NowUsingSkill.NoWalking) APlaner.position = new Checkers(MPlaner.position);
        APlaner.position = new Checkers(APlaner.position);

        //Move planner
        MPlaner.position = new Checkers(CursorPos);
        MPlaner.Collider.enabled = false;
        
    }
    protected override void AttackPlaningUpd() // Calling(void Update), when you planing your attacks
    {
        //Move planner
        if(NowUsingSkill.NoWalking)
        {
            MPlaner.position = position;
            MPlaner.Renderer.enabled = false;
            MPlaner.Collider.enabled = true;
        }

        //Attack planner
        APlaner.position = new Checkers(CursorPos);

        //Mouse Scroll
        CurrentSkillIndex = Mathf.Clamp(CurrentSkillIndex + (int)(Input.GetAxis("Mouse ScrollWheel") * 10), 0, Parameters.AvailableSkills.Count - 1);
    }

    protected async override void ParametersUpdate()
    {
        await Task.Delay(1);
        

        foreach(Skill skill in Parameters.AvailableSkills)
        {
            skill.From = MPlaner;
            skill.To = APlaner;
        }
        position = new Checkers(position);

        // Move planner
        if(!MPlanerChecker())MPlaner.position = position;
        WalkWay.Clear();
        if (MPlanerChecker()){
            WalkWay.AddRange(Checkers.PatchWay.WayTo(new Checkers(position), new Checkers(MPlaner.position)));

            MPlaner.LineRenderer.positionCount = Checkers.PatchWay.WayTo(new Checkers(position), new Checkers(MPlaner.position)).Length;
            MPlaner.LineRenderer.SetPositions(Checkers.PatchWay.WayTo(new Checkers(position).ToVector3, new Checkers(MPlaner.position).ToVector3)); 
        }
        MPlaner.LineRenderer.enabled = MPlanerChecker();

        // Attack planner
        APlaner.Renderer.enabled = APlaner.position != position & APlaner.position !=  MPlaner.position & NowUsingSkill.Type != (HitType.Empty & HitType.OnSelf & HitType.SwordSwing & HitType.Constant);
        APlaner.Renderer.material.color = (!NowUsingSkill.Check())? Color.green : Color.red;
        NowUsingSkill.Graphics();        
        
        AttackZone.Clear();
        await foreach(Attack attack in NowUsingSkill.DamageZone())
        {
            AttackZone.Add(attack);
        }
        AttackVsualizationClear();
        AttackVisualization(AttackZone);

        DebugLogsPrint.LogEvent.Invoke();
    }

    protected override void ControlChange() // Calling, when global event change control mode 
    {
        ParametersUpdate();
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
                if(position.x == WalkWay[PointNum].ToVector3.x & position.y == WalkWay[PointNum].ToVector3.y){ PointNum++; }
                yield return null;
            }
            yield break;
        }
        StartCoroutine(MPlanerMove());
        await Task.Run(MPlanerMove);
        ParametersUpdate();

        Debug.Log("I walked");
    }

}