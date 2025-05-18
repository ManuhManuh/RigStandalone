using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System;

public class PlayerNameTag : MonoBehaviourPun
{
    [SerializeField] private TextMeshProUGUI playerTag;

    // Start is called before the first frame update
    private void Start()
    {
        if (photonView.IsMine)
        {
            return;
        }

        SetName();
    }
    // Update is called once per frame
    public void SetName()//gt owners nickname and set as is
    {
        playerTag.text = photonView.Owner.NickName;
       
    }

   

}
