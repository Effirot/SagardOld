using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(1, 100)]public float cameraSpd = 20, rotSpd = 20;
    Camera Camera => transform.Find("Main Camera").GetComponent<Camera>();
    Transform UICanvas => GameObject.Find("GameUI").transform;

    public GameObject QuickUi;

    [SerializeField] private bool toBase = false;

    void Update()
    {
        Vector3 hor = Input.GetAxis("Horizontal") * Time.deltaTime * Vector3.right;
        Vector3 ver = Input.GetAxis("Vertical") * Time.deltaTime * Vector3.forward;

        Vector3 rot = new Vector3(0, Input.GetAxis("Camera rot"), 0) * Time.deltaTime * 100;

        transform.Translate(Time.deltaTime * (hor + ver) * cameraSpd * 100);

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
