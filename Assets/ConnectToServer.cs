using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public TMP_InputField nameInput;
    public TMP_Text buttonText;

    private void Start()
    {
        if (PhotonNetwork.NickName != null) nameInput.text = PhotonNetwork.NickName;
    }

    public void OnClickConnect()
    {
        if (nameInput.text.Length > 0)
        {
            PhotonNetwork.NickName = nameInput.text;
            buttonText.text = "Connecting...";
            PhotonNetwork.AutomaticallySyncScene = true;
            if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
            else JoinLobby();
        } else
        {
            StartCoroutine(ShowNameIsTooShort());
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void JoinLobby () {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void QuitGame() //for quit button
    {
        Application.Quit();
    }

    private IEnumerator ShowNameIsTooShort()
    {
        string originalText = buttonText.text;
        buttonText.text = "nameTooShort";
        yield return new WaitForSeconds(1f);
        buttonText.text = originalText;
    }

}
