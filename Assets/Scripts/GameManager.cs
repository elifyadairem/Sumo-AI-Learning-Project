using UnityEngine;
using UnityEngine.SceneManagement; // Sahne yüklemek için þart
using System.Collections; // Delay (Bekleme) için gerekli

public class GameManager : MonoBehaviour
{
    [Header("Takip Edilecekler")]
    public Transform player;
    public Transform enemy;

    [Header("Ayarlar")]
    public float fallLimit = -5.0f; 

    private bool isGameOver = false;

    void Update()
    {
        
        if (isGameOver) return;

        
        if (player.position.y < fallLimit)
        {
            EndGame("Kaybettin!");
        }

        
        else if (enemy.position.y < fallLimit)
        {
            EndGame("Kazandýn!");
        }
    }

    void EndGame(string message)
    {
        isGameOver = true; 
        Debug.Log(message); 

        StartCoroutine(RestartLevel());
    }

    IEnumerator RestartLevel()
    {
        Debug.Log("Oyun bitti, menüye dönülüyor...");

        
        yield return new WaitForSeconds(2f);

    
        
        SceneManager.LoadScene(0);
    }
}