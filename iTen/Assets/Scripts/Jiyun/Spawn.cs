using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Spawn : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public GameObject player;
    void Start()
    {
        //PhotonNetwork.SerializationRate = 30;
        //PhotonNetwork.SendRate = 30;
        
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        //int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint(0);

        if (spawnPoint != null)
        {
            //GameObject player = PhotonNetwork.Instantiate("player", spawnPoint.position, spawnPoint.rotation, 0);
            Instantiate(player, spawnPoint.position, spawnPoint.rotation);
        }
    }    

    /*public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("UpdateAllPlayersSpawn", RpcTarget.All);
        }
    }

    [PunRPC]
    void UpdateAllPlayersSpawn()
    {
        SpawnPlayer();
    }
    */
}
