using UnityEngine;
using System.Collections; 

public class WallManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject[] walls;      
    public float moveSpeed = 8.0f;  
    public float maxHeight = 1.5f; 
    public float minHeight = -1.0f; 
    public float waitTime = 0.2f;   

    
    private float[] targetYPositions;

    void Start()
    {
      
        targetYPositions = new float[walls.Length];

        
        for (int i = 0; i < walls.Length; i++)
        {
            targetYPositions[i] = walls[i].transform.position.y;
        }

        
        StartCoroutine(RandomWallMover());
    }

    void Update()
    {
        
        for (int i = 0; i < walls.Length; i++)
        {
           
            Vector3 currentPos = walls[i].transform.position;

            
            float newY = Mathf.MoveTowards(currentPos.y, targetYPositions[i], moveSpeed * Time.deltaTime);

            
            walls[i].transform.position = new Vector3(currentPos.x, newY, currentPos.z);
        }
    }

   
    IEnumerator RandomWallMover()
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime); 

           
            int randomIndex = Random.Range(0, walls.Length);

          
            if (Mathf.Abs(targetYPositions[randomIndex] - maxHeight) < 0.1f)
            {
                targetYPositions[randomIndex] = minHeight; 
            }
            else
            {
                targetYPositions[randomIndex] = maxHeight; 
            }
        }
    }
}