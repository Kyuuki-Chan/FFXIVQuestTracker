namespace FFXIVQuestTracker.Models;

/// <summary>
/// Représente une quête du jeu avec toutes ses informations
/// </summary>
public class QuestInfo
{
    // Identifiant unique de la quête dans le jeu
    public uint Id { get; set; }

    // Nom de la quête
    public string Name { get; set; } = string.Empty;

    // Type de quête (Main Scenario, Side Quest, etc.)
    public string Type { get; set; } = string.Empty;

    // Extension du jeu (ARR, Heavensward, etc.)
    public string Expansion { get; set; } = string.Empty;

    // Niveau requis pour faire la quête
    public byte Level { get; set; }

    // Est-ce que le joueur a complété cette quête ?
    public bool IsCompleted { get; set; }
    public string Zone { get; set; } = string.Empty;
    public string Job { get; set; } = string.Empty;
    public string IssuerNpc { get; set; } = string.Empty;
    public string LodestoneUrl { get; set; } = string.Empty;
}