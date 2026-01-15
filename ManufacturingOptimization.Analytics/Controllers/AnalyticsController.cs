using Microsoft.AspNetCore.Mvc;
using ManufacturingOptimization.Analytics.Services;
using System.Text;

namespace ManufacturingOptimization.Analytics.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsStore _store;

    public AnalyticsController(IAnalyticsStore store)
    {
        _store = store;
    }

    [HttpGet("dashboard")]
    public IActionResult GetDashboard()
    {
        var data = _store.GetSummary();
        return Ok(data);
    }

    // [US-24-T5] New Export Endpoint
    [HttpGet("export")]
    public IActionResult ExportData()
    {
        var history = _store.GetAll();
        var csv = new StringBuilder();

        // 1. Add CSV Header
        csv.AppendLine("RequestId,Provider,Strategy,Timestamp");

        // 2. Add Data Rows
        foreach (var item in history)
        {
            // Note: In a real app, handle commas in data with quotes
            csv.AppendLine($"{item.RequestId},{item.SelectedProviderId},{item.SelectedStrategyName},{DateTime.UtcNow}");
        }

        // 3. Return as a File
        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", "analytics_export.csv");
    }
}