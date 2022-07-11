using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField nameInput;
    public TMP_InputField createInput;
    public TMP_InputField joinInput;
    public TMP_Text roomName;

    public GameObject lobbyPanel;
    public GameObject roomPanel;

    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemList = new List<RoomItem>();
    public Transform contentObject;

    public float timeBetweenUpdates = 1.5f;
    float nextUpdateTime;

    public List<PlayerItem> playerItemsList = new List<PlayerItem>();
    public PlayerItem playerItemPrefab;
    public Transform playerItemParent;

    public GameObject playButton;

    void Start ()
    {
        if (PhotonNetwork.NickName != null) nameInput.text = PhotonNetwork.NickName;
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 0) //set min players to play
        {
            playButton.SetActive(true);
        } else
        {
            playButton.SetActive(false);
        }
    }

    public void CreateRoom()
    {
        if (nameInput.text.Length > 0 && createInput.text.Length > 0)
        {
            PhotonNetwork.CreateRoom(createInput.text, new RoomOptions() { MaxPlayers = 20, BroadcastPropsChangeToAll = true } );
        }
    }

    public void JoinRoom()
    {
        if (nameInput.text.Length > 0 && joinInput.text.Length > 0)
        {
            PhotonNetwork.JoinRoom(joinInput.text);
        }
    }

    public override void OnJoinedRoom()//for typingin code to join
    {
        PhotonNetwork.NickName = nameInput.text;
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    public void BackButton() //for the back button
    {
        PhotonNetwork.NickName = nameInput.text;
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("Menu");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time >-nextUpdateTime)
        {
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
        
    }

    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach (RoomItem item in roomItemList)
        {
            Destroy(item.gameObject);
        }
        roomItemList.Clear();

        foreach(RoomInfo room in list)
        {
            RoomItem newRoom = Instantiate(roomItemPrefab, contentObject);
            newRoom.SetRoomName(room.Name);
            roomItemList.Add(newRoom);
        }
    }

    public void JoinRoom(string roomName) //for clicking shown rooms
    {
        if (nameInput.text.Length > 0) PhotonNetwork.JoinRoom(roomName);
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    void UpdatePlayerList()
    {
        foreach (PlayerItem item in playerItemsList)
        {
            Destroy(item.gameObject);
        }
        playerItemsList.Clear();

        if (PhotonNetwork.CurrentRoom == null) return;

        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerItemParent);
            newPlayerItem.SetPlayerInfo(player.Value);

            if (player.Value == PhotonNetwork.LocalPlayer)
            {
                newPlayerItem.ApplyLocalChanges();
            }

            playerItemsList.Add(newPlayerItem);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    public void OnClickPlayGame ()
    {
        PhotonNetwork.LoadLevel("Map");
    }
}
