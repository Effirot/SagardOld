using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System;

[CreateAssetMenu(fileName = "Item", menuName = "SagardCL objects/Item", order = 51)]
public class Item : Descript, Sendable
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
    public bool CanTakeOff = true;
    public bool DestroyOnDeath = false;
    public bool Artifacers = false;
    public bool UseInCrafts = true;
    [Space]
    public int WalkDistance = 0;

    
    [SerializeReference]public IHealthBar Health = new Health();
    public bool ReplaceHealthBar = false;
    [SerializeReference]public IStaminaBar Stamina = new Stamina();
    public bool ReplaceStaminaBar = false;
    [SerializeReference]public ISanityBar Sanity = new Sanity();
    public bool ReplaceSanityBar = false;

    
    public IStateBar OtherStates;

    public List<Effect> Resists = new List<Effect>();
    [Space]
    public List<Skill> AdditionSkills = new List<Skill>();

    public static Item CompoundParameters(List<Item> items) 
    {
        foreach(Item item in items)
        {
            
        }
        return new Item(); 
    }
}





