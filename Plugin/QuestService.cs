using Dalamud.Plugin.Services;
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

    public QuestService(IDataManager dataManager, IPluginLog log)
    {
        this.dataManager = dataManager;
        this.log = log;
    }

    /// <summary>
    /// Récupère toutes les quêtes du jeu
    /// </summary>
    public void LogQuestCount()
    {
        // On lit la table Quest dans les fichiers du jeu
        var questSheet = dataManager.GetExcelSheet<Quest>();

        // On compte combien il y en a
        log.Information($"Nombre total de quêtes dans le jeu : {questSheet.Count}");
    }
}