using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.Linq;

namespace FFXIVQuestTracker;

public sealed class Plugin : IDalamudPlugin
{
    private readonly ICommandManager commandManager;
    private readonly IPluginLog log;
    private readonly QuestService questService;


    internal readonly IClientState ClientState;
    internal readonly IPlayerState PlayerState;
    internal readonly IDataManager DataManager;

    private const string CommandName = "/questtracker";

    // Les services sont injectés automatiquement par Dalamud via le constructeur
    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IClientState clientState,
        IPlayerState playerState,
        IDataManager dataManager,
        IPluginLog log)
    {
        this.commandManager = commandManager;
        this.log = log;

        ClientState = clientState;
        PlayerState = playerState;
        DataManager = dataManager;
        questService = new QuestService(dataManager, log);
        var quests = questService.GetAllQuests();
        log.Information($"Première quête : {quests.First().Name} - {quests.First().Expansion}");


        commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Affiche le statut du serveur Quest Tracker"
        });

        log.Information("FFXIV Quest Tracker démarré !");
    }

    public void Dispose()
    {
        commandManager.RemoveHandler(CommandName);
        log.Information("FFXIV Quest Tracker arrêté.");
    }

    private void OnCommand(string command, string args)
    {
        log.Information("Quest Tracker : commande reçue !");
    }
}