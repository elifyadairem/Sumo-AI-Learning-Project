using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 10f;
    public float ramForce = 10f;
    public float jumpForce = 300f;

    [Header("Ses Efektleri")]
    public AudioSource audioSource; 
    public AudioClip jumpClip;      
    public AudioClip dashClip;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    void FixedUpdate()
    {
      
        float moveH = 0;
        float moveV = 0;

        if (Input.GetKey(KeyCode.A)) moveH = -1;
        if (Input.GetKey(KeyCode.D)) moveH = 1;
        if (Input.GetKey(KeyCode.W)) moveV = 1;
        if (Input.GetKey(KeyCode.S)) moveV = -1;

        Vector3 movement = new Vector3(moveH, 0.0f, moveV);
        rb.AddForce(movement * speed);
    }

    void Update()
    {
        // SOL SHIFT: Toslama
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            rb.AddForce(rb.linearVelocity.normalized * ramForce, ForceMode.Impulse);

            if (dashClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(dashClip); 
            }
        }

        
        if (Input.GetKeyDown(KeyCode.Space) && transform.position.y < 3.5f)
        {
            Debug.Log("SPACE");
            rb.AddForce(Vector3.up * jumpForce);

            
            if (jumpClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(jumpClip); 
            }
        }

     
    }
}