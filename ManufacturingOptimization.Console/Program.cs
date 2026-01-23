using Spectre.Console;
using System.Net.Http.Json;
using System.Text.Json;
using ManufacturingOptimization.Common.Models.DTOs;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using AutoMapper;

// Configuration
var apiUrl = Environment.GetEnvironmentVariable("GATEWAY_API_URL") ?? "http://localhost:5000";
var httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };

var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<OptimizationRequestModel, OptimizationRequestDto>()
        .ForMember(dest => dest.MotorSpecs, opt => opt.MapFrom(src => new MotorSpecificationsDto
        {
            PowerKW = src.MotorSpecs.PowerKW,
            AxisHeightMM = src.MotorSpecs.AxisHeightMM,
            CurrentEfficiency = src.MotorSpecs.CurrentEfficiency.ToString(),
            TargetEfficiency = src.MotorSpecs.TargetEfficiency.ToString(),
            MalfunctionDescription = src.MotorSpecs.MalfunctionDescription
        }))
        .ForMember(dest => dest.Constraints, opt => opt.MapFrom(src => new OptimizationRequestConstraintsDto
        {
            MaxBudget = src.Constraints.MaxBudget,
            RequiredDeadline = src.Constraints.RequiredDeadline
        }))
        .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.RequestId))
        .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
        .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
});
var mapper = mapperConfig.CreateMapper();

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

    await Task.CompletedTask;
}

