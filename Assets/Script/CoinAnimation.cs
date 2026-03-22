using UnityEngine;

public class CoinAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float rotationSpeed = 100f;
    public float bobbingHeight = 0.2f;
    public float bobbingSpeed = 3f;

    private Vector3 startLocalPos;
    private Transform playerTransform;

    void Start()
    {
        startLocalPos = transform.localPosition;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            transform.LookAt(playerTransform);
            transform.Rotate(90, 0, 0);
        }

        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime, Space.Self);
        float newY = startLocalPos.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        transform.localPosition = new Vector3(startLocalPos.x, newY, startLocalPos.z);
    }
}