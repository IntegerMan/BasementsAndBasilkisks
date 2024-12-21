using MattEland.DigitalDungeonMaster.ClientShared;
using MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;
using MattEland.DigitalDungeonMaster.ConsoleApp.Menus;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.ConsoleApp;

public class DigitalDungeonMasterWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _lifeTime;
    private readonly ILogger<DigitalDungeonMasterWorker> _logger;
    private readonly ApiClient _client;
    private readonly LoginMenu _loginMenu;
    private readonly MainMenu _mainMenu;
    private readonly AdventureRunner _adventureRunner;

    public DigitalDungeonMasterWorker(IHostApplicationLifetime lifeTime, 
        ILogger<DigitalDungeonMasterWorker> logger,
        ApiClient client,
        LoginMenu loginMenu,
        MainMenu mainMenu,
        AdventureRunner adventureRunner)
    {
        _lifeTime = lifeTime;
        _logger = logger;
        _client = client;
        _loginMenu = loginMenu;
        _mainMenu = mainMenu;
        _adventureRunner = adventureRunner;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogDebug("Starting Digital Dungeon Master Worker");
            
            // Using UTF8 allows more capabilities for Spectre.Console.
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            
            // Display the header
            DisplayHelpers.RenderHeader();

            bool keepGoing = true;
            do
            {
                if (!_client.IsAuthenticated)
                {
                    keepGoing = await _loginMenu.RunAsync();
                }

                if (_client.IsAuthenticated)
                {
                    (keepGoing, AdventureInfo? adventure) = await _mainMenu.RunAsync();

                    if (keepGoing && adventure is not null)
                    {
                        keepGoing = await _adventureRunner.RunAsync(adventure);
                    }
                }
            } while (keepGoing);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "An unhandled exception of type {Type} occurred in the main loop: {Message}",
                ex.GetType().FullName, ex.Message);
    
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        }

        DisplayHelpers.SayDungeonMasterLine("Goodbye, Adventurer!");
        
        _lifeTime.StopApplication();
    }
}