using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using SagardCL;
using System;
using UnityEngine.Events;

// ================================================================= Health Bar ===========================================================================================================
public class Health : IHealthBar
{
    [Header("Health")]
    [SerializeField, Range(0, 50)] int _Value = 0;
    public int Value { get { return _Value; } set { _Value = value; } }
    [SerializeField, Range(0, 50)] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _ArmorMelee;
    public int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange;
    public int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }

    public void Update() { }
    public bool Updatable { get { return _Updatable; } set { _Updatable = value; } }
    [SerializeField] bool _Updatable;

    public object Clone() { return this.MemberwiseClone(); }

    public void GetDamage(Attack attack)
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
public class HealthOver : IHealthBar
{
    public HealthOver() { IStepEndUpdate.StateList.AddListener(Update); }
    [Header("Over Max Health")]
    [SerializeField, Range(0, 50)] int _Value = 0;
    public int Value { get { return _Value; } set { _Value = value; }}
    [SerializeField, Range(0, 50)] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [SerializeField] int _OverMax;
    public int OverMax { get { return _OverMax; } set { _OverMax = value; } }
    [Space]
    [SerializeField] int _ArmorMelee;
    public int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange;
    public int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }

    public void Update()
    {
        if(Value > Max) this._Value -= 1;
    }
    public bool Updatable { get { return _Updatable; } set { _Updatable = value; } }
    [SerializeField] bool _Updatable;

    public object Clone() { object clone = this.MemberwiseClone(); IStepEndUpdate.StateList.AddListener(((HealthOver)clone).Update); return clone; }

    public void GetDamage(Attack attack)
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
public class HealthCorpse : IHealthBar
{
    public HealthCorpse() { IStepEndUpdate.StateList.AddListener(Update); Value = Max; }
    [Header("Corpse Health")]
    [SerializeField, Range(0, 50)] int _Value = 9;
    public int Value { get { return _Value; } set { _Value = value; }}
    [SerializeField, Range(0, 50)] int _Max = 9;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _ArmorMelee = 2;
    public int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange = 4;
    public int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }

    private int CorpseTimer = 3;
    public void Update()
    {
        if(CorpseTimer <= 1) { _Value -= 1; CorpseTimer = 3; return; }
        CorpseTimer -= 1;
    }
    public bool Updatable { get { return _Updatable; } set { _Updatable = value; } }
    [SerializeField] bool _Updatable;

    public object Clone() { return this.MemberwiseClone(); }

    public void GetDamage(Attack attack)
    {
        switch(attack.damageType)
        {
            case DamageType.Pure: _Value -= (attack.damage / 2); break;
            case DamageType.Melee: _Value -= (attack.damage / 2) - ArmorMelee; break;
            case DamageType.Range: _Value -= (attack.damage / 2) - ArmorRange; break;
            case DamageType.Rezo: _Value -= (attack.damage / 2) - (int)Mathf.Round((ArmorRange + ArmorMelee) * 1.25f); break;
            case DamageType.Terra: _Value -= attack.damage * 2; break;
 
            case DamageType.Heal: CorpseTimer += attack.damage; break;
            case DamageType.MetalHeal: _Value -= 3; break;
        }
    }
    public Color BarColor{ get{ return new Color(0, 0, 0); } }
}
public class Metal : IHealthBar
{
    [Header("Metal")]
    [SerializeField, Range(0, 50)] int _Value = 0;
    public int Value { get { return _Value; } set { _Value = value; } }
    [SerializeField, Range(0, 50)] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _ArmorMelee;
    public int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange;
    public int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }

    public void Update() { }
    public bool Updatable { get { return _Updatable; } set { _Updatable = value; } }
    [SerializeField] bool _Updatable;

    public object Clone() { return this.MemberwiseClone(); }

    public void GetDamage(Attack attack)
    {
        switch(attack.damageType)
        {
            case DamageType.Pure: _Value -= attack.damage; break;
            case DamageType.Melee: _Value -= attack.damage - ArmorMelee; break;
            case DamageType.Range: _Value -= attack.damage - ArmorRange; break;
            case DamageType.Rezo: _Value -= attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.75f); break;
            case DamageType.Terra: _Value -= attack.damage / 4; break;
 
            case DamageType.Heal: _Value -= attack.damage / 2; break;
            case DamageType.MetalHeal: _Value =  Mathf.Clamp(Value + attack.damage - (int)Mathf.Round((ArmorRange + ArmorMelee) * 0.2f), 0, Max); ; break;
        }
    }
    public Color BarColor{ get{ return new Color(1, 0, 0); } }
}
// ================================================================= Stamina Bar ===========================================================================================================
public class Stamina : IStaminaBar
{
    [Header("Stamina Bar")]
    [SerializeField, Range(0, 50)] int _Value = 0;
    public int Value { get { return _Value; } set { _Value = value; }}
    [SerializeField, Range(0, 50)] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _RestEffect = 0;
    public int RestEffectivity { get{ return _RestEffect; } set { _RestEffect = value; } }
    [Space]
    [SerializeField] int _WalkUseStamina;
    public int WalkUseStamina { get { return _WalkUseStamina; } set { _WalkUseStamina = value; } }

    public void Rest() { _Value = Mathf.Clamp(_Value + RestEffectivity, 0, Max); }
    public void GetTired(int value){ _Value = Mathf.Clamp(Value - value, 0, Max); }

    public void Update() { }
    public bool Updatable { get { return _Updatable; } set { _Updatable = value; } }
    [SerializeField] bool _Updatable;

    public object Clone() { return this.MemberwiseClone(); }

    public Color BarColor{ get{ return new Color(0.8f, 1f, 0); } }
}
// ================================================================= Sanity Bar ============================================================================================================
public class Sanity : ISanityBar
{
    [Header("Sanity Bar")]
    [SerializeField, Range(0, 50)] int _Value = 0;
    public int Value { get { return _Value; } set { _Value = value; }}
    [SerializeField, Range(0, 50)] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _SanityShield = 0;
    public int SanityShield { get { return _SanityShield; } set { _SanityShield = value; } }

    public void Update() { }
    public bool Updatable { get { return _Updatable; } set { _Updatable = value; } }
    [SerializeField] bool _Updatable;

    public object Clone() { return this.MemberwiseClone(); }

    public Color BarColor{ get{ return new Color(0.3f, 0, 0.3f); } }
}
// ================================================================= Other Bars ============================================================================================================
