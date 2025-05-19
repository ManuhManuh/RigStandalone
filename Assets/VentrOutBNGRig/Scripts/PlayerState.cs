using UnityEngine;
using System.Collections.Generic;

namespace VentRout.Orienteering.Rig
{
    public class PlayerState : MonoBehaviour
    {
        public string PlayerName
        {
            get { return playerName; }
            set { playerName = value; }
        }
        private string playerName = "Player000";

        public string PlayerCountry
        {
            get { return playerCountry; }
            set { playerCountry = value; }
        }
        private string playerCountry = "USA";

        public string PlayerBibNumber
        {
            get { return playerBibNumber; }
            set { playerBibNumber = value; }
        }
        private string playerBibNumber = "000";

        public int SelectedAvatar => selectedAvatar;

        [SerializeField] private List<GameObject> compasses = new List<GameObject>();
        [SerializeField] private List<GameObject> watches = new List<GameObject>();

        [SerializeField] private int selectedAvatar = 0;
        [SerializeField] private int selectedWatch = 0;
        [SerializeField] private int selectedCompass = 0;

        public float playerPaceLength = 1.25f;
        public bool mapInHand = false;

        private void Start()
        {
            OnSelectAvatar(selectedAvatar);
            OnSelectWatch(selectedWatch);
            OnSelectCompass(selectedCompass);

        }
        public void OnSelectAvatar(int newAvatarIndex)
        {
            selectedAvatar = newAvatarIndex;

        }

        public void OnSelectWatch(int newWatchIndex)
        {
            selectedWatch = newWatchIndex;
            for (int i = 0; i < watches.Count; i++)
            {
                watches[i].SetActive(i == selectedWatch);

            }
        }

        public void OnSelectCompass(int newCompassIndex)
        {
            selectedCompass = newCompassIndex;
            for (int i = 0; i < compasses.Count; i++)
            {
                compasses[i].SetActive(i == selectedCompass);

            }
        }
    }

}