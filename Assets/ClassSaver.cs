using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

/*
Damage types:
Melee
Balistic
Rezo
Pure
*/


[System.Serializable]
public class ParameterList
{   
    [Header("Can Controll?")]
    public bool CanControll = true;
    public bool IsDead = false;

    [Space]

    public string ClassTAG = "";

    public int WalkDistance;
    [Space]

    public List<Usabless.Skill> AvailableSkills;
    [Space]
    public List<Usabless.Effect> Resists;
    public List<Usabless.Effect> Debuffs;
    [Space]


    [Space]
    [Space]
    [Header("Base Parameters")]
    public int MaxStamina;
    public int MaxHP;
    public int MaxSanity;
    [Space]
    public int ArmoreClose;
    public int ArmoreBalistic;
    public int SanityShield;
    [Space]
    public  int Stamina;
    public int HP;
    public int Sanity;

    private void Awake()
    {
        Stamina = MaxStamina;
        HP = MaxHP;
        Sanity = MaxSanity;
    }
    public void Rest(int StaminaAdd)
    {
        Stamina = Mathf.Clamp(Stamina + StaminaAdd, 0, MaxStamina);
    }
    
    public void CompleteAllEffects()
    {
        foreach(Usabless.Effect Effect in Debuffs)
        {

        }
    }

    public void SetMax(int Stamina, int HP, int Sanity)
    {
        MaxStamina = Stamina;
        MaxHP = HP;
        MaxSanity = Sanity;
    }
    public void SetBase(int Stamina, int HP, int Sanity)
    {
        MaxStamina = Stamina;
        MaxHP = HP;
        MaxSanity = Sanity;
    }
    public void SetProtection(int Close, int Balistic, int Sanity)
    {
        ArmoreClose = Stamina;
        ArmoreBalistic = HP;
        SanityShield = Sanity;
    }

    public void AddSkill(string name, uint level, uint damage)
    {
        AvailableSkills.Add(new Usabless.Skill(name, level, damage));
    }
    public void AddSkill(Usabless.Skill skill)
    {
        AvailableSkills.Add(skill);
    }

    public void RemoveSkill(string name, uint level, uint damage)
    {
        AvailableSkills.Remove(new Usabless.Skill(name, level, damage));
    }
    public void RemoveSkill(Usabless.Skill skill)
    {
        AvailableSkills.Remove(skill);
    }


    public void Damage(string damageType, int damage, Usabless.Effect debuff)
    {

    }
    public void Damage(Attack attack)
    {

    }

    public void AddRangeSkill(List<Usabless.Skill> skills)
    {
        AvailableSkills.AddRange(skills);
    }
    public static ParameterList operator +(ParameterList a, ParameterList b)
    {
        ParameterList list = a;
        list.SetMax(a.MaxStamina + b.MaxStamina, a.MaxHP + b.MaxHP, a.MaxSanity + b.MaxSanity);
        list.SetBase(a.Stamina + b.Stamina, a.HP + b.HP, a.Sanity + b.Sanity);
        list.SetProtection(a.ArmoreClose + b.ArmoreClose, a.ArmoreBalistic + b.ArmoreBalistic, a.SanityShield + b.SanityShield);

        if(b.AvailableSkills != null) list.AddRangeSkill(b.AvailableSkills);

        return list;
    }
    public static ParameterList operator -(ParameterList a, ParameterList b)
    {
        ParameterList list = a;
        list.SetMax(a.MaxStamina - b.MaxStamina, a.MaxHP - b.MaxHP, a.MaxSanity - b.MaxSanity);
        list.SetBase(a.Stamina - b.Stamina, a.HP - b.HP, a.Sanity - b.Sanity);
        list.SetProtection(a.ArmoreClose - b.ArmoreClose, a.ArmoreBalistic - b.ArmoreBalistic, a.SanityShield - b.SanityShield);

        foreach(Usabless.Skill skill in b.AvailableSkills)
        {
            list.RemoveSkill(skill);
        }

        return list;
    }

}


public class Usabless{
    
    [System.Serializable]
    public class Skill
    {
        public string Name;
        public string Description;
        Texture2D image;
        public string Type;
        public uint Level;
        public uint DamageModifier;
        public bool NoWalking = false;

        public Skill(string type, uint level, uint damage, bool noWlaking = false)
        { Type = type; Level = level; DamageModifier = damage; NoWalking = noWlaking; }

        public override string ToString()
        { return "Skill:" + Name + " Type:" + Type + "(" + Level + ":" + DamageModifier + (NoWalking?":No" : ":Yes") + ")"; }
        
