﻿namespace MexinamitWorkerBot.Version;

public static class Version
{
    private const double VersionNumber = 1.1;
    private const string VersionName = "ver.doNotTouchMe";
    public static async Task<double> HandelVersionAsync(CancellationToken ct)
    {
        if (!File.Exists(VersionName))
        {
            File.Create(VersionName);
            await File.WriteAllLinesAsync(VersionName, new List<string>() { $"{VersionNumber}" }, ct);
        }

        var read = await File.ReadAllLinesAsync(VersionName, ct);
        if (read is not { Length: > 0 })
        {
            await File.WriteAllLinesAsync(VersionName, new List<string>() { $"{VersionNumber}" }, ct);
        };
        
        var readVersion = read.ToList();
        var canParseVersion = double.TryParse(readVersion.First(), out var version);
        if (!canParseVersion) return -1;

        if (version is not VersionNumber)
            await File.WriteAllLinesAsync(VersionName, new List<string>() { $"{VersionNumber}" }, ct);
        
        return VersionNumber;

    }
}