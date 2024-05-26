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

    private int _selectedSlot = 0;
    public int SelectedSlot { get => _selectedSlot; set {
            _selectedSlot = value;
        }
    }
    public string GetSlotKey() {
        return GetSlotKey(SelectedSlot);
    }
    public string GetSlotKey(int key) {
        return $"playerData-{key}";
    }

    public event Action SaveDeletedEvent;

    public void SaveVolume(float volume) {
        PlayerPrefs.SetFloat("volume", Mathf.Clamp01(volume));
    }

    public float GetVolume() {
        return PlayerPrefs.GetFloat("volume", 1f);
    }

    public void DeleteSave() {
        PlayerPrefs.DeleteKey(GetSlotKey());
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
        SaveData(playerData, SelectedSlot);
    }

    public void SaveData(SerializablePlayerData playerData, int slot) {
        playerData.saveVersion = _saveVersion;
        PlayerPrefs.SetString(GetSlotKey(slot), JsonUtility.ToJson(playerData));
        PlayerPrefs.Save();
    }

    public SerializablePlayerData LoadPlayerData() {
        SerializablePlayerData dataToLoad = JsonUtility.FromJson<SerializablePlayerData>(PlayerPrefs.GetString(GetSlotKey()));
        if (dataToLoad?.saveVersion < _saveVersion) {
            Debug.LogError("incompatible player data save version.");
            return null;
        }
        return dataToLoad;
    }

    public SerializablePlayerData GetSaveData(int saveNumber) {
        try {
            return JsonUtility.FromJson<SerializablePlayerData>(PlayerPrefs.GetString(GetSlotKey(saveNumber)));
        } catch (ArgumentException error) {
            Debug.LogError(error);
            return null;
        }
    }
}
