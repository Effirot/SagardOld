using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atest : MonoBehaviour
{
    public GameObject PlatformPrefab;
    public Mesh[][] Modificators;
    Map map = new Map(111, new Vector2(10, 10), MapType.Desert);

    

    void Start()
    {
        for(int x = 0; x < 10; x++)
        {
            for(int z = 0; z < 10; z++)
            {
                Instantiate(PlatformPrefab, new Vector3(x, map.GetPlatformUp(x, z), z), Quaternion.Euler(0, Random.Range(0, 360), 0), transform);
            }
        }
    }


}
