using UnityEngine;
using VentRout.Orienteering.Gameplay;

public class GameState : Singleton<GameState>
{
    // This is a test version of GameState that only has generic properties
    // (those that are likely needed for any scenario, not specifically the orienteering sim)

    public bool Multiplayer = false;
    public string SelectedRoomName;
    public int MaxPlayers = 8;
    public int SelectedSceneIndex = 1;

}

