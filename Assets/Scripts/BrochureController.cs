using UnityEngine;
using UnityEngine.UI; // UI elemanlarýna eriþim için
using Firebase;         // Firebase çekirdek kütüphanesi
using Firebase.Database; // Realtime Database
using Firebase.Extensions; // Task extension'larý (Main Thread için þart)
using System.Collections.Generic; // Dictionary kullanýmý için

public class BrochureController : MonoBehaviour
{
    [Header("Data Settings")]
    [Tooltip("Firebase'deki node ismi. Örn: car_01")]
    public string brochureID;

    [Header("UI References")]
    public Text titleText;
    public Text priceText;

    // Veritabaný kök referansý
    private DatabaseReference _dbReference;
    private bool _isFirebaseInitialized = false;

    void Start()
    {
        // Firebase'in baðýmlýlýklarýný kontrol et (Android/iOS için kritik)
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                // Baðlantý baþarýlý, referansý al
                _dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                _isFirebaseInitialized = true;
                Debug.Log($"[{brochureID}] Firebase baðlantýsý hazýr.");
            }
            else
            {
                Debug.LogError($"Firebase baþlatýlamadý: {dependencyStatus}");
            }
        });
    }

    /// <summary>
    /// Vuforia hedefi gördüðünde bu fonksiyonu çaðýracak.
    /// </summary>
    public void LoadBrochureData()
    {
        if (!_isFirebaseInitialized)
        {
            Debug.LogWarning("Firebase henüz hazýr deðil!");
            return;
        }

        // Kullanýcýya geri bildirim ver
        titleText.text = "Veri çekiliyor...";
        priceText.text = "...";

        // Asenkron veri okuma isteði: brochures/car_01
        _dbReference.Child("brochures").Child(brochureID).GetValueAsync().ContinueWithOnMainThread(task => {

            if (task.IsFaulted)
            {
                Debug.LogError("Veri okuma hatasý!");
                titleText.text = "Hata";
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists && snapshot.Value != null)
                {
                    // Gelen JSON verisini Dictionary olarak parse et
                    // Not: Firebase JSON objelerini Dictionary<string, object> olarak döndürür.
                    Dictionary<string, object> data = snapshot.Value as Dictionary<string, object>;

                    if (data != null)
                    {
                        // UI Güncelleme (Main Thread garantisi var)
                        if (data.ContainsKey("title")) titleText.text = data["title"].ToString();
                        if (data.ContainsKey("price")) priceText.text = data["price"].ToString();
                    }
                }
                else
                {
                    Debug.LogWarning($"'{brochureID}' ID'li veri bulunamadý.");
                    titleText.text = "Veri Yok";
                }
            }
        });
    }
}