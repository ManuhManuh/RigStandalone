using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using VentRout.Orienteering.Rig;
using UnityEngine.SceneManagement;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject[] _characterPrefabs;
    [SerializeField]
    private PlayerState playerState;

    // Add the spawnedPlayerPrefab variable back to the class level
    private GameObject spawnedPlayerPrefab;
    private const string PlayerCharacterPrefKey = "AvatarChosen";

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneChanged; 

    }

    public void OnSceneChanged(Scene scene, LoadSceneMode loadSceneMode)
    {
        Debug.Log($"OnSceneChange triggered");
        Debug.Log($"In room? {PhotonNetwork.InRoom}");  
        if (PhotonNetwork.IsConnected && GameState.Instance.Multiplayer && PhotonNetwork.InRoom)
        {
            SpawnPlayerCharacter();
        }
    }
    private void SpawnPlayerCharacter()
    {

        int playerCharacter = playerState.SelectedAvatar;
  
        // Set custom properties for this player
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { PlayerCharacterPrefKey, playerCharacter }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        // Instantiate this player's character and assign it to spawnedPlayerPrefab
        spawnedPlayerPrefab = PhotonNetwork.Instantiate(
            _characterPrefabs[playerCharacter].name,
            transform.position,
            transform.rotation
        );

        // Set the player's name tag, if applicable
        spawnedPlayerPrefab.GetComponent<PlayerNameTag>()?.SetName();

    }

    public override void OnLeftRoom() // Method called when we leave a room
    {
        base.OnLeftRoom();
        if (spawnedPlayerPrefab != null)
        {
            PhotonNetwork.Destroy(spawnedPlayerPrefab);
        }
    }
}