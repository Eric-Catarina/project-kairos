// Assets/Scripts/DeathManager.cs
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    private int _deathCount = 0;
    public int DeathCount => _deathCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Se precisar persistir entre cenas
    }

    public void IncrementDeathCount()
    {
        _deathCount++;
    }

    public void ResetDeathCount()
    {
        _deathCount = 0;
    }
}