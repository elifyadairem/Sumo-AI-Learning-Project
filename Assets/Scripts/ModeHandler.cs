using UnityEngine;

public class ModeHandler : MonoBehaviour
{
    [Header("Kontrolcü Scriptleri")]
    public EnemyController humanController; // Ok tuþlarý
    public EnemyAIController aiController;    // AI Kontrolcüsü
    public QLearningBrain brain;              // Beyin (Hafýza)

    void Awake()
    {
        // Menüden seçilen modu oku (Varsayýlan 0: 2 Kiþilik)
        int mode = PlayerPrefs.GetInt("GameMode", 0);

        switch (mode)
        {
            case 0: // 2 KÝÞÝLÝK MOD
                Debug.Log("<color=cyan>MOD: 2 Kiþilik (Player vs Player)</color>");
                humanController.enabled = true;
                aiController.enabled = false;
                break;

            case 1: // AKILLI MOD (EÐÝTÝLMÝÞ JSON'U KULLAN)
                Debug.Log("<color=green>MOD: Akýllý AI (Öðrenme Kapalý - Sadece Uygulama)</color>");
                humanController.enabled = false;
                aiController.enabled = true;

                TextAsset brainFile = Resources.Load<TextAsset>("brain_data");

                if (brainFile != null)
                {
                    // Dosyayý bulduysak beyne gönder
                    brain.LoadFromTextAsset(brainFile);
                }
                else
                {
                    Debug.LogError("HATA: Resources klasöründe 'brain_data' bulunamadý!");
                }

                

                //  Epsilon SIFIR: Artýk macera arama, sadece en iyi bildiðin hareketi yap.
                brain.epsilon = 0.0f;

                //  Alpha SIFIR: Tabloyu güncelleme. Öðrenme bitti, sadece sýnavdasýn.
                brain.alpha = 0.0f;

                // AI çalýþsýn diye true yapýyoruz (Ama alpha 0 olduðu için tablo deðiþmeyecek)
                aiController.isSmartMode = true;
                break;

            case 2: // SALAK MOD
                Debug.Log("<color=red>MOD: Rastgele AI (Rastgele)</color>");
                humanController.enabled = false;
                aiController.enabled = true;

                

                // Sarhoþ ayarlarý
                brain.epsilon = 1.0f; // Hep rastgele
                brain.alpha = 0.0f;   // Öðrenme kapalý
                aiController.isSmartMode = false;
                break;
        }
    }
}