using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    GameObject spawnedPlayer;
    int team = 0;

    private void Start()
    {
        //if ((int)PhotonNetwork.LocalPlayer.CustomProperties["team"] != 0 && (int)PhotonNetwork.LocalPlayer.CustomProperties["team"] != 1) team = 0;
        //else team = (int)PhotonNetwork.LocalPlayer.CustomProperties["team"];
        GameObject playerToSpawn = playerPrefabs[team];
        spawnedPlayer = PhotonNetwork.Instantiate(playerToSpawn.name, transform.position, Quaternion.identity);
    }
}
