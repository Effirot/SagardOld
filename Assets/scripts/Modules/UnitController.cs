using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;


public class UnitController : MonoBehaviour
{
    uint ID => GetComponent<IDgenerator>().ID;


    [Space(3)]

    [SerializeField]protected GameObject AttackVisualizer;
    private protected int SkillIndex;
    public int CurrentSkillIndex { get { return SkillIndex; } set { if(value != SkillIndex) ParametersUpdate(); SkillIndex = value;} }

    public Skill NowUsingSkill => Parameters.AvailableSkills?[CurrentSkillIndex];
    protected Vector3 position{ get{ return transform.position; } set{ transform.position = value; } }
    
    [SerializeField]private protected AllInOne MPlaner;
    [SerializeField]private protected AllInOne APlaner;

    protected Collider Collider => GetComponent<MeshCollider>();

    private Checkers LastPose = new Checkers();
    protected Checkers CursorPos
    { 
        get
        { 
            Checkers pos = new Checkers(GameObject.Find("3DCursor").transform.position);
            if(LastPose != pos) { LastPose = pos; ChangePos(); } 
            return pos; 
        } 
    }
    [Space(3)]
    [SerializeField] private PlayerControlList baseParameters;
    public PlayerControlList Parameters => baseParameters ; 

    protected List<Attack> AttackZone = new List<Attack>();
    private List<GameObject> attackPointsVisuals = new List<GameObject>();
    
    protected void AttackVisualization()
    {   
        AttackVsualizationClear();
        foreach(Attack attack in AttackZone)
        {
            if(attack.damage == 0) continue;
            GameObject obj = Instantiate(AttackVisualizer, attack.Where, AttackVisualizer.transform.rotation, InGameEvents.AttackFolders);

            obj.GetComponent<SpriteRenderer>().color = (attack.damage > 0)? new Color(attack.damage * 0.07f, 0, 0.05f) : new Color(0, -attack.damage * 0.07f, 0.05f, 0.3f);

            attackPointsVisuals.Add(obj);
        }   
        
        void AttackVsualizationClear()
        {
            foreach(GameObject obj in attackPointsVisuals) { Destroy(obj); }
        }
    }

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
            if(a == (ID)) 
            {
                OnMouseTest = b;
                switch(b){
                    default: StandingIn(); return;
                    case 1: MovePlaningIn(); return;
                    case 2: AttackPlaningIn(); return;
                }
            }
            else OnMouseTest = 0;
        });
        InGameEvents.StepSystem.AddListener((a) => 
        { 
            ParametersUpdate(); 
            switch(a)
            {
                case 1: Walking(); return;
                case 2: PriorityAttacking(); return;
                case 3: Attacking(); return;
                case 4: Dead(); return;
                case 5: Rest(); return;
            }
        } );
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
    protected int OnMouseTest {
        get { return MouseTest; }
        set { MouseTest = value; if(value == 0) ControlChange(); }
    }

    void Update()
    {   
        switch(MouseTest)
        {
            default: StandingUpd(); return;
            case 1: MovePlaningUpd(); return;
            case 2: AttackPlaningUpd(); return;
        }
    }
    
    protected virtual void StandingUpd() {}
    protected virtual void MovePlaningUpd() {}
    protected virtual void AttackPlaningUpd() {}
    
    protected virtual void StandingIn() { CurrentSkillIndex = 0; }
    protected virtual void MovePlaningIn() {}
    protected virtual void AttackPlaningIn() {}

    protected virtual void ParametersUpdate(){ }

    protected virtual void ControlChange() { ParametersUpdate(); }
    protected virtual void ChangePos() { if(OnMouseTest != 0) ParametersUpdate(); }


    protected virtual void Walking() { Debug.Log("I walked"); }
    protected virtual void PriorityAttacking() {  Debug.Log("I did it"); }
    protected virtual void Attacking( ) { Debug.Log("I attacked"); AttackZone.Clear(); APlaner.position = MPlaner.position; ParametersUpdate(); }
    protected virtual void Dead() { Debug.Log("I'm dead, not big surprise"); }
    protected virtual void Rest() { Debug.Log("I'm resting"); }
}


