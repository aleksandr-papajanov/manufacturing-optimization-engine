using Spectre.Console;
using System.Net.Http.Json;
using System.Text.Json;
using ManufacturingOptimization.Common.Models;

// Configuration
var apiUrl = Environment.GetEnvironmentVariable("GATEWAY_API_URL") ?? "http://localhost:5000";
var httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };

// Welcome screen
AnsiConsole.Clear();
AnsiConsole.Write(new FigletText("Manufacturing").Centered().Color(Color.Blue));
AnsiConsole.Write(new FigletText("Optimization").Centered().Color(Color.Green));
AnsiConsole.WriteLine();

// Select role
var role = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[yellow]Who are you?[/]")
        .AddChoices("Customer", "Provider"));

AnsiConsole.Clear();

if (role == "Customer")
{
    await RunCustomerMode();
}
else
{
    await RunProviderMode();
}

async Task RunCustomerMode()
{
    AnsiConsole.Write(new Rule("[green]Customer Mode[/]").RuleStyle("green"));
    AnsiConsole.WriteLine();

    while (true)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]What do you want to do?[/]")
                .AddChoices(
                    "Submit Optimization Request",
                    "View Providers",
                    "Exit"));

        switch (choice)
        {
            case "Submit Optimization Request":
                await SubmitOptimizationRequest();
                break;
            case "View Providers":
                await GetProviders();
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
}

async Task RunProviderMode()
{
    AnsiConsole.Write(new Rule("[blue]Provider Mode[/]").RuleStyle("blue"));
    AnsiConsole.WriteLine();
    
    AnsiConsole.MarkupLine("[yellow]Provider mode will be implemented in future sprints.[/]");
    AnsiConsole.MarkupLine("[grey]This will include:[/]");
    AnsiConsole.MarkupLine("  • Register provider capabilities");
    AnsiConsole.MarkupLine("  • Respond to estimate requests");
    AnsiConsole.MarkupLine("  • Accept/reject work assignments");
    AnsiConsole.WriteLine();
    
    AnsiConsole.MarkupLine("[grey]Press any key to exit...[/]");
    Console.ReadKey(true);
}

