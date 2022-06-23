using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using System.Threading.Tasks;

public abstract class UnitController : MonoBehaviour
{
    protected uint ID => GetComponent<IDgenerator>().ID;

    public int CurrentSkillIndex { get { return SkillRealizer.SkillIndex; } set { if(value != SkillRealizer.SkillIndex) MouseWheelTurn(); SkillRealizer.SkillIndex = value;} }
    [SerializeField]protected int MouseTest = 0;

    [SerializeField]private protected AllInOne MPlaner { get{ return SkillRealizer.From; } set { SkillRealizer.From = value; } }
    [SerializeField]private protected AllInOne APlaner { get{ return SkillRealizer.To; } set { SkillRealizer.To = value; } }

    protected Vector3 position{ get{ return transform.position; } set{ transform.position = value; } }
    protected Collider Collider => GetComponent<MeshCollider>();

    private Checkers LastPose = new Checkers();
    protected Checkers CursorPos
    { get { 
            Checkers pos = new Checkers(GameObject.Find("3DCursor").transform.position);
            if(LastPose != pos) { LastPose = pos; ChangePos(); } 
            return pos; } }
    [Space(3)]

    [Space, Header("Base Parameters")]
    public Color Team;
    [Space]
    public bool CanControl = true;
    public int WalkDistance;
    [Space] 
    [SerializeReference] public HealthBar Health = new HealthOver();  // health parameters
    [SerializeReference] public StaminaBar Stamina = new Stamina(); // Stamina parameters
    [SerializeReference] public SanityBar Sanity = new Sanity(); // sanity parameters
    [Space(3)]
    [SerializeReference] public List<StateBar> OtherStates = new List<StateBar>();

    [Space] // Debuff's parameters
    public List<Effect> Resists;
    public List<Effect> Debuff;

    // Skills parameters
    [Space]
    public Skill SkillRealizer;

    private List<GameObject> attackPointsVisuals = new List<GameObject>();
    protected void AttackVisualization(List<Attack> AttackZone)
    {   
        foreach(Attack attack in AttackZone)
        {
            if(attack.damage == 0) continue;
            GameObject obj = Instantiate(PoleAttackVisualizer.Visual, attack.Where, PoleAttackVisualizer.Visual.transform.rotation, InGameEvents.AttackFolders);
            attackPointsVisuals.Add(obj);
            
            obj.GetComponent<SpriteRenderer>().color = (attack.damageType != DamageType.Heal)? new Color(attack.damage * 0.07f, 0, 0.1f, 0.9f) : new Color(0, attack.damage * 0.07f, 0.1f, 0.9f);
        }         
    }
    protected void AttackVsualizationClear() { foreach(GameObject obj in attackPointsVisuals) { Destroy(obj); } }


    void Awake()
    {
        position = new Checkers(position);
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
        InGameEvents.AttackTransporter.AddListener((a) => { 
            Attack find = a.Find((a) => a.Where == new Checkers(position));

            if(find.Where == new Checkers(position)){
                if(find.damageType != DamageType.Heal)GetDamage(find);
                else GetHeal(find);
                }
        });
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
        return WalkDistance + 0.5f >= Checkers.Distance(MPlaner.position, position); 
    }

    List<Attack> AttackZone = new List<Attack>();
    List<Checkers> WalkWay = new List<Checkers>();


    protected void StandingUpd() // Calling(void Update), when you no planing
    {
        //Base model
        MPlaner.Renderer.enabled = MPlanerChecker();
        
        //Move planner
        MPlaner.Collider.enabled = true;
        if(!MPlanerChecker() & InGameEvents.canControl) MPlaner.position = position;
    
        //Attack planner
        if(!SkillRealizer.Check()) APlaner.position = MPlaner.position;
        APlaner.Renderer.enabled = SkillRealizer.Check();
    }
    protected void MovePlaningUpd() // Calling(void Update), when you planing your moving
    {
        MPlaner.Renderer.enabled = true;
        
        if(SkillRealizer.NowUsing.NoWalking) APlaner.position = new Checkers(MPlaner.position);

        //Move planner
        MPlaner.position = new Checkers(CursorPos);
        MPlaner.Collider.enabled = false;
        
    }
    protected void AttackPlaningUpd() // Calling(void Update), when you planing your attacks
    {

        //Move planner
        if(SkillRealizer.NowUsing.NoWalking)
        {
            MPlaner.position = position;
            MPlaner.Renderer.enabled = false;
            MPlaner.Collider.enabled = true;
        }

        //Attack planner
        APlaner.position = new Checkers(CursorPos);

        //Mouse Scroll
        CurrentSkillIndex = Mathf.Clamp(CurrentSkillIndex + (int)(Input.GetAxis("Mouse ScrollWheel") * 10), 0, SkillRealizer.AvailbleSkills.Count - 1);
    }

