using UnityEngine;

public class EnemyController : MonoBehaviour
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

        // SADECE OK TUÞLARI (Player 1 WASD kullanýyor, karýþmasýn)
        if (Input.GetKey(KeyCode.LeftArrow)) moveH = -1;
        if (Input.GetKey(KeyCode.RightArrow)) moveH = 1;
        if (Input.GetKey(KeyCode.UpArrow)) moveV = 1;
        if (Input.GetKey(KeyCode.DownArrow)) moveV = -1;

        Vector3 movement = new Vector3(moveH, 0.0f, moveV);
        rb.AddForce(movement * speed);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            Debug.Log("SAÐ SHÝFT");
            rb.AddForce(rb.linearVelocity.normalized * ramForce, ForceMode.Impulse);
            if (dashClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(dashClip);
            }
        }


        if (Input.GetKeyDown(KeyCode.Return) && transform.position.y < 3.5f)
        {
            Debug.Log("ENTER");
            rb.AddForce(Vector3.up * jumpForce);
            if (jumpClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(jumpClip);
            }
        }


    }
}