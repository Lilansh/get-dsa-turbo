using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "CardMatch/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelID = "4x4";
    public int gridWidth = 4;
    public int gridHeight = 4;

    [HideInInspector] public int bestScore;
    [HideInInspector] public float bestTime;
}
