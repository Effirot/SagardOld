using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using SagardCL;


public class UnitController : MonoBehaviour
{
    uint ID => GetComponent<IDgenerator>().ID;

    [Space(3)]

    protected GameObject AttackVisualizer;
    public int SkillIndex;
    protected int CurrentSkillIndex { get { return SkillIndex; } set { if(value != SkillIndex) ParametersUpdate(); SkillIndex = value;} }

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
    [SerializeField] protected PlayerControlList baseParameters;
    public PlayerControlList Parameters => baseParameters; 

    public List<Attack> AttackZone = new List<Attack>();

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
        AttackVisualizer = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Gizmos/Attack/AttackVisualizer.prefab") as GameObject;
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
    private void Tick() 
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

    protected virtual void StandingUpd() {}
    protected virtual void MovePlaningUpd() {}
    protected virtual void AttackPlaningUpd() {}

    protected virtual void ParametersUpdate(){ Tick(); }

    protected virtual void ControlChange() { ParametersUpdate(); }
    protected virtual void ChangePos() { if(OnMouseTest != 0) ParametersUpdate(); }
}


