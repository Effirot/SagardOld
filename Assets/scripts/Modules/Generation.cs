using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;

public abstract class Generation : MonoBehaviour
{
    [SerializeField] GameObject AttackVisual;
    [System.Serializable]
    protected class PlatformVisual
    {
        [SerializeField]GameObject platform;
        [SerializeField]Mesh[] MeshVariants;
        [SerializeField]Material[] MaterialVariants;

        public PlatformVisual(GameObject Platform) { platform = Platform; MeshVariants = null; }
        public PlatformVisual(GameObject Platform, Mesh Mesh) { platform = Platform; MeshVariants = new Mesh[] { Mesh }; }
        public PlatformVisual(GameObject Platform, Mesh[] Meshes) { platform = Platform; MeshVariants = Meshes; }


        public GameObject GetPlatform()
        {
            GameObject result = platform;

            if(MeshVariants.Length != 0) result.GetComponent<MeshFilter>().mesh = MeshVariants[Random.Range(0, MeshVariants.Length)];
            if(MaterialVariants.Length != 0) result.GetComponent<Renderer>().material = MaterialVariants[Random.Range(0, MaterialVariants.Length)];

            return result;
        }
    }



    private static GameObject[,] AttackVisualsRealizers;
    private static List<Attack>[,] AllAttackZoneArchive;
    protected void LetsGenerate(Map map, PlatformVisual[] Platform){
        ClearMap();

        AttackVisualsRealizers = new GameObject[map.XScale, map.ZScale];
        AllAttackZoneArchive = new List<Attack>[map.XScale, map.ZScale];
        

        for(int x = 0; x < map.XScale - 1; x++)
        for(int z = 0; z < map.ZScale - 1; z++)
        {
            PlatformVisual NowPlatform = Platform[map.GetModifier(x, z)];
            if(map.GetLet(x, z) >= 0){
                GameObject obj = Instantiate(
                NowPlatform.GetPlatform(), 
                new Vector3(x, map.GetUp(x, z), z), 
                Quaternion.Euler(0, Random.Range(0, 3) * 90, 0), 
                transform);

                obj.tag = "Map";
            }

            AllAttackZoneArchive[x, z] = new List<Attack>();

            GameObject AttackGizmo = Instantiate<GameObject>(AttackVisual, new Checkers(x, z, 0.04f), AttackVisual.transform.rotation, transform);
            AttackVisualsRealizers[x, z] = AttackGizmo;
            
            AttackGizmo.SetActive(false);
        }
    }
    private void ClearMap(){
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Map"))
        {   
            Destroy(obj);
        }
    }

    public static void DrawAttack(List<Attack> AttackZone, UnitController sender)
    {
        foreach(List<Attack> attacks in AllAttackZoneArchive) { if(attacks != null) attacks.RemoveAll((a) => a.WhoAttack == sender); }
        foreach(Attack attack in AttackZone) {  try{ AllAttackZoneArchive[attack.Where.x, attack.Where.z].Add(attack); } catch { }  }

        CheckAllGizmos();
    }
    private static void CheckAllGizmos()
    {
        for(int x = 0; x < AllAttackZoneArchive.GetLength(0) - 1; x++)
        for(int z = 0; z < AllAttackZoneArchive.GetLength(1) - 1; z++)
        {
            List<Attack> attackList = AllAttackZoneArchive[x, z];
            if(attackList.Count == 0) { AttackVisualsRealizers[x, z].SetActive(false); continue; }

            AttackVisualsRealizers[x, z].SetActive(true);
            AttackVisualsRealizers[x, z].GetComponent<SpriteRenderer>().color = AttackPaints(attackList);
        }
        Color AttackPaints(List<Attack> attacks)
        {
            Color result = Color.black;
            foreach (Attack attack in attacks)
            {
                switch (attack.damageType)
                {
                    default: result += Color.HSVToRGB(0.01f, 1, attack.damage * 0.06f); break;
                    case DamageType.MetalHeal: goto case DamageType.Heal; 
                    case DamageType.Heal: result += Color.HSVToRGB(0.42f, 1, attack.damage * 0.06f); break;
                    case DamageType.Rezo: result += Color.HSVToRGB(67f / 360f, 1, attack.damage * 0.06f); break;
                    case DamageType.Pure: result += Color.HSVToRGB(274f / 360f, 1, attack.damage * 0.06f); break;
                    
                }
            }
            return result;
        }


    }





}

[System.Serializable]
public class Map
{
    public struct MapCell
    {
        public int Modifier;
        public float Up;
        public int Let;
        
        public MapCell(int Mod, float up, int let = 0)
        { Modifier = Mod; Up = Mathf.Clamp(up, 0, 4); Let = let; }
    }

    static Mesh map;
    [SerializeField] int scaleX, scaleZ;
    [SerializeField] static uint key;

    MapCell[,] PlatformMatrix = new MapCell[,] {};
    public delegate MapCell MapFormula(int x, int y, uint key);

    public Map(MapFormula Formula, uint Key, Vector2 Scale)
    {
        scaleX = (int)Scale.x; scaleZ = (int)Scale.y; key = Key; 
        PlatformMatrix = GenerateRelief(Formula);
    }
    public Map(MapFormula Formula, int PlayerNum, uint Key)
    {
        scaleX = PlayerNum * 9 + (((int)key / 23)%7); scaleZ = PlayerNum * 9 + (((int)key / 14)%7); 
        key = Key;
        PlatformMatrix = GenerateRelief(Formula);
    }
    public Map(MapFormula Formula, int PlayerNum)
    {
        scaleX = PlayerNum * 9 + (((int)key / 23)%7); scaleZ = PlayerNum * 9 + (((int)key / 14)%7); 
        key = (uint)Random.Range(0, 99999999);
        PlatformMatrix = GenerateRelief(Formula);
    }
    public Map(MapFormula Formula)
    {
        key = (uint)Random.Range(0, 99999999);
        scaleX = 2 * 9 + (((int)key / 23)%7); scaleZ = 2 * 9 + (((int)key / 14)%7); 
        PlatformMatrix = GenerateRelief(Formula);
    }
    public Map()
    {
        key = (uint)Random.Range(0, 99999999);
        scaleX = 2 * 9 + (((int)key / 23)%7); scaleZ = 2 * 9 + (((int)key / 14)%7); 
        PlatformMatrix = GenerateRelief((a, b, c) => new MapCell(0, 0));
    }
    
    
    public int XScale { get{ return scaleX; } }
    public int ZScale { get{ return scaleZ; } }

    protected MapCell[,] GenerateRelief(MapFormula _formula)
    {
        MapCell[,] result = new MapCell[scaleX, scaleZ];
        for(int x = 0; x < scaleX; x++)
        {
            for(int z = 0; z < scaleZ; z++)
            {   
                result[x, z] = new MapCell(_formula(x, z, key).Modifier, _formula(x, z, key).Up, _formula(x, z, key).Let);
            }
        }
        return result;
    }

    public int GetModifier(int x, int z) { return PlatformMatrix[x, z].Modifier; }
    public float GetUp(int x, int z) { return PlatformMatrix[x, z].Up; }
    public int GetLet(int x, int z) { return PlatformMatrix[x, z].Let; }
}