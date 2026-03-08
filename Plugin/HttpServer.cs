using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using Dalamud.Plugin.Services;
using FFXIVQuestTracker.Models;

namespace FFXIVQuestTracker;

/// <summary>
/// Serveur HTTP local qui expose les données de quêtes
/// sur http://localhost:5000
/// </summary>
public class HttpServer : IDisposable
{
    private readonly HttpListener listener;
    private readonly QuestService questService;
    private readonly IPluginLog log;
    private bool isRunning;

    // Le thread qui tourne en arrière-plan pour écouter les requêtes
    private Thread? listenerThread;

    public HttpServer(QuestService questService, IPluginLog log)
    {
        this.questService = questService;
        this.log = log;

        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5000/");
    }

    /// <summary>
    /// Démarre le serveur HTTP en arrière-plan
    /// </summary>
    public void Start()
    {
        listener.Start();
        isRunning = true;

        // On démarre un thread séparé pour ne pas bloquer le jeu
        listenerThread = new Thread(Listen)
        {
            IsBackground = true,
            Name = "QuestTracker HTTP Server"
        };
        listenerThread.Start();

        log.Information("Serveur HTTP démarré sur http://localhost:5000/");
    }

    /// <summary>
    /// Boucle principale qui écoute les requêtes entrantes
    /// </summary>
    private void Listen()
    {
        while (isRunning)
        {
            try
            {
                // On attend une requête (bloquant jusqu'à réception)
                var context = listener.GetContext();
                HandleRequest(context);
            }
            catch (HttpListenerException)
            {
                // Normal quand on arrête le serveur
                break;
            }
            catch (Exception ex)
            {
                log.Error($"Erreur serveur HTTP : {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gère une requête HTTP entrante
    /// </summary>
    private void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        // On autorise les requêtes depuis n'importe quelle origine (CORS)
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.ContentType = "application/json; charset=utf-8";

        try
        {
            // Route : GET /quests → retourne toutes les quêtes
            if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/quests")
            {
                var quests = questService.GetAllQuests();
                var json = JsonSerializer.Serialize(quests);
                var buffer = Encoding.UTF8.GetBytes(json);

                response.StatusCode = 200;
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);

                log.Information($"GET /quests → {quests.Count} quêtes envoyées");
            }
            else
            {
                // Route inconnue → 404
                response.StatusCode = 404;
                var buffer = Encoding.UTF8.GetBytes("{\"error\": \"Route inconnue\"}");
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            log.Error($"Erreur lors du traitement de la requête : {ex.Message}");
        }
        finally
        {
            response.OutputStream.Close();
        }
    }

    /// <summary>
    /// Arrête le serveur proprement
    /// </summary>
    public void Dispose()
    {
        isRunning = false;
        listener.Stop();
        listener.Close();
        log.Information("Serveur HTTP arrêté.");
    }
}
