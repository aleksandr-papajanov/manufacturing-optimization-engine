using Spectre.Console;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Models; // Ensure this project reference exists!

// 1. Ensure the Base Address matches your Docker port (5000 or 8080)
var apiUrl = Environment.GetEnvironmentVariable("GATEWAY_API_URL") ?? "http://localhost:5000";
var httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };

AnsiConsole.Write(new FigletText("Manufacturing").Centered().Color(Color.Blue));
AnsiConsole.Write(new FigletText("Optimization").Centered().Color(Color.Green));

while (true)
{
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[green]What do you want to do?[/]")
            .AddChoices(
                "Get Providers List", 
                "Submit Request (US-06)",
                "Run Random Demo (Legacy)", 
                "Exit"
            ));

    switch (choice)
    {
        case "Get Providers List":
            await GetProviders();
            break;
        case "Submit Request (US-06)":
            await SubmitCustomRequest();
            break;
        case "Run Random Demo (Legacy)":
            await RequestOptimization();
            break;
        case "Exit":
            AnsiConsole.MarkupLine("[yellow]Goodbye![/]");
            return;
    }

    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
    Console.ReadKey(true);
    AnsiConsole.Clear();
}

// --- NEW FUNCTION: US-06 Custom Request ---
async Task SubmitCustomRequest()
{
    AnsiConsole.MarkupLine("[yellow]Reading test_request.json...[/]");

    try
    {
        // 1. Locate the file
        string filePath = Path.Combine(AppContext.BaseDirectory, "test_request.json");
        
        if (!File.Exists(filePath)) 
        {
            AnsiConsole.MarkupLine("[red]Error: test_request.json not found![/]");
            AnsiConsole.MarkupLine("Make sure you added the file to the project and set 'Copy to Output Directory' to 'Copy if newer'.");
            return;
        }

        // 2. Read and Parse JSON
        string jsonContent = await File.ReadAllTextAsync(filePath);
        var requestData = JsonSerializer.Deserialize<MotorRequest>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (requestData == null)
        {
            AnsiConsole.MarkupLine("[red]Error: Failed to deserialize JSON.[/]");
            return;
        }

        // 3. Show User what we are sending
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Attribute");
        table.AddColumn("Value");
        table.AddRow("Request ID", requestData.RequestId.ToString());
        table.AddRow("Customer ID", requestData.CustomerId);
        table.AddRow("Target Efficiency", $"[green]{requestData.Specs.TargetEfficiency}[/]");
        table.AddRow("Power", $"{requestData.Specs.PowerKW} kW");
        table.AddRow("Priority", requestData.Constraints.Priority.ToString());
        
        AnsiConsole.Write(table);

// 4. Send to Gateway
        await AnsiConsole.Status()
            .StartAsync("Submitting request to Gateway...", async ctx =>
            {
                var payload = new 
                {
                    RequestId = requestData.RequestId,
                    CustomerId = requestData.CustomerId,
                    Power = requestData.Specs.PowerKW.ToString(),
                    TargetEfficiency = requestData.Specs.TargetEfficiency.ToString()
                };

                // FIX: Update URL to match the new route "api/optimization/submit"
                var response = await httpClient.PostAsJsonAsync("/api/optimization/submit", payload);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[green]✓ Success! Request submitted.[/]");
                    string responseBody = await response.Content.ReadAsStringAsync();
                    AnsiConsole.WriteLine(responseBody);
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ Failed: {response.StatusCode}[/]");
                    string error = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[dim]{Markup.Escape(error)}[/]");
                }
            });
    }
    catch (Exception ex)
    {
        AnsiConsole.WriteException(ex);
    }
}

// --- EXISTING FUNCTIONS ---

async Task RequestOptimization()
{
    Guid? commandId = null;
    
    await AnsiConsole.Status()
        .StartAsync("Sending random demo request...", async ctx =>
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
        .StartAsync("Waiting for response (simulated)...", async ctx =>
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

// --- DTOs ---
record OptimizationRequestResponse(Guid CommandId);
record ProvidersListResponse(int TotalProviders, List<ProviderInfo> Providers);
record StatusResponse(string Status, JsonElement? Data);
record ProviderInfo(Guid ProviderId, string ProviderType, string ProviderName, DateTime RegisteredAt);