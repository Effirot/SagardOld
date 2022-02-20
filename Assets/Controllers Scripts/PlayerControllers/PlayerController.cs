using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{    
    private Rigidbody rb; RigidbodyConstraints Constraints; private MeshCollider Coll;
    


    private PlayerParameterList Parameters; private Skills Skills;


    private bool push = false, Changer = false;

    private float LastCursorY, upDistance = 1f, UpSpeed = 0.01f, RotationSpeed = 0.01f;


    private Transform Cursore;

    public int active = 0;
    
    [NonSerialized]
    public float YUpPos;


    public bool[] ActionOptions = new bool[3] {false, false, false};

    //Planer object
    private GameObject Planer;
    private GameObject GlobalController;



    void Start()
    {
        Skills = GetComponent<Skills>();
        Parameters = GetComponent<PlayerParameterList>();

        Cursore = GameObject.Find("3DCursore").transform;

        rb = GetComponent<Rigidbody>();
        Coll = GetComponent<MeshCollider>();
        Constraints = rb.constraints;




        {
            // Started stabilization
            transform.position = VectorInInt(transform.position, 7);
        }

        {
            // Planner
            Renderer FigureRend = GetComponent<Renderer>();

            Planer = new GameObject("Planner(" + name + ")", typeof(MeshFilter), typeof(MeshRenderer), typeof(LineRenderer));
            Planer.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
            Planer.GetComponent<Renderer>().material = GetComponent<Renderer>().material;
            
            Planer.tag = "Planer";
            Planer.transform.localScale = transform.localScale * 0.9f;     

            Planer.transform.position = transform.position; 
        }
    }

    void Update()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit upDistHit))
        {
            YUpPos = upDistHit.point.y + upDistance;
        }

        if (!GlobalStepController.Planning) {
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
                

                if (Input.GetKeyDown(KeyCode.Mouse0) && push) Changer = !Changer;

                if (Changer) { active = 1; }
                else active = 0;
            }

            if ((Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse0)) && !push) Changer = false;
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

        transform.position = Vector3.MoveTowards(transform.position, VectorInInt(transform.position, YUpPos + Mathf.Sin(Time.fixedTime) * 0.0000001f), UpSpeed / 30);
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
        bool InMaxDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(Planer.transform.position.x, 0, Planer.transform.position.z)) > Distance + 0.5;   
        
        bool OnAnotherPlaner = true;
        foreach (GameObject Planers in GameObject.FindGameObjectsWithTag("Planer"))
        {
            if (VectorInInt(Planers.transform.position) == VectorInInt(Planer.transform.position)) OnAnotherPlaner = false;
        }

        bool Conditions = Forcibility || OnThisObject || InMaxDistance || OnAnotherPlaner;

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

                float distance = Vector3.Distance(Planer.transform.localPosition, Cursore.position);

                Planer.transform.localPosition = VectorInInt(Cursore.position, Cursore.position.y);
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

                Planer.transform.localPosition = VectorInInt(Cursore.position, Cursore.position.y);
                Planer.transform.eulerAngles += new Vector3(0, RotationSpeed * 2, 0);

                ActionOptions[0] = true;

                Planer.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
            }
        }
    }

    // Math
    Vector3 VectorInInt(Vector3 Vector, float Y = 0)
    {
        return new Vector3(Convert.ToInt32(Vector.x), Y, Convert.ToInt32(Vector.z));
    }



}
