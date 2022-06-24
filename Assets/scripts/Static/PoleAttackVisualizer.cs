using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using UnityEngine.Events;

public static class PoleAttackVisualizer
{
    public static GameObject Visual = GameObject.Find("AttackVisualizer") ?? new GameObject("Attack is here, you forgot to put AttackVisualizer.prefab to scene. Idiot");
    public static List<Attack>[,] AttackList = new List<Attack>[1000, 1000];
    
    // private static void ObjectColor()
    // {

    //     {
    //         if(attack == null | attack == new List<Attack>()) continue;

    //         List<Color> colors = new List<Color>();

    //         var obj = Instantiate(Visual, new Checkers(attack[0].Where), Visual.transform.rotation);
    //         foreach(Attack ListedAttack in attack)
    //         {
    //             switch(ListedAttack.damageType)
    //             {
    //                 default: colors.Add(new Color(ListedAttack.damage * 0.09f, ListedAttack.damage * 0.01f, ListedAttack.damage * 0.01f)); break;
    //                 // case DamageType.Melee: break; 
    //                 // case DamageType.Range: break;

    //                 case DamageType.Pure: colors.Add(new Color(ListedAttack.damage * 0.09f, 0f, ListedAttack.damage * 0.09f));  break;

    //                 case DamageType.Heal: colors.Add(new Color(0, ListedAttack.damage * 0.07f, ListedAttack.damage * 0.04f)); break;
    //             }
    //         }
    //         obj.GetComponent<SpriteRenderer>().color = AttacksColor();

    //         Color AttacksColor()
    //         {
    //             Color col = colors[0];
    //             foreach(var color in colors)
    //                 col += color;
    //             return col;
    //         }
    //     }
    // }
}