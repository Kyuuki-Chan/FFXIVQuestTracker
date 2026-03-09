using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVQuestTracker.Models;
using Lumina.Excel.Sheets;
using FFXIVClientStructs.FFXIV.Client.Game;

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
    private static readonly Dictionary<string, string> JobAbbrevMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "GLD", "PLD" }, { "PAL", "PLD" },
        { "MRD", "WAR" }, { "GUE", "WAR" },
        { "DRK", "DRK" }, { "GNB", "GNB" },
        { "CNJ", "WHM" }, { "MBL", "WHM" },
        { "SCH", "SCH" }, { "ÉRU", "SCH" },
        { "AST", "AST" }, { "SGE", "SGE" },
        { "PGL", "MNK" }, { "MOI", "MNK" },
        { "LNC", "DRG" }, { "CHE", "DRG" },
        { "ROG", "NIN" }, { "NIN", "NIN" },
        { "SAM", "SAM" }, { "RPR", "RPR" }, { "VPR", "VPR" },
        { "ARC", "BRD" }, { "MCH", "MCH" }, { "DNC", "DNC" },
        { "THM", "BLM" }, { "ACN", "SMN" },
        { "RDM", "RDM" }, { "PCT", "PCT" }, { "BLU", "BLU" },
        { "CRP","CRP"},{"BSM","BSM"},{"ARM","ARM"},{"GSM","GSM"},
        { "LTW","LTW"},{"WVR","WVR"},{"ALC","ALC"},{"CUL","CUL"},
        { "MIN","MIN"},{"BTN","BTN"},{"FSH","FSH"},
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
                IsCompleted = IsQuestCompleted(quest.RowId),
            });
        }
        // Zone
        var zone = string.Empty;
        try { zone = quest.IssuerLocation.Value.PlaceName.Value.Name.ToString(); } catch { }

        // Job requis
        var job = string.Empty;
        try
        {
            var abbr = quest.ClassJobRequired.Value.Abbreviation.ToString();
            if (!string.IsNullOrWhiteSpace(abbr) && abbr != "ADV")
                job = JobAbbrevMap.TryGetValue(abbr, out var mapped) ? mapped : abbr.ToUpperInvariant();
        }
        catch { }

        // NPC donneur
        var npc = string.Empty;
        try { npc = quest.IssuerStart.Value.Singular.ToString(); } catch { }

        // URL Lodestone (FR, ID en hex)
        var lodestoneUrl = $"https://fr.finalfantasyxiv.com/lodestone/playguide/db/quest/detail/?id={quest.RowId:x}";

        result.Add(new QuestInfo
        {
            Id           = quest.RowId,
            Name         = name,
            Expansion    = expansionName,
            Level        = (byte)quest.ClassJobLevel[0],
            IsCompleted  = IsQuestCompleted(quest.RowId),
            Zone         = zone,
            Job          = job,
            IssuerNpc    = npc,
            LodestoneUrl = lodestoneUrl,
        });


        log.Information($"Quêtes chargées : {result.Count} quêtes valides sur {questSheet.Count} entrées");

        return result;
        
    }
    /// <summary>
    /// Vérifie si une quête est complétée par le joueur
    /// </summary>
    private static unsafe bool IsQuestCompleted(uint questId)
    {
        // QuestManager lit directement la mémoire du jeu
        // Le mot clé "unsafe" est nécessaire pour accéder à la mémoire native
        return QuestManager.IsQuestComplete((ushort)questId);
}
}