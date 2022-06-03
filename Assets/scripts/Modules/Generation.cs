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
                if(!map.GetExist(x, z)){
                    GameObject obj = Instantiate(
                    NowPlatform.Platform, 
                    new Vector3(x, map.GetUp(x, z), z), new Quaternion(), transform);

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
        public bool Let = false;
        
        public MapCell(int Mod, float up, bool let = false)
        { Modifier = Mod; Up = up; Let = let; }
    }

    static Mesh map;
    [SerializeField] int scaleX, scaleZ;
    [SerializeField] static uint key;


    MapCell[,] MapPlatformParameters = new MapCell[,] {};

    public Map(uint Key, Vector2 Scale)
    {
        scaleX = (int)Scale.x; scaleZ = (int)Scale.y; key = Key; 
        MapPlatformParameters = GenerateRelief();
    }
    public Map(int PlayerNum, uint Key)
    {
        scaleX = PlayerNum * 9 + (((int)key / 23)%7); scaleZ = PlayerNum * 9 + (((int)key / 14)%7); 
        key = Key;
        MapPlatformParameters = GenerateRelief();
    }
    public Map(int PlayerNum)
    {
        scaleX = PlayerNum * 9 + (((int)key / 23)%7); scaleZ = PlayerNum * 9 + (((int)key / 14)%7); 
        key = (uint)Random.Range(0, 99999999);
        MapPlatformParameters = GenerateRelief();
    }
    public Map()
    {
        scaleX = 2 * 9 + (((int)key / 23)%7); scaleZ = 2 * 9 + (((int)key / 14)%7); 
        key = (uint)Random.Range(0, 99999999);
        MapPlatformParameters = GenerateRelief();
    }
    
    
    public int XScale { get{ return scaleX; } }
    public int ZScale { get{ return scaleZ; } }

    private MapCell[,] GenerateRelief()
    {
        MapCell[,] result = new MapCell[scaleX, scaleZ];
        for(int x = 0; x < scaleX; x++)
        {
            for(int z = 0; z < scaleZ; z++)
            {   
                float noise = 0;
                // //Mathf.Round(Mathf.PerlinNoise(
                //     200 / (float)(x + 1) + (float)key / 45,
                //     200 / (float)(z + 1) + (float)key / 31)
                //     * 10) / 10;
                result[x, z] = new MapCell(0, noise);
            }
        }
        return result;
    }

    public int GetModifier(int x, int z) { return MapPlatformParameters[x, z].Modifier; }
    public float GetUp(int x, int z) { return MapPlatformParameters[x, z].Up; }
    public bool GetLet(int x, int z) { return MapPlatformParameters[x, z].Let; }
    public bool GetExist(int x, int z) { return MapPlatformParameters[x, z] == null; }
}