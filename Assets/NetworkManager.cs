using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    //instance creation
    public static NetworkManager instance;
    //make network manager a singleton
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            gameObject.SetActive(false);
        } else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master server");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created room: " + PhotonNetwork.CurrentRoom.Name);
    }
}
