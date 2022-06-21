using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using SagardCL;


public abstract class StateBar
{
    Color BarColor{ get; }

    int State;
    int Max;  
}
public interface HealthBar
{
    int ArmorMelee { get; set; } 
    int ArmorRange {get; set; }

    public void GetDamage(Attack attack);
}
public interface StaminaBar
{
    int RestEffectivity{ get; set; }
    void Rest();
}
public interface SanityBar
{
    
}

public interface AmmoBar
{

}


// ================================================================= Health Bar ===========================================================================================================

public class Health : StateBar, HealthBar
{
    [XmlArrayAttribute]
    public int State;
    public int Max;

    public int ArmorMelee{ get; set; }
    public int ArmorRange{ get; set; }

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
    [XmlArrayAttribute]
    public int State;
    public int Max;
    public int OverMax = 3;

    public int ArmorMelee{ get; set; }
    public int ArmorRange{ get; set; }

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