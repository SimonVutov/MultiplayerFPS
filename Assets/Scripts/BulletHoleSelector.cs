using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BulletHoleSelector : MonoBehaviour
{
    public Material[] mats;
    public int choosenMat;

    void Start()
    {
        choosenMat = Random.Range(0, 6);

        GetComponent<ParticleSystemRenderer>().material = mats[choosenMat];
    }
}