using UnityEngine;

public class CollectionsPageController : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] DatabaseManager db;

    [Header("Special card lock panel (GreyLockPanel over Special Card)")]
    [SerializeField] GameObject specialCardLock;

    void Start()
    {
        if (db == null) return;

        // This already unlocks / locks ALL ingredient cards by reading Firebase
        // and toggling each icon's child "GreyLockPanel".
        db.LoadUserCollection(); // :contentReference[oaicite:2]{index=2}

        // Firebase loads async, so refresh special lock a few times safely
        Invoke(nameof(RefreshSpecialCardLock), 0.25f);
        Invoke(nameof(RefreshSpecialCardLock), 0.75f);
        Invoke(nameof(RefreshSpecialCardLock), 1.25f);
    }

    void RefreshSpecialCardLock()
    {
        if (db == null || specialCardLock == null) return;

        // Uses your DB rule: all ingredients collected (including pineapple >= 2).
        bool specialUnlocked = db.AreAllIngredientsCollected(); // :contentReference[oaicite:3]{index=3}
        specialCardLock.SetActive(!specialUnlocked);

        Debug.Log($"[Collections] SpecialUnlocked={specialUnlocked}");
    }
}
