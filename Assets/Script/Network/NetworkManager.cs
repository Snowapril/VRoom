using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    #region Private SerializeField

    /// <summary>
    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    /// </summary>
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 4;
    
    
    

    #endregion
    
    
    #region MonoBehaviour CallBacks
    
    private void Awake()
    {
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    
    void Start()
    {
        ConnecteToServer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    #endregion

    #region Photon CallBacks

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected To Master");
        //PhotonNetwork.JoinOrCreateRoom("Room1", new RoomOptions {MaxPlayers = maxPlayersPerRoom}, TypedLobby.Default);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayersPerRoom;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        PhotonNetwork.JoinOrCreateRoom("Room1", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined a Room");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("A New Player Joined the room");
    }

    #endregion

    #region Private Function

    void ConnecteToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    #endregion
    
}
