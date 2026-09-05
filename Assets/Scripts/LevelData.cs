using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "ColorGame/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName = "Level 1 - Room 1 (Armory)";
    public int requiredDummies = 5;
    public float timerDuration = 90f;
    public int startingAmmo = 30;
    public int maxAmmo = 30;
    public string nextSceneName = "Level2";
    public Vector2 roomBoundsMin = new Vector2(-14f, -14f);
    public Vector2 roomBoundsMax = new Vector2(14f, 14f);
}
