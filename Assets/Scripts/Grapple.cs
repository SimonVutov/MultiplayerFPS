using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    Controls controls;
    Camera cam;
    public GameObject grapplePos;
    public GameObject ropeStartPos;
    LineRenderer line;

    Rigidbody hitRigidbody;

    float range = 40f;
    float grappleStrength = 200f;

    private bool addGrappleForce = false;
    private bool addRigidbodyForce = false;
    private Vector3 hitLocation = Vector3.zero;
     
    private void Awake()
    {
        controls = new Controls();

        controls.Gameplay.Aim.performed += ctx => StartGrapple();
        controls.Gameplay.Aim.canceled += ctx => EndGrapple();
    }

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
        line = GetComponent<LineRenderer>();
        line.enabled = false;
    }

    void Update()
    {
        if (addGrappleForce)
        {
            GetComponent<Rigidbody>().AddForce((grapplePos.transform.position - transform.position) * grappleStrength * Time.deltaTime);
            Debug.DrawRay(transform.position, grapplePos.transform.position - transform.position, Color.green);
        }

        //if (addRigidbodyForce) hitRigidbody.AddForceAtPosition((transform.position - grapplePos.transform.position) * grappleStrength * Time.deltaTime, hitLocation);
        if (addRigidbodyForce) hitRigidbody.AddForce((transform.position - grapplePos.transform.position) * grappleStrength * Time.deltaTime);

        line.SetPosition(0, ropeStartPos.transform.position);
        line.SetPosition(line.positionCount - 1, grapplePos.transform.position);
    }

    void StartGrapple()
    {
        if (!GetComponent<GunManager>().holdingGun)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range))
            {
                line.enabled = true;
                grapplePos.transform.parent = hit.transform;
                grapplePos.transform.position = hit.point;
                addGrappleForce = true;
                addRigidbodyForce = true;

                if (hit.transform.GetComponent<Rigidbody>() != null) hitRigidbody = hit.transform.GetComponent<Rigidbody>();
                else addRigidbodyForce = false;

                hitLocation = hit.point;
            }
        }
    }

    public void EndGrapple()
    {
        addGrappleForce = false;
        addRigidbodyForce = false;
        line.enabled = false;
    }

    private void OnEnable() { controls.Gameplay.Enable(); }
    private void OnDisable() { controls.Gameplay.Disable(); }
}
