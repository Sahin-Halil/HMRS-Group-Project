using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class KeyCardManager : MonoBehaviour
{
    public static KeyCardManager Instance;

    [SerializeField] private int totalCards = 8;
    [SerializeField] private TMP_Text keyCardText;

    private int collectedCards = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateUI();
    }

    public void CollectCard(int cardNumber)
    {
        collectedCards++;
        UpdateUI();
    }

    public bool HasAllCards()
    {
        return collectedCards >= totalCards;
    }

    public int GetCollectedCards()
    {
        return collectedCards;
    }

    public void SetCollectedCards(int count)
    {
        collectedCards = count;
        Debug.Log($"Key cards set to: {collectedCards}/{totalCards}");
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (keyCardText == null)
        {
            GameObject textObj = GameObject.Find("KeyCardText");
            if (textObj != null)
            {
                keyCardText = textObj.GetComponent<TMP_Text>();
            }
            else
            {
                Debug.LogWarning("KeyCardText not found in scene");
            }
        }

        if (keyCardText != null)
        {
            keyCardText.text = $"Key Cards: {collectedCards}/{totalCards}";
        }
    }

    public void ResetCards()
    {
        collectedCards = 0;

        // Re-enable all key cards
        KeyCard[] allCards = FindObjectsOfType<KeyCard>(true);
        foreach (KeyCard card in allCards)
        {
            card.gameObject.SetActive(true);
        }

        UpdateUI();
    }
}