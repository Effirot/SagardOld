using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParameterList : MonoBehaviour
{

    
    public string ClassTAG;

    public int WalkDistance = 2, MaxStamina = 3, MaxHP, MaxSanity;

    public int Stamina = 3, HP, Sanity;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void recreation()
    {
        Stamina = MaxStamina;
    }
    public void Walk()
    {

    }
}
