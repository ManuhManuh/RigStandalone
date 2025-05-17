using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FootstepClips", menuName = "Footstep Clips")]
public class TerrainClipSet : ScriptableObject
{
    public string terrainName;
    public List<AudioClip> footstepClips;
}
