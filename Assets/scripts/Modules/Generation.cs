using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generation : MonoBehaviour
{
    protected class PlatformVisual
    {
        static GameObject platform;
        static Mesh[] MeshVariants;

        public PlatformVisual(GameObject Platform) { platform = Platform; MeshVariants = null; }
        public PlatformVisual(GameObject Platform, Mesh Mesh) { platform = Platform; MeshVariants = new Mesh[] { Mesh }; }
        public PlatformVisual(GameObject Platform, Mesh[] Meshes) { platform = Platform; MeshVariants = Meshes; }

        public GameObject Platform { get{ return platform; } }
        public Mesh[] Meshes { get{ return MeshVariants; } }
    }

    protected void Letsgenerate(Map map, PlatformVisual[] Platform){
        ClearMap();
        for(int x = 0; x < map.XScale; x++)
        {
            for(int z = 0; z < map.ZScale; z++)
            {
                PlatformVisual NowPlatform = Platform[map.GetModifier(x, z)];
                if(map.GetLet(x, z) >= 0){
                    GameObject obj = Instantiate(
                    NowPlatform.Platform, 
                    new Vector3(x, map.GetUp(x, z), z), Quaternion.Euler(new Vector3(0, Random.Range(0, 3) * 90, 0)), transform);

                    obj.tag = "Map";
                    if(NowPlatform.Meshes!= null) obj.GetComponent<MeshFilter>().mesh = NowPlatform.Meshes[Random.Range(0, NowPlatform.Meshes.Length)];
                }
            }
        }
    }
    private void ClearMap(){
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Map"))
        {   
            Destroy(obj);
        }
    }
}

[System.Serializable]
public class Map
{
    private class MapCell
    {
        public int Modifier;
        public float Up;
        public int Let = 0;
        
        public MapCell(int Mod, float up, int let = 0)
        { Modifier = Mod; Up = up; Let = let; }
    }

    static Mesh map;
    [SerializeField] int scaleX, scaleZ;
    [SerializeField] static uint key;

    MapCell[,] PlatformMatrix = new MapCell[,] {};
    public delegate float MapFormula(int x, int y, uint key);

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
        PlatformMatrix = GenerateRelief((a, b, c) => 0);
    }
    
    
    public int XScale { get{ return scaleX; } }
    public int ZScale { get{ return scaleZ; } }

    private MapCell[,] GenerateRelief(MapFormula _formula)
    {
        MapCell[,] result = new MapCell[scaleX, scaleZ];
        for(int x = 0; x < scaleX; x++)
        {
            for(int z = 0; z < scaleZ; z++)
            {   
                result[x, z] = new MapCell(0, _formula(x, z, key));
            }
        }
        return result;
    }

    public int GetModifier(int x, int z) { return PlatformMatrix[x, z].Modifier; }
    public float GetUp(int x, int z) { return PlatformMatrix[x, z].Up; }
    public int GetLet(int x, int z) { return PlatformMatrix[x, z].Let; }
}