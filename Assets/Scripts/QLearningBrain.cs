using System.Collections.Generic;
using UnityEngine;
using System.IO;

// --- JSON PAKETLEME SINIFLARI (Class Dýþýnda Tanýmlý) ---
[System.Serializable]
public class QEntry
{
    public string state;    // Durum Adý (Örn: "Front_Close_Safe")
    public float[] values;  // Puanlar
}

[System.Serializable]
public class QData
{
    public List<QEntry> entries = new List<QEntry>(); // Paket listesi
}

public class QLearningBrain : MonoBehaviour
{
    // Q-Table: Ana Hafýza
    private Dictionary<string, float[]> qTable = new Dictionary<string, float[]>();

    [Header("Hyperparameters")]
    public float alpha = 0.1f;    // Öðrenme Hýzý
    public float gamma = 0.9f;    // Geleceðe verilen önem
    public float epsilon = 1.0f;  // Keþfetme Oraný
    public float epsilonDecay = 0.995f;
    public float minEpsilon = 0.01f;

    private int actionCount = 6;  // 6 Hareket (Ýleri, Geri, Sað, Sol, Zýpla, Dash)
    private string savePath;

    void Awake()
    {
        savePath = Application.persistentDataPath + "/qtable.json";
        LoadQTable();
    }

