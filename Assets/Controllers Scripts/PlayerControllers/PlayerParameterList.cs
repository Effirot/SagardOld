using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerParameterList : MonoBehaviour
{

    private Transform Cursore;


    public string ClassTAG;

    public int WalkDistance, MaxStamina, MaxHP, MaxSanity, Armour;
    public int AttackStamina;

    public int Stamina, HP, Sanity;


    public string[] AvailableAbilities;
    public int[,] AttackMatrix; // For close attacks
    //0 - NoDamage, 1 - 1 Damage, 2 - 2 Damage ...//

    public int AttackRange; //For range attack
    


    private void ParametersAppointment(int[] Parameters, string[] Abilities)
    {
        WalkDistance = Parameters[0]; MaxStamina = Parameters[1]; MaxHP = Parameters[2]; Armour = Parameters[3];
        AttackStamina = Parameters[4];

        AvailableAbilities = Abilities;
    }
    public void CloseAttack()
    {



    }
    public void RangeAttack(string Ability)
    {
        


    }




    public void Updator()
    {
        switch (ClassTAG)
        {
            default:
                ParametersAppointment(
                new int[5] //Walk Distance, Max Stamina, Max HP, Armour, Attack Stamina
                    { 3, 3, 4, 1, 1 }, 
                new string[] //Available Abilities
                    { "Close attack" });

                AttackMatrix = new int[5, 6] {
                    {0, 0, 0, 0, 0, 0 },
                    {0, 0, 0, 0, 0, 0 },
                    {0, (0), 2, 1, 0, 0 },
                    {0, 0, 0, 0, 0, 0 },
                    {0, 0, 0, 0, 0, 0 }
                };

                break;
            case "Warior":


                break;

            case "Archer":


                break;
        }
    }







    void Start()
    {
        Cursore = GameObject.Find("3DCursore").transform;

        Updator();

    }

    public void recreation()
    {
        Stamina = MaxStamina;
    }
    public void Walk()
    {

    }


    public void AbilitieComplete()
    {
        switch (AvailableAbilities[0])
        {
            case "Close attack":

                //transform.eulerAngles =  Vector3.MoveTowards(transform.eulerAngles, new Vector3(0, RotateIn4() * 90, 0), 0.9f);
                transform.eulerAngles = new Vector3(0, RotateIn4() * -90 - 90, 0);

                break;
            case "Range attack":

                break;


        }

    }

    private int RotateIn4()
    {
        float a = Vector3.Angle(new Vector3(transform.position.x, 0, transform.position.z + 200), new Vector3(Cursore.position.x, 0, Cursore.position.z));
        float b = Vector3.Angle(new Vector3(transform.position.x + 200, 0, transform.position.z), new Vector3(Cursore.position.x, 0, Cursore.position.z));
        return Convert.ToInt32(a + 45) / 90 + 1 + ((b >= 45) ? 0 : 2);
    }
}
