using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float CameraSPDZ = 20, CameraSPDX = 20, rotSpeed = 1;

    void Start()
    {
    }

    void Update()
    {
        transform.eulerAngles += new Vector3(0, Input.GetAxis("Rotor") * rotSpeed, 0);
        transform.position += new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * CameraSPDX, 0, Input.GetAxis("Vertical") * Time.deltaTime * CameraSPDZ);
    }
}
