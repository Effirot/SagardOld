using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using SagardCL;
using SagardCL.ParameterManipulate;
using System;
using UnityEngine.Events;

// ================================================================= Health Bar ===========================================================================================================
[Serializable]public struct Health : IHealthBar
{
    [field: Header("Health")]
    [field: SerializeField, Range(-10, 35)] public int Value { get; set; }
    [field: SerializeField, Range(-10, 35)] public int Max { get; set; }
    [field: SerializeField, Range(0, 10)] public int OverMax { get; set; }
    [field: Space]
    [field: SerializeField] public int ArmorMelee{ get; set; }
    [field: SerializeField] public int ArmorRange{ get; set; }

    [field: SerializeField] public float Immunity{ get; set; }

    public Color BarColor{ get{ return new Color(1, 0, 0); } }
}
[Serializable]public struct HealthOver : IHealthBar, IStepEndUpdate
{
    [field: Header("Metal")]
    [field: SerializeField, Range(-10, 35)] public int Value { get; set; }
    [field: SerializeField, Range(-10, 35)] public int Max { get; set; }
    [field: SerializeField, Range(0, 10)] public int OverMax { get; set; }
    [field: Space]
    [field: SerializeField] public int ArmorMelee{ get; set; }
    [field: SerializeField] public int ArmorRange{ get; set; }

    [field: SerializeField] public float Immunity{ get; set; }

    public void StepEnd()
    {
        if(this.Value > this.Max) this.Value = Mathf.Clamp(Value - 1, 0, Max + OverMax); 
    }

    public Color BarColor{ get{ return new Color(1, 0.1f, 0); } }
}
[Serializable]public struct HealthCorpse : IHealthBar, IStepEndUpdate
{
    [field: Header("Metal")]
    [field: SerializeField, Range(-10, 35)] public int Value { get; set; }
    [field: SerializeField, Range(-10, 35)] public int Max { get; set; }
    [field: Space]
    [field: SerializeField] public int ArmorMelee{ get; set; }
    [field: SerializeField] public int ArmorRange{ get; set; }

    [field: SerializeField] public float Immunity{ get; set; }

    [field: SerializeField, Range(0, 5)] private int CorpseTimer { get; set; }
    public void StepEnd()
    {
        if(CorpseTimer <= 1) { Value -= 1; CorpseTimer = 3; return; }
        CorpseTimer -= 1;
    }

    public Color BarColor{ get{ return new Color(0, 0, 0); } }
}
[Serializable]public struct Metal : IHealthBar
{
    [field: Header("Metal")]
    [field: SerializeField, Range(-10, 35)] public int Value { get; set; }
    [field: SerializeField, Range(-10, 35)] public int Max { get; set; }
    [field: Space]
    [field: SerializeField] public int ArmorMelee{ get; set; }
    [field: SerializeField] public int ArmorRange{ get; set; }

    [field: SerializeField] public float Immunity{ get; set; }

    int IHealthBar.Repair(Attack attack) { return (int)Mathf.Clamp(Value + attack.Damage, 0, Max - Value); }
    int IHealthBar.Heal(Attack attack) { return -2; }

    public Color BarColor{ get{ return new Color(1, 0.7f, 0); } }
}
// ================================================================= Stamina Bar ===========================================================================================================
[Serializable]public struct Stamina : IStaminaBar
{
    [field: Header("Stamina Bar")]
    [field: SerializeField, Range(-10, 35)] public int Value { get; set;}
    [field: SerializeField, Range(-10, 35)] public int Max { get; set; }
    [field: Space]
    [field: SerializeField] public int RestEffectivity { get; set; }
    [field: Space]
    [field: SerializeField] public int WalkUseStamina { get; set; }

    public void Rest() { Value = Mathf.Clamp(Value + RestEffectivity, 0, Max); }
    public void GetTired(int value){ Value = Mathf.Clamp(Value - value, 0, Max); }

    public Color BarColor{ get{ return new Color(0.8f, 1f, 0); } }
}
// ================================================================= Sanity Bar ============================================================================================================
[Serializable]public struct Sanity : ISanityBar
{
    [field: Header("Sanity Bar")]
    [field: SerializeField, Range(-10, 35)] public int Value { get; set;}
    [field: SerializeField, Range(-10, 35)] public int Max { get; set; }
    [field: Space]
    [field: SerializeField] public int SanityShield { get; set; }

    public Color BarColor{ get{ return new Color(0.3f, 0, 0.3f); } }
}
// ================================================================= Other Bars ============================================================================================================
