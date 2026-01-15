using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    [Range(1, 50)]
    public float timeScale = 1.0f; // Editörden hýzý artýrabilirsin

    void Update()
    {
        Time.timeScale = timeScale;
    }
}