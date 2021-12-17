using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Skills : MonoBehaviour
{

    private GameObject Cursore;
    private PlayerController Controller;
    private LineRenderer LnRend;
    private PlayerParameterList Parameters;





    [SerializeField]
    private Material[] Materials;

    public Vector3 ToPoint;

    void Start()
    {
        Cursore = GameObject.Find("3DCursore");
        LnRend = GetComponent<LineRenderer>();

        ToPoint = transform.position;

        Controller = GetComponent<PlayerController>();
        Parameters = GetComponent<PlayerParameterList>();
    }
    
    
    public void SkillUsing(bool Changing, int AbilitieID = 0, bool Reset = false)
    {
        string AbilitieName = Parameters.AvailableAbilities[AbilitieID]; 


        if(Changing)
        {
            ToPoint = VectorInInt(Cursore.transform.position, Cursore.transform.position.y);
            transform.eulerAngles =  new Vector3(0, Quaternion.LookRotation(transform.position - Cursore.transform.position).eulerAngles.y, 0);
        }

        switch (AbilitieName)
        {
            case "Close attack":

            break;

            case "Range attack":
            int Range = 5;
            int Damage = 3;
            string Debuff = "";
            string DamageType = "";

            if (!((Reset || Controller.ActionOptions[1] || Changing))) {LnRend.enabled = false;}
            else {LnRend.enabled = true; }


            ParabolePaint(transform.position, ToPoint, OKRange(Range), 1f, 3);
            ACtionDeselecting(OKRange(Range));
            ToPointWithoutWay(VectorInInt(ToPoint, 10), OKRange(Range), Damage, AbilitieName, Debuff, DamageType);             
            break;
        }
    }



    public GameObject obj = null;
    void ToPointWithoutWay(Vector3 EndPoint, bool WhereOk, int Damage, string SkillName, string Debuff, string DamageType)
    {
        //----------------------------------------------------------------For Ballistic Attacks----------------------------------------------------------------         
        
        if(Physics.Raycast(EndPoint, Vector3.down * 1000, out RaycastHit hit, LayerMask.GetMask("Founded")) && hit.collider.gameObject.tag == "Map" && WhereOk)
        {
            
        }
    }
















    private Vector3 VectorInInt(Vector3 Vector, float Y = 0)
    {
        return new Vector3(Convert.ToInt32(Vector.x), Y, Convert.ToInt32(Vector.z));
    }
    private RaycastHit RayTest(Vector3 PosFrom, Vector3 PosTo)
    {
        var direction = PosTo - PosFrom;
        var distance = Vector3.Distance(PosFrom, PosTo);

        Physics.Raycast(PosFrom, direction, out RaycastHit hit, distance);
        return hit;
    }    
    void ParabolePaint(Vector3 StartPoint, Vector3 EndPoint, bool WhereOK, float HowUp = 1, float DitalizationLevel = 2f)
    {
        var heading = EndPoint - StartPoint;
        var distance = heading.magnitude;
        var direction = heading / distance;

        LnRend.positionCount = 2 + Convert.ToInt32(distance * DitalizationLevel);
        LnRend.SetPosition(0, StartPoint);
        LnRend.SetPosition(LnRend.positionCount - 1, EndPoint);        

        float Distance = Vector3.Distance(VectorInInt(transform.position, Controller.YUpPos + 0.1f), ToPoint);  

        LnRend.material = WhereOK ? Materials[0] : Materials[1]; 

        Controller.ActionOptions = WhereOK ? new bool[] { false, true } : new bool[] { false, false };


        Vector3 Mover = StartPoint;
        for(int i = 1; i < LnRend.positionCount - 1; i++)
        {
            float y = (i - ((LnRend.positionCount - 2) / 2));
            float Formule = -(y*y) / (100 / HowUp);                        

            LnRend.SetPosition(i, new Vector3(Mover.x, (Formule) , Mover.z));

            Mover = Vector3.MoveTowards(Mover, EndPoint, distance / (LnRend.positionCount - 2));
        }
        float dist = Vector3.Distance(LnRend.GetPosition(LnRend.positionCount - 1), LnRend.GetPosition(LnRend.positionCount - 2));
        
        for(int i = 1; i < LnRend.positionCount - 1; i++)
        {
            LnRend.SetPosition(i, LnRend.GetPosition(i) + new Vector3(0, Mover.y + dist, 0));
        }
    }
    bool OKRange(int AttackRange)
    {
        bool InRange = AttackRange >= Vector3.Distance(transform.position, LnRend.GetPosition(LnRend.positionCount - 1)) - 0.5f;
        bool HymSelf = VectorInInt(transform.position) != VectorInInt(ToPoint);
        return (InRange & HymSelf);
    }
    void ACtionDeselecting(bool Range)
    {
        if (Range)
        {
            Controller.ActionOptions = new bool[3] {false, true, false};
        }
        else
        {
            Controller.ActionOptions = new bool[3] {false, false, false};
        }
    }
    
}
