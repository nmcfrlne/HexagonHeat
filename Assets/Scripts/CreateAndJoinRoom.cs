using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class CreateAndJoinRoom : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField usernameInput;
    [SerializeField] private InputField createInput;
    [SerializeField] private InputField joinInput;

    public GameObject usernameButton;
    public GameObject joincreateUI;

    public void SetUserName()
    {
        PhotonNetwork.LocalPlayer.NickName = usernameInput.text;
        joincreateUI.SetActive(true);
    }

    public void CreateRoom()
    {
        //When you create a room you automatically join it
        PhotonNetwork.CreateRoom(createInput.text);
        SoundManager.soundInstance.audioSource.PlayOneShot(SoundManager.soundInstance.click);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
        SoundManager.soundInstance.audioSource.PlayOneShot(SoundManager.soundInstance.click);
    }

    //Called automatically when a room is joined
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    private void Update()
    {
        if(usernameInput.text.Length > 1 && usernameInput.text.Length < 15)
        {
            usernameButton.SetActive(true);
        }
        else
        {
            usernameButton.SetActive(false);
        }
    }
}
