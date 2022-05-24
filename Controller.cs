using System;
using UnityEngine;
using SagardCL.Usabless;
using SagardCL;

public class Controller : MonoBehaviour
{
    private Collider Collider;

    public int SkillIndex;

    [SerializeField]private GameObject Platform;
    [SerializeField]private AllInOne MPlaner;
    [SerializeField]private AllInOne APlaner;

    public ParameterList baseParameter;
    private ParameterList Parameters;

    

    void Awake()
    {   
        name += " (" + baseParameter.ClassTAG + ") ";
    
        /*Parameter Set*/
        {
            Collider = GetComponent<Collider>();
            transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);

            Parameters = baseParameter;            
        }   

        parameterEdited();
    }




    void parameterEdited()
    {
        foreach(Skill skill in Parameters.AvailableSkills)
        {
            if (skill.NoWalking) skill.From = gameObject;
            else skill.From = MPlaner.Planer;
        }
    }




    void Update()
    {

        
        if(GlobalStepController.Planning && Parameters.CanControll && !Parameters.IsDead)
        {
            Parameters = baseParameter;            

            bool WalkPlaneDistanceChecker = Vector3.Distance(new Checkers(MPlaner.pos()), new Checkers(transform.position)) <= Parameters.WalkDistance + 0.5f;
            
            //Graphics
            {

                
            }

            Debug.DrawLine(transform.position, MPlaner.transform.position, Color.blue);
            // Planning
            switch(MouseTest())
            {   
                default: 
                {
                    StayOnFloor();
                    
                    /*Move planer*/ 
                    {
                        bool WalkPlanerTest = WalkPlanerChecker(WalkPlaneDistanceChecker, MPlaner);
                        PlanerStay(WalkPlanerTest, MPlaner, transform);

                        MPlaner.Planer.transform.position = new Checkers(MPlaner.transform.position, transform.position.y - new Checkers(transform.position).up);
                    }
                    /*Attack planer*/
                    { 
                        bool SkillCheck = Parameters.AvailableSkills[SkillIndex].Check(APlaner.pos());
                        PlanerStay(SkillCheck, APlaner, MPlaner.pos());
                    }
                }
                break;

                case "active": 
                {
                    Checkers Some = new Checkers(transform.position, Sinus());
                    transform.position = Vector3.MoveTowards(transform.position, Some, 0.02f);

                    /*Move planer*/ 
                    {
                        PlanerStay(WalkPlanerChecker(WalkPlaneDistanceChecker & !Parameters.AvailableSkills[SkillIndex].NoWalking, MPlaner), MPlaner, transform);        
                        MPlaner.Planer.transform.position = new Checkers(MPlaner.pos(), transform.position.y - new Checkers(transform.position).up);
                    }
                    /*APlaner*/
                    {
                        Vector3 to = new Checkers(APlaner.pos(), 0.3f);

                        SetObjectToCursor(APlaner.Planer);
                        PlanerStay(true, APlaner, MPlaner.pos());
                        
                        bool SkillCheck = Parameters.AvailableSkills[SkillIndex].Check(to);
                        APlaner.Renderer().material.color = PlanerPainter(SkillCheck);

                        if(SkillCheck) Parameters.AvailableSkills[SkillIndex].Complete(to);
                    }
                }
                break;

                case "move": 
                {
                    // MovePlaner
                    {
                        PlanerStay(true, MPlaner, transform.position);

                        MPlaner.Collider().enabled = false;
                        MPlaner.Material().color = PlanerPainter(WalkPlanerChecker(WalkPlaneDistanceChecker, MPlaner));
                        SetObjectToCursor(MPlaner.Planer);
                    }
                    // APlaner
                    {
                        PlanerStay(false, APlaner, MPlaner.pos());
                    }
                    transform.position = Vector3.MoveTowards(transform.position, new Checkers(transform.position, 0.6f), 0.03f);
                }
                break;
            }
        }
        else
        {
            // // Acting
            // switch(StepTest())
            // {
            //     default: 
            //         StayOnFloor();
            //     break;
            //     case "active": 
            //         StayOnFloor();
            //     break;
            //     case "move": 

            //     break;
            //     case "onEffect":
            //         StayOnFloor();
            //     break;
            //     case "death": 

            //     break;
            // }
        }
    }

    CameraRayer Cursor;
    Vector3 CursorPos() 
    { 
        if (Cursor == null) Cursor = GameObject.Find("3DCursor").GetComponent<CameraRayer>(); 
        return Cursor.Pos; 
    }

    void StayOnFloor(){
        transform.position = Vector3.MoveTowards(transform.position, new Checkers(transform.position), 0.004f + Vector3.Distance(transform.position, new Checkers(transform.position)) / 8);
    }
    void SetObjectToCursor(GameObject Planer)
    {
        Planer.transform.position = new Checkers(CursorPos(), 0.6f);
    }
    void PlanerStay(bool IsCorrect, AllInOne Planer, Vector3 Position)
    {
        if(!IsCorrect)
        {
            Planer.Planer.transform.position = Position;

            if(Planer.Renderer()) Planer.Renderer().enabled = false;
            if(Planer.Collider()) Planer.Collider().enabled = false;
        }
        else
        {
            if(Planer.Renderer()) Planer.Renderer().enabled = true;
            if(Planer.Collider()) Planer.Collider().enabled = true;
        }
    }

    float Sinus() { return (Mathf.Sin(Time.fixedTime) / 4) + 0.6f; }

    bool WalkPlanerChecker(GameObject Planer, bool Other = true)
    {
        Vector3 StartPos = new Vector3(0, 100, 0) + Planer.transform.position;
        
        bool OnOtherPlaner = true;

        foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(0, 100, 0) + MPlaner.pos(), -Vector3.up, 105, LayerMask.GetMask("Object"))) 
        { 
            if(hit.collider.gameObject != Planer) {OnOtherPlaner = false; break; }
        }

        bool OnSelf = 
        (int)MathF.Round(transform.position.x) == (int)MathF.Round(Planer.transform.position.x) 
        && 
        (int)MathF.Round(transform.position.z) == (int)MathF.Round(Planer.transform.position.z);
        
        return Other && OnOtherPlaner && !OnSelf;
    }

    Color PlanerPainter(bool Material)
    {
        if(Material) return Color.green;
        else return Color.red;
    }
    
    bool Changer = false, push = false;
    string MouseTest(){
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            try{
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity) & (hit.collider.gameObject == gameObject || hit.collider.gameObject == MPlaner.Planer || hit.collider.gameObject == APlaner.Planer)) push = true;
                else Changer = false;
            }
            catch { }
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) push = false;

        if (Input.GetKey(KeyCode.Mouse1) && push) { Changer = false; return "move";  }
        else { if (Input.GetKeyDown(KeyCode.Mouse0) && push  ) Changer = !Changer; }

        if (Changer) { return "active"; }
        else return "";
    }


}