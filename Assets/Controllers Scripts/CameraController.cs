using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float CameraSPDZ = 20, CameraSPDX = 20;
    public Transform Camera;

    void Start()
    {

    }

    void Update()
    {

        transform.localPosition += new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * CameraSPDX, 0, Input.GetAxis("Vertical") * Time.deltaTime * CameraSPDZ);
    }
}
