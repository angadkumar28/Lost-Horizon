using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject coinPrefab;
    public float laneDistance = 3f;
    public float coinSpacing = 2.5f;
    public int maxCoinsInRow = 5;

    [Header("Height Settings")]
    public float groundHeight = 1.5f;
    public float airHeight = 4.0f;
    [Range(0, 1)] public float airChance = 0.3f;

    private float lastZPosition;

    void Start()
    {
        lastZPosition = transform.position.z;
        SpawnCoinRow();
    }

    void Update()
    {
        if (transform.localPosition.z > lastZPosition + 10f)
        {
            RefreshCoins();
        }
        
        lastZPosition = transform.localPosition.z;
    }

    public void RefreshCoins()
    {
        ClearOldCoins();
        SpawnCoinRow();
    }

    void ClearOldCoins()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Coin"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    void SpawnCoinRow()
    {
        if (coinPrefab == null) return;

        int randomLane = Random.Range(-1, 2); 
        float spawnX = randomLane * laneDistance;
        float spawnY = (Random.value < airChance) ? airHeight : groundHeight;
        int coinsToSpawn = Random.Range(3, maxCoinsInRow + 1);

        for (int i = 0; i < coinsToSpawn; i++)
        {
            GameObject newCoin = Instantiate(coinPrefab, transform);
            newCoin.transform.localPosition = new Vector3(spawnX, spawnY, 10f + (i * coinSpacing));
        }
    }
}