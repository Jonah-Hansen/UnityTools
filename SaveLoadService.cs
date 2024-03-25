using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SaveLoadService", menuName = "Services/SaveLoad")]
public class SaveLoadService : ScriptableObject
{
    //! if making any changes to the structure of `SerializablePlayerData`, increment this value to stop incompatible player data from being loaded.
    private readonly int _saveVersion = 0;
    private readonly int _minimumSaves = 3;

    public string SelectedSlotKey { get; set; } = "playerData-0";
    
    private int _selectedSlot;
    public int SelectedSlot { get => _selectedSlot; set {
            _selectedSlot = value;
            SelectedSlotKey = $"playerData-{SelectedSlot}";
        } 
    }

    public event Action<int> SaveDeletedEvent;

    public void DeleteSave() {
        PlayerPrefs.DeleteKey(SelectedSlotKey);
        SaveDeletedEvent?.Invoke(SelectedSlot);
    }

    public SerializablePlayerData[] GetSaves() {
        List<SerializablePlayerData> saves = new();
        int numberOfSaves = PlayerPrefs.GetInt("numberOfSaves");
        if(numberOfSaves == 0) {
            numberOfSaves = _minimumSaves;
            PlayerPrefs.SetInt("numberOfSaves", numberOfSaves);
        }
        for(int i = 0; i < numberOfSaves; i++) {
            saves.Add(GetSaveData(i));
        }
        return saves.ToArray();
    }

    public void SaveData(SerializablePlayerData playerData) {
        bool wasNewSave = playerData.isNewSave;
        playerData.isNewSave = false;
        playerData.saveVersion = _saveVersion;
        PlayerPrefs.SetString(SelectedSlotKey, JsonUtility.ToJson(playerData));
        PlayerPrefs.Save();
        // alwasy have at least 1 empty slot.
        if (wasNewSave && GetSaves().All(save => save != null)) {
            int numberOfSaves = PlayerPrefs.GetInt("numberOfSaves");
            PlayerPrefs.SetInt("numberOfSaves", numberOfSaves + 1);
        }
    }

    public SerializablePlayerData LoadPlayerData() {   
        SerializablePlayerData dataToLoad = JsonUtility.FromJson<SerializablePlayerData>(PlayerPrefs.GetString(SelectedSlotKey));
        if (dataToLoad?.saveVersion < _saveVersion) {
            Debug.LogError("incompatible player data save version.");
            return null;
        }
        return dataToLoad;
    }

    private SerializablePlayerData GetSaveData(int saveNumber) {
        try {
            return JsonUtility.FromJson<SerializablePlayerData>(PlayerPrefs.GetString("playerData-" + saveNumber.ToString()));
        } catch (ArgumentException error) {
            Debug.LogError(error);
            return null;
        }
    }
}
