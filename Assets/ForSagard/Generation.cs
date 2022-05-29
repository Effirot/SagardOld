using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generation : MonoBehaviour
{
    Map map = new Map((uint)Random.Range(0, 99991999), new Vector2(10, 10), MapType.Desert);

    class PlatformVisual
    {
        public GameObject platform;
        public Mesh[] MeshVariants;
    }

    void Letsgenerate(PlatformVisual[] Platform){
        for(int x = 0; x < map.XScale; x++)
        {
            for(int z = 0; z < map.ZScale; z++)
            {
                GameObject obj = Instantiate(
                    Platform[map.GetModifier(x, z)].platform, 
                    new Vector3(x, map.GetUp(x, z), z), Quaternion.Euler(0, Random.Range(0, 360), 0), transform);

                
            }
        }
    }
}

public enum MapType
{
    Desert = 1,
    WeatheredDesert = 2,
    SwampedDesert = 3,
    MagnetAnomaly = 4
}

[System.Serializable]
public class Map
{
    private class MapCell
    {
        public int Modifier;
        public float Up;
        public bool Let = false;
        
        public MapCell(int Mod, float up, bool let = false)
        { Modifier = Mod; Up = up; Let = let; }
    }

    static Mesh map;
    int scaleX, scaleZ;
    [SerializeField] static uint key;
    [SerializeField] MapType type;

    MapCell[,] MapPlatformParameters = new MapCell[,] {};



    public Map(uint Key, Vector2 Scale, MapType Type)
    {
        scaleX = (int)Scale.x; scaleZ = (int)Scale.y; key = Key; type = Type;
        MapPlatformParameters = GenerateMap();
    }
    public Map(uint Key, int PlayerNum)
    {
        scaleX = (int); scaleZ = (int)Scale.y; 
        key = Key;
        MapPlatformParameters = GenerateMap();
    }
    
    public int XScale { get{ return scaleX; } }
    public int ZScale { get{ return scaleZ; } }

    private MapCell[,] GenerateMap()
    {
        MapCell[,] result = new MapCell[scaleX, scaleZ];
        for(int x = 0; x < scaleX; x++)
        {
            for(int z = 0; z < scaleZ; z++)
            {   
                float noise = Mathf.Round(Mathf.PerlinNoise(
                    200 / (float)(x + 1) + (float)key / 45,
                    200 / (float)(z + 1) + (float)key / 31)
                    * 10) / 10;
                result[x, z] = new MapCell(0, noise);
            }
        }
        return result;
    }

    public int GetModifier(int x, int z) { return MapPlatformParameters[x, z].Modifier; }
    public float GetUp(int x, int z) { return MapPlatformParameters[x, z].Up; }
    public bool GetLet(int x, int z) { return MapPlatformParameters[x, z].Let; }
}