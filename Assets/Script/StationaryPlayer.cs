using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StationaryPlayer : MonoBehaviour
{
    private CharacterController controller;
    private Animator anim;
    private WorldGenerator worldGenerator; 

    [Header("Movement Settings")]
    public float laneDistance = 3f;
    public float sideSpeed = 15f;
    private int targetLane = 1; 

    [Header("Jump & Gravity")]
    public float jumpForce = 12f;
    public float gravity = -30f; 
    private float verticalVelocity;

    [Header("Slide Settings")]
    public float slideDuration = 1.0f;
    private bool isSliding = false;
    private float originalHeight;
    private Vector3 originalCenter;

    [Header("UI Panels & Text")]
    public GameObject mainMenuPanel; 
    public GameObject gameOverPanel; 
    public GameObject pausePanel;      
    public GameObject pauseButton;     
    public Text coinText;             
    public Text finalScoreText;      
    public Text highScoreText;       
    public Text countdownText;         
    private int coinCount = 0;

    [Header("Audio Settings")]
    public AudioSource musicSource;   
    public AudioSource sfxSource;     
    public AudioClip backgroundMusic;
    public AudioClip jumpSound;
    public AudioClip deathSound;
    public AudioClip coinSound;       
    public AudioClip countdownTickSound;
    public AudioClip countdownGoSound;

    public static bool skipMenuOnRestart = false;
    private bool blockInputThisFrame = false;
    private Vector2 startTouch;
    private bool isDead = false;
    private bool isGameStarted = false; 
    private bool isPaused = false;

    void Start()
    {
        Application.targetFrameRate = 60;
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        worldGenerator = Object.FindFirstObjectByType<WorldGenerator>();
        
        originalHeight = controller.height;
        originalCenter = controller.center;

        isDead = false;
        coinCount = 0; 
        
        if (coinText != null) coinText.gameObject.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(false); 
        if (pausePanel != null) pausePanel.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        if (skipMenuOnRestart)
        {
            StartGame();
            skipMenuOnRestart = false; 
        }
        else
        {
            isGameStarted = false;
            Time.timeScale = 0;
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (blockInputThisFrame || isPaused)
        {
            blockInputThisFrame = false;
            return;
        }

        if (!isGameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
            }
            return; 
        }

        if (isDead) return;

        HandleInput();

        float targetX = (targetLane - 1) * laneDistance;
        float nextX = Mathf.MoveTowards(transform.position.x, targetX, sideSpeed * Time.deltaTime);
        float moveX = nextX - transform.position.x;

        if (controller.isGrounded)
        {
            verticalVelocity = -2f; 
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || CheckSwipe("Up"))
                Jump();
            else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || CheckSwipe("Down")) && !isSliding)
                StartCoroutine(Slide());
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
            if (transform.position.y < -1f) Die();
        }

        controller.Move(new Vector3(moveX, verticalVelocity * Time.deltaTime, 0));
    }

    public void StartGame()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        if (coinText != null) coinText.gameObject.SetActive(true); 
        if (pauseButton != null) pauseButton.SetActive(true); 

        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        Time.timeScale = 1;
        isGameStarted = true;
        isDead = false;
        blockInputThisFrame = true;
        UpdateCoinUI();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0; 
        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false);
        if (musicSource != null) musicSource.Pause();
    }

    public void ResumeButton()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        StartCoroutine(ResumeCountdown());
    }

    private IEnumerator ResumeCountdown()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "3";
            countdownText.color = Color.red;
            if (sfxSource != null && countdownTickSound != null) sfxSource.PlayOneShot(countdownTickSound);
            yield return new WaitForSecondsRealtime(1f); 
            countdownText.text = "2";
            countdownText.color = Color.yellow;
            if (sfxSource != null && countdownTickSound != null) sfxSource.PlayOneShot(countdownTickSound);
            yield return new WaitForSecondsRealtime(1f);
            countdownText.text = "1";
            countdownText.color = Color.white;
            if (sfxSource != null && countdownTickSound != null) sfxSource.PlayOneShot(countdownTickSound);
            yield return new WaitForSecondsRealtime(1f);
            countdownText.text = "GO!";
            countdownText.color = Color.green;
            if (sfxSource != null && countdownGoSound != null) sfxSource.PlayOneShot(countdownGoSound);
            yield return new WaitForSecondsRealtime(0.5f);
            
            countdownText.gameObject.SetActive(false);
        }

        Time.timeScale = 1;
        isPaused = false;
        if (pauseButton != null) pauseButton.SetActive(true);
        if (musicSource != null) musicSource.UnPause();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (coinText != null) coinText.gameObject.SetActive(false); 
        if (pauseButton != null) pauseButton.SetActive(false); 

        if (musicSource != null) musicSource.Stop();
        if (sfxSource != null && deathSound != null) sfxSource.PlayOneShot(deathSound);

        if (worldGenerator != null) worldGenerator.StopWorld();
        if (anim != null) anim.SetTrigger("Stumble");

        int savedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (coinCount > savedHighScore)
        {
            PlayerPrefs.SetInt("HighScore", coinCount);
            savedHighScore = coinCount;
        }

        if (finalScoreText != null) finalScoreText.text = "Score: " + coinCount;
        if (highScoreText != null) highScoreText.text = "High Score: " + savedHighScore;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(2.0f);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0; 
    }

    private void CollectCoin(GameObject coin)
    {
        coinCount++;
        UpdateCoinUI();
        if (sfxSource != null && coinSound != null) sfxSource.PlayOneShot(coinSound);
        Destroy(coin);
    }

    private void UpdateCoinUI()
    {
        if (coinText != null) coinText.text = "Coins: " + coinCount.ToString();
    }

    public void RestartGame()
    {
        skipMenuOnRestart = true; 
        Time.timeScale = 1; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        skipMenuOnRestart = false; 
        Time.timeScale = 1; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void HandleInput()
    {
        if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && targetLane > 0) targetLane--;
        if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && targetLane < 2) targetLane++;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) startTouch = t.position;
            else if (t.phase == TouchPhase.Ended)
            {
                float x = t.position.x - startTouch.x;
                float y = t.position.y - startTouch.y;
                if (Mathf.Abs(x) > Mathf.Abs(y) && Mathf.Abs(x) > 50f)
                {
                    if (x > 0 && targetLane < 2) targetLane++;
                    else if (x < 0 && targetLane > 0) targetLane--;
                }
            }
        }
    }

    private bool CheckSwipe(string direction)
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Ended)
            {
                float yDist = (t.position.y - startTouch.y) / Screen.height;
                if (direction == "Up" && yDist > 0.05f) return true;
                if (direction == "Down" && yDist < -0.05f) return true;
            }
        }
        return false;
    }

    private void Jump()
    {
        if (isSliding) ResetCollider();
        verticalVelocity = jumpForce;
        if (sfxSource != null && jumpSound != null) sfxSource.PlayOneShot(jumpSound);
        if (anim != null) anim.SetTrigger("Jump");
    }

    private IEnumerator Slide()
    {
        isSliding = true;
        if (anim != null) anim.SetTrigger("Slide");
        controller.height = originalHeight * 0.5f;
        controller.center = new Vector3(originalCenter.x, originalCenter.y * 0.5f, originalCenter.z);
        yield return new WaitForSeconds(slideDuration);
        if (!isDead) ResetCollider();
    }

    private void ResetCollider()
    {
        controller.height = originalHeight;
        controller.center = originalCenter;
        isSliding = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (other.CompareTag("Coin")) CollectCoin(other.gameObject);
        else if (other.CompareTag("Obstacle") || other.CompareTag("JumpObstacle") || other.CompareTag("SlideObstacle")) Die();
    }
}