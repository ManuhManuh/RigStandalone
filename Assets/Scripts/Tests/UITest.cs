using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VentRout.Orienteering.Gameplay;
using VentRout.Orienteering.Rig;
using Photon.Pun;



public class UITest : MonoBehaviour
{
    [SerializeField] private PlayerState playerState;
    [SerializeField] private NetworkManager networkManager;
    private int pushCount = 0;
    [SerializeField] private TMP_Text textDisplay;

    public void OnButtonPushed()
    {
        //pushCount++;
        //textDisplay.text = $"This button has been pushed {pushCount} times";
        if(PhotonNetwork.IsConnectedAndReady)
        {
            networkManager.InitializeRoom();

        }
        
    }

    public void OnOtherButtonPushed()
    {
        playerState.PlayerBibNumber = string.Format("{0:000}", Random.Range(1, 999));
        playerState.PlayerName = "Player" + playerState.PlayerBibNumber;
        textDisplay.text = $"{playerState.PlayerName}, push to enter map!";

    }
}
