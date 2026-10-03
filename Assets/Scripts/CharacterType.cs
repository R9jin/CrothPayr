using UnityEngine;

public enum CharacterType
{
    CharacterA_Roll,
    CharacterB_DoubleJump,
    CharacterC_Teleport
}

public static class CharacterSelection
{
    private const string PREF_KEY = "SelectedCharacter";

    private static CharacterType _selectedCharacter = CharacterType.CharacterA_Roll;
    private static bool _loaded = false;

    public static CharacterType SelectedCharacter
    {
        get
        {
            if (!_loaded)
            {
                _selectedCharacter = (CharacterType)PlayerPrefs.GetInt(PREF_KEY, (int)CharacterType.CharacterA_Roll);
                _loaded = true;
            }
            return _selectedCharacter;
        }
        set
        {
            _selectedCharacter = value;
            PlayerPrefs.SetInt(PREF_KEY, (int)value);
            PlayerPrefs.Save();
            _loaded = true;
        }
    }

    public static string GetCharacterName(CharacterType type)
    {
        switch (type)
        {
            case CharacterType.CharacterA_Roll:
                return "Operative A - Acrobat";
            case CharacterType.CharacterB_DoubleJump:
                return "Operative B - Striker";
            case CharacterType.CharacterC_Teleport:
                return "Operative C - Mystic";
            default:
                return "Unknown";
        }
    }

    public static string GetSkillName(CharacterType type)
    {
        switch (type)
        {
            case CharacterType.CharacterA_Roll:
                return "Combat Roll";
            case CharacterType.CharacterB_DoubleJump:
                return "Double Jump";
            case CharacterType.CharacterC_Teleport:
                return "Phase Teleport";
            default:
                return "None";
        }
    }

    public static string GetSkillDescription(CharacterType type)
    {
        switch (type)
        {
            case CharacterType.CharacterA_Roll:
                return "Rapid evasive roll in movement direction. Has 2 charges with recharge cooldown. [Key: Q]";
            case CharacterType.CharacterB_DoubleJump:
                return "Allows a second vertical leap while in mid-air. Passive ability with no cooldown. [Key: Space in air]";
            case CharacterType.CharacterC_Teleport:
                return "Instantly teleports forward a short distance safely. Tactical ability with cooldown. [Key: Q or E]";
            default:
                return "";
        }
    }
}
