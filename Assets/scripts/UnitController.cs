using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;

public class UnitController : MonoBehaviour
{
    public Color Team;
    public int SkillIndex;
    public Skill NowUsingSkill
    { get{  try{return Parameters.AvailableSkills[SkillIndex];}
            catch{ return Skill.Empty();}} }
    protected Vector3 position{ get{ return transform.position; } set{ transform.position = value; } }
    
    [SerializeField]private protected GameObject Platform;
    [SerializeField]private protected AllInOne MPlaner;
    [SerializeField]private protected AllInOne APlaner;

    protected Checkers CursorPos(float Up){ return new Checkers(GameObject.Find("3DCursor").transform.position, Up); }
    
    [Space(3)]
    public ParameterList baseParameters;
    public ParameterList Parameters{ get{ return baseParameters; }}
    
    void Start()
    {
        foreach(Skill skill in Parameters.AvailableSkills)
        {
            skill.From = MPlaner;
            skill.To = APlaner;
        }
        parameterEdited();


        Platform.transform.eulerAngles += new Vector3(0, Random.Range(0f, 360f), 0);
    }

    protected enum Control { move, active, none }
    bool Changer = false, push = false;
    protected Control MouseTest(){
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            try{
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity) & (hit.collider.gameObject == gameObject || hit.collider.gameObject == MPlaner.Planer || hit.collider.gameObject == APlaner.Planer)) push = true;
                else Changer = false;
            }
            catch { }
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) push = false;

        if (Input.GetKey(KeyCode.Mouse1) && push) { Changer = false; return Control.move;  }
        else { if (Input.GetKeyDown(KeyCode.Mouse0) && push  ) Changer = !Changer; }

        if (Changer) { return Control.active; }
        else return Control.none;
    }
    protected bool MPlanerChecker(bool Other = true)
    {        
        bool OnOtherPlaner()
        {  
            foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(0, 100, 0) + MPlaner.position, -Vector3.up, 105, LayerMask.GetMask("Object"))) 
            { 
                if(hit.collider.gameObject != MPlaner.Planer) { return false; }
            }
            return true;
        }
        bool OnSelf()
        {
            return 
            (int)Mathf.Round(transform.position.x) == (int)Mathf.Round(MPlaner.position.x) 
            && 
            (int)Mathf.Round(transform.position.z) == (int)Mathf.Round(MPlaner.position.z);
        }
        bool OnDistance()
        {
            return Parameters.WalkDistance + 0.5f >= Checkers.Distance(MPlaner.position, transform.position); 
        }
        
        return Other && OnOtherPlaner() && !OnSelf() && OnDistance();
    }

    protected void parameterEdited()
    {
        // Move planner
        MPlaner.Model.transform.eulerAngles = Platform.transform.eulerAngles;

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
    }



}