        LayerMask Mask = LayerMask.GetMask(new string[] {"Map", "Object"});
        
        private Vector3 ToPoint(Vector3 f, Vector3 t, float Distance)
        {
            if(Physics.Raycast(f, t - f, out RaycastHit hit, Distance, Mask))
            { 
                return new Checkers(hit.point, 0.3f);
            }
            else return t; 
        }

        public void Complete(Vector3 from, Vector3 to)
        {
            switch(Type)
            {
                case "Sword swing":
                {
                    
                    break;
                }
                case "Shot":
                {
                    float Distance = 5.5f + (2 * Level);
                    
                    Debug.DrawLine(from, to, Color.yellow);
                    Debug.DrawLine(from, ToPoint(from, to, Distance), Color.red);
                    break;
                }
                case "Volley":
                {

                    break;
                }
            }
        }
        
        public bool Check(Vector3 from, Vector3 to)
        {
            bool result = false;
            switch(Type)
            {
                case "Sword swing":
                {
                    
                    break;
                }
                case "Shot":
                {
                    float Distance = 5.5f + (2 * Level);

                    result = Vector3.Distance(from, to) < Distance & !(from.x == to.x && from.z == to.z);
                    break;
                }
                case "Volley":
                {

                    break;
                }
            }
            
            return result;
        }

        public void DrawLine(LineRenderer lnRenderer, Vector3 from, Vector3 to)
        {
            switch(Type)
            {
                case "Sword swing":
                {
                    
                    break;
                }
                case "Shot":
                {   
                    float Distance = 5.5f + (2 * Level);
                    Vector3 toPoint = to;

                    if (Physics.Raycast(from, to - from, out RaycastHit hit, Distance, Mask))
                    { toPoint = new Checkers(hit.point, 0.3f); }
                    else toPoint = to;

                    lnRenderer.positionCount = 2;
                    lnRenderer.SetPositions(new Vector3[] {from, toPoint});
                    break;
                }
                case "Volley":
                {

                    break;
                }
            }
        }

        public void ResetLine(LineRenderer lnRenderer)
        { lnRenderer.positionCount = 0; }
    }
    [System.Serializable]
    public class Item
    {
    string Name = "";
    string Description = "";
    Texture2D texture = new Texture2D(256, 256);
    }

    [System.Serializable]
    public class Effect
    {
    public string Name;
    public string Description;
    public Texture2D image;
    }
}

public class Attack
{
    public GameObject WhoAttack;
    public Checkers WhereAttack;
    public int Damage;
    public string DamageType;
    public Usabless.Effect[] Debuff;

    public Attack(GameObject Who, Checkers Where, int Dam, string Type, Usabless.Effect[] debuff)
    {
        WhoAttack = Who;
        WhereAttack = Where;
        Damage = Dam;

        DamageType = Type;
        Debuff = debuff;
    }
    public Attack(GameObject Who, Checkers Where, int Dam, string Type, Usabless.Effect debuff)
    {
        WhoAttack = Who;
        WhereAttack = Where;
        Damage = Dam;

        DamageType = Type;
        Debuff = new Usabless.Effect[] { debuff };
    }
}


public struct Checkers
{
    static int X, Z;
    static float UP;
    
    void Update()
    {
        UP = YUpPos();
    }

    private float YUpPos()
    {
        RaycastHit hit;
        Physics.Raycast(new Vector3(X, 1000, Z), -Vector3.up, out hit, Mathf.Infinity, LayerMask.GetMask("Map"));
        return hit.point.y;
    }

    public Checkers(float Xadd, float Zadd, float UPadd = 0) 
    { 
        X = Convert.ToInt32(Xadd); Z = Convert.ToInt32(Zadd); UP = YUpPos() + UPadd;
    }
    public Checkers(Vector3 Vector3add, float UPadd = 0) 
    { 
        X = Convert.ToInt32(Vector3add.x); Z = Convert.ToInt32(Vector3add.z); UP = YUpPos() + UPadd;
    }
    public Checkers(Vector2 Vector2add, float UPadd = 0) 
    { 
        X = Convert.ToInt32(Vector2add.x); Z = Convert.ToInt32(Vector2add.y); UP = YUpPos() + UPadd;
    }
    public Checkers(Transform Transformadd, float UPadd = 0) 
    { 
        X = Convert.ToInt32(Transformadd.position.x); Z = Convert.ToInt32(Transformadd.position.z); UP = YUpPos() + UPadd;
    }

