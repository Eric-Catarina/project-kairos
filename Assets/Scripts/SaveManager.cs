// Local: Assets/Scripts/Core/SaveManager.cs

using UnityEngine;
using System.IO;
using System.Linq; // Adicionado para facilitar a busca de perfis

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public GameData Data { get; private set; }
    private string _saveFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _saveFilePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        LoadGame();
    }

    public void LoadGame()
    {
        if (File.Exists(_saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(_saveFilePath);
                Data = JsonUtility.FromJson<GameData>(json);
                Debug.Log("Save file loaded successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load save data: {e.Message}. Creating new data.");
                CreateNewSaveData();
            }
        }
        else
        {
            Debug.Log("No save file found. Creating new data.");
            CreateNewSaveData();
        }

        // Garante que sempre haja um perfil ativo
        if (Data.profiles.Count == 0 || !Data.profiles.Any(p => p.profileId == Data.activeProfileId))
        {
            CreateNewProfile(true); // Cria um novo perfil e o define como ativo
        }
    }
    
    public void SaveGame()
    {
        try
        {
            string json = JsonUtility.ToJson(Data, true); // O 'true' formata o JSON para ser legível
            File.WriteAllText(_saveFilePath, json);
            Debug.Log($"Game data saved to {_saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }

    private void CreateNewSaveData()
    {
        Data = new GameData();
    }
    
    // --- Funções de Acesso a Dados ---
    
    public UserProfile GetActiveUserProfile()
    {
        return Data.profiles.FirstOrDefault(p => p.profileId == Data.activeProfileId);
    }
    
    public GameSettings GetSettings()
    {
        return Data.settings;
    }

    public UserProfile CreateNewProfile(bool setActive)
    {
        var newProfile = new UserProfile
        {
            profileId = System.Guid.NewGuid().ToString(),
            profileName = $"Player{Random.Range(1000, 9999)}"
        };
        Data.profiles.Add(newProfile);

        if (setActive)
        {
            Data.activeProfileId = newProfile.profileId;
        }
        
        SaveGame();
        return newProfile;
    }

    // Garante que o jogo seja salvo ao fechar
    private void OnApplicationQuit()
    {
        SaveGame();
    }
}