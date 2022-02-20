using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Controller : MonoBehaviour
{
    public string Action = "";
    
    private RigidbodyConstraints Constraints; private Rigidbody rb; private Collider Collider;
    
    
    
    private bool OnClick = false;
    public bool CanControll = true;

    public Vector3 ToPos;

    void Awake()
    {   
        /*Parameter Set*/{        
            Collider = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
            Constraints = rb.constraints;

            ToPos = transform.position;
        }
        /*Move Controller*/{

        }        
        /**/
    
    
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
                break;

                case "active": 
                    SwitchPhysics(false);
                    transform.position = Vector3.MoveTowards(Pos, YUpPos() + (Sinus * 0.1f), 0.002f);
                break;

                case "move": 
                   SwitchPhysics(false);
                   transform.position = Vector3.MoveTowards(Pos, YUpPos() + new Vector3(0, 1, 0), 0.03f);
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


    void SwitchPhysics(bool ON){
        rb.constraints = (ON)? Constraints : RigidbodyConstraints.FreezeAll;
        Collider.enabled = ON;
    }
    Vector3 VectorToInt(Vector3 Pos)
    {
        return new Vector3((int)Pos.x, transform.position.y, (int)Pos.z);
    }
    Vector3 YUpPos()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity);
        return new Vector3(transform.position.x, hit.point.y, transform.position.z);
    }


    bool Changer = false, push = false;
    string MouseTest(){
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit) & hit.transform.gameObject == gameObject)
            {
                push = true;
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
