using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepController : MonoBehaviour
{
    //[SerializeField] private Terrain terrain;
    [SerializeField] private TerrainState terrainState;
    //[SerializeField] private float seaLevel;

    [SerializeField] private float sampleSpacing = 0.05f;

    [SerializeField] private AudioSource footstepsSource;
    [SerializeField] private List<TerrainClipSet> terrainClips = new List<TerrainClipSet>();

    [SerializeField] private AudioSource waterSource;
    [SerializeField] private List<AudioClip> waterClips = new List<AudioClip>();

    [SerializeField] private AudioSource boundaryWarningSource;
    [SerializeField] private AudioClip boundaryWarning;


    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private float paceLength = 1.25f;

    //private bool inWater = false;
    bool currentlyPlayingStep = false;
    bool currentlyPlayingBoundaryWarning = false;

    private void Start()
    {
        StartCoroutine(SampleTerrain());

    }

    private IEnumerator SampleTerrain()
    {
        float distanceMoved = 0f;
        Vector3 originalPosition = playerRb.gameObject.transform.position;
        Vector3 newPosition;

        while (true)
        {
            newPosition = playerRb.gameObject.transform.position;
            distanceMoved += Mathf.Abs((originalPosition - newPosition).magnitude);

            if (Mathf.Abs(playerRb.velocity.magnitude) > 0 && distanceMoved >= paceLength && !currentlyPlayingStep)
            {
                //player has moved far enough to complete a step
                distanceMoved = 0;
                float[,,] alphaMap = terrainState.AlphaMapAtPosition;
                currentlyPlayingStep = true;
                PlayFootstep(alphaMap);

            }

            originalPosition = newPosition;
            yield return new WaitForSeconds(sampleSpacing);

        }

    }

    private void PlayFootstep(float[,,] alphaMap)
    {
        // check looping boundary warning - stop or start as required
        if (terrainState.NearBoundary && !currentlyPlayingBoundaryWarning)
        {
            boundaryWarningSource.clip = boundaryWarning;
            currentlyPlayingBoundaryWarning = true;
            boundaryWarningSource.Play();

        }
        if (!terrainState.NearBoundary && currentlyPlayingBoundaryWarning)
        {
            currentlyPlayingBoundaryWarning = false;
            boundaryWarningSource.Stop();
        }

        // add footstep (on top if necessary)
        if (terrainState.InWater)
        {
            // play random walking in water sound
            waterSource.PlayOneShot(waterClips[Random.Range(0, waterClips.Count - 1)], 0.25f);
        }
        else
        {
            // texture indexes are to match those in Gaia biome Spawner
            for (int textureIndex = 0; textureIndex < terrainClips.Count - 1; textureIndex++)
            {
                //Debug.Log($"Trying texture index {textureIndex}");
                if (alphaMap[0, 0, textureIndex] > 0)
                {
                    // Get list of clips for the texture at this index, pick a random one, and play at the volume percentage it was rendered at
                    footstepsSource.PlayOneShot(terrainClips[textureIndex].footstepClips[Random.Range(0, terrainClips[textureIndex].footstepClips.Count - 1)],
                                            alphaMap[0, 0, textureIndex]);
                }
            }

        }
        currentlyPlayingStep = false;
    }
}
