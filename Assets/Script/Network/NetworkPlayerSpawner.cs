using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;

    private GameObject spawnedPlayerPrefab;
    private GameObject spawnedPlayerAvartar;
    
    
    #region Photon Callbacks
    
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", transform.position, transform.rotation);
        spawnedPlayerAvartar = PhotonNetwork.Instantiate(this.playerPrefab.name, transform.position, transform.rotation);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawnedPlayerAvartar);
        PhotonNetwork.Destroy(spawnedPlayerPrefab);
    }

    #endregion
}
