using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{    
    private Rigidbody rb;
    RigidbodyConstraints Constraints;

    private MeshCollider Coll;
    private PlayerParameterList Parameters;
    private Skills Skills;
    private bool push = false, push2 = false;

    private float LastCursorY, upDistance = 1f, UpSpeed = 0.01f, RotationSpeed = 0.01f;


    private Transform Cursore;

    public int active = 0, Stepped = 0;
    
    [NonSerialized]
    public float YUpPos;


    public bool[] ActionOptions = new bool[3] {false, false, false};

    //Planer object
    private GameObject Planer;



    void Start()
    {

        Skills = GetComponent<Skills>();

        Cursore = GameObject.Find("3DCursore").transform;

        rb = GetComponent<Rigidbody>();
        Constraints = rb.constraints;

        Coll = GetComponent<MeshCollider>();
        Parameters = GetComponent<PlayerParameterList>();

        {
            // Started stabilization
            transform.position = VectorInInt(transform.position, 7);
        }

        {
            // Planner
            Planer = Instantiate(gameObject, transform.position, transform.rotation);

            Destroy(Planer.GetComponent<PlayerController>());


            Destroy(Planer.GetComponent<MeshCollider>());
            Destroy(Planer.GetComponent<Rigidbody>());

            Planer.name = name + " Planer";
            Planer.tag = "Cursore";
            Planer.transform.localScale = transform.localScale * 0.9f;

        }
    }

    void Update()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit upDistHit))
        {
            YUpPos = upDistHit.point.y + upDistance;
        }

        if (!GameObject.Find("GlobalMapController").GetComponent<GlobalStepController>().StepActive) {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit) & hit.transform.gameObject == gameObject)
                {
                    push = true;
                }
            }
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) push = false;

            if (Input.GetKey(KeyCode.Mouse1) && push) { active = 2; LastCursorY = Cursore.position.y; }
            else
            {
                

                if (Input.GetKeyDown(KeyCode.Mouse0) && push) push2 = !push2;

                if (push2) { active = 1; }
                else active = 0;
            }

            if ((Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse0)) && !push) push2 = false;
        }










        // ----------------------------------------The first Player Switcher------------------------------------//      
        
            
            switch (active)
            {
                default:
                    sleep(); PlannerPosition(false);
                break;
                case 1:
                    clicked(); PlannerPosition(false, true);
                break;
                case 2:
                    WalkPlane(); PlannerPosition(true);
                break;                
                case 3:
                    Walking();
                break;
                case 4:
                    Action();
                break;
                case 5:
                    Death();
                break;
            }

    }


    // Voids
    private void clicked()
    {
        Skills.SkillUsing(true, 0);

        rb.constraints = RigidbodyConstraints.FreezeAll;
        Coll.enabled = true;

        transform.position = Vector3.MoveTowards(transform.position, VectorInInt(transform.position, YUpPos + Mathf.Sin(Time.fixedTime) / 10), UpSpeed / 30);
    }

    private void WalkPlane()
    {
        Skills.SkillUsing(false, 0, true);
        Skills.ToPoint = transform.position;


        ActionOptions = new bool[] { true, false };
        

        rb.constraints = RigidbodyConstraints.FreezeAll;
        Coll.enabled = false;

        transform.position = Vector3.MoveTowards(transform.position, new Vector3(Convert.ToInt32(transform.position.x), YUpPos, Convert.ToInt32(transform.position.z)), 0.1f);
        transform.eulerAngles += new Vector3(0, RotationSpeed * 2, 0);
    }

    private void sleep()
    {
        Skills.SkillUsing(false, 0);



        rb.constraints = Constraints;
        Coll.enabled = true;
    }

    private void Walking()
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;

        transform.position = Vector3.MoveTowards(transform.position, Planer.transform.position, 0.1f);

        Skills.ToPoint = transform.position;
    }

    private void Action()
    {
        
    }

    private void Death()
    {

    }





    //Planner Controlling
    void PlannerPosition(bool IsActive, bool Forcibility = false)
    {
        int Distance = Parameters.Stamina > 0 ? Parameters.WalkDistance : 1;

        bool OnThisObject = VectorInInt(transform.position, 0) == VectorInInt(Planer.transform.position, 0);
        bool InMaxDistance = Vector3.Distance(transform.position, Planer.transform.position) > Distance + 0.5;   
        
        bool Conditions = Forcibility || OnThisObject || InMaxDistance;

        if (Conditions)
        {
            if (!IsActive)
            { 
                Planer.transform.position = transform.position;

                Planer.GetComponent<Renderer>().enabled = false;

                ActionOptions[0] = false;
            }
            else
            {
                Planer.GetComponent<Renderer>().enabled = true;

                Planer.transform.localPosition = Vector3.MoveTowards(Planer.transform.localPosition, Cursore.position, 0.1f);
                Planer.transform.eulerAngles += new Vector3(0, RotationSpeed * 2, 0);

                ActionOptions[0] = false;

                Planer.GetComponent<Renderer>().material.color = Color.red;
            }
        }

        else
        {
            if (!IsActive)
            { 
                Planer.transform.eulerAngles += new Vector3(0, RotationSpeed, 0);
                Planer.transform.position = Vector3.MoveTowards(Planer.transform.position, new Vector3(Convert.ToInt32(Planer.transform.position.x), upDistance / 4 + LastCursorY, Convert.ToInt32(Planer.transform.position.z)), 0.06f);

                ActionOptions[0] = true;

            }
            else
            {
                Planer.GetComponent<Renderer>().enabled = true;

                Planer.transform.localPosition = Vector3.MoveTowards(Planer.transform.localPosition, Cursore.position, 0.1f);
                Planer.transform.eulerAngles += new Vector3(0, RotationSpeed * 2, 0);

                ActionOptions[0] = true;

                Planer.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
            }
        }
    }





    // Math
    Vector3 VectorInInt(Vector3 Vector, float Y)
    {
        return new Vector3(Convert.ToInt32(Vector.x), Y, Convert.ToInt32(Vector.z));
    }



}
