using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    public GameObject[] segmentPrefabs;
    public float segmentLength = 210f;
    public int initialSegments = 3;
    public float worldSpeed = 15f;

    [Header("Obstacle Prefabs")]
    public GameObject jumpObstaclePrefab;
    public GameObject slideObstaclePrefab;
    public GameObject wallObstaclePrefab;
    public float obstacleYOffset = 2f;
    public float laneDistance = 3f;
    public int obstacleWavesPerSegment = 12; 

    private List<GameObject> activeSegments = new List<GameObject>();
    private Transform worldAnchor;
    private bool isGameOver = false;
    private int lastExitLane = 1;

    void Start()
    {
        GameObject anchorObj = new GameObject("WorldAnchor");
        worldAnchor = anchorObj.transform;

        for (int i = 0; i < initialSegments; i++)
        {
            float safeZone = (i == 0) ? 100f : 0f;
            SpawnSegment(i * segmentLength, safeZone);
        }
    }

    void Update()
    {
        if (isGameOver) return;
        worldAnchor.Translate(Vector3.back * worldSpeed * Time.deltaTime);

        if (activeSegments.Count > 0)
        {
            float firstSegmentWorldZ = worldAnchor.position.z + activeSegments[0].transform.localPosition.z;
            if (firstSegmentWorldZ < -segmentLength)
            {
                MoveSegmentForward();
            }
        }
    }

    void SpawnSegment(float zPos, float safeZone = 0f)
    {
        GameObject go = Instantiate(segmentPrefabs[Random.Range(0, segmentPrefabs.Length)], new Vector3(0, 0, zPos), Quaternion.identity);
        go.transform.SetParent(worldAnchor);
        StartCoroutine(GenerateObstaclesSlowly(go, safeZone));
        activeSegments.Add(go);
    }

    public void MoveSegmentForward()
    {
        GameObject go = activeSegments[0];
        activeSegments.RemoveAt(0);

        float lastZ = activeSegments[activeSegments.Count - 1].transform.localPosition.z;
        go.transform.localPosition = new Vector3(0, 0, lastZ + segmentLength);
        foreach (Transform child in go.transform)
        {
            if (child.CompareTag("Obstacle") || child.CompareTag("JumpObstacle") || child.CompareTag("SlideObstacle")) 
                Destroy(child.gameObject);
        }

        StartCoroutine(GenerateObstaclesSlowly(go, 0f)); 
        activeSegments.Add(go);
    }

    IEnumerator GenerateObstaclesSlowly(GameObject segment, float minZ)
    {
        int currentSafeLane = lastExitLane;
        float distanceBetweenWaves = segmentLength / obstacleWavesPerSegment;

        for (int i = 0; i < obstacleWavesPerSegment; i++)
        {
            if (isGameOver) yield break;

            float zPos = (i * distanceBetweenWaves) + Random.Range(5f, distanceBetweenWaves - 5f);
            if (zPos < minZ) continue;

            if (Random.Range(0, 10) < 3) 
                currentSafeLane = Mathf.Clamp(currentSafeLane + (Random.value > 0.5f ? 1 : -1), 0, 2);

            for (int lane = 0; lane < 3; lane++)
            {
                if (lane != currentSafeLane && Random.value < 0.6f)
                {
                    SpawnObstacle(segment, lane, zPos);
                }
            }
            yield return null; 
        }
        lastExitLane = currentSafeLane;
    }

    void SpawnObstacle(GameObject segment, int lane, float zPos)
    {
        float laneX = (lane - 1) * laneDistance;
        GameObject prefabToSpawn = null;
        string selectedTag = "Obstacle";

        float typeRand = Random.value;
        if (typeRand < 0.33f) { prefabToSpawn = jumpObstaclePrefab; selectedTag = "JumpObstacle"; }
        else if (typeRand < 0.66f) { prefabToSpawn = slideObstaclePrefab; selectedTag = "SlideObstacle"; }
        else { prefabToSpawn = wallObstaclePrefab; }

        if (prefabToSpawn != null)
        {
            GameObject obs = Instantiate(prefabToSpawn, segment.transform);
            obs.transform.localPosition = new Vector3(laneX, obstacleYOffset, zPos);
            obs.tag = selectedTag;

            Collider col = obs.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }
    }

    public void StopWorld() => isGameOver = true;
}