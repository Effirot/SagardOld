using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using System.Threading.Tasks;

public abstract class UnitController : MonoBehaviour
{
    protected uint ID => GetComponent<IDgenerator>().ID;
    
    [Space(3)]

    [SerializeField]protected GameObject AttackVisualizer;
    [SerializeField]protected GameObject UiPreset;

    public int CurrentSkillIndex { get { return NowUsingSkill.SkillIndex; } set { if(value != NowUsingSkill.SkillIndex) MouseWheelTurn(); NowUsingSkill.SkillIndex = value;} }
    [SerializeField]protected int MouseTest = 0;

    [SerializeField]public LifeParameters Parameters;
    public Skill NowUsingSkill => Parameters.SkillRealizer;

    [SerializeField]private protected AllInOne MPlaner { get{ return NowUsingSkill.From; } set { NowUsingSkill.From = value; } }
    [SerializeField]private protected AllInOne APlaner { get{ return NowUsingSkill.To; } set { NowUsingSkill.To = value; } }


    protected Vector3 position{ get{ return transform.position; } set{ transform.position = value; } }
    protected Collider Collider => GetComponent<MeshCollider>();

    private Checkers LastPose = new Checkers();
    protected Checkers CursorPos
    { get { 
            Checkers pos = new Checkers(GameObject.Find("3DCursor").transform.position);
            if(LastPose != pos) { LastPose = pos; ChangePos(); } 
            return pos; } }
    [Space(3)]




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

        InGameEvents.MapUpdate.AddListener(ParametersUpdate);
        InGameEvents.MouseController.AddListener((UnityAction<uint, int>)((id, b) => 
        { 
            if(id == ID) { MouseTest = b; }
            else MouseTest = 0;
            switch(MouseTest)
            {
                default: StandingIn(); return;
                case 1: MovePlaningIn(); return;
                case 2: AttackPlaningIn(); return;
            }
            
        }));
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
        if(new Checkers(position) == new Checkers(MPlaner.position))
            return false;
        
        //OnDistance
        return Parameters.WalkDistance + 0.5f >= Checkers.Distance(MPlaner.position, position); 
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
    
    protected abstract void StandingUpd();
    protected abstract void MovePlaningUpd();
    protected abstract void AttackPlaningUpd();

    protected abstract void StandingIn();
    protected abstract void MovePlaningIn();
    protected abstract void AttackPlaningIn();

    protected abstract void ParametersUpdate();

    protected abstract void ChangePos();
    protected abstract void MouseWheelTurn();

    protected virtual async Task Walking() { await Task.Delay(Random.Range(0, 2300)); Debug.Log("I walked"); }
    protected virtual async Task PriorityAttacking() { await Task.Delay(Random.Range(0, 2300)); Debug.Log("I did it"); }
    protected virtual async Task Attacking( ) { await Task.Delay(Random.Range(0, 2300)); Debug.Log("I attacked"); }
    protected virtual async Task Dead() { await Task.Delay(Random.Range(0, 2300)); Debug.Log("I'm dead, not big surprise"); }
    protected virtual async Task Rest() { await Task.Delay(Random.Range(0, 2300)); Debug.Log("I'm resting"); }

    protected virtual async void GetDamage(List<Attack> attack) { await Task.Delay(Random.Range(0, 2300)); Debug.Log("i got a damage!"); }
}
