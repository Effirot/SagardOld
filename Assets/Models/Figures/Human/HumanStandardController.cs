using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Threading.Tasks;

public class HumanStandardController : UnitController
{
    protected override void Standing() // Calling(void Update), when you no planing
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
    protected override void MovePlaning() // Calling(void Update), when you planing your moving
    {
        //Base model
        position = new Checkers(position, 0.7f);
        //Platform.transform.eulerAngles = new Vector3(0, Quaternion.LookRotation(MPlaner.pos - transform.position, -Vector3.up).eulerAngles.y + 180, 0);
        
        //Move planner
        MPlaner.position = new Checkers(CursorPos, 0.7f);
        MPlaner.Renderer.material.color = (!MPlanerChecker())? Color.green : Color.red;
        MPlaner.Renderer.enabled = true;
        MPlaner.Collider.enabled = false;
        
        //Attack planner
        if(Input.GetMouseButtonDown(1))APlaner.position = MPlaner.position;
        APlaner.Renderer.enabled = false;
    }
    protected override void AttackPlaning() // Calling(void Update), when you planing your attacks
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
        APlaner.LineRenderer.enabled = NowUsingSkill.Type != (HitType.Empty|HitType.OnSelf|HitType.SwordSwing|HitType.Constant);

        //Mouse Scroll
        SkillIndex = Mathf.Clamp(SkillIndex + (int)(Input.GetAxis("Mouse ScrollWheel") * 10), 0, Parameters.AvailableSkills.Count - 1);
    }

    protected async override void ParametersUpdate()
    {
        await Task.Delay(2);
        // Move planner
        if (MPlanerChecker()){
            MPlaner.LineRenderer.positionCount = Checkers.PatchWay.WayTo(new Checkers(position), new Checkers(MPlaner.position)).Length;
            MPlaner.LineRenderer.SetPositions(Checkers.PatchWay.WayTo(new Checkers(position).ToVector3, new Checkers(MPlaner.position).ToVector3)); 
        }
        MPlaner.LineRenderer.enabled = MPlanerChecker();

        // Attack planner
        if (NowUsingSkill.Check()){
            APlaner.LineRenderer.positionCount = NowUsingSkill.Line().Length;
            APlaner.LineRenderer.SetPositions(NowUsingSkill.Line()); 
        }

        APlaner.Renderer.material.color = (!NowUsingSkill.Check())? Color.green : Color.red;
        APlaner.LineRenderer.enabled = 
        NowUsingSkill.Check() & 
        NowUsingSkill.Type != (HitType.Empty|HitType.OnSelf|HitType.SwordSwing|HitType.Constant);

        AttackZone = NowUsingSkill.DamageZone();
    }

    protected override void ControlChange() // Calling, when global event change control mode 
    {
        ParametersUpdate();
    }

}