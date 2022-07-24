using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Reflection;
using System;

public abstract class Effect
{
    public string Name;
    public Sprite Icon;
    public string Description;

    bool Permanent = false;
    int Timer = 0;
    bool Hidden = false;

    CharacterCore Target;
    

    
    public void GetMethod(string name) { MethodInfo method = this.GetType().GetMethod(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public); } 

    public BalanceChanger Stats;

    public abstract void CombineDuplicates(Effect a, Effect b);

    // void StepEndUpdate() { }
    // Attack DamageEffect(Attack attack) { return null; }
    // void LostHealth() { }
}
