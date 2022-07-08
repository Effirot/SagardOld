using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using System.Threading.Tasks;
using System;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif


[Serializable] public abstract class UnitController : MonoBehaviour, IPlayerStats
{
    #region // ============================================================ Useful Stuff ==================================================================================================
    private protected AllInOne MPlaner { get{ return SkillRealizer.From; } set { SkillRealizer.From = value; } }
    private protected AllInOne APlaner { get{ return SkillRealizer.To; } set { SkillRealizer.To = value; } }
    protected Vector3 position{ get{ return transform.position; } set{ transform.position = value; } }
    Collider Collider => GetComponent<MeshCollider>();

    private Checkers LastPose = new Checkers();
    Checkers CursorPos { get { Checkers pos = CursorController.Pos; if(LastPose != pos) { LastPose = pos; ChangePos(); } return pos; } }
    
    public void ChangeFigureColor(Color color, float speed, Material material) { StartCoroutine(ChangeMaterialColor(material, color, speed)); }
    public void ChangeFigureColor(Color color, float speed, Material[] material = null) 
    { 
        if(material == null) material = new Material[] { transform.parent.Find("MPlaner/Platform").GetComponent<Renderer>().material, 
                                                         transform.parent.Find("MPlaner/Platform/Figure").GetComponent<Renderer>().material };
        foreach (Material mat in material) StartCoroutine(ChangeMaterialColor(mat, color, speed)); 
    }
    public void ChangeFigureColorWave(Color color, float speed, Material[] material = null) 
    { 
        if(material == null) material = new Material[] { transform.parent.Find("MPlaner/Platform").GetComponent<Renderer>().material, 
                                                         transform.parent.Find("MPlaner/Platform/Figure").GetComponent<Renderer>().material };
        
        List<Task> tasks = new List<Task>();
        foreach (Material mat in material) { StartCoroutine(Wave(mat, color, speed)); }

        IEnumerator Wave(Material material, Color color, float speed)
        { 
            Color Save = material.color;
            yield return ChangeMaterialColor(material, color, speed); 
            yield return ChangeMaterialColor(material, Save, speed); 
        }
    }
    static IEnumerator ChangeMaterialColor(Material material, Color color, float speed) {
        while(material.color != color)
        {
            material.color = Color.Lerp(material.color, color, speed);
            speed *= 1.1f;
            yield return new WaitForFixedUpdate();
        }
    }

    public static List<T> CombineLists<T>(List<T> a, List<T> b) 
    {
        List<T> result = new List<T>();
        result.AddRange(a);
        result.AddRange(b);
        return result;
    }
    public static List<T> CombineLists<T>(List<List<T>> a)
    {
        List<T> result = new List<T>();
        foreach(List<T> b in a)
        {
            result.AddRange(b);
        }
        return result;
    }
    #endregion
    #region // =========================================================== All parameters =================================================================================================

    #region // ================================== controlling
    
    public Color Team { get { return _Team; } set { _Team = value; } }
    [SerializeField] Color _Team;

    [SerializeField] bool _CanControl = true;
    [SerializeField] bool _Corpse = false;
    [SerializeField] int _WalkDistance = 5;
    
    public bool CanControl { get{ return _CanControl & !_Corpse; } set { _CanControl = value; } }
    public bool Corpse { get { return _Corpse; } set{ _Corpse = value; } }
    public int WalkDistance { get { return _WalkDistance; } set { _WalkDistance = value; } }

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
        return WalkDistance + AllItemStats.WalkDistance + 0.5f >= Checkers.Distance(MPlaner.position, position); 
    }

    #endregion
    #region // ================================== parameters
    
    IHealthBar _Health;
    ISanityBar _Sanity;
    IStaminaBar _Stamina;

    [SerializeReference, SerializeReferenceButton] IHealthBar BaseHealth;
    [SerializeReference, SerializeReferenceButton] ISanityBar BaseSanity;
    [SerializeReference, SerializeReferenceButton] IStaminaBar BaseStamina;
    [SerializeReference, SerializeReferenceButton] List<IOtherBar> _OtherStates;
    
    public IHealthBar Health { get{ return _Health; } set{ _Health = value; } }
    public ISanityBar Sanity { get { return _Sanity; } set{ _Sanity = value; } } 
    public IStaminaBar Stamina { get{ return _Stamina; } set{ _Stamina = value; } }
    public List<IOtherBar> OtherStates { get { return CombineLists<IOtherBar>(_OtherStates, AllItemStats.AdditionState); } set{ _OtherStates = value; } }

    #endregion
    #region // ================================== effects
    
    [SerializeReference, SerializeReferenceButton] List<Effect> _Debuff;
    [SerializeReference, SerializeReferenceButton] List<Effect> _Resists;

    public List<Effect> Debuff { get { return _Debuff; } set{ _Debuff = value; } }
    public List<Effect> Resists { get { return _Resists; } set{ _Resists = value; }}

    #endregion
    #region // ================================== inventory
    
    [SerializeField] List<Item> _Inventory;
    public int InventorySize = 1;
    [SerializeField] public List<Item> ArtifacerItems;
    public int ArtifacerItemsCount = 0;
    public List<Item> Inventory { get { return CombineLists<Item>(_Inventory, ArtifacerItems); } set{ _Inventory = value; } }

    public ParamsChanger AllItemStats;
    #endregion
    #region // ================================== Skills
   
    public SkillCombiner SkillRealizer { get{ return _SkillRealizer; } set { _SkillRealizer = value; } }
    [SerializeField] SkillCombiner _SkillRealizer = new SkillCombiner();

    protected List<Attack> AttackZone = new List<Attack>();
    protected List<Checkers> WalkWay = new List<Checkers>();
    
    public int CurrentSkillIndex { get { return SkillRealizer.SkillIndex; } set { if(value != SkillRealizer.SkillIndex) MouseWheelTurn(); SkillRealizer.SkillIndex = value; } }
    #endregion

    #endregion
    #region // ========================================================= OnStart Parameters ===============================================================================================
    int MouseTest = 0;
    void Awake()
    {
        Health = BaseHealth.Clone() as IHealthBar;
        Stamina = BaseStamina.Clone() as IStaminaBar;
        Sanity = BaseSanity.Clone() as ISanityBar;

        InGameEvents.MapUpdate.AddListener(ParametersUpdate);
        InGameEvents.MouseController.AddListener((id, b) => 
        { 
            if(id != MPlaner.Planer | !(!Corpse & CanControl)) { MouseTest = 0; return; }
            MouseTest = b;
            switch(MouseTest)
            {
                default: StandingIn(); return;
                case 1: MovePlaningIn(); return;
                case 2: AttackPlaningIn(); return;
            }
        });
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
        InGameEvents.StepSystem.Add(Summon);
        InGameEvents.AttackTransporter.AddListener((a) => { 
            
            Attack find = a.Find((a) => a.Where == new Checkers(position));
            if(find.Where == new Checkers(position)){
                GetDamage(find);
            }
        });
    
        AfterInventoryUpdate();
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

    // Control use methods   
    async void MouseWheelTurn(){ await AttackPlannerUpdate();  }
    async void ChangePos() {  if(MouseTest == 2) await AttackPlannerUpdate(); if(MouseTest == 1) ParametersUpdate(); }
    
    // Standing methods
    void StandingUpd() // Calling(void Update), when you no planing
    {
        
        if(!WalkChecker()) MPlaner.position = position;
        MPlaner.Renderer.enabled = WalkChecker();
    
        //Attack planner
        if(!SkillRealizer.Check()) APlaner.position = MPlaner.position;
        APlaner.Renderer.enabled = SkillRealizer.Check();
        
        position = new Checkers(position);
    }
    void StandingIn()
    {
        UnitUIController.UiEvent.Invoke("CloseForPlayer", gameObject, this);
        
        InGameEvents.MapUpdate.Invoke();

        if(!WalkChecker() & InGameEvents.Controllable) MPlaner.position = position;
        MPlaner.Collider.enabled = true;
    }
    // Move planning methods
    void MovePlaningUpd() // Calling(void Update), when you planing your moving
    {
        MPlaner.Renderer.enabled = true;
        
        if(SkillRealizer.NowUsing.NoWalking) APlaner.position = new Checkers(MPlaner.position);

        //Move planner
        MPlaner.position = new Checkers(CursorPos);
        MPlaner.Collider.enabled = false;
    }
    async void MovePlaningIn()
    {
        APlaner.position = new Checkers(APlaner.position);
        UnitUIController.UiEvent.Invoke("CloseForPlayer", gameObject, this);
        await MovePlannerUpdate();
    }
    // Attack planning methods
    void AttackPlaningUpd() // Calling(void Update), when you planing your attacks
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
    async void AttackPlaningIn()
    {
        CurrentSkillIndex = 0;
        UnitUIController.UiEvent.Invoke("OpenForPlayer", MPlaner.Planer, this);
        await AttackPlannerUpdate();
    }


    #region // =============================== Step System
    async Task Walking()
    {
        if(WalkWay.Count == 0) return;
        MouseTest = 4;
        WillRest = false;
        await Task.Delay(10);

        await MovePlannerUpdate();
        StartCoroutine(MPlanerMove());
        await Task.Delay(200);
        await Task.Run(MPlanerMove);
        Stamina.GetTired(Stamina.WalkUseStamina);
        StopCoroutine(MPlanerMove());
        MouseTest = 0;
        ParametersUpdate();
    }
    IEnumerator MPlanerMove()
    {   
        int PointNum = 1;
        for(float i = 0.00001f; position != MPlaner.position; i *= 1.30f)
        {
            position = Vector3.MoveTowards(position, WalkWay[PointNum], i);
            MPlaner.LineRenderer.SetPosition(0, position);
            if(position == WalkWay[PointNum].ToVector3() & position != WalkWay[WalkWay.Count - 1].ToVector3()){ PointNum++; }
            yield return new WaitForFixedUpdate();
        }
        yield break;
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
        await Task.Delay(Random.Range(900, 2700));

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
        Corpse = true;
        try { ZeroHealth(); } catch {}
    }
    async Task Rest() 
    { 
        if(!WillRest) { WillRest = true; return; }
        await Task.Delay(Random.Range(0, 2300)); 
        AfterInventoryUpdate(); 
        Stamina.Rest();
    }
    private bool WillRest = true;
    #endregion

    #region // =============================== Update methods
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
            await foreach(Checkers step in Checkers.PatchWay.WayTo(new Checkers(position), new Checkers(MPlaner.position), 20)){
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
    public virtual void AfterInventoryUpdate()
    {
        AllItemStats = Item.CompoundParameters(Inventory);

        Health.Max = BaseHealth.Max + AllItemStats.Health.Max;
        Health.Value = Health.Value + AllItemStats.Health.Max;

        Stamina.Max = BaseStamina.Max + AllItemStats.Stamina.Max;
        Stamina.Value = Stamina.Value + AllItemStats.Stamina.Max;

        Sanity.Max = BaseSanity.Max + AllItemStats.Sanity.Max;
        Sanity.Value = Sanity.Value + AllItemStats.Sanity.Max;
    } 
    
    public virtual void GetDamage(Attack attack)
    {
        Health.GetDamage(attack); 
        if(attack.damage > 0) ChangeFigureColorWave(attack.DamageColor(), 0.2f);
    }
    public virtual void ZeroHealth()
    {
        if(Health is HealthCorpse) { Destroy(transform.parent.gameObject); }
        else {
            Health = new HealthCorpse() { Max = Health.Max, Value = Health.Max, ArmorMelee = Health.ArmorMelee, ArmorRange = Health.ArmorRange };

            ChangeFigureColor(new Color(0.6f, 0.6f, 0.6f), 0.2f);
        }
    }
    
    #endregion
    
    #endregion
}