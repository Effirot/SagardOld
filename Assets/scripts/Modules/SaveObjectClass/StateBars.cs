using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using SagardCL;
using System;

public abstract class StateBar
{
    Color BarColor{ get; }

    public abstract int State { get; }
    public abstract int Max { get; set; }  
}

public abstract class HealthBar : StateBar
{
    public abstract int ArmorMelee { get; set; } 
    public abstract int ArmorRange {get; set; }

    public abstract void GetDamage(Attack attack);
}
public abstract class StaminaBar : StateBar
{
    public abstract void GetTired(int value);
    public abstract int RestEffectivity{ get; set; }
    public abstract int WalkUseStamina{ get; set; }

    public abstract void Rest();
}
public abstract class SanityBar : StateBar
{
    public abstract int SanityShield { get; set; } 
}

public abstract class AmmoBar : StateBar
{

}


// ================================================================= Health Bar ===========================================================================================================
public class Health : HealthBar
{
    [SerializeField, Range(0, 50)] int _State = 0;
    public override int State { get { return _State; } }
    [SerializeField, Range(0, 50)] int _Max = 0;
    public override int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _ArmorMelee;
    public override int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange;
    public override int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }

    public override void GetDamage(Attack attack)
    {
        switch(attack.damageType)
        {
            case DamageType.Pure: _State -= attack.damage; break;
            case DamageType.Melee: _State -= attack.damage - ArmorMelee; break;
            case DamageType.Range: _State -= attack.damage - ArmorRange; break;
            case DamageType.Rezo: _State -= attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f); break;
            case DamageType.Terra: _State -= attack.damage / 4; break;
 
            case DamageType.Heal: _State = Mathf.Clamp(State + attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.2f), 0, Max); break;
            case DamageType.MetalHeal: _State -= 1; break;
        }
    }
    public Color BarColor{ get{ return new Color(1, 0, 0); } }
}
public class HealthOver : HealthBar
{
    [SerializeField, Range(0, 50)] int _State = 0;
    public override int State { get { return _State; } }
    [SerializeField, Range(0, 50)] int _Max = 0;
    public override int Max { get { return _Max; } set { _Max = value; } }
    [SerializeField] int _OverMax;
    public int OverMax { get { return _OverMax; } set { _OverMax = value; } }
    [Space]
    [SerializeField] int _ArmorMelee;
    public override int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange;
    public override int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }
    
    public HealthOver() { InGameEvents.StepEnd.AddListener( () => { if(_State > _Max){ _State--; Debug.Log("Health OverMax"); }} ); }
    // public HealthOver() { InGameEvents.AddListener(); }
    public override void GetDamage(Attack attack)
    {
        switch(attack.damageType)
        {
            case DamageType.Pure: _State -= attack.damage; break;
            case DamageType.Melee: _State -= attack.damage - ArmorMelee; break;
            case DamageType.Range: _State -= attack.damage - ArmorRange; break;
            case DamageType.Rezo: _State -= attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f); break;
            case DamageType.Terra: _State -= attack.damage / 4; break;
 
            case DamageType.Heal: _State = Mathf.Clamp(State + attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.2f), 0, Max + OverMax); break;
        }
    }
    public Color BarColor{ get{ return new Color(1, 0.1f, 0); } }
}
public class HealthCorpse : HealthBar
{
    [SerializeField, Range(0, 50)] int _State = 9;
    public override int State { get { return _State; } }
    [SerializeField, Range(0, 50)] int _Max = 9;
    public override int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _ArmorMelee = 2;
    public override int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange = 4;
    public override int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }


    private int CorpseTimer = 0;
    public HealthCorpse() { InGameEvents.StepEnd.AddListener(() => {
        if(CorpseTimer >= 4) { CorpseTimer = 0; _State -= 1; return; }
        CorpseTimer++;
        }); }

    public override void GetDamage(Attack attack)
    {
        switch(attack.damageType)
        {
            case DamageType.Pure: _State -= attack.damage; break;
            case DamageType.Melee: _State -= attack.damage - ArmorMelee; break;
            case DamageType.Range: _State -= attack.damage - ArmorRange; break;
            case DamageType.Rezo: _State -= attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f); break;
            case DamageType.Terra: _State -= attack.damage / 4; break;
 
            case DamageType.Heal: _State = Mathf.Clamp(State + attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.2f), 0, Max); break;
            case DamageType.MetalHeal: _State -= 1; break;
        }
    }
    public Color BarColor{ get{ return new Color(0, 0, 0); } }
}
// ================================================================= Stamina Bar ===========================================================================================================
public class Stamina : StaminaBar
{
    [SerializeField, Range(0, 50)] int _State = 0;
    public override int State { get { return _State; } }
    [SerializeField, Range(0, 50)] int _Max = 0;
    public override int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _RestEffect = 0;
    public override int RestEffectivity { get{ return _RestEffect; } set { _RestEffect = value; } }
    [Space]
    [SerializeField] int _WalkUseStamina;
    public override int WalkUseStamina { get { return _WalkUseStamina; } set { _WalkUseStamina = value; } }

    public override void Rest() { _State = Mathf.Clamp(_State + RestEffectivity, 0, Max); }

    public override void GetTired(int value){ _State = Mathf.Clamp(State - value, 0, Max); }


    public Color BarColor{ get{ return new Color(1, 0, 0); } }

}
// ================================================================= Sanity Bar ============================================================================================================
public class Sanity : SanityBar
{
    [SerializeField, Range(0, 50)] int _State = 0;
    public override int State { get { return _State; } }
    [SerializeField, Range(0, 50)] int _Max = 0;
    public override int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _SanityShield = 0;
    public override int SanityShield { get { return _SanityShield; } set { _SanityShield = value; } }


    public Color BarColor{ get{ return new Color(0.7f, 0, 0.7f); } }
}