using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;

public class UnitController : MonoBehaviour
{
    uint ID => GetComponent<IDgenerator>().ID;

    [Space(3)]

    public static int SkillIndex;
    public Skill NowUsingSkill => Parameters.AvailableSkills?[SkillIndex];
    protected Vector3 position{ get{ return transform.position; } set{ transform.position = value; } }
    
    [SerializeField]private protected GameObject Platform;
    [SerializeField]private protected AllInOne MPlaner;
    [SerializeField]private protected AllInOne APlaner;

    protected Checkers CursorPos(float Up = 0) => new Checkers(GameObject.Find("3DCursor").transform.position, Up);
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
        InGameEvents.MapUpdate.AddListener(ParameterUpdate);
        InGameEvents.MouseController.AddListener((a, b) => { MouseTest = a==(ID | MPlaner.Parent.GetComponent<UnitController>().ID)? b:0; });
    }
    
    protected bool MPlanerChecker(bool Other = true)
    {        
        if(!Other) return false;
        bool OnOtherPlaner()
        {  
            foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(0, 100, 0) + MPlaner.position, -Vector3.up, 105, LayerMask.GetMask("Object"))) 
            { 
                if(hit.collider.gameObject != MPlaner.Planer) { return false; }
            }
            return true;
        }
        bool OnSelf()
        {
            return 
            (int)Mathf.Round(transform.position.x) == (int)Mathf.Round(MPlaner.position.x) 
            && 
            (int)Mathf.Round(transform.position.z) == (int)Mathf.Round(MPlaner.position.z);
        }
        bool OnDistance()
        {
            return Parameters.WalkDistance + 0.5f >= Checkers.Distance(MPlaner.position, transform.position); 
        }
        
        return OnOtherPlaner() && !OnSelf() && OnDistance();
    }

    int MouseTest = 0;
    void Update()
    {   
        foreach(Attack attack in AttackZone) Debug.DrawLine(attack.Where, new Checkers(attack.Where, 1f), attack.damage >= 0? new Color(attack.damage * 0.15f, 0, 0): new Color(0, Mathf.Abs(attack.damage) * 0.15f, 0) );
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

    protected virtual void ParameterUpdate(){ }
}


