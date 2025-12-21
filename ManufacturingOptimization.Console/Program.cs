using Spectre.Console;
using System.Net.Http.Json;
using System.Text.Json;

var apiUrl = Environment.GetEnvironmentVariable("GATEWAY_API_URL") ?? "http://localhost:5000";
var httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };

AnsiConsole.Write(new FigletText("Manufacturing").Centered().Color(Color.Blue));
AnsiConsole.Write(new FigletText("Optimization").Centered().Color(Color.Green));

while (true)
{
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[green]What do you want to do?[/]")
            .AddChoices("Get Providers List", "Request Optimization", "Exit"));

    switch (choice)
    {
        case "Get Providers List":
            await GetProviders();
            break;
        case "Request Optimization":
            await RequestOptimization();
            break;
        case "Exit":
            AnsiConsole.MarkupLine("[yellow]Goodbye![/]");
            return;
    }

    AnsiConsole.WriteLine();
}

async Task RequestOptimization()
{
    Guid? commandId = null;
    
    await AnsiConsole.Status()
        .StartAsync("Sending request...", async ctx =>
        {
            try
            {
                var response = await httpClient.PostAsync("/api/optimization/request", null);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<OptimizationRequestResponse>();
                    commandId = result?.CommandId;
                    AnsiConsole.MarkupLine("[green]✓ Request sent![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ Error: {response.StatusCode}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            }
        });
    
    if (commandId.HasValue)
    {
        await WaitForResponse(commandId.Value);
    }
}

async Task WaitForResponse(Guid commandId)
{
    await AnsiConsole.Status()
        .StartAsync("Waiting for response...", async ctx =>
        {
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                
                try
                {
                    var response = await httpClient.GetAsync($"/api/optimization/status/{commandId}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<StatusResponse>();
                        
                        if (result?.Status == "completed" && result.Data != null)
                        {
                            var providerId = result.Data.Value.GetProperty("providerId").GetGuid();
                            var providerResponse = result.Data.Value.GetProperty("response").GetString();
                            
                            var color = providerResponse == "accepted" ? "green" : "yellow";
                            AnsiConsole.MarkupLine($"[{color}]✓ Response: {providerResponse}[/]");
                            AnsiConsole.MarkupLine($"[dim]Provider: {providerId}[/]");
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    // Continue waiting
                }
            }
            
            AnsiConsole.MarkupLine("[red]✗ Timeout waiting for response[/]");
        });
}

async Task GetProviders()
{
    await AnsiConsole.Status()
        .StartAsync("Getting providers list...", async ctx =>
        {
            try
            {
                var response = await httpClient.GetAsync("/api/providers");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ProvidersListResponse>();
                    
                    if (result?.Providers?.Any() == true)
                    {
                        var table = new Table()
                            .Border(TableBorder.Rounded)
                            .BorderColor(Color.Green)
                            .AddColumn("[yellow]Provider ID[/]")
                            .AddColumn("[yellow]Type[/]")
                            .AddColumn("[yellow]Name[/]")
                            .AddColumn("[yellow]Registered At[/]");
                        
                        foreach (var provider in result.Providers)
                        {
                            table.AddRow(
                                provider.ProviderId.ToString(),
                                provider.ProviderType,
                                provider.ProviderName,
                                provider.RegisteredAt.ToString("yyyy-MM-dd HH:mm:ss")
                            );
                        }
                        
                        AnsiConsole.Write(table);
                        AnsiConsole.MarkupLine($"\n[green]Total Providers:[/] {result.TotalProviders}");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]No providers registered yet[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ Error: {response.StatusCode}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            }
        });
}

record OptimizationRequestResponse(Guid CommandId);
record ProvidersListResponse(int TotalProviders, List<ProviderInfo> Providers);
record StatusResponse(string Status, JsonElement? Data);
record ProviderInfo(Guid ProviderId, string ProviderType, string ProviderName, DateTime RegisteredAt);