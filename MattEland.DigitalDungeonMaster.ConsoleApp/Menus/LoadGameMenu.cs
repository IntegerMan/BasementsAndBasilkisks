using MattEland.DigitalDungeonMaster.ClientShared;
using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class LoadGameMenu
{
    private readonly ApiClient _client;

    public LoadGameMenu(ApiClient client)
    {
        _client = client;
    }
    
    public async Task<AdventureInfo?> RunAsync()
    {
        List<AdventureInfo> adventures = [];
        await AnsiConsole.Status().StartAsync("Fetching adventures...",
            async _ =>
            {
                IEnumerable<AdventureInfo> loadedAdventures = await _client.LoadAdventuresAsync();
                
                // TODO: It'd be good to be able to continue creation of an adventure
                adventures.AddRange(loadedAdventures.Where(a => a.Status != AdventureStatus.Building));
            });
    
        if (!adventures.Any())
        {
            AnsiConsole.MarkupLine("[Red]No adventures found for this user. Please create an adventure first.[/]");
            return null;
        }

        AdventureInfo cancel = new()
        {
            Name = "Cancel"
        };
        adventures.Add(cancel);

        AdventureInfo adventure = AnsiConsole.Prompt(new SelectionPrompt<AdventureInfo>()
            .Title("Select an adventure")
            .AddChoices(adventures)
            .UseConverter(a => a.Name + (a == cancel ? string.Empty : $" ({a.Ruleset})")));

        if (adventure == cancel)
        {
            return null;
        }
        
        AnsiConsole.MarkupLineInterpolated($"Selected Adventure: [Yellow]{adventure.Name}[/], Ruleset: [Yellow]{adventure.Ruleset}[/]");
        return adventure;
    }
}