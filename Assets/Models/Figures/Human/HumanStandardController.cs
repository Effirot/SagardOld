using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Threading.Tasks;

public class HumanStandardController : UnitController
{
    protected override void StandingUpd() // Calling(void Update), when you no planing
    {
        //Base model
        position = Vector3.MoveTowards(position, new Checkers(position), 2.5f * Time.deltaTime);
        
        //Move planner
        MPlaner.position = (!MPlanerChecker())?
        position :
        MPlaner.position;

        MPlaner.Renderer.enabled = MPlanerChecker();
        MPlaner.Collider.enabled = true;
    
        //Attack planner
        APlaner.position = (!NowUsingSkill.Check())?
        MPlaner.position :
        APlaner.position;

        APlaner.Renderer.enabled = NowUsingSkill.Check();
    }
    protected override void MovePlaningUpd() // Calling(void Update), when you planing your moving
    {
        //Base model
        position = new Checkers(position, 0.7f);
        //Platform.transform.eulerAngles = new Vector3(0, Quaternion.LookRotation(MPlaner.pos - transform.position, -Vector3.up).eulerAngles.y + 180, 0);
        
        if(NowUsingSkill.NoWalking) APlaner.position = new Checkers(MPlaner.position);
        APlaner.position = new Checkers(APlaner.position, 0.7f);

        //Move planner
        MPlaner.position = new Checkers(CursorPos, 0.7f);
        
        MPlaner.Renderer.enabled = true;
        MPlaner.Collider.enabled = false;
        
    }
    protected override void AttackPlaningUpd() // Calling(void Update), when you planing your attacks
    {
        //Base model
        position = new Checkers(position, 0.7f);

        //Move planner
        if(NowUsingSkill.NoWalking)
        {
            MPlaner.position = position;
            MPlaner.Renderer.enabled = false;
            MPlaner.Collider.enabled = true;
        }

        //Attack planner
        APlaner.position = new Checkers(CursorPos, 0.7f);

        //Mouse Scroll
        CurrentSkillIndex = Mathf.Clamp(CurrentSkillIndex + (int)(Input.GetAxis("Mouse ScrollWheel") * 10), 0, Parameters.AvailableSkills.Count - 1);
    }

    protected async override void ParametersUpdate()
    {
        await Task.Delay(1);
        // Move planner
        if (MPlanerChecker()){
            MPlaner.LineRenderer.positionCount = Checkers.PatchWay.WayTo(new Checkers(position), new Checkers(MPlaner.position)).Length;
            MPlaner.LineRenderer.SetPositions(Checkers.PatchWay.WayTo(new Checkers(position).ToVector3, new Checkers(MPlaner.position).ToVector3)); 
        }
        MPlaner.LineRenderer.enabled = MPlanerChecker();

        MPlaner.Renderer.material.color = (!MPlanerChecker())? Color.green : Color.red;

        // Attack planner
        APlaner.Renderer.enabled = APlaner.position != position & APlaner.position !=  MPlaner.position & NowUsingSkill.Type != (HitType.Empty & HitType.OnSelf & HitType.SwordSwing & HitType.Constant);
        APlaner.Renderer.material.color = (!NowUsingSkill.Check())? Color.green : Color.red;
        NowUsingSkill.Graphics();        

        AttackZone = NowUsingSkill.DamageZone();
    }

    protected override void ControlChange() // Calling, when global event change control mode 
    {
        ParametersUpdate();
    }

}