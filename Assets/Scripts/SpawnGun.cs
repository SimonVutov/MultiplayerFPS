using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnGun : MonoBehaviour
{
    public GameObject objectToSpawnOnNetwork;

    void Start()
    {
        GameObject gunSpawned = PhotonNetwork.InstantiateRoomObject(objectToSpawnOnNetwork.name, transform.position, Quaternion.identity, 0, null);
        
        gunSpawned.transform.parent = null;
    }
}
