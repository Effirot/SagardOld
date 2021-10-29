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
    private bool push = false, push2 = false, Actived = false;

    private float dist;



    private Transform Cursore;

    public int active = 0;
    public float upDistance = 0.3f, UpSpeed = 0.01f, RotationSpeed = 0.01f, MinTime = 0.5f;
    public bool SteppedEnd = false;

    //Planer object
    private GameObject Planer;







    void Start()
    {
        Cursore = GameObject.Find("3DCursore").transform;

        rb = GetComponent<Rigidbody>();
        Coll = GetComponent<MeshCollider>();
        Parameters = GetComponent<PlayerParameterList>();

        {
            // ������������
            transform.position = new Vector3(Convert.ToInt32(transform.position.x), Convert.ToInt32(transform.position.y), Convert.ToInt32(transform.position.z));


        }

        {
            // �����������
            Planer = Instantiate(gameObject, transform.position, transform.rotation);

            Destroy(Planer.GetComponent<PlayerController>());


            Destroy(Planer.GetComponent<MeshCollider>());
            Destroy(Planer.GetComponent<Rigidbody>());

            Planer.name = name + "Planer";
            Planer.tag = "Cursore";
            Planer.transform.localScale = transform.localScale * 0.9f;
        }
    }

    void Update()
    {
        if (active != 3) {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) & hit.transform.gameObject.name == name)
                {
                    push = true;
                }
            }
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) push = false;

            if (Input.GetKey(KeyCode.Mouse1) && push) { active = 2; }
            else
            {

                if (Input.GetKeyDown(KeyCode.Mouse0) && push) push2 = !push2;

                if (push2) { UpPos = new Vector3(Convert.ToInt32(transform.position.x), upDistance, Convert.ToInt32(transform.position.z)); active = 1; }
                else active = 0;
            }

            if ((Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse0)) && !push) push2 = false;
        }



        // ----------------------------------------������� ������������� �������------------------------------------//
        

        switch (active)
        {
            case 0:
                sleep(); PlanerPassive(); break;
            case 1:
                clicked(); PlanerPassive(); break;
            case 2:
                planned(); PlanerActive(); break;
            case 3:
                stepped(Actived); Debug.Log("������ ����"); break;
        }
    }


    // ������ �������� �������
    void clicked()
    {
        dist = Vector3.Distance(transform.position, UpPos);

        rb.constraints = RigidbodyConstraints.FreezeAll;
        Coll.enabled = true;

        transform.position = Vector3.MoveTowards(transform.position, UpPos + new Vector3(0, Mathf.Sin(Time.fixedTime) / 10, 0), UpSpeed + dist / 30);
        transform.eulerAngles += new Vector3(0, RotationSpeed, 0);
    }
    void planned()
    {

        rb.constraints = RigidbodyConstraints.FreezeAll;
        Coll.enabled = false;

        transform.position = Vector3.MoveTowards(transform.position, new Vector3(Convert.ToInt32(transform.position.x), 0.1f, Convert.ToInt32(transform.position.z)), 0.1f);
        transform.eulerAngles += new Vector3(0, RotationSpeed * 2, 0);

    }

    void sleep()
    {
        rb.constraints = RigidbodyConstraints.None;
        Coll.enabled = true;
    }
    void stepped(bool a)
    {
        if (a)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            if (transform.position != Planer.transform.position) {
                transform.position = Vector3.MoveTowards(transform.position, Planer.transform.position, 0.06f);
                Parameters.Walk();
                SteppedEnd = false;


            }
            else {
                SteppedEnd = true;
                
                
            }
        }
        else Parameters.recreation(); PlanerPassive();
    }




    //��������� ������

    void PlanerPassive()
    {
        if ((Convert.ToInt32(transform.position.x) == Convert.ToInt32(Planer.transform.position.x)) && (Convert.ToInt32(transform.position.y) == Convert.ToInt32(Planer.transform.position.z)) || (Vector3.Distance(transform.position, Planer.transform.position) > Parameters.WalkDistance + 0.5 && !push))
        {
            Planer.transform.localPosition = transform.position;

            Planer.GetComponent<Renderer>().enabled = false;
            
            Actived = false;
        }

        else
        {
            Planer.transform.eulerAngles += new Vector3(0, RotationSpeed, 0);
            Planer.transform.position = Vector3.MoveTowards(Planer.transform.position, new Vector3(Planer.transform.position.x, upDistance / 4, Planer.transform.position.z), 0.1f);

            Actived = true;
        }
    }
    void PlanerActive()
    {
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
}
