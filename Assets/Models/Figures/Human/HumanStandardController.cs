using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;

public class HumanStandardController : UnitController
{
    protected override void Standing()
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
    protected override void MovePlaning()
    {
        //Base model
        position = new Checkers(position, 0.7f);
        //Platform.transform.eulerAngles = new Vector3(0, Quaternion.LookRotation(MPlaner.pos - transform.position, -Vector3.up).eulerAngles.y + 180, 0);
        
        //Move planner
        MPlaner.position = CursorPos(0.7f);
        MPlaner.Renderer.material.color = (!MPlanerChecker())? Color.green : Color.red;
        MPlaner.Renderer.enabled = true;
        MPlaner.Collider.enabled = false;
        
        //Attack planner
        if(Input.GetMouseButtonDown(1))APlaner.position = MPlaner.position;
        APlaner.Renderer.enabled = false;
    }
    protected override void AttackPlaning()
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
        APlaner.position = CursorPos(1f);

        APlaner.Renderer.material.color = (!NowUsingSkill.Check())? Color.green : Color.red;
        APlaner.Renderer.enabled = NowUsingSkill.Type != HitType.Empty | NowUsingSkill.Type != HitType.OnSelf;

        //Mouse Scroll
        SkillIndex = Mathf.Clamp(SkillIndex + (int)(Input.GetAxis("Mouse ScrollWheel") * 10), 0, Parameters.AvailableSkills.Count - 1);
    }

    protected override void ParameterUpdate()
    {
        // Move planner
        if (MPlanerChecker()){
            MPlaner.LineRenderer.positionCount = Checkers.PatchWay.WayTo(position, MPlaner.position).Length;
            MPlaner.LineRenderer.SetPositions(Checkers.PatchWay.WayTo(position, MPlaner.position)); 
            Debug.DrawLine(position, MPlaner.position, Color.yellow);
        }
        MPlaner.LineRenderer.enabled = MPlanerChecker();

        // Attack planner
        if (NowUsingSkill.Check()){
            APlaner.LineRenderer.positionCount = NowUsingSkill.Line().Length;
            APlaner.LineRenderer.SetPositions(NowUsingSkill.Line()); 
        }
        APlaner.LineRenderer.enabled = NowUsingSkill.Check();

        AttackZone = NowUsingSkill.DamageZone();
    }



}