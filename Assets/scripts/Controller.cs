using System;
using UnityEngine;
using SagardCL.Usabless;
using SagardCL;

public class Controller : MonoBehaviour
{
    private Collider Collider;

    public int SkillIndex;

    private GameObject MovePlaner;
    private GameObject AttackPlaner;
    [Space]
    [Space]
    [Space]
    [SerializeField] Material MovePlanerMaterial;
    [SerializeField] Material MovePlanerLineMaterial;
    LineRenderer MoveLnRenderer;
    [Space]
    [SerializeField] Mesh AttackPlanerMesh;
    [SerializeField] Material AttackPlanerMaterial;
    [SerializeField] Material AttackPlanerLineMaterial;
    LineRenderer AttackLnRenderer;
    
    [Space(1)]
    [SerializeField]Plan planed;
    [Space(2)]

    public ParameterList baseParameter;
    private ParameterList Parameters;

    

    void Awake()
    {   
        name += " (" + baseParameter.ClassTAG + ") ";

        /*Move Controller*/
        {
            MovePlaner = new GameObject(name + " : MovePlaner", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider), typeof(LineRenderer));
            
            MovePlaner.transform.position = transform.position;
            MovePlaner.transform.rotation = transform.rotation;

            MovePlaner.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
            
            MovePlaner.GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshCollider>().sharedMesh;
            MovePlaner.GetComponent<MeshCollider>().convex = true;
            MovePlaner.GetComponent<MeshCollider>().isTrigger = true;

            MovePlaner.GetComponent<Renderer>().material = MovePlanerMaterial;

            MovePlaner.GetComponent<LineRenderer>().positionCount = 1;

            MovePlaner.layer = LayerMask.NameToLayer("Object"); 
        }
        /*Attack Controller*/
        {
            AttackPlaner = new GameObject(name + " : AttackPlaner", typeof(MeshFilter), typeof(MeshRenderer), typeof(LineRenderer));
            AttackPlaner.transform.position = transform.position;
            AttackPlaner.transform.rotation = transform.rotation;

            AttackPlaner.layer = LayerMask.NameToLayer("Cursor"); 
            
            AttackPlaner.GetComponent<MeshFilter>().sharedMesh = AttackPlanerMesh;
            AttackPlaner.GetComponent<MeshRenderer>().material = AttackPlanerMaterial;

        }
        /*Parameter Set*/
        {
            Collider = GetComponent<Collider>();
            transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);