async Task SubmitOptimizationRequest()
{
    AnsiConsole.Write(new Rule("[yellow]Submit Optimization Request[/]"));
    AnsiConsole.WriteLine();

    // Generate random MotorRequest
    var random = new Random();
    var efficiencyClasses = new[] { MotorEfficiencyClass.IE1, MotorEfficiencyClass.IE2, MotorEfficiencyClass.IE3, MotorEfficiencyClass.IE4 };

    var motorRequest = new OptimizationRequestModel
    {
        RequestId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid().ToString(),
        MotorSpecs = new MotorSpecificationsModel
        {
            PowerKW = random.Next(50, 200),
            AxisHeightMM = random.Next(63, 315),
            CurrentEfficiency = efficiencyClasses[random.Next(efficiencyClasses.Length)],
            TargetEfficiency = efficiencyClasses[random.Next(efficiencyClasses.Length)],
            MalfunctionDescription = random.Next(0, 2) == 0 ? "Normal operation" : "Reduced efficiency, overheating"
        },
        Constraints = new OptimizationRequestConstraintsModel
        {
            MaxBudget = random.Next(0, 3) == 0 ? null : random.Next(5000, 20000),
            RequiredDeadline = random.Next(0, 3) == 0 ? null : DateTime.Now.AddDays(random.Next(30, 90))
        }
    };

    var motorRequestDto = mapper.Map<OptimizationRequestDto>(motorRequest);

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
    List<OptimizationStrategyModel>? strategies = null;

    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("[yellow]Submitting request to Gateway...[/]", async ctx =>
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/api/optimization/request", motorRequestDto);

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
                                var statusResult = await statusResponse.Content.ReadFromJsonAsync<StrategiesResponseDto>();
                                
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
            strategy.Warranty?.Description ?? "-",
            strategy.Warranty?.IncludesInsurance == true ? "[green]✓[/]" : "[dim]-[/]"
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

    // Send selection to Gateway and retrieve plan
    OptimizationPlanDto? plan = null;
    
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("[yellow]Submitting strategy selection...[/]", async ctx =>
        {
            try
            {
                var selectionDto = new SelectOptimizationStrategyRequestDto
                {
                    RequestId = requestId,
                    SelectedStrategyId = selectedStrategy.Id
                };

                var response = await httpClient.PostAsJsonAsync("/api/optimization/select", selectionDto);

                if (response.IsSuccessStatusCode)
                {
                    ctx.Status("[green]✓ Strategy selected![/]");
                    
                    // Wait and poll for plan to be generated
                    ctx.Status("[yellow]Waiting for optimization plan...[/]");
                    
                    var startTime = DateTime.UtcNow;
                    var timeout = TimeSpan.FromSeconds(30);
                    var pollInterval = TimeSpan.FromSeconds(1);
                    
                    while (DateTime.UtcNow - startTime < timeout)
                    {
                        await Task.Delay(pollInterval);
                        
                        try
                        {
                            var planResponse = await httpClient.GetAsync($"/api/optimization/plan/{requestId}");
                            
                            if (planResponse.IsSuccessStatusCode)
                            {
                                plan = await planResponse.Content.ReadFromJsonAsync<OptimizationPlanDto>();
                                ctx.Status("[green]✓ Optimization plan retrieved![/]");
                                break;
                            }
                        }
                        catch
                        {
                            // Continue polling
                        }
                    }
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
    
    // Display plan outside of Status context to avoid concurrent interactive operations
    AnsiConsole.WriteLine();
    
    if (plan != null)
    {
        DisplayOptimizationPlan(plan);
    }
    else
    {
        AnsiConsole.MarkupLine("[red]✗ Timeout waiting for optimization plan[/]");
        AnsiConsole.MarkupLine("[dim]The plan may still be processing. Try retrieving it later using the Request ID.[/]");
    }
}

void DisplayOptimizationPlan(OptimizationPlanDto plan)
{
    if (plan.SelectedStrategy == null)
    {
        AnsiConsole.MarkupLine("[red]✗ Plan has no selected strategy[/]");
        return;
    }

    AnsiConsole.Write(new Rule("[green]Final Optimization Plan[/]").RuleStyle("green"));
    AnsiConsole.WriteLine();
    
    // Plan overview
    var overviewTable = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Green)
        .AddColumn("[yellow]Property[/]")
        .AddColumn("[green]Value[/]");
    
    overviewTable.AddRow("Plan ID", $"[bold]{plan.PlanId}[/]");
    overviewTable.AddRow("Request ID", plan.RequestId.ToString());
    overviewTable.AddRow("Strategy", $"[bold]{plan.SelectedStrategy.StrategyName}[/]");
    overviewTable.AddRow("Priority", plan.SelectedStrategy.Priority.ToString());
    overviewTable.AddRow("Workflow Type", plan.SelectedStrategy.WorkflowType);
    overviewTable.AddRow("Status", $"[{GetStatusColor(plan.Status)}]{plan.Status.ToString()}[/]");
    overviewTable.AddRow("Total Cost", $"€{plan.SelectedStrategy.Metrics.TotalCost:N2}");
    overviewTable.AddRow("Total Duration", $"{plan.SelectedStrategy.Metrics.TotalDuration.TotalDays:N1} days ({plan.SelectedStrategy.Metrics.TotalDuration.TotalHours:N1} hours)");
    overviewTable.AddRow("Average Quality", $"{plan.SelectedStrategy.Metrics.AverageQuality:P0}");
    overviewTable.AddRow("Total Emissions", $"{plan.SelectedStrategy.Metrics.TotalEmissionsKgCO2:N2} kg CO₂");
    overviewTable.AddRow("Warranty", plan.SelectedStrategy.Warranty?.Description ?? "-");
    overviewTable.AddRow("Insurance", plan.SelectedStrategy.Warranty?.IncludesInsurance == true ? "[green]✓ Included[/]" : "[dim]Not included[/]");
    overviewTable.AddRow("Solver Status", plan.SelectedStrategy.Metrics.SolverStatus ?? "-");
    overviewTable.AddRow("Objective Value", plan.SelectedStrategy.Metrics.ObjectiveValue.ToString("N6"));
    overviewTable.AddRow("Created At", plan.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"));
    
    AnsiConsole.Write(new Panel(overviewTable)
        .Header("[yellow]Plan Overview[/]")
        .BorderColor(Color.Green));
    
    AnsiConsole.WriteLine();
    
    // Process steps with providers
    if (plan.SelectedStrategy.Steps?.Any() == true)
    {
        var stepsTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn(new TableColumn("[yellow]Step[/]").Centered())
            .AddColumn("[yellow]Process[/]")
            .AddColumn("[yellow]Provider[/]")
            .AddColumn(new TableColumn("[yellow]Provider ID[/]").Centered())
            .AddColumn(new TableColumn("[yellow]Cost[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Duration[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Quality[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]CO₂[/]").RightAligned());
        
        foreach (var step in plan.SelectedStrategy.Steps.OrderBy(s => s.StepNumber))
        {
            stepsTable.AddRow(
                step.StepNumber.ToString(),
                step.Process.ToString(),
                $"[bold]{step.SelectedProviderName}[/]",
                step.SelectedProviderId.ToString()[..8] + "...",
                $"€{step.Estimate.Cost:N2}",
                $"{step.Estimate.Duration.TotalHours:N1}h",
                $"{step.Estimate.QualityScore:P0}",
                $"{step.Estimate.EmissionsKgCO2:N2} kg"
            );
        }
        
        AnsiConsole.Write(new Panel(stepsTable)
            .Header($"[blue]Execution Plan ({plan.SelectedStrategy.Steps.Count} steps)[/]")
            .BorderColor(Color.Blue));
        
        AnsiConsole.WriteLine();
    }
    
    // Success summary
    AnsiConsole.Write(new Panel($"""
        [green]✓ Your optimization plan is ready for execution![/]
        
        [bold]Plan ID:[/] [cyan]{plan.PlanId}[/]
        [bold]Strategy:[/] {plan.SelectedStrategy.StrategyName}
        
        [dim]{plan.SelectedStrategy.Description}[/]
        
        [yellow]Next steps:[/]
        • Providers will be notified to prepare for execution
        • You will receive updates as work progresses
        • Track progress via Plan ID: {plan.PlanId}
        """)
        .Header("[green]Plan Confirmed[/]")
        .BorderColor(Color.Green)
        .Padding(1, 1));
    
    AnsiConsole.WriteLine();
    
    // Display full plan as JSON
    AnsiConsole.WriteLine();
    var json = JsonSerializer.Serialize(plan, new JsonSerializerOptions 
    { 
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
    
    AnsiConsole.Write(new Panel(Markup.Escape(json))
        .Header("[yellow]Complete Optimization Plan (JSON)[/]")
        .BorderColor(Color.Yellow)
        .Expand());
}

string GetStatusColor(string status)
{
    return status switch
    {
        "InProgress" => "blue",
        "Completed" => "green",
        "Failed" => "red",
        _ => "white"
    };
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
                    var result = await response.Content.ReadFromJsonAsync<ProvidersResponseDto>();
                    
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