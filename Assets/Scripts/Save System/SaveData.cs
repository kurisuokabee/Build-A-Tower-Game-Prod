using System;

/// <summary>
/// Everything that gets written to the save file.
/// Fields must be public and [Serializable] so JsonUtility can read and write them.
/// To save something new, add a section class here and see README.md ("Adding more to the save").
/// </summary>
[Serializable]
public class SaveData
{
    public int version = SaveSystem.CurrentVersion;
    public string savedAtUtc;

    public EconomySaveData economy = new EconomySaveData();
}

[Serializable]
public class EconomySaveData
{
    public int level = 1;
    public int blocks;
    public int exp;
    public int currentLevelExp;
    public int upgradePoints;
}
