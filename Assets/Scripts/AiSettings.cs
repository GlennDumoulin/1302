using UnityEngine;

[CreateAssetMenu(fileName = "AiSettings")]
public class AiSettings : ScriptableObject
{
    [Header("Difficulty Info")]
    public string DifficultyName = string.Empty;
    public GameObject FlemishDecks = null;
    public GameObject FrenchDecks = null;

    [Header("Chance Values")]
    [Range(0.0f, 1.0f)] public float ReloadChance = 0.5f;
    [Range(0.0f, 1.0f)] public float MoveToCampfireChance = 0.95f;
    [Range(0.0f, 1.0f)] public float FleeDamageableCampfireChance = 0.5f;
    [Range(0.0f, 1.0f)] public float FleeKillableCampfireChance = 0.7f;

    [Range(0.0f, 1.0f)] public float AvoidAntiHorseChance = 0.5f;
    [Range(0.0f, 1.0f)] public float AvoidMudChanceDefault = 0.3f;
    [Range(0.0f, 1.0f)] public float AvoidMudChanceCharging = 0.5f;
}
