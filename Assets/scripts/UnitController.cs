using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;

public class UnitController : MonoBehaviour
{
    public Color Team;
    public int SkillIndex;
    Skill NowUsingSkill{ get{ return Parameters.AvailableSkills[SkillIndex];} }
    Vector3 position{ get{ return transform.position; } set{ transform.position = value; } }
    public List<Attack> AttackList
    {
        get{ 
            if(NowUsingSkill.Check()) return NowUsingSkill.DamageZone(); 
            return new List<Attack>();
        }
    }
    
    [SerializeField]private GameObject Platform;
    [SerializeField]private AllInOne MPlaner;
    [SerializeField]private AllInOne APlaner;

    Checkers CursorPos(float Up){ return new Checkers(GameObject.Find("3DCursor").transform.position, Up); }
    
    [Space(3)]
    public ParameterList baseParameters;
    public ParameterList Parameters{ get{ return baseParameters; }}
    
    void Start()
    {
        foreach(Skill skill in Parameters.AvailableSkills)
        {
            skill.From = MPlaner.Planer;
            skill.To = APlaner.Planer;
        }
        parameterEdited();

        Platform.transform.eulerAngles += new Vector3(0, Random.Range(0f, 360f), 0);
    }



    void Update()
    {   
        parameterEdited();

        MPlaner.Model.transform.eulerAngles = Platform.transform.eulerAngles;
        APlaner.Model.transform.eulerAngles = Platform.transform.eulerAngles;

        switch(MouseTest())
        {
            default:
            {
                //Base model
                position = Vector3.Lerp(position, new Checkers(position), 
                2.5f * Time.deltaTime);
                
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
            break;
            case Controll.move:
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
            break;
            case Controll.active:
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
                APlaner.Renderer.enabled = true;
            }
            break;
        }
    }









    enum Controll { move, active, none }
    bool Changer = false, push = false;
    Controll MouseTest(){
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            try{
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity) & (hit.collider.gameObject == gameObject || hit.collider.gameObject == MPlaner.Planer || hit.collider.gameObject == APlaner.Planer)) push = true;
                else Changer = false;
            }
            catch { }
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) push = false;

        if (Input.GetKey(KeyCode.Mouse1) && push) { Changer = false; return Controll.move;  }
        else { if (Input.GetKeyDown(KeyCode.Mouse0) && push  ) Changer = !Changer; }

        if (Changer) { return Controll.active; }
        else return Controll.none;
    }
    bool MPlanerChecker(bool Other = true)
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


    void parameterEdited()
    {
        if (MPlanerChecker()){
            MPlaner.LineRenderer.positionCount = Checkers.PatchWay.WayTo(position, MPlaner.position).Length;
            MPlaner.LineRenderer.SetPositions(Checkers.PatchWay.WayTo(position, MPlaner.position)); 
            Debug.DrawLine(position, MPlaner.position, Color.yellow);
        }
        MPlaner.LineRenderer.enabled = MPlanerChecker();

        if (NowUsingSkill.Check()){
            APlaner.LineRenderer.positionCount = NowUsingSkill.Line().Length;
            APlaner.LineRenderer.SetPositions(NowUsingSkill.Line()); 
        }
        APlaner.LineRenderer.enabled = NowUsingSkill.Check();
    }
}