async Task SubmitOptimizationRequest()
{
    AnsiConsole.Write(new Rule("[yellow]Submit Optimization Request[/]"));
    AnsiConsole.WriteLine();

    // Generate random MotorRequest
    var random = new Random();
    var efficiencyClasses = new[] { MotorEfficiencyClass.IE1, MotorEfficiencyClass.IE2, MotorEfficiencyClass.IE3, MotorEfficiencyClass.IE4 };

    var motorRequest = new OptimizationRequest
    {
        RequestId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid().ToString(),
        MotorSpecs = new MotorSpecifications
        {
            PowerKW = random.Next(50, 200),
            AxisHeightMM = random.Next(63, 315), // Standard IEC motor sizes
            CurrentEfficiency = efficiencyClasses[random.Next(efficiencyClasses.Length)],
            TargetEfficiency = efficiencyClasses[random.Next(efficiencyClasses.Length)],
            MalfunctionDescription = random.Next(0, 2) == 0 ? "Normal operation" : "Reduced efficiency, overheating"
        },
        Constraints = new OptimizationRequestConstraints
        {
            MaxBudget = random.Next(0, 3) == 0 ? null : random.Next(5000, 20000), // 33% chance of no budget limit
            RequiredDeadline = random.Next(0, 3) == 0 ? null : DateTime.Now.AddDays(random.Next(30, 90)) // 33% chance of no deadline
        }
    };

    // Display generated request
    var table = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Blue)
        .AddColumn("[yellow]Property[/]")
        .AddColumn("[yellow]Value[/]");

    table.AddRow("Request ID", motorRequest.RequestId.ToString());
    table.AddRow("Customer ID", motorRequest.CustomerId);
    table.AddRow("Power", $"{motorRequest.MotorSpecs.PowerKW} kW");
    table.AddRow("Axis Height", $"{motorRequest.MotorSpecs.AxisHeightMM} mm");
    table.AddRow("Current Efficiency", motorRequest.MotorSpecs.CurrentEfficiency.ToString());
    table.AddRow("Target Efficiency", motorRequest.MotorSpecs.TargetEfficiency.ToString());
    table.AddRow("Malfunction", motorRequest.MotorSpecs.MalfunctionDescription ?? "-");
    table.AddRow("Max Budget", motorRequest.Constraints.MaxBudget.HasValue ? $"€{motorRequest.Constraints.MaxBudget.Value:N2}" : "No limit");
    table.AddRow("Required Deadline", motorRequest.Constraints.RequiredDeadline?.ToString("yyyy-MM-dd") ?? "No deadline");

    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();

    if (!AnsiConsole.Confirm("[cyan]Submit this request?[/]"))
    {
        AnsiConsole.MarkupLine("[yellow]Request cancelled.[/]");
        return;
    }

    AnsiConsole.WriteLine();

    // Submit request to Gateway
    Guid requestId = motorRequest.RequestId;
    List<OptimizationStrategyDto>? strategies = null;

    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("[yellow]Submitting request to Gateway...[/]", async ctx =>
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/api/optimization/request", motorRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    
                    ctx.Status("[green]✓ Request submitted successfully![/]");
                    AnsiConsole.MarkupLine($"[dim]Request ID: {requestId}[/]");
                    AnsiConsole.WriteLine();

                    // Poll for strategies
                    ctx.Status("[yellow]Waiting for optimization strategies...[/]");
                    
                    var startTime = DateTime.UtcNow;
                    var timeout = TimeSpan.FromMinutes(10);
                    var pollInterval = TimeSpan.FromSeconds(3);
                    
                    while (DateTime.UtcNow - startTime < timeout)
                    {
                        await Task.Delay(pollInterval);
                        
                        try
                        {
                            var statusResponse = await httpClient.GetAsync($"/api/optimization/strategies/{requestId}");
                            
                            if (statusResponse.IsSuccessStatusCode)
                            {
                                var statusResult = await statusResponse.Content.ReadFromJsonAsync<StrategiesResponse>();
                                
                                if (statusResult?.IsReady == true && statusResult.Strategies?.Any() == true)
                                {
                                    strategies = statusResult.Strategies;
                                    ctx.Status("[green]✓ Strategies ready![/]");
                                    break;
                                }
                                
                                ctx.Status($"[yellow]Generating strategies... ({(int)(DateTime.UtcNow - startTime).TotalSeconds}s)[/]");
                            }
                        }
                        catch
                        {
                            // Continue polling
                        }
                    }
                    
                    if (strategies == null)
                    {
                        AnsiConsole.MarkupLine("[red]✗ Timeout waiting for strategies[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ Failed to submit: {response.StatusCode}[/]");
                    var error = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[dim]{Markup.Escape(error)}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
                AnsiConsole.WriteException(ex);
            }
        });

    if (strategies == null || !strategies.Any())
    {
        AnsiConsole.MarkupLine("[yellow]No strategies available. Request may still be processing.[/]");
        return;
    }

    // Display strategies
    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[green]Available Optimization Strategies[/]").RuleStyle("green"));
    AnsiConsole.WriteLine();

    var strategiesTable = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Green)
        .AddColumn(new TableColumn("[yellow]#[/]").Centered())
        .AddColumn("[yellow]Strategy[/]")
        .AddColumn(new TableColumn("[yellow]Cost[/]").RightAligned())
        .AddColumn(new TableColumn("[yellow]Duration[/]").RightAligned())
        .AddColumn(new TableColumn("[yellow]Quality[/]").RightAligned())
        .AddColumn(new TableColumn("[yellow]Emissions[/]").RightAligned())
        .AddColumn(new TableColumn("[yellow]Warranty[/]").Centered())
        .AddColumn(new TableColumn("[yellow]Insurance[/]").Centered());

    int index = 1;
    foreach (var strategy in strategies)
    {
        strategiesTable.AddRow(
            index.ToString(),
            $"[bold]{strategy.StrategyName}[/]\n[dim]{strategy.Description}[/]",
            $"€{strategy.Metrics.TotalCost:N2}",
            $"{strategy.Metrics.TotalDuration.TotalDays:N1} days",
            strategy.Metrics.AverageQuality.ToString("N2"),
            $"{strategy.Metrics.TotalEmissionsKgCO2:N1} kg",
            strategy.WarrantyTerms ?? "-",
            strategy.IncludesInsurance ? "[green]✓[/]" : "[dim]-[/]"
        );
        index++;
    }

    AnsiConsole.Write(strategiesTable);
    AnsiConsole.WriteLine();

    // Customer selects strategy
    var selectedIndex = AnsiConsole.Prompt(
        new SelectionPrompt<int>()
            .Title("[cyan]Select your preferred strategy:[/]")
            .AddChoices(Enumerable.Range(1, strategies.Count).ToArray())
            .UseConverter(i => $"{i}. {strategies[i - 1].StrategyName}"));

    var selectedStrategy = strategies[selectedIndex - 1];

    // Send selection to Gateway
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("[yellow]Submitting strategy selection...[/]", async ctx =>
        {
            try
            {
                var selectionDto = new
                {
                    RequestId = requestId,
                    StrategyId = selectedStrategy.StrategyId,
                    StrategyName = selectedStrategy.StrategyName
                };

                var response = await httpClient.PostAsJsonAsync("/api/optimization/select", selectionDto);

                if (response.IsSuccessStatusCode)
                {
                    ctx.Status("[green]✓ Strategy selected![/]");
                    AnsiConsole.WriteLine();
                    
                    // Display detailed strategy information
                    var detailsTable = new Table()
                        .Border(TableBorder.Rounded)
                        .BorderColor(Color.Green)
                        .AddColumn("[yellow]Property[/]")
                        .AddColumn("[green]Value[/]");
                    
                    detailsTable.AddRow("Strategy Name", $"[bold]{selectedStrategy.StrategyName}[/]");
                    detailsTable.AddRow("Priority", selectedStrategy.Priority.ToString());
                    detailsTable.AddRow("Workflow Type", selectedStrategy.WorkflowType);
                    detailsTable.AddRow("Total Cost", $"€{selectedStrategy.Metrics.TotalCost:N2}");
                    detailsTable.AddRow("Total Duration", $"{selectedStrategy.Metrics.TotalDuration.TotalHours:N1} hours ({selectedStrategy.Metrics.TotalDuration.TotalDays:N1} days)");
                    detailsTable.AddRow("Average Quality", $"{selectedStrategy.Metrics.AverageQuality:P0}");
                    detailsTable.AddRow("Total Emissions", $"{selectedStrategy.Metrics.TotalEmissionsKgCO2:N2} kg CO₂");
                    detailsTable.AddRow("Warranty Terms", selectedStrategy.WarrantyTerms ?? "-");
                    detailsTable.AddRow("Insurance", selectedStrategy.IncludesInsurance ? "[green]Included[/]" : "[dim]Not included[/]");
                    detailsTable.AddRow("Solver Status", selectedStrategy.Metrics.SolverStatus ?? "-");
                    detailsTable.AddRow("Objective Value", selectedStrategy.Metrics.ObjectiveValue.ToString("N4"));
                    detailsTable.AddRow("Generated At", selectedStrategy.GeneratedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"));
                    
                    AnsiConsole.Write(new Panel(detailsTable)
                        .Header("[yellow]Selected Strategy Details[/]")
                        .BorderColor(Color.Green));
                    
                    AnsiConsole.WriteLine();
                    
                    // Display process steps
                    if (selectedStrategy.Steps?.Any() == true)
                    {
                        var stepsTable = new Table()
                            .Border(TableBorder.Rounded)
                            .BorderColor(Color.Blue)
                            .AddColumn(new TableColumn("[yellow]Step[/]").Centered())
                            .AddColumn("[yellow]Activity[/]")
                            .AddColumn("[yellow]Provider[/]")
                            .AddColumn(new TableColumn("[yellow]Cost[/]").RightAligned())
                            .AddColumn(new TableColumn("[yellow]Time[/]").RightAligned())
                            .AddColumn(new TableColumn("[yellow]Quality[/]").RightAligned())
                            .AddColumn(new TableColumn("[yellow]Emissions[/]").RightAligned());
                        
                        foreach (var step in selectedStrategy.Steps.OrderBy(s => s.StepNumber))
                        {
                            stepsTable.AddRow(
                                step.StepNumber.ToString(),
                                step.Activity,
                                step.SelectedProviderName,
                                $"€{step.Estimate.Cost:N2}",
                                $"{step.Estimate.Duration.TotalHours:N1}h",
                                step.Estimate.QualityScore.ToString("P0"),
                                $"{step.Estimate.EmissionsKgCO2:N2} kg"
                            );
                        }
                        
                        AnsiConsole.Write(new Panel(stepsTable)
                            .Header($"[blue]Process Steps ({selectedStrategy.Steps.Count} total)[/]")
                            .BorderColor(Color.Blue));
                        
                        AnsiConsole.WriteLine();
                    }
                    
                    // Summary panel
                    AnsiConsole.Write(new Panel($"""
                        [green]✓ Your optimization plan is confirmed![/]
                        
                        [dim]Request ID: {requestId}[/]
                        [dim]Strategy ID: {selectedStrategy.StrategyId}[/]
                        
                        {selectedStrategy.Description}
                        """)
                        .Header("[yellow]Plan Confirmation[/]")
                        .BorderColor(Color.Green)
                        .Padding(1, 1));
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ Failed to select strategy: {response.StatusCode}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            }
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
                            .AddColumn("[yellow]Status[/]");
                        
                        foreach (var provider in result.Providers)
                        {
                            table.AddRow(
                                provider.Id.ToString(),
                                provider.Type,
                                provider.Name,
                                provider.Enabled ? "[green]Active[/]" : "[dim]Inactive[/]"
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
record ProvidersListResponse(int TotalProviders, List<ProviderDto> Providers);

record ProviderDto(
    Guid Id,
    string Type,
    string Name,
    bool Enabled,
    List<ProviderProcessCapabilityDto> ProcessCapabilities,
    ProviderTechnicalCapabilitiesDto TechnicalCapabilities
);

record ProviderProcessCapabilityDto(
    string ProcessName,
    decimal CostPerHour,
    double SpeedMultiplier,
    double QualityScore,
    double EnergyConsumptionKwhPerHour,
    double CarbonIntensityKgCO2PerKwh,
    bool UsesRenewableEnergy
);

record ProviderTechnicalCapabilitiesDto(
    double AxisHeight,
    double Power,
    double Tolerance
);

record StrategiesResponse(bool IsReady, List<OptimizationStrategyDto>? Strategies, string? Status);

record OptimizationStrategyDto(
    Guid StrategyId,
    string StrategyName,
    string Priority,
    string WorkflowType,
    List<OptimizationProcessStepDto> Steps,
    OptimizationMetricsDto Metrics,
    string WarrantyTerms,
    bool IncludesInsurance,
    string Description,
    DateTime GeneratedAt
);

record OptimizationProcessStepDto(
    int StepNumber,
    string Activity,
    Guid SelectedProviderId,
    string SelectedProviderName,
    ProcessEstimateDto Estimate
);

record ProcessEstimateDto(
    decimal Cost,
    TimeSpan Duration,
    double QualityScore,
    double EmissionsKgCO2
);

record OptimizationMetricsDto(
    decimal TotalCost,
    TimeSpan TotalDuration,
    double AverageQuality,
    double TotalEmissionsKgCO2,
    string SolverStatus,
    double ObjectiveValue
);