using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using SagardCL;


public class HumanStandardController : UnitController
{
    public override void GetDamage(Attack attack) 
    { 
        Health.GetDamage(attack); 
        if(attack.damage > 0) ChangeFigureColorWave(attack.DamageColor(), 0.2f);
    }



    [Space(100)]
    [Header("Start parameters")]
    [SerializeReference] IHealthBar HP = new HealthOver();
    [SerializeReference] IStaminaBar Stam = new Stamina();
    [SerializeReference] ISanityBar San = new Sanity();
    [Space]
    [SerializeField] int InventorySize = 1;
    [SerializeReference] List<Item> Items = new List<Item>(2);

    void Start() {

        Health = HP;
        Stamina = Stam;
        Sanity = San;

        Inventory = Items;

    }
}