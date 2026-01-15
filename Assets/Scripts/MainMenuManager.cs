using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuManager : MonoBehaviour
{
    



    public void QuitGame()
    {
        Debug.Log("Oyundan Çýkýlýyor...");

        Application.Quit();

 
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void Start2PlayerGame()
    {
        PlayerPrefs.SetInt("GameMode", 0); // 0 = 2 Kiþilik
        SceneManager.LoadScene(1);
    }

    public void StartSmartMode()
    {
        // 1 = Akýllý, 0 = Salak
        PlayerPrefs.SetInt("GameMode", 1);
        SceneManager.LoadScene(1); // Oyun sahnesini aç
    }

    // SALAK MOD BUTONU ÝÇÝN
    public void StartDumbMode()
    {
        PlayerPrefs.SetInt("GameMode", 2);
        SceneManager.LoadScene(1);
    }
}