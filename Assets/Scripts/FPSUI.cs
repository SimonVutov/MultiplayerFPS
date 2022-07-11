using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSUI : MonoBehaviour
{
    public TextMeshProUGUI text;
    
    void Update()
    {
        if (GetComponent<GunManager>().holdingGun) text.text = (((GetComponent<Jetpack>().startFlyTime - GetComponent<Jetpack>().boostUsed)) * 10).ToString("0") + "\n" + GetComponent<GunManager>().gun.GetComponent<GunShoot>().gunAmmoUI;
        else text.text = (((GetComponent<Jetpack>().startFlyTime - GetComponent<Jetpack>().boostUsed)) * 10).ToString("0");
    }
}
