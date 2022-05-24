using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using SagardCL.Usabless;

public class UnitController : MonoBehaviour
{
    public int SkillIndex;

    [SerializeField]private GameObject Platform;
    [SerializeField]private AllInOne MPlaner;
    [SerializeField]private AllInOne APlaner;

    public ParameterList Parameters;
    
    public ParameterList baseParameter;
    

    void Start()
    {
        parameterEdited();
    }

    void Update()
    {
        parameterEdited();
        switch(MouseTest())
        {
            default:
            {
                //Base model
                transform.position = Vector3.MoveTowards(transform.position, new Checkers(transform.position), 
                0.004f + Vector3.Distance(transform.position, new Checkers(transform.position)) / 8 * Time.deltaTime);
                
                //Move planner
                //MPlaner.Planer.transform.position
            
                //Attack planner
                //APlaner.Planer.transform.position
            
            }
            break;
            case Controll.move:
            {

            }
            break;
            case Controll.active:
            {
                
            }
            break;
        }
    }









    bool WalkPlanerChecker(GameObject Planer, bool Other = true)
    {
        Vector3 StartPos = new Vector3(0, 100, 0) + Planer.transform.position;
        
        bool OnOtherPlaner = true;

        foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(0, 100, 0) + MPlaner.pos(), -Vector3.up, 105, LayerMask.GetMask("Object"))) 
        { 
            if(hit.collider.gameObject != Planer) {OnOtherPlaner = false; break; }
        }

        bool OnSelf = 
        (int)Mathf.Round(transform.position.x) == (int)Mathf.Round(Planer.transform.position.x) 
        && 
        (int)Mathf.Round(transform.position.z) == (int)Mathf.Round(Planer.transform.position.z);
        
        return Other && OnOtherPlaner && !OnSelf;
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

    void parameterEdited()
    {
        Parameters = baseParameter;

        foreach(Skill skill in Parameters.AvailableSkills)
        {
            if (skill.NoWalking) skill.From = gameObject;
            else skill.From = MPlaner.Planer;
        }
    }


    bool WalkPlanerChecker(GameObject Planer, bool Other = true)
    {
        Vector3 StartPos = new Vector3(0, 100, 0) + Planer.transform.position;
        
        bool OnOtherPlaner = true;

        foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(0, 100, 0) + MPlaner.pos, -Vector3.up, 105, LayerMask.GetMask("Object"))) 
        { 
            if(hit.collider.gameObject != Planer) {OnOtherPlaner = false; break; }
        }

        bool OnSelf = 
        (int)Mathf.Round(transform.position.x) == (int)Mathf.Round(Planer.transform.position.x) 
        && 
        (int)Mathf.Round(transform.position.z) == (int)Mathf.Round(Planer.transform.position.z);
        
        return Other && OnOtherPlaner && !OnSelf;
    }
}



[System.Serializable]
public class AllInOne
{
    public GameObject Planer;


    public AllInOne(GameObject planer) { Planer = planer; }

    public Vector3 pos
    { 
        get{ return Planer.transform.position; }
    }


    public GameObject Model { get{ return Planer.transform.Find("Model").gameObject; } }
    public Material Material { get{ return Model.GetComponent<Material>(); } }
    public Collider Collider { get{  return Model.GetComponent<MeshCollider>(); } }
    public Renderer Renderer { get{  return Model.GetComponent<Renderer>(); } }

    public LineRenderer LineRenderer { get{ return Planer.GetComponent<LineRenderer>(); } }
}
