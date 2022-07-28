using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using SagardCL;
using SagardCL.ParameterManipulate;
using System;
using UnityEngine.Events;

// ================================================================= Health Bar ===========================================================================================================
[Serializable]public class Health : IHealthBar
{
    [Header("Health")]
    [SerializeField, Range(-10, 35)] int _Value = 0;
    public int Value { get { return _Value; } set { _Value = value; } }
    [SerializeField, Range(-10, 35)] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _ArmorMelee;
    public int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange;
    public int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }

    [Range(0, 1), SerializeField] float _Immunity;
    public float Immunity{ get { return _Immunity; } set { _Immunity = Mathf.Clamp(value, 0, 1); } }

    public object Clone() { return this.MemberwiseClone(); }

    public Color BarColor{ get{ return new Color(1, 0, 0); } }
}
[Serializable]public class HealthOver : IHealthBar, IStepEndUpdate
{
    [Header("Over Max Health")]
    [SerializeField, Range(-10, 35)] int _Value = 0;
    public int Value { get { return _Value; } set { _Value = value; }}
    [SerializeField, Range(-10, 35)] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [SerializeField] int _OverMax;
    public int OverMax { get { return _OverMax; } set { _OverMax = value; } }
    [Space]
    [SerializeField] int _ArmorMelee;
    public int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange;
    public int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }

    [Range(0, 1), SerializeField] float _Immunity;
    public float Immunity{ get { return _Immunity; } set { _Immunity = Mathf.Clamp(value, 0, 1); } }

    public void StepEnd()
    {
        if(this.Value > this.Max) this.Value = Mathf.Clamp(Value - 1, 0, Max + OverMax); 
    }

    public object Clone() { return this.MemberwiseClone(); }

    public Color BarColor{ get{ return new Color(1, 0.1f, 0); } }
}
[Serializable]public class HealthCorpse : IHealthBar, IStepEndUpdate
{
    [Header("Corpse Health")]
    [SerializeField, Range(-10, 35)] int _Value = 9;
    public int Value { get { return _Value; } set { _Value = value; }}
    [SerializeField, Range(-10, 35)] int _Max = 9;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _ArmorMelee = 2;
    public int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange = 4;
    public int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }

    public float Immunity{ get { return 1; } set { } }

    private int CorpseTimer = 3;
    public void StepEnd()
    {
        if(CorpseTimer <= 1) { _Value -= 1; CorpseTimer = 3; return; }
        CorpseTimer -= 1;
    }

    public object Clone() { return this.MemberwiseClone(); }

    public Color BarColor{ get{ return new Color(0, 0, 0); } }
}
[Serializable]public class Metal : IHealthBar
{
    [Header("Metal")]
    [SerializeField, Range(-10, 35)] int _Value = 0;
    public int Value { get { return _Value; } set { _Value = value; } }
    [SerializeField, Range(-10, 35)] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _ArmorMelee;
    public int ArmorMelee{ get { return _ArmorMelee; } set { _ArmorMelee = value; } }
    [SerializeField] int _ArmorRange;
    public int ArmorRange{ get { return _ArmorRange; } set { _ArmorRange = value; } }

    [SerializeField] float _Immunity;
    public float Immunity{ get { return _Immunity; } set { _Immunity = value; } }

    public object Clone() { return this.MemberwiseClone(); }

    int IHealthBar.Repair(Attack attack) { return (int)Mathf.Clamp(Value + attack.Damage, 0, Max - Value); }
    int IHealthBar.Heal(Attack attack) { return -2; }

    public Color BarColor{ get{ return new Color(1, 0.7f, 0); } }
}
// ================================================================= Stamina Bar ===========================================================================================================
[Serializable]public class Stamina : IStaminaBar
{
    [Header("Stamina Bar")]
    [SerializeField, Range(-10, 35)] int _Value = 0;
    public int Value { get { return _Value; } set { _Value = value; }}
    [SerializeField, Range(-10, 35)] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _RestEffect = 0;
    public int RestEffectivity { get{ return _RestEffect; } set { _RestEffect = value; } }
    [Space]
    [SerializeField] int _WalkUseStamina;
    public int WalkUseStamina { get { return _WalkUseStamina; } set { _WalkUseStamina = value; } }

    public void Rest() { _Value = Mathf.Clamp(_Value + RestEffectivity, 0, Max); }
    public void GetTired(int value){ _Value = Mathf.Clamp(Value - value, 0, Max); }

    public object Clone() { return this.MemberwiseClone(); }

    public Color BarColor{ get{ return new Color(0.8f, 1f, 0); } }
}
// ================================================================= Sanity Bar ============================================================================================================
[Serializable]public class Sanity : ISanityBar
{
    [Header("Sanity Bar")]
    [SerializeField, Range(-10, 35)] int _Value = 0;
    public int Value { get { return _Value; } set { _Value = value; }}
    [SerializeField, Range(-10, 35)] int _Max = 0;
    public int Max { get { return _Max; } set { _Max = value; } }
    [Space]
    [SerializeField] int _SanityShield = 0;
    public int SanityShield { get { return _SanityShield; } set { _SanityShield = value; } }

    public object Clone() { return this.MemberwiseClone(); }

    public Color BarColor{ get{ return new Color(0.3f, 0, 0.3f); } }
}
// ================================================================= Other Bars ============================================================================================================
