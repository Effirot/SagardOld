using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;


public class UnitController : MonoBehaviour
{
    uint ID => GetComponent<IDgenerator>().ID;

    [Space(3)]

    public int SkillIndex;
    public Skill NowUsingSkill => Parameters.AvailableSkills?[SkillIndex];
    protected Vector3 position{ get{ return transform.position; } set{ transform.position = value; } }
    
    [SerializeField]private protected GameObject Platform;
    [SerializeField]private protected AllInOne MPlaner;
    [SerializeField]private protected AllInOne APlaner;

    private Checkers LastPose = new Checkers();
    protected Checkers CursorPos
    { 
        get
        { 
            Checkers pos = new Checkers(GameObject.Find("3DCursor").transform.position);
            if(LastPose != pos) { LastPose = pos; Changepos(); } 
            return pos; 
        } 
    }
    [Space(3)]
    [SerializeField] protected PlayerControlList baseParameters;
    public PlayerControlList Parameters => baseParameters; 

    public List<Attack> AttackZone = new List<Attack>();
    
    void Awake()
    {
        foreach(Skill skill in Parameters.AvailableSkills)
        {
            skill.From = MPlaner;
            skill.To = APlaner;
        }
        InGameEvents.MapUpdate.AddListener(ParametersUpdate);
        InGameEvents.MouseController.AddListener((a, b) => 
        { 
            if(a == (ID)) OnMouseTest = b; 
            if(a == 0) OnMouseTest = 0;
        });
    }
    
    protected bool MPlanerChecker(bool Other = true)
    {        
        if(!Other) return false;
        
        //OnOtherPlaner
        foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(0, 100, 0) + MPlaner.position, -Vector3.up, 105, LayerMask.GetMask("Object"))) 
        { 
            if(hit.collider.gameObject != MPlaner.Planer) { return false; }
        }
        
        //OnSelf
        if(
        (int)Mathf.Round(transform.position.x) == (int)Mathf.Round(MPlaner.position.x) 
        && 
        (int)Mathf.Round(transform.position.z) == (int)Mathf.Round(MPlaner.position.z))
            return false;
        
        //OnDistance
        return Parameters.WalkDistance + 0.5f >= Checkers.Distance(MPlaner.position, transform.position); 
    }

    private int MouseTest = 0;
    protected int OnMouseTest
    {
        get
        {
            return MouseTest;
        }
        set
        {
            MouseTest = value;
            if(value == 0) ControlChange();
        }
    }

    void Update()
    {   
        switch(MouseTest)
        {
            default: Standing(); return;
            case 1: MovePlaning(); return;
            case 2: AttackPlaning(); return;
        }
    }

    protected virtual void Standing() {}
    protected virtual void MovePlaning() {}
    protected virtual void AttackPlaning() {}

    protected virtual void ParametersUpdate(){  }

    protected virtual void ControlChange() { ParametersUpdate(); }
    protected virtual void Changepos() { if(OnMouseTest != 0) ParametersUpdate(); }
}


