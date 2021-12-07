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

    public Material[] Materials;

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
        if(Changing)
        {
            ToPoint = VectorInInt(Cursore.transform.position, Cursore.transform.position.y);
            transform.eulerAngles =  new Vector3(0, Quaternion.LookRotation(transform.position - Cursore.transform.position).eulerAngles.y, 0);
        }

        switch (Parameters.AvailableAbilities[AbilitieID])
        {
            case "Close attack":

            break;

            case "Range attack":
            int Range = 5;
            int Damage = 2;
            string Debuff = "";
            string DamageType = "";

            if (!((Reset || Controller.ActionOptions[1] || Changing))) {LnRend.enabled = false;}
            else {LnRend.enabled = true; }


            ParabolePaint(transform.position, ToPoint, OKRange(Range), 1f, 3);
            DeselectingRange(OKRange(Range));
            RangeSelectingType(VectorInInt(transform.position, -1), VectorInInt(ToPoint, -1), OKRange(Range), Damage, Debuff, DamageType);

            
            
            break;
        }
    }    
    

    




    void RangeSelectingType(Vector3 StartPoint, Vector3 EndPoint, bool WhereOk, int Damage, string Debuff, string DamageType, bool Falling = false)
    {
        //----------------------------------------------------------------For Range Attacks---------------------------------------------------------------- 

        var heading = EndPoint - StartPoint;
        var distance = heading.magnitude;
        var direction = heading / distance;

        RaycastHit[] Hits = Physics.RaycastAll(StartPoint, direction, distance);
        Debug.DrawRay(StartPoint, direction, new Color(255, 0, 0), 6);


        foreach(RaycastHit CellHit in Hits) 
        {
            CellController Cell = CellHit.transform.gameObject.GetComponent<CellController>();

            Cell.Founded(gameObject, 2);
        }

    }
    

    
    
    















    private Vector3 VectorInInt(Vector3 Vector, float Y = 0)
    {
        return new Vector3(Convert.ToInt32(Vector.x), Y, Convert.ToInt32(Vector.z));
    }

    private GameObject RayTest(Vector3 PosFrom, Vector3 PosTo)
    {
        var heading = PosTo - PosFrom;
        var distance =  heading.magnitude;
        var direction = heading / distance;

        if (Physics.Raycast(PosFrom, direction, out RaycastHit hit, distance)){return hit.collider.gameObject;}
        else return null;
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
            Mover = Vector3.MoveTowards(Mover, EndPoint, distance / (LnRend.positionCount - 2));

            float y = (i - ((LnRend.positionCount - 2) / 2));
            float Formule = -(y*y) / (100 / HowUp);                        

            LnRend.SetPosition(i, new Vector3(Mover.x, (Formule) , Mover.z));
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

    void DeselectingRange(bool Range)
    {
        if (Range)
        {
            Controller.ActionOptions = new bool[] { false, true };
        }
        else
        {
            Controller.ActionOptions = new bool[] { false, false };
        }
    }
    
}
