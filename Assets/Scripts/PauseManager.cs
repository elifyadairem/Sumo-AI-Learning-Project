using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 

public class Pause_Manager : MonoBehaviour
{
    [Header("UI Elemanlarý")]
    public GameObject pauseMenuPanel; 
    public Slider volumeSlider;       

   
    public static bool isPaused = false;

    void Start()
    {
 
        volumeSlider.value = AudioListener.volume;

 
        pauseMenuPanel.SetActive(false);
    }

    void Update()
    {
      
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame(); 
            }
            else
            {
                PauseGame(); 
            }
        }
    }

   
    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true); 
        Time.timeScale = 0f;            
        isPaused = true;
    }

    
    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false); 
        Time.timeScale = 1f;             
        isPaused = false;
    }

  
    public void SetVolume(float volume)
    {
        
        AudioListener.volume = volume;
    }

    
    public void QuitGame()
    {
        Debug.Log("Oyundan Çýkýlýyor...");

     
        Application.Quit();

      
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}