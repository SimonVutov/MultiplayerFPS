using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetpack : MonoBehaviour
{
    Controls controls;

    bool boosting = false;

    [HideInInspector]
    public float boostUsed = 0f;

    public float startFlyTime = 2f;
    public float boostStrength = 2700f;

    void Awake()
    {
        controls = new Controls();

        controls.Gameplay.Jump.performed += ctx => boosting = true;
        controls.Gameplay.Jump.canceled += ctx => boosting = false;
    }

    void Update()
    {
        if (boosting) Boost();
        else DontBoost();
    }

    void Boost()
    {
        if (boostUsed >= 0f && GetComponent<Movement>().grounded) boostUsed -= Time.deltaTime;
        else if (boostUsed < startFlyTime && !GetComponent<Movement>().grounded)
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(0f, boostStrength, 0f) * Time.deltaTime);
            boostUsed += Time.deltaTime;
        }
    }

    void DontBoost()
    {
        if (boostUsed >= 0f && GetComponent<Movement>().grounded) boostUsed -= Time.deltaTime;
    }

    private void OnEnable() { controls.Gameplay.Enable(); }
    private void OnDisable() { controls.Gameplay.Disable(); }
}
