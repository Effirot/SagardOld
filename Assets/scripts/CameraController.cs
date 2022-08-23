using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;

public class CameraController : MonoBehaviour
{
    [Range(1, 100)]public float cameraSpd = 20, rotSpd = 20;
    Camera Camera => transform.Find("Main Camera").GetComponent<Camera>();
    Transform UICanvas => GameObject.Find("GameUI").transform;

    public GameObject QuickUi;

    Vector3 TranslatePos;
    void FixedUpdate()
    {
        TranslatePos += (Input.GetAxis("Horizontal") * transform.right * cameraSpd / 100) + (Input.GetAxis("Vertical") * transform.forward * cameraSpd / 100);
        transform.position = TranslatePos + new Vector3(0, Mathf.Lerp(transform.position.y, new Checkers(transform.position).Layer(CursorController.CurrentPlayerShowLayer).up, 0.04f), 0);

        Vector3 rot = new Vector3(0, Input.GetAxis("Camera rot"), 0);
        transform.eulerAngles -= rot * (rotSpd / 10);
    }



    private void OnTriggerEnter(Collider other)
    {
    }
    private void OnTriggerExit(Collider other)
    {
    }
}
