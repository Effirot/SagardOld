using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System;

[CreateAssetMenu(fileName = "Item", menuName = "SagardCL objects/Standard Item", order = 51)]
public class Item : Descript, Sendable
{
    public bool Artifacer = false;
    public ParamsChanger ThisItem;
    public static implicit operator ParamsChanger(Item item) { return item.ThisItem; }

    public static ParamsChanger CompoundParameters(List<ParamsChanger> items) 
    {
        var result = new ParamsChanger();
        var resists = new List<Effect>();
        var additionStates = new List<IOtherBar>();
        var additionSkills = new List<Skill>();
        foreach(ParamsChanger item in items)
        {
            result.WalkDistance += item.WalkDistance;

            result.Health.Max += item.Health.Max;
            result.Health.ArmorMelee += item.Health.ArmorMelee;
            result.Health.ArmorRange += item.Health.ArmorRange;

            result.Stamina.Max += item.Stamina.Max;
            result.Stamina.WalkUseStamina += item.Stamina.WalkUseStamina;
            result.Stamina.RestEffectivity += item.Stamina.RestEffectivity;
            
            result.Sanity.Max += item.Sanity.Max;
            result.Sanity.SanityShield += item.Sanity.SanityShield;

            resists.AddRange(item.Resists);
            additionStates.AddRange(item.AdditionState);
            additionSkills.AddRange(item.AdditionSkills);
        }
        result.Resists = resists;
        result.AdditionSkills = additionSkills;
        result.AdditionState = additionStates;

        return result; 
    }
    public static ParamsChanger CompoundParameters(List<Item> items) 
    {
        var result = new ParamsChanger();
        var resists = new List<Effect>();
        var additionStates = new List<IOtherBar>();
        var additionSkills = new List<Skill>();
        foreach(ParamsChanger item in items)
        {
            result.WalkDistance += item.WalkDistance;

            result.Health.Max += item.Health.Max;
            result.Health.ArmorMelee += item.Health.ArmorMelee;
            result.Health.ArmorRange += item.Health.ArmorRange;

            result.Stamina.Max += item.Stamina.Max;
            result.Stamina.WalkUseStamina += item.Stamina.WalkUseStamina;
            result.Stamina.RestEffectivity += item.Stamina.RestEffectivity;
            
            result.Sanity.Max += item.Sanity.Max;
            result.Sanity.SanityShield += item.Sanity.SanityShield;

            if(item.Resists != null) resists.AddRange(item.Resists);
            if(item.AdditionState != null) additionStates.AddRange(item.AdditionState);
            if(item.AdditionSkills != null) additionSkills.AddRange(item.AdditionSkills);
        }
        result.Resists = resists;
        result.AdditionSkills = additionSkills;
        result.AdditionState = additionStates;

        return result; 
    }
}

[System.Serializable] public class ParamsChanger
{
    public enum ItemQuality
    {
        Common,
        Crafted,
        Limited,
        Famous,
        InASingleCopy,
    }

    public ItemQuality Quality;
    public bool Throwable = true;
    public bool DestroyOnDeath = false;
    public bool UseInCrafts = true;
    [Space]
    public int WalkDistance = 0;

    [SerializeReference, SerializeReferenceButton]public IHealthBar Health = new Health();
    public bool ReplaceHealthBar = false;
    [SerializeReference, SerializeReferenceButton]public IStaminaBar Stamina = new Stamina();
    public bool ReplaceStaminaBar = false;
    [SerializeReference, SerializeReferenceButton]public ISanityBar Sanity = new Sanity();
    public bool ReplaceSanityBar = false;
    
    public List<IOtherBar> AdditionState;

    public List<Effect> Resists = new List<Effect>();
    [Space]
    public List<Skill> AdditionSkills = new List<Skill>();

    public void ThrowThis(Checkers position)
    {
        if(!Throwable) return;

    }
}


