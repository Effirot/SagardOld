using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerParameterList : MonoBehaviour
{
    
    private GameObject Cursore;
    private LineRenderer LnRend;
    private PlayerController Controller;
    private Skills Skills;


    public bool IsDead = false;


    public string ClassTAG;

    public int MaxStamina, MaxHP, MaxSanity;
    public int Stamina, HP, Sanity;


    public string[] AvailableAbilities;

    public int WalkDistance, Armour, AttackStamina;





    
    

    void Start()
    {
        Cursore = GameObject.Find("3DCursore");
        LnRend = GetComponent<LineRenderer>();

        Controller = GetComponent<PlayerController>();
        Skills = GetComponent<Skills>();

        Updator();
    }

    private void ParametersAppointment(int[] Parameters, string[] Abilities)
    {
        WalkDistance = Parameters[0]; MaxStamina = Parameters[1]; MaxHP = Parameters[2]; MaxSanity = Parameters[3]; Armour = Parameters[4];
        AttackStamina = Parameters[5];

        Stamina = Parameters[1]; HP = Parameters[2]; MaxSanity = Parameters[3];

        AvailableAbilities = Abilities;
    }


    public void Updator()
    {
        switch (ClassTAG)
        {
            default:
                ParametersAppointment(
                new int[6] //Walk Distance, Max Stamina, Max HP, Max Sanity, Armour, Attack Stamina
                    { 3, 3, 4, 5, 1, 1 }, 
                new string[] //Available Abilities
                    { "Range attack" });
                break;
            case "Warior":


                break;

            case "Archer":


                break;
        }
    }


    public void Rest()
    {
        Stamina = MaxStamina;
    }

    
    public void Walk()
    {

    }




}
