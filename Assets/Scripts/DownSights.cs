using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownSights : MonoBehaviour
{
    Controls controls;
    GameObject gunHolder;
    Vector3 gunPos;
    float startXPos;
    float startYPos;

    float startFeildOfView;
    float targetFeildOfView;

    float velocit;

    [HideInInspector]
    public bool aiming;

    private void Awake()
    {
        startFeildOfView = Camera.main.fieldOfView;
        targetFeildOfView = startFeildOfView;

        controls = new Controls();

        controls.Gameplay.Aim.performed += ctx => StartAim();
        controls.Gameplay.Aim.canceled += ctx => EndAim();

        gunHolder = transform.GetChild(0).GetChild(0).gameObject;
        startXPos = gunHolder.transform.localPosition.x;
        startYPos = gunHolder.transform.localPosition.y;
        gunPos = gunHolder.transform.localPosition;
    }

    void Update()
    {
        gunHolder.transform.localPosition = Vector3.Lerp(gunHolder.transform.localPosition, gunPos, 20f * Time.deltaTime);
        if (GetComponent<GunManager>().holdingGun) Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, targetFeildOfView, ref velocit, 0.1f);
        else Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, startFeildOfView, ref velocit, 0.1f);
    }

    void StartAim()
    {
        aiming = true;
        targetFeildOfView = Camera.main.fieldOfView * 0.7f;
        gunPos = new Vector3(0f, gunHolder.transform.localPosition.y, gunHolder.transform.localPosition.z);
    }

    public void EndAim()
    {
        targetFeildOfView = startFeildOfView;
        aiming = false;
        gunPos = new Vector3(startXPos, startYPos, gunHolder.transform.localPosition.z);
    }

    private void OnEnable() { controls.Gameplay.Enable(); }
    private void OnDisable() { controls.Gameplay.Disable(); }
}
