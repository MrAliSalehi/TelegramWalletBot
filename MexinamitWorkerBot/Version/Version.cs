namespace MexinamitWorkerBot.Version;

public static class Version
{
    private const double VersionNumber = 1.0;
    private const string VersionName = "ver.doNotTouchMe";
    public static async Task<double> HandelVersionAsync(CancellationToken ct)
    {
        if (!File.Exists(VersionName))
            File.Create(VersionName);

        var read = await File.ReadAllLinesAsync(VersionName, ct);
        if (read is not { Length: > 0 }) return -1;
        
        var readVersion = read.ToList();
        var canParseVersion = double.TryParse(readVersion.First(), out var version);
        if (!canParseVersion) return -1;

        if (version is not VersionNumber)
            await File.WriteAllLinesAsync(VersionName, new List<string>() { $"{VersionNumber}" }, ct);
        
        return VersionNumber;

    }
}