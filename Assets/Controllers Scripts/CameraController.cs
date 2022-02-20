using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float CameraSPDZ = 20, CameraSPDX = 20;
    public Transform Camera;

    void Update()
    {
        Vector3 hor = Input.GetAxis("Horizontal") * Time.deltaTime * CameraSPDX * Vector3.right;
        Vector3 ver = Input.GetAxis("Vertical") * Time.deltaTime * CameraSPDZ * Vector3.forward;

        Vector3 rot = new Vector3(0, Input.GetAxis("Camera rot"), 0);

        transform.Translate(hor + ver);
        transform.eulerAngles += rot;



    }
}