    protected void StandingIn()
    {
        UnitUIController.UiEvent.Invoke(UnitUIController.WhatUiDo.Close, gameObject, this);
        ParametersUpdate();
    }
    protected async void MovePlaningIn()
    {
        await MovePlannerUpdate();
    }
    protected async void AttackPlaningIn()
    {
        CurrentSkillIndex = 0;
        UnitUIController.UiEvent.Invoke(UnitUIController.WhatUiDo.Open, MPlaner.Planer, this);
        await AttackPlannerUpdate();
    }

    protected async void MouseWheelTurn(){ await AttackPlannerUpdate();  }
    protected async void ChangePos() {  if(MouseTest == 2) await AttackPlannerUpdate(); if(MouseTest == 1) ParametersUpdate(); }
 
    protected async void ParametersUpdate()
    {
        await MovePlannerUpdate();
        await AttackPlannerUpdate();
    }
    
    protected async Task MovePlannerUpdate()
    {
        await Task.Delay(1);

        // Move planner
        if(!MPlanerChecker()) { MPlaner.LineRenderer.enabled = false; return;}
        MPlaner.LineRenderer.enabled = true;
        WalkWay.Clear();
        if (MPlanerChecker()){
            await foreach(Checkers step in Checkers.PatchWay.WayTo(new Checkers(position), new Checkers(MPlaner.position))){
                WalkWay.Add(step);
            }
            MPlaner.LineRenderer.positionCount = WalkWay.Count;
            MPlaner.LineRenderer.SetPositions(Checkers.ToVector3List(WalkWay).ToArray()); 
        }
    }
    protected async Task AttackPlannerUpdate()
    {
        await Task.Delay(1);
        APlaner.position = new Checkers(APlaner.position);
        // Attack planner
        AttackZone.Clear();
        if(SkillRealizer.NowUsing.NoWalking) await MovePlannerUpdate();
        await foreach(Attack attack in SkillRealizer.Realize())
        {
            AttackZone.Add(attack);
        }
        AttackVsualizationClear();
        AttackVisualization(AttackZone);

        APlaner.Renderer.enabled = APlaner.position != position & APlaner.position !=  MPlaner.position & SkillRealizer.NowUsing.Type != (HitType.Empty & HitType.OnSelfPoint & HitType.Arc & HitType.Constant);
        APlaner.Renderer.material.color = (!SkillRealizer.Check())? Color.green : Color.red;
        SkillRealizer.Graphics(); 
    }

    protected virtual async Task Walking()
    {
        if(WalkWay == null) return;
        await Task.Delay(10);
        IEnumerator MPlanerMove()
        {
            int PointNum = 1;
            for(float i = 0.00001f; position != MPlaner.position; i *= 1.025f)
            {
                position = Vector3.MoveTowards(position, WalkWay[PointNum], i);
                MPlaner.LineRenderer.SetPosition(0, position);
                if(new Checkers(position) == WalkWay[PointNum] & new Checkers(position) != WalkWay[WalkWay.Count - 1]){ PointNum++; }
                yield return null;
            }
            yield break;
        }
        StartCoroutine(MPlanerMove());
        await Task.Run(MPlanerMove);
        ParametersUpdate();
    }
    protected virtual async Task PriorityAttacking()
    {
        if(AttackZone == null) return;
        if(!SkillRealizer.NowUsing.PriorityAttacking) return;
        await Task.Delay(Random.Range(0, 2700));

        InGameEvents.AttackTransporter.Invoke(AttackZone);
        AttackZone.Clear();
        APlaner.position = position;
        await AttackPlannerUpdate();
    }
    protected virtual async Task Attacking()
    {
        if(AttackZone == null) return;
        if(SkillRealizer.NowUsing.PriorityAttacking) return;
        await Task.Delay(Random.Range(0, 2700));

        InGameEvents.AttackTransporter.Invoke(AttackZone);
        AttackZone.Clear();
        APlaner.position = position;
        await AttackPlannerUpdate();
    }
    protected virtual async Task Dead() { await Task.Delay(Random.Range(0, 2300)); Debug.Log("I'm dead, not big surprise"); }
    protected virtual async Task Rest() { await Task.Delay(Random.Range(0, 2300)); Debug.Log("I'm resting"); }

    protected void GetDamage(Attack attack) 
    {
        Health.GetDamage(attack);        
    }
    protected virtual void GetHeal(Attack attack)
    {
        Health.GetDamage(attack);
    }
}
