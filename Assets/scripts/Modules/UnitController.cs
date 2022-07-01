using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using System.Threading.Tasks;

public abstract class UnitController : MonoBehaviour, IPlayerStats
{
    private protected AllInOne MPlaner { get{ return SkillRealizer.From; } set { SkillRealizer.From = value; } }
    private protected AllInOne APlaner { get{ return SkillRealizer.To; } set { SkillRealizer.To = value; } }

    public Color Team { get { return _Team; } set { _Team = value; } }
    [SerializeField] Color _Team;

    public bool CanControl { get{ return _CanControl & !_Corpse; } set { _CanControl = value; } }
    [SerializeField] bool _CanControl = true;
    public bool Corpse { get { return Corpse; } set{ Corpse = value; } }
    [SerializeField] bool _Corpse = false;
    public bool Artifacer { get { return Artifacer; } set{ Artifacer = value; } }
    [SerializeField] bool _Artifacer = false;
    public int WalkDistance { get { return _WalkDistance; } set { _WalkDistance = value; } }
    [SerializeField] int _WalkDistance = 5;


    public IHealthBar Health { get{ return _Health; } set{ _Health = value; } }
    IHealthBar _Health;
    public IStaminaBar Stamina { get{ return _Stamina; } set{ _Stamina = value; } }
    IStaminaBar _Stamina;
    public ISanityBar Sanity { get { return _Sanity; } set{ _Sanity = value; } } 
    ISanityBar _Sanity;
    

    public List<IStateBar> OtherStates { get { return _OtherStates; } set{ _OtherStates = value; }}
    List<IStateBar> _OtherStates = new List<IStateBar>();

    public List<Effect> Resists { get { return _Resists; } set{ _Resists = value; }}
    List<Effect> _Resists = new List<Effect>();
    public List<Effect> Debuff { get { return _Debuff; } set{ _Debuff = value; }}
    List<Effect> _Debuff = new List<Effect>();

    public List<Item> Inventory;

    public SkillCombiner SkillRealizer { get{ return _SkillRealizer; } set { _SkillRealizer = value; } }
    [SerializeField]SkillCombiner _SkillRealizer = new SkillCombiner();

    protected Vector3 position{ get{ return transform.position; } set{ transform.position = value; } }
    protected Collider Collider => GetComponent<MeshCollider>();

    public int CurrentSkillIndex { get { return SkillRealizer.SkillIndex; } set { if(value != SkillRealizer.SkillIndex) MouseWheelTurn(); SkillRealizer.SkillIndex = value;} }
    

    
    protected int MouseTest = 0;
    protected List<Attack> AttackZone = new List<Attack>();
    protected List<Checkers> WalkWay = new List<Checkers>();

    private Checkers LastPose = new Checkers();
    protected Checkers CursorPos { get {
        Checkers pos = new Checkers(GameObject.Find("3DCursor").transform.position);
        if(LastPose != pos) { LastPose = pos; ChangePos(); } 
        return pos; } 
    }

