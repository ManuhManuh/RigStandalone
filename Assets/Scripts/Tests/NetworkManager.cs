using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using VentRout.Orienteering.Rig;

namespace VentRout.Orienteering.Gameplay
{
    public class NetworkManager : MonoBehaviourPunCallbacks //monoBehaviorPunCallbacks be able to override initial function when someone joins server, room
    {
        private const string ROOM_NAME_FORMAT = "{0}_{1}";

        [SerializeField] private PlayerState playerState;

        private void Start()
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.JoinLobby();
            }
            else
            {
                ConnectToServer();
            }

        }

        public void ConnectToServer()// to access from outside this script
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.JoinLobby();
                return;
            }
            bool success = PhotonNetwork.ConnectUsingSettings();

        }

        public override void OnConnectedToMaster()//alert when we connected to server
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("OnJoinedLobby callback received");

        }
        public void InitializeRoom()
        {

            PhotonNetwork.NickName = playerState.PlayerName;
            PhotonNetwork.SetPlayerCustomProperties(new Hashtable { { "Country", playerState.PlayerCountry } });
            PhotonNetwork.SetPlayerCustomProperties(new Hashtable { { "BibNumber", playerState.PlayerBibNumber } });
            Debug.Log($"Nickname set in InitializeRoom: {PhotonNetwork.NickName}");

            if (GameState.Instance.Multiplayer)
            {
                Debug.Log("Multiplayer triggered");

                RoomOptions roomOptions = new RoomOptions(); //create new room options to set parameters below
                roomOptions.MaxPlayers = (byte)GameState.Instance.MaxPlayers; //set max player
                roomOptions.IsVisible = true; // all players can see room
                roomOptions.IsOpen = true;//allow indivduals to join room after it is created
                roomOptions.EmptyRoomTtl = 0; // delete room when last player leaves

                // Add custom room properties
                roomOptions.CustomRoomProperties = new Hashtable
            {
                { "sceneIndex", GameState.Instance.SelectedSceneIndex }
            };

                //generate a room name based on level and mode
                //string roomName = string.Format(ROOM_NAME_FORMAT, PlayerState.Instance.SelectedLevel.Name, PlayerState.Instance.CurrentGameModeIndex);
                string roomName = GameState.Instance.SelectedRoomName;

                //string roomName = string.Format(ROOM_NAME_FORMAT, levelSettings.Name, modeIndex/*, RoomIDGenerator.GetNewRoomID()*/);
                //to share data with another  player  must  be in the same room
                PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default, null);// "test" is creating jsut one room for test purposes roomSettings.Name,
            }
            else    // Single player
            {
                Debug.Log("Single player triggered");

                RoomOptions roomOptions = new RoomOptions(); //create new room options to set parameters below
                roomOptions.MaxPlayers = 1; //set max player
                roomOptions.IsVisible = false; // all players can see room
                roomOptions.IsOpen = false;//allow indivduals to join room after it is created
                roomOptions.EmptyRoomTtl = 0; // delete room when last player leaves

                // Add custom room properties
                roomOptions.CustomRoomProperties = new Hashtable
            {
                { "sceneIndex", GameState.Instance.SelectedSceneIndex }
            };

                string roomName = string.Format(ROOM_NAME_FORMAT, GameState.Instance.SelectedRoomName, "0");
                //string roomName = string.Format(ROOM_NAME_FORMAT, levelSettings.Name, modeIndex/*, RoomIDGenerator.GetNewRoomID()*/);
                //to share data with another  player  must  be in the same room
                PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default, null);// "test" is creating jsut one room for test purposes roomSettings.Name,
            }
        }

        public override void OnJoinedRoom()//alert when we joined the room
        {
            //Load scene
            // Retrieve the scene index from room properties
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("sceneIndex", out object sceneIndex))
            {

                Debug.Log($"Attempting to load scene index {sceneIndex}");

                PhotonNetwork.LoadLevel((int)sceneIndex);
            }
        }

        void JoinRoom(string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
        }
    }
}