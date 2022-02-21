using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Controller : MonoBehaviour
{
    private RigidbodyConstraints Constraints; private Rigidbody rb; private Collider Collider; private LineRenderer lnRend;




    public string Action = "";
    public string PlannedAction = "";

    
    public GameObject MovePlaner;
    public GameObject AttackPlaner;
    
    
    public bool CanControll = true;

    public Vector3 ToPos;
    public Vector3 LookTo;

    void Awake()
    {   
        /*Parameter Set*/
        {        
            Collider = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
            Constraints = rb.constraints;
            lnRend = GetComponent<LineRenderer>();

            ToPos = transform.position;
        }
        /*Move Controller*/
        {
            MovePlaner = new GameObject(name + " : MovePlaner", typeof(MeshFilter), typeof(MeshRenderer));
            MovePlaner.transform.position = transform.position;
            MovePlaner.transform.rotation = transform.rotation;

            MovePlaner.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;

            MovePlaner.transform.parent = transform;
        }
        /*Attack Controller*/
        {
            AttackPlaner = new GameObject(name + " : AttackPlaner", typeof(MeshFilter), typeof(MeshRenderer));
            AttackPlaner.transform.position = transform.position;
            AttackPlaner.transform.rotation = transform.rotation;

            AttackPlaner.transform.parent = transform;
        }
    
    }

    void Update()
    {
        if(GlobalStepController.Planning && CanControll)
        {
            Vector3 Pos = transform.position;
            Vector3 Sinus = new Vector3(0, 1 + Mathf.Sin(Time.fixedTime), 0);

            ToPos = VectorToInt(ToPos);

            Action = MouseTest();

            switch(Action)
            {
                // Planning
                default: 
                    SwitchPhysics(true);
                    // MovePlaner.transform.position = transform.position;
                    // MovePlaner.transform.rotation = transform.rotation;

                    // AttackPlaner.transform.position = transform.position;
                    // AttackPlaner.transform.rotation = transform.rotation;
                break;

                case "active": 
                    SwitchPhysics(false);          
                    
                    transform.position = Vector3.MoveTowards(Pos, YUpPos() + (Sinus * 0.1f) + new Vector3(0, 1, 0), 0.002f);
                    
                break;

                case "move": 
                    SwitchPhysics(false);
                    SetObjectToCursor(MovePlaner);

                    transform.position = Vector3.MoveTowards(Pos, YUpPos() + new Vector3(0, 1, 0), 0.03f);

                    LookingTo(CursorPos());
                break;
            }
        }
        else
        {
            switch(Action)
            {
                // Acting
                default: 
                    SwitchPhysics(false);
                break;
                case "active": 
                    SwitchPhysics(false);
                break;
                case "move": 
                    SwitchPhysics(false);
                break;
                case "death": 
                    SwitchPhysics(false);
                break;
            }
        }
    }
    
    Vector3 YUpPos(){
        RaycastHit hit;
        Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity);
        return new Vector3(transform.position.x, hit.point.y, transform.position.z);
    }
    Vector3 VectorToInt(Vector3 Pos)
    {
        return new Vector3((int)Pos.x, transform.position.y, (int)Pos.z);
    }
    Vector3 CursorPos() 
    { 
        CameraRayer Cursor = GameObject.Find("3DCursor").GetComponent<CameraRayer>(); 
        return Cursor.SelectedCell.transform.position; 
    }

    void SwitchPhysics(bool ON){
        rb.constraints = (ON)? Constraints : RigidbodyConstraints.FreezeAll;
        Collider.enabled = ON;
    }
    void LookingTo(Vector3 position)
    { 
        transform.rotation = Quaternion.LookRotation(new Vector3(position.x, transform.position.y, position.z));
    }
    void SetObjectToCursor(GameObject Planer)
    {
        Planer.transform.position = CursorPos();
    }

    
    
    
    
    
    
    
    
    bool Changer = false, push = false;
    string MouseTest(){
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit) & hit.transform.gameObject == gameObject)
            {
                push = true;
            }
            else
            {
                Changer = false;
            }
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) push = false;

        if (Input.GetKey(KeyCode.Mouse1) && push) { return "move";  }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && push) Changer = !Changer;
        }

        if (Changer) { return "active"; }
        else return "";
    }    
}
