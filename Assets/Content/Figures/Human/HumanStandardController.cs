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


    public override HealthBar Health { get{ return _Health; } set{ _Health = value; } }
    [SerializeReference] HealthBar _Health = new HealthOver();
    public override StaminaBar Stamina { get{ return _Stamina; } set{ _Stamina = value; } }
    [SerializeReference] StaminaBar _Stamina = new Stamina();
    public override SanityBar Sanity { get { return _Sanity; } set{ _Sanity = value; } } 
    [SerializeReference] SanityBar _Sanity = new Sanity();
    

    public override List<StateBar> OtherStates { get { return _OtherStates; } set{ _OtherStates = value; }}
    [SerializeReference] List<StateBar> _OtherStates = new List<StateBar>();


    public override List<Effect> Resists { get { return _Resists; } set{ _Resists = value; }}
    [SerializeReference] List<Effect> _Resists = new List<Effect>();
    public override List<Effect> Debuff { get { return _Debuff; } set{ _Debuff = value; }}
    [SerializeReference] List<Effect> _Debuff = new List<Effect>();


    public override Skill SkillRealizer { get{ return _Skill; } set { _Skill = value; } }
    [SerializeReference] Skill _Skill = new Skill();







    protected override void GetDamage(Attack attack) { Health.GetDamage(attack); }
    protected override void GetHeal(Attack attack) { Health.GetDamage(attack);}
}