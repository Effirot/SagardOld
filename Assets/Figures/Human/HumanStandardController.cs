using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Threading.Tasks;

public class HumanStandardController : UnitController, PlayerStats
{    
    public override Color Team { get { return _Team; } set { _Team = value; } }
    [SerializeField] Color _Team = new Color();


    public override bool CanControl { get{ return _CanControl & !_Corpse; } set { _CanControl = value; } }
    [SerializeField] bool _CanControl = true;
    public override bool Corpse { get { return Corpse; } set{ Corpse = value; } }
    [SerializeField] bool _Corpse = false;
    public override int WalkDistance { get { return _WalkDistance; } set { _WalkDistance = value; } }
    [SerializeField] int _WalkDistance = 5;


    public override IHealthBar Health { get{ return _Health; } set{ _Health = value; } }
    [SerializeReference] IHealthBar _Health = new HealthOver();
    public override IStaminaBar Stamina { get{ return _Stamina; } set{ _Stamina = value; } }
    [SerializeReference] IStaminaBar _Stamina = new Stamina();
    public override ISanityBar Sanity { get { return _Sanity; } set{ _Sanity = value; } } 
    [SerializeReference] ISanityBar _Sanity = new Sanity();
    

    public override List<IStateBar> OtherStates { get { return _OtherStates; } set{ _OtherStates = value; }}
    [SerializeReference] List<IStateBar> _OtherStates = new List<IStateBar>();


    public override List<Effect> Resists { get { return _Resists; } set{ _Resists = value; }}
    [SerializeReference] List<Effect> _Resists = new List<Effect>();
    public override List<Effect> Debuff { get { return _Debuff; } set{ _Debuff = value; }}
    [SerializeReference] List<Effect> _Debuff = new List<Effect>();


    public override SkillCombiner SkillRealizer { get{ return _SkillRealizer; } set { _SkillRealizer = value; } }
    [SerializeReference] SkillCombiner _SkillRealizer = new SkillCombiner();

    protected override void GetDamage(Attack attack) { Health.GetDamage(attack); }
    protected override void GetHeal(Attack attack) { Health.GetDamage(attack); }

    public Skill TestPerishableSkill;
    
    void Start() {
        if(TestPerishableSkill) SkillRealizer.AdditionBaseSkills.Add(new Perishable<Skill>(TestPerishableSkill, 1));
    }
}