    async void Awake()
    {
        InGameEvents.MapUpdate.AddListener(ParametersUpdate);
        InGameEvents.MouseController.AddListener((id, b) => 
        { 
            if(id != MPlaner.Planer) { MouseTest = 0; return; }
            MouseTest = b;
            switch(MouseTest)
            {
                default: StandingIn(); return;
                case 1: MovePlaningIn(); return;
                case 2: AttackPlaningIn(); return;
            }
        });
        InGameEvents.StepSystem.Add(Summon);
        InGameEvents.AttackTransporter.AddListener((a) => { 
            Attack find = a.Find((a) => a.Where == new Checkers(position));

            if(find.Where == new Checkers(position)){
                if(find.damageType != DamageType.Heal)GetDamage(find);
                else GetHeal(find);
            }
        });
    
        
        await Task.Delay(1);
        position = new Checkers(position);
    }
    void Update()
    {   
        switch(MouseTest)
        {
            default: StandingUpd(); return;
            case 1: MovePlaningUpd(); return;
            case 2: AttackPlaningUpd(); return;
            case 4: break;
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
    protected bool WalkChecker(bool Other = true)
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
        
        //OnStamina
        if(Stamina.WalkUseStamina > Stamina.Value) return false;
        //OnDistance
        return WalkDistance + 0.5f >= Checkers.Distance(MPlaner.position, position); 
    }

    protected void StandingUpd() // Calling(void Update), when you no planing
    {
        //Base model
        MPlaner.Renderer.enabled = WalkChecker();
        
        //Move planner
        MPlaner.Collider.enabled = true;
        if(!WalkChecker() & InGameEvents.Controllable) MPlaner.position = position;
    
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
        UnitUIController.UiEvent.Invoke("CloseForPlayer", gameObject, this);
        position = new Checkers(position);
        
        InGameEvents.MapUpdate.Invoke();
    }
    protected async void MovePlaningIn()
    {
        UnitUIController.UiEvent.Invoke("CloseForPlayer", gameObject, this);
        await MovePlannerUpdate();
    }
    protected async void AttackPlaningIn()
    {
        CurrentSkillIndex = 0;
        UnitUIController.UiEvent.Invoke("OpenForPlayer", MPlaner.Planer, this);
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
        if(!WalkChecker()) { MPlaner.LineRenderer.enabled = false; WalkWay.Clear(); return; }
        MPlaner.LineRenderer.enabled = true;
        WalkWay.Clear();
        if (WalkChecker()){
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
        Generation.DrawAttack(AttackZone, this);
        

        APlaner.Renderer.enabled = APlaner.position !=  MPlaner.position & SkillRealizer.NowUsing.Type != (HitType.Empty & HitType.OnSelfPoint & HitType.Arc & HitType.Constant);
        APlaner.Renderer.material.color = (!SkillRealizer.Check())? Color.green : Color.red;
        SkillRealizer.Graphics(); 
    }

    async Task Walking()
    {
        if(WalkWay.Count == 0) return;
        WillRest = false;
        await Task.Delay(10);
        IEnumerator MPlanerMove()
        {   
            int PointNum = 1;
            for(float i = 0.00001f; position != MPlaner.position; i *= 1.040f)
            {
                position = Vector3.MoveTowards(position, WalkWay[PointNum], i);
                MPlaner.LineRenderer.SetPosition(0, position);
                if(position == WalkWay[PointNum].ToVector3() & position != WalkWay[WalkWay.Count - 1].ToVector3()){ PointNum++; }
                yield return new WaitForEndOfFrame();
            }
            yield break;
        }
        await MovePlannerUpdate();
        StartCoroutine(MPlanerMove());
        await Task.Run(MPlanerMove);
        Stamina.GetTired(Stamina.WalkUseStamina);
        ParametersUpdate();
    }
    async Task PriorityAttacking()
    {
        if(AttackZone.Count == 0) return;
        if(!SkillRealizer.NowUsing.PriorityAttacking) return;
        WillRest = false;
        await Task.Delay(Random.Range(0, 2700));

        InGameEvents.AttackTransporter.Invoke(AttackZone);
        AttackZone.Clear();
        APlaner.position = MPlaner.position;

        await AttackPlannerUpdate();
    }
    async Task Attacking()
    {
        if(AttackZone.Count == 0) return;
        if(SkillRealizer.NowUsing.PriorityAttacking) return;
        WillRest = false;
        await Task.Delay(Random.Range(0, 2700));
        Debug.Log("Attacked " + MPlaner.Planer.name);

        Stamina.GetTired(SkillRealizer.StaminaWaste());
        InGameEvents.AttackTransporter.Invoke(AttackZone);
        AttackZone.Clear();
        APlaner.position = MPlaner.position;
        
        await AttackPlannerUpdate();
    }
    async Task Dead() 
    { 
        if(Health.Value > 0) return;
        await Task.Delay(Random.Range(10, 100)); 
        TransformIntoCorpse();
    }
    private bool WillRest = true;
    async Task Rest() 
    { 
        if(!WillRest) { WillRest = true; return;}
        await Task.Delay(Random.Range(0, 2300)); 
        Stamina.Rest();
    }

    public abstract void GetDamage(Attack attack);
    public abstract void GetHeal(Attack attack);

    public virtual void TransformIntoCorpse()
    {

    }
}
