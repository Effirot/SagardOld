using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using SagardCL;


public class HumanStandardController : UnitController
{
    public override void GetDamage(Attack attack) { Health.GetDamage(attack); }
    public override void GetHeal(Attack attack) { Health.GetDamage(attack); }


    [SerializeReference] IHealthBar HP = new HealthOver();
    [SerializeReference] IStaminaBar Stam = new Stamina();
    [SerializeReference] ISanityBar San = new Sanity();
    [Space]
    [SerializeField] int InventorySize = 1;

    public Skill TestPerishableSkill;
    void Start() {
        if(TestPerishableSkill) SkillRealizer.AdditionBaseSkills.Add(new Perishable<Skill>(TestPerishableSkill, 1));

        Health = HP;
        Stamina = Stam;
        Sanity = San;

        Inventory = new List<Item>(InventorySize);

    }
}