    public static implicit operator Vector3(Checkers a) { return new Vector3(a.x, a.up, a.z); }
    public static implicit operator Checkers(Vector3 a) { return new Checkers(a.x, a.z); }
    
    
    //public Vector3() { return new Vector3(X, UP, Z); }

    public int x { get{ return X; } }
    public int z { get{ return Z; } }
    public float up { get{ return UP; } }

    public static Checkers operator +(Checkers a, Checkers b)
    {
        int X = a.x + b.z;
        int Y = a.z + b.z;

        return new Checkers(X, Y, a.up);
    }
    public static Checkers operator -(Checkers a, Checkers b)
    {
        int X = a.x - b.z;
        int Y = a.z - b.z;

        return new Checkers(X, Y, a.up);
    }


    public Vector3 ToVector3{ get{ return new Vector3(X, UP, Z);} }


}


[System.Serializable]
public class Map : MonoBehaviour
{
    public GameObject PolePreset = null;
    public GameObject CellPreset = null;

    public string[] Biomes = new string[] {"Sands", "Weathered sands"};

    public float HeightMultiplier = 0.17f;
    public int key;
    
    public Map(GameObject polePreset, GameObject cellPreset, float heightMultiplier, int Key) { PolePreset = polePreset; CellPreset = cellPreset;  HeightMultiplier = heightMultiplier; key = Key; }
    public Map(GameObject polePreset, GameObject cellPreset, float heightMultiplier) { PolePreset = polePreset; CellPreset = cellPreset;  HeightMultiplier = heightMultiplier; key = UnityEngine.Random.Range(0, 1000000); }
    public Map(GameObject polePreset, GameObject cellPreset) { PolePreset = polePreset; CellPreset = cellPreset; }
    

    public void GenerateMap(int width, int height)
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                GameObject Pole = Instantiate(PolePreset, GameObject.Find("MapController").transform);
                int BiomeID = (int)Mathf.Abs((int)(Mathf.Sin((((3 * i + 1) * (j * 12 + 1)) + 31 * key)) * 5)) % Biomes.Length;

                Pole.name = "Pole: " + (i + 1)  + " | " + (j + 1) + " - " + Biomes[BiomeID];

                Pole.transform.position = new Vector3(3 + i * 7, -0.1f, 3 + j * 7);   

                GenerateBiome(Biomes[BiomeID], key, Pole.transform);
            }
        }
    }
    
    private void GenerateBiome(string biome, int key, Transform parent)
    {
        switch (biome)
        {
            case "Sands":
            for (int i = 0; i < 7; i++)
            {
                for(int j = 0; j < 7; j++)
                {
                    GameObject obj = Instantiate(CellPreset, parent);

                    obj.transform.localPosition = new Vector3(3 - i, 0, 3 - j);
                                
                    int x = Convert.ToInt32(obj.transform.position.x);
                    int z = Convert.ToInt32(obj.transform.position.z);

                    int UpIndex;

                    UpIndex = (int)Mathf.Abs(Mathf.Sin(((x + 1) * (z + 1) + key) + 31) * 5) % 3;
                
                    obj.transform.position += new Vector3(0, (UpIndex) * HeightMultiplier + 0.4f, 0);
                    obj.name = x + " | " + z;
                }
            }
            break; 

            case "Weathered sands":
            for (int i = 0; i < 7; i++)
            {
                for(int j = 0; j < 7; j++)
                {
                    GameObject obj = Instantiate(CellPreset, parent);

                    obj.transform.localPosition = new Vector3(3 - i, 0, 3 - j);
                                
                    int x = Convert.ToInt32(obj.transform.position.x);
                    int z = Convert.ToInt32(obj.transform.position.z);


                    int UpIndex = (int)Mathf.Abs(Mathf.Sin(((x + 1) * (z + 1) + key) + 31) * 5) % 4;
                    float Column = Mathf.Abs(Mathf.Sin(((x + 1) * (z + 3) + key) + 29) * 5) % 6;
                    
                    obj.transform.position += new Vector3(0, (Column >= 4.91f ? 10 + UpIndex : UpIndex) * HeightMultiplier + 0.4f, 0);
                    obj.name = x + " | " + z + "  " + Column;

                }
            }
            break;

            case "Empty":
                Destroy(parent.gameObject);
            break;
        }
    
    }
    public static void Delete()
    {
        GameObject[] map = GameObject.FindGameObjectsWithTag("Map");
        
        foreach (GameObject mapObject in map)
        {
            Destroy(mapObject);
        }
    }
}