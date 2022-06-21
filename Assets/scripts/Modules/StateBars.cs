using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using SagardCL;
using System;

public interface StateBar
{
    Color BarColor{ get; }

    int State { get; set; }
    int Max { get; set; }  
}

public interface HealthBar : StateBar
{
    int ArmorMelee { get; set; } 
    int ArmorRange {get; set; }

    void GetDamage(Attack attack);
}
public interface StaminaBar : StateBar
{
    int RestEffectivity{ get; set; }
    void Rest();
}
public interface SanityBar : StateBar
{
    int SanityShield { get; set; } 
}

public interface AmmoBar : StateBar
{

}


// ================================================================= Health Bar ===========================================================================================================
public class Health : HealthBar
{
    [SerializeField] int _State = 0;
    public int State { get { return _State; } set { _State = value; } }
    [SerializeField] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _ArmorMelee;
    public int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange;
    public int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }

    public void GetDamage(Attack attack)
    {
        switch(attack.damageType)
        {
            case DamageType.Pure: State -= attack.damage; break;
            case DamageType.Melee: State -= attack.damage - ArmorMelee; break;
            case DamageType.Range: State -= attack.damage - ArmorRange; break;
            case DamageType.Rezo: State -= attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f); break;
            case DamageType.Terra: State -= attack.damage / 4; break;
 
            case DamageType.Heal: State = Mathf.Clamp(State + attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.2f), 0, Max); break;
        }
    }
    public Color BarColor{ get{ return new Color(1, 0, 0); } }
}
public class HealthOver : HealthBar
{
    [SerializeField] int _State = 0;
    public int State { get { return _State; } set { _State = value; } }
    [SerializeField] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [SerializeField] int _OverMax;
    public int OverMax { get { return _OverMax; } set { _OverMax = value; } }
    [Space]
    [SerializeField] int _ArmorMelee;
    public int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange;
    public int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }
    
    // public HealthOver() { InGameEvents.AddListener(); }
    public void GetDamage(Attack attack)
    {
        switch(attack.damageType)
        {
            case DamageType.Pure: State -= attack.damage; break;
            case DamageType.Melee: State -= attack.damage - ArmorMelee; break;
            case DamageType.Range: State -= attack.damage - ArmorRange; break;
            case DamageType.Rezo: State -= attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f); break;
            case DamageType.Terra: State -= attack.damage / 4; break;
 
            case DamageType.Heal: State = Mathf.Clamp(State + attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.2f), 0, Max + OverMax); break;
        }
    }
    public Color BarColor{ get{ return new Color(1, 0, 0); } }
}
// ================================================================= Stamina Bar ===========================================================================================================
public class Stamina : StaminaBar
{
    [SerializeField] int _State = 0;
    public int State { get { return _State; } set { _State = value; } }
    [SerializeField] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _RestEffect = 0;
    public int RestEffectivity { get{ return _RestEffect; } set { _RestEffect = value; } }
    [Space]
    [SerializeField] int _WalkUseStamina;
    public int WalkUseStamina { get { return _WalkUseStamina; } set { _WalkUseStamina = value; } }

    public void Rest() { }


    public Color BarColor{ get{ return new Color(1, 0, 0); } }

}
// ================================================================= Sanity Bar ============================================================================================================
public class Sanity : SanityBar
{
    [SerializeField] int _State = 0;
    public int State { get { return _State; } set { _State = value; } }
    [SerializeField] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _SanityShield = 0;
    public int SanityShield { get { return _SanityShield; } set { _SanityShield = value; } }


    public Color BarColor{ get{ return new Color(0.7f, 0, 0.7f); } }
}