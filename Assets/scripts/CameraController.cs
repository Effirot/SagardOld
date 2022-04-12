using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(1, 100)]public float cameraSpd = 20, rotSpd = 20;
    public Camera Camera;
    float StartedFieldOfView;
    float toFieldOfView = 0;

    [SerializeField] private bool toBase = false;

    void Start()
    {
        StartedFieldOfView = Camera.fieldOfView;
        toFieldOfView = StartedFieldOfView;
    }

    void Update()
    {
        Vector3 hor = Input.GetAxis("Horizontal") * Time.deltaTime * Vector3.right;
        Vector3 ver = Input.GetAxis("Vertical") * Time.deltaTime * Vector3.forward;

        Vector3 rot = new Vector3(0, Input.GetAxis("Camera rot"), 0);

        toFieldOfView = Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel") * -60 + toFieldOfView, 15, StartedFieldOfView);
        Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, toFieldOfView, 0.1f);

        transform.Translate((hor + ver) * cameraSpd);
        Debug.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y + 10000, transform.position.z));

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
