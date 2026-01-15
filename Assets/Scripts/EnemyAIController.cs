using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    public float speed = 10f;
    public float ramForce = 10f;
    public float jumpForce = 300f;

    [Header("AI Bileşenleri")]
    public Transform playerTarget;
    public QLearningBrain brain;
    public Transform groundCheck; // Kenardan düşmeyi algılamak için

    private Rigidbody rb;
    private string currentState;
    private int currentAction;
    private bool isTraining = true;

    public bool isSmartMode = true;

    // Ödül hesaplama değişkenleri
    private Vector3 startPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;

        // İlk durumu al
    

        currentState = brain.GetStateKey(transform.position, playerTarget.position, true, GetDistanceToEdge());
        // Sonsuz döngüde karar verme mekanizmasını başlat
        InvokeRepeating("MakeDecision", 0f, 0.1f); // Saniyede 5 karar (Hızlı tepki)
    }

    // Kenara olan mesafeyi ölçmek için basit bir Raycast mantığı
    float GetDistanceToEdge()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
            return 10f; // Altımızda zemin var
        return 0f; // Zemin yok, tehlike!
    }

    void MakeDecision()
    {
        if (!isTraining) return;

        // 1. Mevcut durumu kaydetmiştim (currentState)

        // 2. Action Seç
        currentAction = brain.ChooseAction(currentState);

        // 3. Hareketi Uygula
        ExecuteAction(currentAction);
    }

    void ExecuteAction(int action)
    {
        Vector3 movement = Vector3.zero;

        switch (action)
        {
            case 0: movement = Vector3.forward; break; // W
            case 1: movement = Vector3.back; break;    // S
            case 2: movement = Vector3.left; break;    // A
            case 3: movement = Vector3.right; break;   // D
            case 4: // Zıpla
                if (transform.position.y < 2.0f)
                    rb.AddForce(Vector3.up * jumpForce);
                break;
            case 5: // Dash
                rb.AddForce(rb.linearVelocity.normalized * ramForce, ForceMode.Impulse);
                break;
        }

        if (action < 4) // Sadece hareket actionları için
            rb.AddForce(movement * speed);
    }

    void Update()
    {
        // 1. Yeni durumu (State) al
        string nextState = brain.GetStateKey(transform.position, playerTarget.position, true, GetDistanceToEdge());

        float reward = 0;
        bool episodeEnded = false; // Tur bitti mi?

        // --- ENEMY DÜŞTÜ MÜ? (Kötü Sonuç) ---
        // GameManager'daki fallLimit -5.0f idi
        if (transform.position.y < -4.0f)
        {
            reward = -100f; // Büyük Ceza
            episodeEnded = true;
        }

        // --- PLAYER DÜŞTÜ MÜ? (İyi Sonuç - Kazandın) ---
        else if (playerTarget.position.y < -4.0f)
        {
            reward = 50f; // Player düştüyse biz kazanmışızdır
            episodeEnded = true;
        }

        // --- HAYATTA KALMA ÖDÜLÜ ---
        else
        {
            reward = -0.1f; // Hafif ceza veriyoruz ki "sabit durmak" yerine maçı bitirmeye çalışsın (Time Penalty)
            //.
        }

        if (isSmartMode)
        {
            // Öğrenme fonksiyonunu çağır
            brain.Learn(currentState, currentAction, reward, nextState);
        }
        currentState = nextState;

        // --- RESETLEME (Scene Reload YERİNE Işınlama) ---
        if (episodeEnded)
        {
            ResetPositions(); 
        }
    }

    void ResetPositions()
    {
        // --- 1. ENEMY RESET ---
        // Hızları sıfırla ki eski momentumla uçmasın
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Rastgele bir konum belirle (X ve Z ekseninde)
        // Böylece hep aynı senaryoyu ezberlemez
        float randomX_Enemy = Random.Range(-3f, 3f);
        float randomZ_Enemy = Random.Range(-3f, 3f);

        // Y değerini 2.0f yapıyoruz ki havadan düşsün, yere gömülmesin
        transform.position = new Vector3(randomX_Enemy, 2.0f, randomZ_Enemy);

        // Rotasyonu düzelt (Yere düz bassın)
        transform.rotation = Quaternion.identity;


        // --- 2. PLAYER RESET ---
        Rigidbody playerRb = playerTarget.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            // Player'ı uyutup uyandırarak fiziği tazeleyebiliriz (Opsiyonel)
            playerRb.Sleep();
            playerRb.WakeUp();
        }

        // Player'ı Enemy'den uzağa veya rastgele başka bir yere koy
        // Çakışmamaları için Enemy'nin tam tersine koyabiliriz veya tamamen rastgele yapabiliriz.
        // Şimdilik tamamen rastgele ama arenanın içinde:
        float randomX_Player = Random.Range(-3f, 3f);
        float randomZ_Player = Random.Range(-3f, 3f);

        // Player'ı da havadan (Y = 2.0f) başlatıyoruz
        playerTarget.position = new Vector3(randomX_Player, 2.0f, randomZ_Player);

        // Player'ın da dik durmasını sağla
        playerTarget.rotation = Quaternion.identity;

        if (isSmartMode)
        {
            brain.DecayEpsilon();
        }
    }

    // Çarpışma algılama (Player'a vurdu mu?)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Player'a vurduk! Büyük ödül.
            brain.Learn(currentState, currentAction, 50f, currentState);
        }
    }

    // Bölümü yeniden başlatmak yerine pozisyonları resetle (Hızlı Eğitim İçin)
 
}