using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SaveLoadService", menuName = "ScriptableObjects/Services/SaveLoad")]
public class SaveLoadService : ScriptableObject
{
    readonly int _minimumSaves = 3;
    public event Action SaveDeletedEvent;
    int _selectedSlot;
    public int SelectedSlot { get => _selectedSlot; set {
            _selectedSlot = value;
            SelectedSlotKey = $"playerData-{SelectedSlot}";
        } 
    }
    public string SelectedSlotKey { get; set; } = "playerData-0";

    public void DeleteSave() => PlayerPrefs.DeleteKey(SelectedSlotKey);

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

    SerializablePlayerData GetSaveData(int saveNumber) {   
       return JsonUtility.FromJson<SerializablePlayerData>(PlayerPrefs.GetString("playerData-" + saveNumber.ToString()));
    }
}