    void Update()
    {
        // Manuel Kayýt ve Kontrol Tuþlarý
        if (Input.GetKeyDown(KeyCode.S)) SaveQTable();

        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("Klasör açýlýyor: " + Application.persistentDataPath);
            Application.OpenURL(Application.persistentDataPath);
        }
    }

    // --- GELÝÞMÝÞ STATE MANTIÐI (72 Farklý Durum) ---
    public string GetStateKey(Vector3 myPos, Vector3 targetPos, bool isGrounded, float distanceToEdge)
    {
        Vector3 relativePos = targetPos - myPos;

        // 1. YÖN (12 Dilim - Saat Yönü Gibi)
        // Düþmanýn tam olarak nerede olduðunu anlamak için 360 dereceyi 30'ar derecelik 12 dilime böleriz.
        float angle = Mathf.Atan2(relativePos.x, relativePos.z) * Mathf.Rad2Deg;
        int angleIndex = Mathf.RoundToInt(angle / 30.0f); // -6 ile +6 arasý bir sayý üretir
        string direction = "Dir" + angleIndex; // Örn: "Dir-3" (Sol Arka), "Dir0" (Tam Ön)

        // 2. MESAFE (5 Kademe)
        // Mesafeyi hassaslaþtýrýyoruz ki ne zaman saldýrýp ne zaman kovalayacaðýný bilsin.
        float dist = Vector3.Distance(myPos, targetPos);
        string distanceState = "";
        if (dist < 1.5f) distanceState = "Melee";       // Vurma mesafesi (Dash at!)
        else if (dist < 3.0f) distanceState = "Close";  // Çok yakýn (Hazýrlan)
        else if (dist < 6.0f) distanceState = "Medium"; // Orta (Kovala)
        else if (dist < 10.0f) distanceState = "Far";   // Uzak (Yaklaþ)
        else distanceState = "VeryFar";                 // Çok Uzak (Ara)

        // 3. GÜVENLÝK (Kenar Kontrolü - 3 Kademe)
        string safety = "";
        if (distanceToEdge < 1.0f) safety = "Critical"; // Düþmek üzeresin! (Acil kaç)
        else if (distanceToEdge < 2.5f) safety = "Warning"; // Kenara yaklaþtýn (Dikkatli ol)
        else safety = "Safe"; // Ortadasýn (Rahat ol)

        // 4. DURUM (Hava / Yer - 2 Kademe)
        // AI havada olduðunu bilirse boþuna zýplamaya çalýþmaz veya havada Dash atabilir.
        // Yüksekliði 1.2f'den büyükse havada kabul ediyoruz.
        string airStatus = myPos.y > 1.2f ? "Air" : "Gnd";

        // Örnek Çýktý: "Dir0_Melee_Safe_Gnd" (Önümde, Vurma mesafesinde, Güvendeyim, Yerdeyim)
        return $"{direction}_{distanceState}_{safety}_{airStatus}";
    }

    // --- KARAR VERME (Choose Action) ---
    public int ChooseAction(string state)
    {
        if (!qTable.ContainsKey(state))
        {
            qTable[state] = new float[actionCount]; // Yeni state keþfedildi
        }

        // Exploration (Keþfet)
        if (Random.value < epsilon)
        {
            return Random.Range(0, actionCount);
        }

        // Exploitation (Sömür - En iyiyi seç)
        float[] values = qTable[state];
        int bestAction = 0;
        float maxVal = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > maxVal)
            {
                maxVal = values[i];
                bestAction = i;
            }
        }
        return bestAction;
    }

    // --- ÖÐRENME (Bellman Equation) ---
    public void Learn(string state, int action, float reward, string nextState)
    {
        if (!qTable.ContainsKey(nextState))
            qTable[nextState] = new float[actionCount];

        float currentQ = qTable[state][action];

        // Gelecekteki en iyi hamlenin deðeri
        float maxNextQ = qTable[nextState][0];
        foreach (float val in qTable[nextState])
            if (val > maxNextQ) maxNextQ = val;

        // Formül: Q_new = Q_old + alpha * (Reward + gamma * maxQ_next - Q_old)
        float newQ = currentQ + alpha * (reward + gamma * maxNextQ - currentQ);

        qTable[state][action] = newQ;

        // State sayýsýný logluyoruz (Debug için)
        // Debug.Log($"Öðrenilen State: {qTable.Count}");
    }

    public void DecayEpsilon()
    {
        if (epsilon > minEpsilon)
            epsilon *= epsilonDecay;
    }

    // --- KAYDETME ---
    public void SaveQTable()
    {
        QData data = new QData();
        foreach (var pair in qTable)
        {
            QEntry entry = new QEntry();
            entry.state = pair.Key;
            entry.values = pair.Value;
            data.entries.Add(entry);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Tablo Kaydedildi: " + savePath);
    }

    // --- YÜKLEME ---
    public void LoadQTable()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            QData data = JsonUtility.FromJson<QData>(json);

            qTable.Clear();
            foreach (var entry in data.entries)
            {
                qTable[entry.state] = entry.values;
            }
            Debug.Log($"Eski beyin yüklendi. Hafýzada {qTable.Count} state var.");
        }
        else
        {
            Debug.LogWarning("Kayýt dosyasý bulunamadý, sýfýrdan baþlanýyor.");
        }
    }

    void OnApplicationQuit()
    {
        SaveQTable();
    }

    // ... (Üstteki kodlarýn aynen kalsýn) ...

    // YENÝ EKLENECEK FONKSÝYON:
    // Bu fonksiyon Resources'tan gelen dosyayý alýp Q-Table'a dönüþtürür.
    // --- YÜKLEME (Resources - WebGL ve Final Build Ýçin) ---
    // Bu fonksiyon Resources klasöründeki TextAsset dosyasýný okur
    public void LoadFromTextAsset(TextAsset file)
    {
        // 1. Dosya Kontrolü
        if (file == null)
        {
            Debug.LogError("HATA: Yüklenecek beyin dosyasý (TextAsset) boþ!");
            return;
        }

        // 2. Metni Oku
        string jsonText = file.text;

        // 3. Metni Veriye Çevir
        QData data = JsonUtility.FromJson<QData>(jsonText);

        // 4. Hafýzayý Temizle ve Yükle
        qTable.Clear();
        foreach (var entry in data.entries)
        {
            qTable[entry.state] = entry.values;
        }

        // --- ÝÞTE LOG BURAYA GELÝYOR ---
        // Döngü bitti, hafýza doldu, þimdi baþarý mesajýný veriyoruz:
        Debug.Log($"<color=green>BAÞARILI:</color> Resources üzerinden beyin yüklendi. {qTable.Count} adet durum (state) hafýzaya alýndý.");
    }
} // Class sonu
