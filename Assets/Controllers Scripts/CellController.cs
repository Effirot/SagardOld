using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellController : MonoBehaviour
{
    public int Upped = 0;
    float StartPosY;

    GameObject[] PolesAround;


    // Start is called before the first frame update
    void Start()
    {
        transform.position += new Vector3(0, Random.Range(-0.02f, 0.02f), 0);
        StartPosY = transform.position.y;

        StartCoroutine(UpUpdate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator UpUpdate()
    {
        while(true){
            transform.position = new Vector3(transform.position.x, StartPosY + Upped * 0.16f, transform.position.z);
            yield return new WaitForSeconds(0.03f);
        }
    }
}
