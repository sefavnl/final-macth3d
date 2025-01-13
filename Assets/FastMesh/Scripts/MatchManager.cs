using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager'ý kullanabilmek için
using System.Collections; // IEnumerator için gerekli namespace
using System.Collections.Generic;

public class MatchManager : MonoBehaviour
{
    public ParticleSystem destroyEffect;
    private List<GameObject> matchingObjects = new List<GameObject>();
    public float destroyDelay = 0.5f;
    public Color defaultColor = Color.white;
    public Color mismatchColor = Color.red;
    public Color successColor = Color.green;
    public float mismatchDuration = 1f;
    public float successDuration = 1f;

    private Renderer planeRenderer;
    private bool isPairingInProgress = false;

    private int score = 0;
    private GUIStyle scoreStyle;
    private int successfulMatches = 0;

    private bool showMismatchMessage = false;
    private GUIStyle mismatchMessageStyle;

    // Oyun durumu deðiþkenleri
    private bool gameStarted = false;
    private bool gameEnded = false;
    private float gameTime = 60f; // 60 saniye oyun süresi
    private float timeLeft;

    private GUIStyle startButtonStyle;
    private GUIStyle gameOverStyle;
    private GUIStyle matchCountStyle;
    private GUIStyle timeLeftStyle;
    private GUIStyle restartButtonStyle;
    private GUIStyle resetButtonStyle;
    private GUIStyle extraTimeButtonStyle;

    // Ekstra zaman deðiþkenleri
    public float extraTimeAmount = 10f; // Ekstra zaman miktarý
    private bool canUseExtraTime = true; // Ekstra zaman düðmesinin durumu

    void Start()
    {
        planeRenderer = GetComponent<Renderer>();
        if (planeRenderer != null)
        {
            planeRenderer.material.color = defaultColor;
        }

        // Skor yazýsý için stil
        scoreStyle = new GUIStyle
        {
            fontSize = 25,
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft,
            font = (Font)Resources.Load("Fonts/Arial-Bold") // Kendi fontunuza göre deðiþtirebilirsiniz.
        };

        // Yanlýþ eþleþme mesajý için stil
        mismatchMessageStyle = new GUIStyle
        {
            fontSize = 20,
            normal = { textColor = Color.red },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        // Oyun bitirme ve baþlama mesajlarý için stil
        startButtonStyle = new GUIStyle
        {
            fontSize = 40,
            normal = { textColor = Color.yellow },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 10, 10),
            border = new RectOffset(5, 5, 5, 5)
        };

        gameOverStyle = new GUIStyle
        {
            fontSize = 50,
            normal = { textColor = Color.red },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        matchCountStyle = new GUIStyle
        {
            fontSize = 20,
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperRight
        };

        timeLeftStyle = new GUIStyle
        {
            fontSize = 20,
            normal = { textColor = Color.green },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft
        };

        restartButtonStyle = new GUIStyle
        {
            fontSize = 40,
            normal = { textColor = Color.green },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 10, 10),
            border = new RectOffset(5, 5, 5, 5)
        };

        resetButtonStyle = new GUIStyle
        {
            fontSize = 20,
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 10, 10),
            border = new RectOffset(3, 3, 3, 3)
        };

