using UnityEngine;

public class AggressivePlayerBot : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    public float speed = 10f;
    public float arenaLimit = 4.0f; // Kenardan kaçma mesafesi

    [Header("Saldýrý Ayarlarý")]
    public Transform enemyTarget;   // Kime saldýracaðýz?
    public float ramForce = 15f;    // Toslama gücü
    public float attackRange = 5.0f;// Ne kadar yakýndayken saldýrsýn?
    public float attackCooldown = 2.0f; // Kaç saniyede bir saldýrsýn?

    private Rigidbody rb;
    private Vector3 moveDirection;
    private float moveTimer;
    private float attackTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        PickNewDirection();
    }

    void Update()
    {
        // --- 1. Hareket Zamanlayýcýsý ---
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0)
        {
            PickNewDirection();
            moveTimer = 0.5f; // Yarým saniyede bir yön deðiþtir
        }

        // --- 2. Saldýrý Zamanlayýcýsý ---
        attackTimer -= Time.deltaTime;

        // Eðer Enemy menzildeyse ve saldýrý sýrasý geldiyse
        if (enemyTarget != null && attackTimer <= 0)
        {
            float distance = Vector3.Distance(transform.position, enemyTarget.position);
            if (distance < attackRange)
            {
                AttackBehavior();
                attackTimer = attackCooldown; // Soðuma süresini baþlat
            }
        }
    }

    void FixedUpdate()
    {
        // Hareketi uygula
        rb.AddForce(moveDirection * speed);
    }

    void PickNewDirection()
    {
        // Kenardan düþmemek için güvenlik önlemi
        float dist = Vector3.Distance(transform.position, Vector3.zero);

        if (dist > arenaLimit)
        {
            // Merkeze dön
            moveDirection = (Vector3.zero - transform.position).normalized;
        }
        else
        {
            // Rastgele gez ama hafifçe düþmana doðru yönel (Sinsi bot)
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

            // Eðer düþman tanýmlýysa %50 ihtimalle ona doðru, %50 rastgele git
            if (enemyTarget != null && Random.value > 0.5f)
            {
                Vector3 toEnemy = (enemyTarget.position - transform.position).normalized;
                moveDirection = (randomDir + toEnemy).normalized;
            }
            else
            {
                moveDirection = randomDir;
            }
        }
    }

    void AttackBehavior()
    {
        // Enemy'ye doðru ani bir kuvvet (Dash) uygula
        Vector3 attackDir = (enemyTarget.position - transform.position).normalized;

        // Player'ý fýrlat
        rb.AddForce(attackDir * ramForce, ForceMode.Impulse);

        // Ýsteðe baðlý: Bazen zýplasýn (Þaþýrtmak için)
        if (Random.value < 0.3f) // %30 ihtimalle zýpla
        {
            rb.AddForce(Vector3.up * 300f);
        }
    }
}