            Parameters = baseParameter;            
        }   
        /*Line Renderer Settings*/
        {
            AttackLnRenderer = AttackPlaner.GetComponent<LineRenderer>();
            AttackLnRenderer.positionCount = 0;
            AttackLnRenderer.materials = new Material[] { AttackPlanerLineMaterial };
            AttackLnRenderer.startWidth = 0;
            AttackLnRenderer.endWidth = 0.1f;

            MoveLnRenderer = MovePlaner.GetComponent<LineRenderer>();
            MoveLnRenderer.positionCount = 0;
            MoveLnRenderer.materials = new Material[] { MovePlanerLineMaterial };
        }
    
    
    }

    void Update()
    {
        if(GlobalStepController.Planning && Parameters.CanControll && !Parameters.IsDead)
        {
            Parameters = baseParameter;            

            bool WalkPlaneDistanceChecker = Vector3.Distance(new Vector3(MovePlaner.transform.position.x, 0, MovePlaner.transform.position.z), new Vector3(transform.position.x, 0, transform.position.z)) <= Parameters.WalkDistance + 0.5f;
            
            //Graphics
            {
                if(Parameters.AvailableSkills[SkillIndex].Check(MovePlaner.transform.position, AttackPlaner.transform.position))
                { Parameters.AvailableSkills[SkillIndex].DrawLine(AttackLnRenderer, new Checkers(MovePlaner.transform.position, 0.3f), AttackPlaner.transform.position); }
                else
                { Parameters.AvailableSkills[SkillIndex].ResetLine(AttackLnRenderer); }

                
            }

            Debug.DrawLine(transform.position, MovePlaner.transform.position, Color.blue);
            // Planning
            switch(MouseTest())
            {   
                default: 
                {
                    StayOnFloor();
                    
                    /*Move planer*/ 
                    {
                        bool WalkPlanerTest = WalkPlanerChecker(WalkPlaneDistanceChecker, MovePlaner);
                        PlanerStay(WalkPlanerTest, MovePlaner, transform);

                        MovePlaner.transform.position = new Checkers(MovePlaner.transform.position, transform.position.y - new Checkers(transform.position).up);
                    }
                    /*Attack planer*/
                    { 
                        bool SkillCheck = Parameters.AvailableSkills[SkillIndex].Check(MovePlaner.transform.position, AttackPlaner.transform.position);
                        PlanerStay(SkillCheck, AttackPlaner, MovePlaner.transform);
                    }
                }
                break;

                case "active": 
                {
                    Checkers Some = new Checkers(transform.position, Sinus());
                    transform.position = Vector3.MoveTowards(transform.position, Some, 0.02f);
                    transform.eulerAngles += new Vector3(0, 0.2f, 0);

                    /*Move planer*/ 
                    {
                        PlanerStay(WalkPlanerChecker(WalkPlaneDistanceChecker & !Parameters.AvailableSkills[SkillIndex].NoWalking, MovePlaner), MovePlaner, transform);        
                        MovePlaner.transform.position = new Checkers(MovePlaner.transform.position, transform.position.y - new Checkers(transform.position).up);
                    }
                    /*AttackPlaner*/
                    {
                        Vector3 from = new Checkers(MovePlaner.transform.position, 0.3f);
                        Vector3 to = new Checkers(AttackPlaner.transform.position, 0.3f);

                        SetObjectToCursor(AttackPlaner);
                        PlanerStay(true, AttackPlaner, MovePlaner.transform);
                        
                        bool SkillCheck = Parameters.AvailableSkills[SkillIndex].Check(from, to);
                        AttackPlaner.GetComponent<Renderer>().material.color = PlanerPainter(SkillCheck);

                        if(SkillCheck) Parameters.AvailableSkills[SkillIndex].Complete(from, to);
                    }
                }
                break;

                case "move": 
                {
                    // MovePlaner
                    {
                        PlanerStay(true, MovePlaner, transform);

                        MovePlaner.GetComponent<MeshCollider>().enabled = false;
                        MovePlaner.GetComponent<Renderer>().material.color = PlanerPainter(WalkPlanerChecker(WalkPlaneDistanceChecker, MovePlaner));
                        SetObjectToCursor(MovePlaner);
                    }
                    // AttackPlaner
                    {
                        PlanerStay(false, AttackPlaner, MovePlaner.transform);
                    }
                    transform.position = Vector3.MoveTowards(transform.position, new Checkers(transform.position, 0.6f), 0.03f);
                }
                break;
            }
        }
        else
        {
            // Acting
            switch(StepTest())
            {
                default: 
                    StayOnFloor();
                break;
                case "active": 
                    StayOnFloor();
                break;
                case "move": 

                break;
                case "onEffect":
                    StayOnFloor();
                break;
                case "death": 

                break;
            }
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
        Planer.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f); 
    }
    void PlanerStay(bool IsCorrect, GameObject Planer, Transform ToTransform)
    {
        if(!IsCorrect)
        {
            Planer.transform.position = ToTransform.position;
            Planer.transform.rotation = ToTransform.rotation;
            Planer.transform.localScale = new Vector3(1f, 1f, 1f); 

            if(Planer.GetComponent<Renderer>()) Planer.GetComponent<Renderer>().enabled = false;
            if(Planer.GetComponent<MeshCollider>()) Planer.GetComponent<MeshCollider>().enabled = false;
        }
        else
        {
            Planer.transform.localScale = new Vector3(1f, 1f, 1f); 
            
            if(Planer.GetComponent<Renderer>()) Planer.GetComponent<Renderer>().enabled = true;
            if(Planer.GetComponent<MeshCollider>()) Planer.GetComponent<MeshCollider>().enabled = true;

            Planer.transform.rotation = transform.rotation;
        }
    }

    float Sinus() { return (Mathf.Sin(Time.fixedTime) / 4) + 0.6f; }

    bool WalkPlanerChecker(bool Other, GameObject Planer)
    {
        Vector3 StartPos = new Vector3(0, 100, 0) + Planer.transform.position;
        
        bool OnOtherPlaner = true;
        RaycastHit[] hits = Physics.RaycastAll(new Vector3(0, 100, 0) + MovePlaner.transform.position, -Vector3.up, 105, LayerMask.GetMask("Object"));
        foreach (RaycastHit hit in hits) { if(hit.collider.gameObject != Planer) OnOtherPlaner = false; }

        bool OnSelf = Convert.ToInt32(transform.position.x) == Convert.ToInt32(Planer.transform.position.x) && Convert.ToInt32(transform.position.z) == Convert.ToInt32(Planer.transform.position.z);
        
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
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity) & (hit.transform.gameObject == gameObject || hit.transform.gameObject == MovePlaner || hit.transform.gameObject == AttackPlaner)) push = true;
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

    string StepTest() 
    {
        string result = "";
        // switch ()
        // {
        //     case "active":
        //         if(planed.AttackPlane) return "attack";
        //     break;
        //     case "move":
        //         if(planed.WalkPlane) return "move";
        //     break;
        // }


        return result;
    }
}

[System.Serializable]
class Plan
{
    public bool AttackPlane = false;
    public bool WalkPlane = false;
    public bool StayPlane = true;

    public Plan(bool attack, bool walk){ AttackPlane = attack; WalkPlane = walk; }
    public Plan(){ AttackPlane = false; WalkPlane = false;  }
    
    void Update() { StayPlane = !AttackPlane & !WalkPlane; }
}