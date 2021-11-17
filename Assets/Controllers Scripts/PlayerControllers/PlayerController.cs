using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Vector3 UpPos;
    private Rigidbody rb;
    private MeshCollider Coll;
    private PlayerParameterList Parameters;
    private bool push = false, push2 = false;

    private float dist, LastCursorY;






    private Transform Cursore;

    public int active = 0, Stepped = 0;
    public float upDistance = 0.4f, UpSpeed = 0.01f, RotationSpeed = 0.01f, MinTime = 0.5f;
    public bool[] ActionOptions = new bool[2] {false, false};

    //Planer object
    private GameObject Planer;







    void Start()
    {
        Cursore = GameObject.Find("3DCursore").transform;

        rb = GetComponent<Rigidbody>();
        Coll = GetComponent<MeshCollider>();
        Parameters = GetComponent<PlayerParameterList>();

        {
            // Started stabilization
            transform.position = new Vector3(Convert.ToInt32(transform.position.x), Convert.ToInt32(transform.position.y), Convert.ToInt32(transform.position.z));


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

    void LateUpdate()
    {
        if (!GameObject.Find("GlobalMapController").GetComponent<GlobalStepController>().StepActive) {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) & hit.transform.gameObject.name == name)
                {
                    push = true;
                }
            }
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) push = false;

            if (Input.GetKey(KeyCode.Mouse1) && push) { active = 2; LastCursorY = Cursore.position.y; }
            else
            {
                

                if (Input.GetKeyDown(KeyCode.Mouse0) && push) push2 = !push2;

                if (push2) { UpPos = VectorInInt(transform.position, upDistance); active = 1; }
                else active = 0;
            }

            if ((Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse0)) && !push) push2 = false;
        }



        // ----------------------------------------The first Player Switcher------------------------------------//


        switch (active)
        {
            default:
                sleep(); PlanerPassive(false);
                break;
            case 1:
                clicked(); PlanerPassive(true);
                break;
            case 2:
                planned(); PlanerActive();
                break;
            case 3:
                Walking();
                break;
            case 4:
                Action();
                break;
        }
    }


    // Voids
    private void clicked()
    {
        Parameters.AbilitieComplete();

        dist = Vector3.Distance(transform.position, UpPos);

        rb.constraints = RigidbodyConstraints.FreezeAll;
        Coll.enabled = true;

        transform.position = Vector3.MoveTowards(transform.position, UpPos + new Vector3(0, Mathf.Sin(Time.fixedTime) / 10, 0), UpSpeed + dist / 30);



    }

    private void planned()
    {

        rb.constraints = RigidbodyConstraints.FreezeAll;
        Coll.enabled = false;

        transform.position = Vector3.MoveTowards(transform.position, new Vector3(Convert.ToInt32(transform.position.x), 0.1f, Convert.ToInt32(transform.position.z)), 0.1f);
        transform.eulerAngles += new Vector3(0, RotationSpeed * 2, 0);
    }

    private void sleep()
    {
        rb.constraints = RigidbodyConstraints.None;
        Coll.enabled = true;
    }

    private void Walking()
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;

        transform.position = Vector3.MoveTowards(transform.position, Planer.transform.position, 0.1f);



        //if (VectorInInt(transform.position, transform.position.y) == VectorInInt(Planer.transform.position, Planer.transform.position.y))
        //{
            
        //}
        //else
        //{
            
        //}
    }

    private void Action()
    {
        
    }





    //Planner Controlling

    void PlanerPassive(bool forcibly)
    {
        if (((Convert.ToInt32(transform.position.x) == Convert.ToInt32(Planer.transform.position.x)) && (Convert.ToInt32(transform.position.z) == Convert.ToInt32(Planer.transform.position.z))) || (Vector3.Distance(transform.position, Planer.transform.position) > Parameters.WalkDistance + 0.5 && !push) || (forcibly))
        {
            Planer.transform.localPosition = transform.position;

            Planer.GetComponent<Renderer>().enabled = false;

            ActionOptions[0] = false;
        }

        else
        {
            Planer.transform.eulerAngles += new Vector3(0, RotationSpeed, 0);
            Planer.transform.position = Vector3.MoveTowards(Planer.transform.position, new Vector3(Planer.transform.position.x, upDistance / 4 + LastCursorY, Planer.transform.position.z), 0.1f);

            ActionOptions[0] = true;
        }
    }
    void PlanerActive()
    {
        push2 = false;

        if (Parameters.Stamina > 0)
        {
            Planer.GetComponent<Renderer>().enabled = true;

            Planer.transform.localPosition = Vector3.MoveTowards(Planer.transform.localPosition, Cursore.position, 1f);
            Planer.transform.eulerAngles += new Vector3(0, RotationSpeed * 2, 0);

            if (Vector3.Distance(transform.position, Planer.transform.position) > Parameters.WalkDistance + 0.5f) Planer.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
            else Planer.GetComponent<Renderer>().material.color = new Color(255, 255, 255);
        }
        //else
    }


    // Math
    Vector3 VectorInInt(Vector3 Vector, float Y)
    {
        return new Vector3(Convert.ToInt32(Vector.x), Convert.ToInt32(Y), Convert.ToInt32(Vector.z));
    }



}
