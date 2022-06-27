using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Threading.Tasks;

public class HumanStandardController : UnitController, PlayerStats
{    
    public HealthBar Health { get{ return _Health; } set{ _Health = value; } }
    public StaminaBar Stamina { get{ return _Stamina; } set{ _Stamina = value; } }
    public SanityBar Sanity { get { return Sanity; } set{ Sanity = value; } } 

    public List<StateBar> OtherStates { get { return _OtherStates; } set{ _OtherStates = value; }}

    public List<Effect> Resists { get { return Resists; } set{ Resists = value; }}
    public List<Effect> Debuff { get { return _Debuff; } set{ _Debuff = value; }}

    protected override void GetDamage(Attack attack) { Health.GetDamage(attack); }
    protected override void GetHeal(Attack attack) { Health.GetDamage(attack); }

    public List<BaseSkill> BaseSkills { get{ return SkillRealizer.AvailbleSkills; } set{ SkillRealizer.AdditionBaseSkills = value; } }
}