/*
 * Proxy
 * Description: Controls access to another object while preserving the same contract.
 * Usage frequency: Common in authorization, lazy loading, remote clients, caching,
 * rate limiting, and generated service clients.
 */

internal sealed class ProxyPattern
{
    public string Description => "Guard a sensitive report service behind an authorization proxy.";

    public string UsageFrequency => "Common";

    public void Run()
    {
        ConsoleSection.Print("Proxy");

        IReportService reportService = new AuthorizedReportProxy(new SensitiveReportService(), hasAccess: true);
        Console.WriteLine(reportService.GetQuarterlyReport());
    }
}

internal interface IReportService
{
    string GetQuarterlyReport();
}

internal sealed class SensitiveReportService : IReportService
{
    public string GetQuarterlyReport() => "Quarterly report: margin improved by 12%.";
}

internal sealed class AuthorizedReportProxy(IReportService innerService, bool hasAccess) : IReportService
{
    public string GetQuarterlyReport() =>
        hasAccess ? innerService.GetQuarterlyReport() : "Access denied.";
}
