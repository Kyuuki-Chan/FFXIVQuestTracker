using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVQuestTracker.Models;
using Lumina.Excel.Sheets;

namespace FFXIVQuestTracker;

/// <summary>
/// Service responsable de la lecture des quêtes du jeu
/// et de la progression du joueur
/// </summary>
public class QuestService
{
    private readonly IDataManager dataManager;
    private readonly IPluginLog log;

    // Dictionnaire pour convertir l'ID d'extension en nom lisible
    private static readonly Dictionary<uint, string> Expansions = new()
    {
        { 0, "A Realm Reborn" },
        { 1, "Heavensward" },
        { 2, "Stormblood" },
        { 3, "Shadowbringers" },
        { 4, "Endwalker" },
        { 5, "Dawntrail" }
    };

    public QuestService(IDataManager dataManager, IPluginLog log)
    {
        this.dataManager = dataManager;
        this.log = log;
    }

    /// <summary>
    /// Récupère toutes les quêtes du jeu sous forme de liste QuestInfo
    /// </summary>
    public List<QuestInfo> GetAllQuests()
    {
        var result = new List<QuestInfo>();

        // On lit la table Quest depuis les fichiers du jeu
        var questSheet = dataManager.GetExcelSheet<Quest>();

        foreach (var quest in questSheet)
        {
            // On ignore les entrées vides (le jeu a des lignes vides dans ses tables)
            var name = quest.Name.ToString();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            // On récupère le nom de l'extension
            var expansionId = quest.Expansion.RowId;
            var expansionName = Expansions.TryGetValue(expansionId, out var exp)
                ? exp
                : "Inconnu";

            result.Add(new QuestInfo
            {
                Id = quest.RowId,
                Name = name,
                Expansion = expansionName,
                Level = (byte)quest.ClassJobLevel[0],
                // IsCompleted sera rempli plus tard via QuestManager
                IsCompleted = false
            });
        }

        log.Information($"Quêtes chargées : {result.Count} quêtes valides sur {questSheet.Count} entrées");

        return result;
    }
}