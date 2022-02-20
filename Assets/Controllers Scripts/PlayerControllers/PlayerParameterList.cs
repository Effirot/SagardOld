using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerParameterList : MonoBehaviour
{
    private PlayerController Controller;
    private Skills Skills;





    [Header("Is figure alive?")]
    public bool IsDead = false;

    [Header("Class of figure")]
    public string ClassTAG;
    
    [Header("Max Parameters")] 
    [Range(0, 100)] public int MaxStamina;
    [Range(0, 100)] public int MaxHP;
    [Range(0, 100)] public int MaxSanity;

    [Header("Current Parameters")]
    public int Stamina;
    public int HP;
    public int Sanity;
    [Header("Armore")]
    public int ArmoreClose;
    public int ArmoreBalistic;
    public int SanityShield;
    [Header("Other")]
    [Range(0, 100)] public int WalkDistance;

    [Header("Skills")]
    public string[] AvailableAbilities;


    
    
    

    void Start()
    {
        {
            Stamina = MaxStamina;
            HP = MaxHP;
            Sanity = MaxSanity;
        }


        Controller = GetComponent<PlayerController>();
        Skills = GetComponent<Skills>();
    }

    public void Rest()
    {
        Stamina = MaxStamina;
    }

    
    public void Walk()
    {

    }
}
