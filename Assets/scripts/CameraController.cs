using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(1, 100)]public float cameraSpd = 20, rotSpd = 20;
    [Range(1, 100)]public Transform Camera;

    [SerializeField]private bool toBase = false;

    void Update()
    {
        Vector3 hor = Input.GetAxis("Horizontal") * Time.deltaTime * Vector3.right;
        Vector3 ver = Input.GetAxis("Vertical") * Time.deltaTime * Vector3.forward;

        Vector3 rot = new Vector3(0, Input.GetAxis("Camera rot"), 0);

        transform.Translate((hor + ver) * cameraSpd);


        if(Input.GetKeyDown("space")) toBase = true;

        if(toBase)
        { 
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), 0.17f); 

            StartCoroutine(WaitASecond());
        }
        else
        {
            transform.eulerAngles -= rot * (rotSpd / 10);
        }

    }

    IEnumerator WaitASecond()
    {
        yield return new WaitForSeconds(0.7f);
        toBase = false;
        yield break;
    }
}
