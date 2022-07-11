using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerItem : MonoBehaviourPunCallbacks
{
    public TMP_Text playerName;
    public TMP_Text playerTeam;

    public Color teamZeroColor = Color.white;
    public Color teamOneColor = Color.red;

    public Image backgroundImage;
    public Color highlightedColor;
    public GameObject switchTeam;

    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
    public int team;

    Player player;

    int teamNum = 0; //local number

    private void Awake()
    {
        playerProperties["team"] = 0;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public void SetPlayerInfo(Player _player)
    {
        playerName.text = _player.NickName;
        player = _player;
        UpdatePlayerItem(player);
    }

    public void ApplyLocalChanges()
    {
        backgroundImage.color = highlightedColor;
        switchTeam.SetActive(true);
    }

    public void OnClickChangeTeam()
    {
        if (teamNum == 0) teamNum = 1;
        else if (teamNum == 1) teamNum = 0;

        playerProperties["team"] = teamNum;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    private void Update()
    {
        playerTeam.text = ((int)playerProperties["team"]).ToString();

        if (team == 0) playerName.color = teamZeroColor; //white
        if (team == 1) playerName.color = teamOneColor; //red
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (player == targetPlayer)
        {
            UpdatePlayerItem(targetPlayer);
        }
    }

    void UpdatePlayerItem(Player player)
    {
        if (player.CustomProperties.ContainsKey("team"))
        {
            team = (int)player.CustomProperties["team"];
            playerProperties["team"] = (int)player.CustomProperties["team"];
        }
        else
        {
            playerProperties["team"] = 0;
        }
    }
}