        extraTimeButtonStyle = new GUIStyle
        {
            fontSize = 20,
            normal = { textColor = Color.cyan },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 10, 10),
            border = new RectOffset(3, 3, 3, 3)
        };
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameStarted && other.CompareTag("Matchable"))  // Sadece oyun baþladýysa iþlem yap
        {
            if (matchingObjects.Count > 0 && matchingObjects[0].GetComponent<ItemType>().type != other.GetComponent<ItemType>().type)
            {
                Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 throwDirection = (other.transform.position - matchingObjects[0].transform.position).normalized;
                    rb.AddForce(throwDirection * 500f);
                }

                StartCoroutine(IndicateMismatchAfterDelay());
                return;
            }

            matchingObjects.Add(other.gameObject);
            other.gameObject.GetComponent<Collider>().enabled = false;
            CheckForMatchingObjects();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (matchingObjects.Contains(other.gameObject))
        {
            matchingObjects.Remove(other.gameObject);
        }
    }

    void CheckForMatchingObjects()
    {
        if (matchingObjects.Count < 2 || isPairingInProgress)
            return;

        int matchCount = 0;
        string typeToMatch = matchingObjects[matchingObjects.Count - 1].GetComponent<ItemType>().type;

        foreach (GameObject obj in matchingObjects)
        {
            if (obj != null && obj.GetComponent<ItemType>().type == typeToMatch)
            {
                matchCount++;
            }
        }

        if (matchCount >= 2)
        {
            isPairingInProgress = true;
            StartCoroutine(AnimateAndDestroyMatchingObjects(typeToMatch));
            StartCoroutine(IndicateSuccessAfterDelay());
            UpdateScore(matchCount);
        }
        else
        {
            StartCoroutine(IndicateMismatchAfterDelay());
        }
    }

    IEnumerator AnimateAndDestroyMatchingObjects(string typeToMatch)
    {
        yield return new WaitForSeconds(destroyDelay);

        for (int i = matchingObjects.Count - 1; i >= 0; i--)
        {
            if (matchingObjects[i] != null && matchingObjects[i].GetComponent<ItemType>().type == typeToMatch)
            {
                Animator animator = matchingObjects[i].GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger("LockInPlace");
                }

                ParticleSystem effect = Instantiate(destroyEffect, matchingObjects[i].transform.position, Quaternion.identity);
                effect.Play();

                Destroy(matchingObjects[i]);
                matchingObjects.RemoveAt(i);

                Destroy(effect.gameObject, effect.main.duration);
            }
        }

        isPairingInProgress = false;
    }

    IEnumerator IndicateMismatchAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        if (planeRenderer != null)
        {
            planeRenderer.material.color = mismatchColor;

            yield return new WaitForSeconds(mismatchDuration);

            planeRenderer.material.color = defaultColor;
        }

        showMismatchMessage = true;

        yield return new WaitForSeconds(2f);

        showMismatchMessage = false;
    }

    IEnumerator IndicateSuccessAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        if (planeRenderer != null)
        {
            planeRenderer.material.color = successColor;

            yield return new WaitForSeconds(successDuration);

            planeRenderer.material.color = defaultColor;
        }
    }

    void UpdateScore(int matchCount)
    {
        score += matchCount * 10;
        successfulMatches++;
    }

    void StartGame()
    {
        gameStarted = true;
        gameEnded = false;
        score = 0;
        successfulMatches = 0;
        timeLeft = gameTime;
        matchingObjects.Clear();
        StartCoroutine(GameTimer());
    }

    void EndGame()
    {
        gameEnded = true;
        gameStarted = false;
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Sahneyi yeniden baþlat
    }

    void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Sahneyi yeniden baþlat
    }

    IEnumerator GameTimer()
    {
        while (timeLeft > 0 && gameStarted)  // gameStarted kontrolü eklenmiþ
        {
            timeLeft -= Time.deltaTime;
            yield return null;
        }
        EndGame();
    }

    void AddExtraTime()
    {
        timeLeft += extraTimeAmount;
        canUseExtraTime = false; // Düðmeyi devre dýþý býrak
        StartCoroutine(EnableExtraTimeButton());
    }

    IEnumerator EnableExtraTimeButton()
    {
        yield return new WaitForSeconds(5f); // 5 saniye bekleme
        canUseExtraTime = true; // Düðmeyi yeniden etkinleþtir
    }

    void OnGUI()
    {
        if (!gameStarted && !gameEnded)
        {
            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 50), "Baþla", startButtonStyle))
            {
                StartGame();
            }
        }
        else if (gameEnded)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 150, 200, 50), "Oyun Bitti!", gameOverStyle);
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 90, 150, 50), "Skor: " + score, matchCountStyle);
            if (GUI.Button(new Rect(Screen.width / 2 - 90, Screen.height / 2, 200, 50), "Tekrar Oyna", restartButtonStyle))
            {
                RestartGame();
            }
        }

        if (gameStarted)
        {
            GUI.Label(new Rect(10, 10, 200, 50), "Skor: " + score, scoreStyle);
            GUI.Label(new Rect(10, 50, 130, 50), "Eþleþmeler: " + successfulMatches, matchCountStyle);
            GUI.Label(new Rect(Screen.width - 150, 10, 140, 50), "Kalan Süre: " + Mathf.Ceil(timeLeft), timeLeftStyle);

            if (canUseExtraTime)
            {
                if (GUI.Button(new Rect(Screen.width - 150, Screen.height - 200, 120, 50), "Ekstra Zaman", extraTimeButtonStyle))
                {
                    AddExtraTime();
                }
            }
            else
            {
                GUI.Label(new Rect(Screen.width - 150, Screen.height - 200, 120, 50), "Bekliyor...", extraTimeButtonStyle);
            }
        }

        if (showMismatchMessage)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 50), "Yanlýþ Eþleþme!", mismatchMessageStyle);
        }

        if (GUI.Button(new Rect(Screen.width - 150, Screen.height - 100, 120, 50), "Sýfýrla", resetButtonStyle))
        {
            ResetGame();
        }
    }
}
