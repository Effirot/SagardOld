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

            }
            break;
            case "move":
            {

            }
            break;
            case "active":
            {
                
            }
            break;
        }
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
}



[System.Serializable]
public class AllInOne
{
    public GameObject Planer;

    public AllInOne(GameObject planer) { Planer = planer; }

    public Vector3 pos(){ return Planer.transform.position;}

    public GameObject Model() { return Planer.transform.Find("Model").gameObject; }
    public Material Material() { return Model().GetComponent<Material>(); }
    public Collider Collider() { return Model().GetComponent<MeshCollider>(); }
    public Renderer Renderer() { return Model().GetComponent<Renderer>(); }

    public LineRenderer LineRenderer() { return Planer.GetComponent<LineRenderer>(); }
}
