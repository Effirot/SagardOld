using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(1, 100)]public float cameraSpd = 20, rotSpd = 20;
    Camera Camera => transform.Find("Main Camera").GetComponent<Camera>();
    Transform UICanvas => GameObject.Find("GameUI").transform;

    public GameObject QuickUi;

    void FixedUpdate()
    {
        Vector3 hor = Input.GetAxis("Horizontal") * transform.right;
        Vector3 ver = Input.GetAxis("Vertical") * transform.forward;
        transform.position += ((hor + ver) * cameraSpd / 100);

        Vector3 rot = new Vector3(0, Input.GetAxis("Camera rot"), 0);
        transform.eulerAngles -= rot * (rotSpd / 10);
    }

    private void OnTriggerEnter(Collider other)
    {
        // if(other.gameObject.tag != "Figure") return;
        
        // GameObject obj = Instantiate(QuickUi, UICanvas);
        // obj.GetComponent<IDgenerator>().ID = other.GetComponent<IDgenerator>().ID;
        // obj.GetComponent<MoveOnUi>().Target = other.gameObject.transform;
    }

    private void OnTriggerExit(Collider other)
    {
    }
}
