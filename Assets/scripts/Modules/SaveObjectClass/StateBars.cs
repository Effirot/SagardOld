using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using SagardCL;
using System;

[Serializable]public abstract class StateBar
{
    Color BarColor{ get; }

    public abstract int Value { get; }
    public abstract int Max { get; set; }  

    public StateBar() { InGameEvents.StepEnd.AddListener(EveryStepEnd); }
    public virtual void EveryStepEnd() { }
}

[Serializable]public abstract class HealthBar : StateBar
{
    public abstract int ArmorMelee { get; set; } 
    public abstract int ArmorRange {get; set; }

    public abstract void GetDamage(Attack attack);

    public HealthBar() { Debug.Log("HealthBar"); InGameEvents.StepEnd.AddListener(EveryStepEnd); }
}
[Serializable]public abstract class StaminaBar : StateBar
{
    public abstract void GetTired(int value);
    public abstract int RestEffectivity{ get; set; }
    public abstract int WalkUseStamina{ get; set; }

    public abstract void Rest();
}
[Serializable]public abstract class SanityBar : StateBar
{
    public abstract int SanityShield { get; set; } 
}

[Serializable]public abstract class AmmoBar : StateBar
{

}


// ================================================================= Health Bar ===========================================================================================================
[Serializable]public class Health : HealthBar
{
    [SerializeField, Range(0, 50)] int _Value = 0;
    public override int Value { get { return _Value; } }
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
            case DamageType.Pure: _Value -= attack.damage; break;
            case DamageType.Melee: _Value -= attack.damage - ArmorMelee; break;
            case DamageType.Range: _Value -= attack.damage - ArmorRange; break;
            case DamageType.Rezo: _Value -= attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f); break;
            case DamageType.Terra: _Value -= attack.damage / 4; break;
 
            case DamageType.Heal: _Value = Mathf.Clamp(Value + attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.2f), 0, Max); break;
            case DamageType.MetalHeal: _Value -= 1; break;
        }
    }
    public Color BarColor{ get{ return new Color(1, 0, 0); } }
}
[Serializable]public class HealthOver : HealthBar
{
    [SerializeField, Range(0, 50)] int _Value = 0;
    public override int Value { get { return _Value; } }
    [SerializeField, Range(0, 50)] int _Max = 0;
    public override int Max { get { return _Max; } set { _Max = value; } }
    [SerializeField] int _OverMax;
    public int OverMax { get { return _OverMax; } set { _OverMax = value; } }
    [Space]
    [SerializeField] int _ArmorMelee;
    public override int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange;
    public override int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }

    public override void EveryStepEnd()
    {
        if(Value > Max) _Value -= 1;
    }

    public override void GetDamage(Attack attack)
    {
        switch(attack.damageType)
        {
            case DamageType.Pure: _Value -= attack.damage; break;
            case DamageType.Melee: _Value -= attack.damage - ArmorMelee; break;
            case DamageType.Range: _Value -= attack.damage - ArmorRange; break;
            case DamageType.Rezo: _Value -= attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f); break;
            case DamageType.Terra: _Value -= attack.damage / 4; break;
 
            case DamageType.Heal: _Value = Mathf.Clamp(Value + attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.2f), 0, Max + OverMax); break;
        }
    }
    public Color BarColor{ get{ return new Color(1, 0.1f, 0); } }
}
[Serializable]public class HealthCorpse : HealthBar
{
    [SerializeField, Range(0, 50)] int _Value = 9;
    public override int Value { get { return _Value; } }
    [SerializeField, Range(0, 50)] int _Max = 9;
    public override int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _ArmorMelee = 2;
    public override int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange = 4;
    public override int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }


    private int CorpseTimer = 0;

    public override void GetDamage(Attack attack)
    {
        switch(attack.damageType)
        {
            case DamageType.Pure: _Value -= attack.damage; break;
            case DamageType.Melee: _Value -= attack.damage - ArmorMelee; break;
            case DamageType.Range: _Value -= attack.damage - ArmorRange; break;
            case DamageType.Rezo: _Value -= attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f); break;
            case DamageType.Terra: _Value -= attack.damage / 4; break;
 
            case DamageType.Heal: _Value = Mathf.Clamp(Value + attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.2f), 0, Max); break;
            case DamageType.MetalHeal: _Value -= 1; break;
        }
    }
    public Color BarColor{ get{ return new Color(0, 0, 0); } }
}
// ================================================================= Stamina Bar ===========================================================================================================
[Serializable]public class Stamina : StaminaBar
{
    [SerializeField, Range(0, 50)] int _Value = 0;
    public override int Value { get { return _Value; } }
    [SerializeField, Range(0, 50)] int _Max = 0;
    public override int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _RestEffect = 0;
    public override int RestEffectivity { get{ return _RestEffect; } set { _RestEffect = value; } }
    [Space]
    [SerializeField] int _WalkUseStamina;
    public override int WalkUseStamina { get { return _WalkUseStamina; } set { _WalkUseStamina = value; } }

    public override void Rest() { _Value = Mathf.Clamp(_Value + RestEffectivity, 0, Max); }

    public override void GetTired(int value){ _Value = Mathf.Clamp(Value - value, 0, Max); }


    public Color BarColor{ get{ return new Color(1, 0, 0); } }

}
// ================================================================= Sanity Bar ============================================================================================================
[Serializable]public class Sanity : SanityBar
{
    [SerializeField, Range(0, 50)] int _Value = 0;
    public override int Value { get { return _Value; } }
    [SerializeField, Range(0, 50)] int _Max = 0;
    public override int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _SanityShield = 0;
    public override int SanityShield { get { return _SanityShield; } set { _SanityShield = value; } }


    public Color BarColor{ get{ return new Color(0.7f, 0, 0.7f); } }
}