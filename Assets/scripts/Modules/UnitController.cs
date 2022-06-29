using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using System.Threading.Tasks;

public interface ObjectOnMap
{
    HealthBar Health{ get; set; }

    List<Effect> Resists{ get; set; }
    List<Effect> Debuff{ get; set; }
}

public interface PlayerStats : ObjectOnMap
{
    Color Team{ get; set; }

    StaminaBar Stamina{ get; set; }
    SanityBar Sanity{ get; set; }

    bool CanControl { get; set; }
    bool Corpse { get; set; }
    int WalkDistance { get; set; }

    List<StateBar> OtherStates{ get; set; }

    Skill SkillRealizer{ get; set; }
}

public abstract class UnitController : MonoBehaviour
{
    private protected AllInOne MPlaner { get{ return SkillRealizer.From; } set { SkillRealizer.From = value; } }
    private protected AllInOne APlaner { get{ return SkillRealizer.To; } set { SkillRealizer.To = value; } }

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

    abstract public Color Team{ get; set; }
    abstract public bool CanControl { get; set; }
    abstract public bool Corpse { get; set; }
    abstract public int WalkDistance { get; set; }
    
    abstract public HealthBar Health { get; set; } // health parameters
    abstract public StaminaBar Stamina { get; set; } // Stamina parameters
    abstract public SanityBar Sanity { get; set; } // sanity parameters

    abstract public List<StateBar> OtherStates { get; set; }

    abstract public List<Effect> Resists { get; set; }
    abstract public List<Effect> Debuff { get; set; }

    abstract public Skill SkillRealizer { get; set; }

    void Awake()
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

    protected virtual async Task Walking()
    {
        if(WalkWay.Count == 0) return;
        WillRest = false;
        await Task.Delay(10);
        IEnumerator MPlanerMove()
        {   
            MouseTest = 4;
            int PointNum = 1;
            for(float i = 0.00001f; position != MPlaner.position; i *= 1.040f)
            {
                position = Vector3.MoveTowards(position, WalkWay[PointNum], i);
                MPlaner.LineRenderer.SetPosition(0, position);
                if(new Checkers(position) == WalkWay[PointNum] & new Checkers(position) != WalkWay[WalkWay.Count - 1]){ PointNum++; }
                yield return new WaitForEndOfFrame();
            }
            MouseTest = 0;
            yield break;
        }
        await MovePlannerUpdate();
        StartCoroutine(MPlanerMove());
        await Task.Run(MPlanerMove);
        Stamina.GetTired(Stamina.WalkUseStamina);
        ParametersUpdate();
    }
    protected virtual async Task PriorityAttacking()
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
    protected virtual async Task Attacking()
    {
        if(AttackZone.Count == 0) return;
        if(SkillRealizer.NowUsing.PriorityAttacking) return;
        WillRest = false;
        await Task.Delay(Random.Range(0, 2700));


        Stamina.GetTired(SkillRealizer.StaminaWaste());
        InGameEvents.AttackTransporter.Invoke(AttackZone);
        AttackZone.Clear();
        APlaner.position = MPlaner.position;
        
        await AttackPlannerUpdate();
    }
    protected virtual async Task Dead() 
    { 
        await Task.Delay(Random.Range(10, 100)); 

    }
    private bool WillRest = true;
    protected virtual async Task Rest() 
    { 
        if(!WillRest) { WillRest = true; return;}
        await Task.Delay(Random.Range(0, 2300)); 
        Stamina.Rest();
    }


    protected abstract void GetDamage(Attack attack);
    protected abstract void GetHeal(Attack attack);
}
