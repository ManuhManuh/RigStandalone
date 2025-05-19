using System.Collections.Generic;
using UnityEngine;
using VentRout.Orienteering.Rig;

namespace VentRout.Orienteering.Gameplay
{
    [System.Serializable]
    public class Level
    {
        public string Name;
        public int SceneIndex;
        public int MaxPlayer;
        public List<GameMode> Modes;
        public Sprite Image;
    }

    public class LobbyManager : MonoBehaviour
    {
        public List<Level> Levels => levels;
        public int defaultAvatarIndex = 0;

        [Header("Levels")]
        [SerializeField] private List<Level> levels;

        [Header("Random Nametag Generations")]
        [SerializeField]
        private List<string> NamesList;

        [SerializeField]
        [Range(10, 999)]
        private int NametagMaxIntValue = 999;

        private int _currentRoomIndex = -1;
        public int CurrentRoomIndex
        {
            get
            {
                return _currentRoomIndex;
            }

            set
            {
                _currentRoomIndex = value;
            }
        }

        private string[] countries = { "USA", "Canada", "UK", "Australia", "Germany", "France", "Japan", "Brazil", "India", "China" };

        private PlayerState playerState;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            playerState = player.GetComponent<PlayerState>();
            if (playerState == null)
            {
                Debug.LogError("PlayerState component not found!!");
            }
        }
        public void SetNametag()
        {
            // check to see if there is a displayName stored from Playfab (generate only if there isn't one already)
            if (playerState.PlayerName == null || playerState.PlayerName == "Player000")
            {
                GenerateRandomNametag();
                SetUpPlayerCustomProperties();
            }

        }

        public void GenerateRandomNametag()
        {
            string nametag = NamesList[Random.Range(0, NamesList.Count)] + Random.Range(0, NametagMaxIntValue + 1).ToString("D3");
            SaveNameTag(nametag);
        }

        public void SaveNameTag(string nametag)
        {
            playerState.PlayerName = nametag;

            //PlayerPrefs.SetString("Nickname", nametag);


        }

        public void SetUpPlayerCustomProperties()
        {
            playerState.PlayerCountry = countries[Random.Range(0, countries.Length - 1)];
            playerState.PlayerBibNumber = GetBibNumber();
        }

        public string GetBibNumber()
        {
            int bibNumber;
            string playerName = playerState.PlayerName;
            bool isNumeric = int.TryParse(playerName.Substring(playerName.Length - 3), out bibNumber);
            if (!isNumeric)
            {
                // Generate random bib number if the name doesn't end in a number of at least 3 digits
                bibNumber = Random.Range(100, 999);
            }

            return bibNumber.ToString();
        }
    }
}