using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using System.Threading.Tasks;

public class UnitController : MonoBehaviour
{
    uint ID => GetComponent<IDgenerator>().ID;

    [Space(3)]

    [SerializeField]protected GameObject AttackVisualizer;
    private int SkillIndex;
    public int CurrentSkillIndex { get { return SkillIndex; } set { if(value != SkillIndex) ParametersUpdate(); SkillIndex = value;} }

    public Skill NowUsingSkill => Parameters.AvailableSkills?[CurrentSkillIndex];
    protected Vector3 position{ get{ return transform.position; } set{ transform.position = value; } }
    
    [SerializeField]private protected AllInOne MPlaner;
    [SerializeField]private protected AllInOne APlaner;

    protected Collider Collider => GetComponent<MeshCollider>();

    private Checkers LastPose = new Checkers();
    protected Checkers CursorPos
    { get { 
            Checkers pos = new Checkers(GameObject.Find("3DCursor").transform.position);
            if(LastPose != pos) { LastPose = pos; ChangePos(); } 
            return pos; } }
    [Space(3)]
    [SerializeField] private PlayerControl baseParameters;
    public PlayerControl Parameters => baseParameters ; 


    private List<GameObject> attackPointsVisuals = new List<GameObject>();
    protected void AttackVisualization(List<Attack> AttackZone)
    {   
        foreach(Attack attack in AttackZone)
        {
            if(attack.damage == 0) continue;
            GameObject obj = Instantiate(AttackVisualizer, attack.Where, AttackVisualizer.transform.rotation, InGameEvents.AttackFolders);
            attackPointsVisuals.Add(obj);
            
            obj.GetComponent<SpriteRenderer>().color = (attack.damageType != DamageType.Heal)? new Color(attack.damage * 0.07f, 0, 0.1f, 0.9f) : new Color(0, attack.damage * 0.07f, 0.1f, 0.9f);
        }         
    }
    protected void AttackVsualizationClear() { foreach(GameObject obj in attackPointsVisuals) { Destroy(obj); } }

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
        InGameEvents.StepSystem.Add(Summon);
        InGameEvents.AttackTransporter.AddListener(GetDamage);


    }
    async Task Summon(int id){ 
        switch(id)
        {
            case 1: await Walking(); return;
            case 2: await PriorityAttacking(); return;
            case 3: await Attacking(); return;
            case 4: await Dead(); return;
            case 5: await Rest(); return;
        }
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

    protected virtual void ControlChange() { }
    protected virtual void ChangePos() { if(OnMouseTest != 0) ParametersUpdate(); }


    protected virtual async Task Walking() { await Task.Delay(Random.Range(0, 2300)); Debug.Log("I walked"); }
    protected virtual async Task PriorityAttacking() { await Task.Delay(Random.Range(0, 2300)); Debug.Log("I did it"); }
    protected virtual async Task Attacking( ) { await Task.Delay(Random.Range(0, 2300)); Debug.Log("I attacked"); }
    protected virtual async Task Dead() { await Task.Delay(Random.Range(0, 2300)); Debug.Log("I'm dead, not big surprise"); }
    protected virtual async Task Rest() { await Task.Delay(Random.Range(0, 2300)); Debug.Log("I'm resting"); }

    protected virtual async void GetDamage(List<Attack> attack) { await Task.Delay(Random.Range(0, 2300)); Debug.Log("i got a damage!"); }
}


