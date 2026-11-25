using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Reads and writes the save file (JSON in Application.persistentDataPath).
///
/// Flow:
///   Pause menu -> Main Menu button   -> SaveSystem.Save()
///   Main menu  -> Continue button    -> SaveSystem.LoadForContinue() -> load the game scene
///   Game scene -> systems read SaveSystem.LoadedData in Awake/Start (null means "new game")
/// </summary>
public static class SaveSystem
{
    public const int CurrentVersion = 1;

    private const string FileName = "save.json";

    /// <summary>
    /// Data from the save file while a "Continue" game is starting. Null on a new game.
    /// Cleared when the main menu opens, when New Game is pressed, and on Restart.
    /// </summary>
    public static SaveData LoadedData { get; private set; }

    public static string SavePath =>
        Path.Combine(Application.persistentDataPath, FileName);

    /// <summary>True only if a save file exists and can actually be read.</summary>
    public static bool HasSave()
    {
        return TryReadFile(out _);
    }

    /// <summary>Collects data from the live game and writes it to disk.</summary>
    public static bool Save()
    {
        if (PlayerEconomy.Instance == null)
        {
            Debug.LogWarning("SaveSystem: no PlayerEconomy in this scene, nothing was saved.");
            return false;
        }

        SaveData data = new SaveData
        {
            version = CurrentVersion,
            savedAtUtc = DateTime.UtcNow.ToString("o"),

            economy = PlayerEconomy.Instance.CreateSaveData()

            // Add other systems here, for example:
            // towerSections = TowerBuilding.Instance.CreateSaveData(),
        };

        string tempPath = SavePath + ".tmp";

        try
        {
            File.WriteAllText(tempPath, JsonUtility.ToJson(data, true));

            // Write to a temp file first so a failed write can't destroy the old save.
            if (File.Exists(SavePath))
                File.Replace(tempPath, SavePath, null);
            else
                File.Move(tempPath, SavePath);

            Debug.Log($"SaveSystem: saved to {SavePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSystem: failed to save. {e.Message}");
            return false;
        }
    }

    /// <summary>Reads the save file and keeps it in LoadedData for the game scene to apply.</summary>
    public static bool LoadForContinue()
    {
        if (!TryReadFile(out SaveData data))
        {
            LoadedData = null;
            return false;
        }

        LoadedData = data;
        return true;
    }

    public static void ClearLoaded()
    {
        LoadedData = null;
    }

    /// <summary>Deletes the save file. Not used by the game, handy for testing.</summary>
    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSystem: failed to delete save. {e.Message}");
        }

        LoadedData = null;
    }

    private static bool TryReadFile(out SaveData data)
    {
        data = null;

        try
        {
            if (!File.Exists(SavePath))
                return false;

            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem: save file could not be read. {e.Message}");
            data = null;
            return false;
        }

        if (data == null || data.economy == null)
            return false;

        if (data.version < 1 || data.version > CurrentVersion)
        {
            Debug.LogWarning($"SaveSystem: unsupported save version {data.version}.");
            data = null;
            return false;
        }

        return true;
    }
}
