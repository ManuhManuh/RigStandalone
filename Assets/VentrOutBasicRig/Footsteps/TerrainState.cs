using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainState : MonoBehaviour
{
    public bool InWater
    {
        get { return inWater; }
        set { InWater = value; }
    }
    private bool inWater = false;

    public bool NearBoundary => nearBoundary;
    private bool nearBoundary = false;

    public float[,,] AlphaMapAtPosition => alphaMapAtPosition;
    private float[,,] alphaMapAtPosition;

    public float WaterLevel => waterLevel;
    public float TerrainSpeed => terrainSpeed;

    [SerializeField] private float seaLevel;
    [SerializeField] private float waterSpeed;
    private float waterLevel;
    private float terrainSpeed;

    public Vector3 TerrainNormalAtPosition => terrainNormalAtPosition;
    private Vector3 terrainNormalAtPosition;

    [SerializeField] private Terrain terrain;
    // texture speed coefficients need to be in the same order as the textures in the terrain
    [SerializeField] private List<float> textureSpeedCoefficients;
    [SerializeField] private List<InlandWaterFeature> inlandWaterFeatures;
    [SerializeField] private float sampleSpacing = 0.05f;
    [SerializeField][Range(0, 1)] private float minBoundaryProximity = 0.05f;

    private Rigidbody playerRb;
    private CapsuleCollider playerCollider;

    private void Start()
    {
        playerRb = GameObject.FindGameObjectWithTag("XRRig").GetComponent<Rigidbody>();
        playerCollider = playerRb.gameObject.GetComponent<CapsuleCollider>();

        StartCoroutine(SampleTerrain());

    }

    private IEnumerator SampleTerrain()
    {
        while (true)
        {
            Vector3 playerPosition = playerRb.transform.position;
            Vector3 mapPosition = GetTerrainCoordinates(playerPosition);

            UpdateInWater();
            UpdateAlphaMap(mapPosition);
            UpdateNormals(mapPosition);
            UpdateTerrainSpeed();

            yield return new WaitForSeconds(sampleSpacing);

        }
    }


    private void UpdateInWater()
    {
        Vector3 colliderBottomPosition = playerRb.transform.position + playerCollider.center;
        inWater = colliderBottomPosition.y <= seaLevel + playerCollider.center.y;
        if (!inWater)
        {
            // check for inland water if not at sea level
            foreach (InlandWaterFeature feature in inlandWaterFeatures)
            {
                if (feature.PlayerInWater)
                {
                    inWater = true;
                    waterLevel = feature.WaterLevel;
                }
            }
            if (!inWater) waterLevel = seaLevel;
        }
    }


    private void UpdateAlphaMap(Vector3 mapPosition)
    {
        float xCoord = mapPosition.x * terrain.terrainData.alphamapWidth;
        float zCoord = mapPosition.z * terrain.terrainData.alphamapHeight;
        int posX = (int)xCoord;
        int posZ = (int)zCoord;
        alphaMapAtPosition = terrain.terrainData.GetAlphamaps(posX, posZ, 1, 1);

    }

    private void UpdateNormals(Vector3 mapPosition)
    {
        float normalX = mapPosition.x;
        float normalZ = mapPosition.z;
        terrainNormalAtPosition = terrain.terrainData.GetInterpolatedNormal(normalX, normalZ);

    }

    private Vector3 GetTerrainCoordinates(Vector3 playerPosition)
    {
        Vector3 playerLocalPosition = playerPosition - terrain.transform.position;
        Vector3 mapPosition = new Vector3
                (playerLocalPosition.x / terrain.terrainData.size.x,
                0,
                playerLocalPosition.z / terrain.terrainData.size.z);
        UpdateBoundaryProximity(mapPosition.x, mapPosition.z);
        return mapPosition;

    }

    private void UpdateBoundaryProximity(float normalizedX, float normalizedZ)
    {
        nearBoundary = normalizedX <= minBoundaryProximity ||
                       normalizedX >= 1 - minBoundaryProximity ||
                       normalizedZ <= minBoundaryProximity ||
                       normalizedZ >= 1 - minBoundaryProximity;
    }

    private void UpdateTerrainSpeed()
    {
        // Terrain speed is coefficient to modify player speed by based on the texture they are on

        if (inWater)
        {
            // play random walking in water sound
            terrainSpeed = waterSpeed;
        }
        else
        {
            int texturesPresent = 0;
            float totalCoefficients = 0;
            // texture indexes are to match those in Gaia biome Spawner
            for (int textureIndex = 0; textureIndex < textureSpeedCoefficients.Count - 1; textureIndex++)
            {
                if (alphaMapAtPosition[0, 0, textureIndex] > 0)
                {
                    texturesPresent++;
                    totalCoefficients += textureSpeedCoefficients[textureIndex];
                }
            }

            terrainSpeed = texturesPresent > 0 ? totalCoefficients / texturesPresent : 1;
        }
    }
}
