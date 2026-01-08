using DLSS_Swapper_Manifest_Builder;
using NewDLLHandler;
using Octokit;
using Serilog;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

Log.Logger = new LoggerConfiguration()
	.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
	.WriteTo.Console()
	.CreateLogger();

Log.Information("Starting processing");


var knownDllHashes = new List<string>();
var handledIssues = new List<int>();
var knownDLLs = new Dictionary<string, List<KnownDLL>>();

var knownDLLHashesFile = "../../../../../../known_dll_hashes.txt";
var handledIssuesFile = "../../../../handled_issues.json";
var knownDLLSourcesFile = "../../../../../../known_dll_sources.json";
var knownDLLSourcesMissingFile = "../../../../../../known_dll_sources_missing.json";
var githubIssuesFile = "github_issues.json";

// This is read-only token for issues on a publicly readable repository, so it isn't super secret
var authTokenPath = "../../../../token.txt";

// Load known DLL hashes
if (File.Exists(knownDLLHashesFile))
{
    var lines = File.ReadAllLines(knownDLLHashesFile);
    foreach (var line in lines)
    {
        if (string.IsNullOrWhiteSpace(line) == true)
        {
            continue;
        }

        knownDllHashes.Add(line);
    }
}

// Need to handle this manually as users like to create a new issue and close it themselves for no reason 
if (File.Exists(handledIssuesFile))
{
    using (var stream = File.OpenRead(handledIssuesFile))
    {
        var tempHandledIssues = JsonSerializer.Deserialize<List<int>>(stream);
        if (tempHandledIssues is null)
        {
            Log.Error($"Could not load {handledIssuesFile}");
            Debugger.Break();
            return;
        }

        foreach (var handledIssue in tempHandledIssues)
        {
            if (handledIssues.Contains(handledIssue) == false)
            {
                handledIssues.Add(handledIssue);
            }
        }
    }
}

// Some issues are triggered as handled manually so they will be skipped lower.
var manuallyHandledIssues = new int[]
{
	3226, 3164, 3160, 3146, 3141, 3139, 3138, 
	3061, 

	3016, 3015, 3007, 2919, 2915, 2907, 2883, 2877, 2858, 2853,
	2810, 2767, 2754, 2739, 2722, 2703, 2687, 2686, 2669, 2656,
    2643, 2638, 2635, 2634, 2615, 2609, 2597, 2591, 2550, 2544, 
    2542, 2541, 2535, 2533, 2532, 2531, 2530, 2529, 2522, 2512, 
    2500, 2495, 2492, 2485, 2453, 2411, 2383, 2371, 2368, 2339, 
    2311, 2287, 2266, 2263, 2262, 2254, 2237, 

	2207, 2202, 2200, 2175, 2170,

    2142, 2108, 2079,

    2032, 2023, 2017, 1989, 1935, 1922, 1907, 1841,
    1829, 1828, 1825, 1800, 1799, 

    1798, 1778, 1747, 1732, 1720, 1686, 

    1639, 1627, 1607, 1597, 1594, 1588, 1571, 1569,
    1561, 1544, 1513, 

    1479, 1467, 1424, 1402, 1353, 1332, 1327, 1325,
    1291, 1289, 1288, 1287, 1279, 1277, 1266, 1256,
    1236, 1232, 1228, 1227, 1225, 1217, 1216, 1251,

    1197, 1164, 1045,
    1096, 1083,1055,
    1013, 831,

    795, 791, 788, 785, 778, 772, 760, 768, 747, 734,
    721, 718, 717, 716, 715, 708, 705,
    688, 672, 665, 656, 653, 646,
    621, 610, 561, 544, 543, 542, 499, 497, 456, 435,
    434, 430, 422, 420, 398, 393, 392, 375, 349, 346,
    340, 339, 328, 318, 289, 281, 277, 276, 273, 262,
    260, 258, 255, 257, 238, 233, 228, 226, 224, 198,
    189, 179, 162, 161, 160, 144, 142, 137, 136,  74,
     69,  54,  35,  17,   7,   6,   2,
};


foreach (var handledIssue in manuallyHandledIssues)
{
    if (handledIssues.Contains(handledIssue) == false)
    {
        handledIssues.Add(handledIssue);
    }
}

// Load existing known DLL sources file
if (File.Exists(knownDLLSourcesFile))
{
    using (var stream = File.OpenRead(knownDLLSourcesFile))
    {
        var tempKnownDLLs = JsonSerializer.Deserialize<Dictionary<string, List<KnownDLL>>>(stream);
        if (tempKnownDLLs is null)
        {
            Log.Error($"Could not load {knownDLLSourcesFile}");
            Debugger.Break();
            return;
        }

        knownDLLs = tempKnownDLLs;
    }
}

// If the categories don't exist, create them
if (knownDLLs.ContainsKey("DLSS") == false)
{
    knownDLLs["DLSS"] = new List<KnownDLL>();
}

if (knownDLLs.ContainsKey("DLSS_G") == false)
{
    knownDLLs["DLSS_G"] = new List<KnownDLL>();
}

if (knownDLLs.ContainsKey("DLSS_D") == false)
{
    knownDLLs["DLSS_D"] = new List<KnownDLL>();
}

if (knownDLLs.ContainsKey("FSR_31_DX12") == false)
{
    knownDLLs["FSR_31_DX12"] = new List<KnownDLL>();
}

if (knownDLLs.ContainsKey("FSR_31_VK") == false)
{
    knownDLLs["FSR_31_VK"] = new List<KnownDLL>();
}

if (knownDLLs.ContainsKey("XeSS") == false)
{
    knownDLLs["XeSS"] = new List<KnownDLL>();
}

if (knownDLLs.ContainsKey("XeLL") == false)
{
    knownDLLs["XeLL"] = new List<KnownDLL>();
}

if (knownDLLs.ContainsKey("XeSS_FG") == false)
{
    knownDLLs["XeSS_FG"] = new List<KnownDLL>();
}

if (knownDLLs.ContainsKey("XeSS_DX11") == false)
{
	knownDLLs["XeSS_DX11"] = new List<KnownDLL>();
}

try
{
    var issues = new List<LocalIssue>();

    // Cache issues data as we are using unauthenticated requests.
    if (File.Exists(githubIssuesFile))
    {
        // Only load from cache if its less than an hour old, otherwise we will fetch new below.
        var fileInfo = new FileInfo(githubIssuesFile);
        var hoursSinceLastModified = (DateTime.Now - fileInfo.LastWriteTime).TotalHours;
        if (hoursSinceLastModified < 1.0)
        {
            using (var stream = File.OpenRead(githubIssuesFile))
            {
                var newIssues = JsonSerializer.Deserialize<List<LocalIssue>>(stream);
                if (newIssues is not null)
                {
                    issues.AddRange(newIssues);
                }
            }
        }
    }

    if (issues.Count == 0)
    {
        var client = new GitHubClient(new ProductHeaderValue("dlss-swapper-manifest-builder"));

        if (File.Exists(authTokenPath))
        {
            var token = File.ReadAllText(authTokenPath);
            client.Credentials = new Credentials(token);
        }

        var owner = "beeradmoore";
        var repository = "dlss-swapper-manifest-builder";

        var issueRequest = new RepositoryIssueRequest()
        {
            State = ItemStateFilter.Open,
            SortProperty = IssueSort.Created,
            SortDirection = SortDirection.Descending,
            Since = new DateTimeOffset(2025, 03, 09, 0, 0, 0, TimeSpan.Zero), // Last updateed
        };

        var newIssues = await client.Issue.GetAllForRepository(owner, repository, issueRequest);
        File.WriteAllText(githubIssuesFile, JsonSerializer.Serialize(newIssues, new JsonSerializerOptions() { WriteIndented = true } ));
        
        foreach (var issue in newIssues)
        {
            issues.Add(LocalIssue.FromIssue(issue));
        }
    }

    var dllVersionRegex = new Regex(@"^-- (?<dll_name>.*), Version: (?<version>.*), Hash: (?<hash>([A-F0-9]{32}))$");
    var titleRegex = new Regex(@"^\[NEW DLLs\] Found on (\d*)-(\d*)-(\d*)$");
    var libraryRegex = new Regex(@"^(.*)Library: (?<library>.*)$");
    var libraryPrefix = "Library:";
    var gamePrefix = "- Game:";

    foreach (var issue in issues)
    {
		if (handledIssues.Contains(issue.Number))
        {
            continue;
        }

        if (issue.Body is null)
        {
            handledIssues.Add(issue.Number);
            Log.Information($"Issue #{issue.Number} has a null body");
            Debugger.Break();
            continue;
        }

        var body = issue.Body;
        if (body.Contains("Paste text from DLSS Swapper report between the markers"))
        {
            body = body.Replace("Paste text from DLSS Swapper report between the markers", string.Empty);
        }
        body = body.Trim('`', ' ', '\t', '\r', '\n');
        var bodyLines = body.Split('\n');

        var currentLibrary = string.Empty;
        var currentGame = string.Empty;
        var addedAtLeastOne = false;
        var isReadingAdditionalComments = false;
        foreach (var bodyLine in bodyLines)
        {
            if (string.IsNullOrWhiteSpace(bodyLine) == true)
            {
                continue;
            }

            if (bodyLine == "```")
            {
                continue;
            }

            // If the title is in the body but not with library following it we can skip the line.
            if (bodyLine.Contains("[NEW DLLs]") == true)
            {
                var titleMatch = titleRegex.Match(bodyLine);
                if (titleMatch.Success)
                {
                    continue;
                }
            }

            if (isReadingAdditionalComments)
            {
                if (bodyLine == "_No response_")
                {
                    continue;
                }

                Log.Information($"Additional notes: {bodyLine}");
                continue;
            }

            // Just the header, skip it.
            if (bodyLine == "### New DLL information")
            {
                continue;
            }

            if (bodyLine == "### Additional notes (optional)")
            {
                isReadingAdditionalComments = true;
                continue;
            }

            if (bodyLine.Contains(libraryPrefix))
            {
                var libraryMatch = libraryRegex.Match(bodyLine);
                if (libraryMatch.Success)
                {
                    currentLibrary = libraryMatch.Groups["library"].Value.Trim();
                }
                else
                {
                    Debugger.Break();
                }

                if (currentLibrary != "ManuallyAdded" &&
                      currentLibrary != "Steam" &&
                      currentLibrary != "GOG" &&
                      currentLibrary != "EpicGamesStore" &&
                      currentLibrary != "XboxApp" &&
                      currentLibrary != "BattleNet" &&
                      currentLibrary != "EAApp" &&
                      currentLibrary != "UbisoftConnect")
                {
                    Log.Information($"Unknown library: {currentLibrary}");
                    Debugger.Break();
                    continue;
                }

                continue;
            }

            if (currentLibrary == string.Empty)
            {
                Debugger.Break();
                continue;
            }


            if (bodyLine.StartsWith(gamePrefix))
            {
                currentGame = bodyLine.Substring(gamePrefix.Length).Trim();
                continue;
            }

            var match = dllVersionRegex.Match(bodyLine);
            if (match.Success)
            {
                var dll = match.Groups["dll_name"].Value.Trim();
               
                var dllType = dll switch
                {
                    "nvngx_dlss.dll" => "DLSS",
                    "nvngx_dlssg.dll" => "DLSS_G",
                    "nvngx_dlssd.dll" => "DLSS_D",
                    "amd_fidelityfx_dx12.dll" => "FSR_31_DX12",
                    "amd_fidelityfx_vk.dll" => "FSR_31_VK",
                    "libxess.dll" => "XeSS",
                    "libxell.dll" => "XeLL",
					"libxess_dx11.dll" => "XeSS_DX11",
					"libxess_fg.dll" => "XeSS_FG",
                    "nvngx_dlss.dll.dlsss" => "DLSS",
                    "nvngx_dlssg.dll.dlsss" => "DLSS_G",
                    "nvngx_dlssd.dll.dlsss" => "DLSS_D",
                    "amd_fidelityfx_dx12.dll.dlsss" => "FSR_31_DX12",
                    "amd_fidelityfx_vk.dll.dlsss" => "FSR_31_VK",
                    "libxess.dll.dlsss" => "XeSS",
					"libxess_dx11.dll.dlsss" => "XeSS_DX11",
					"libxell.dll.dlsss" => "XeLL",
                    "libxess_fg.dll.dlsss" => "XeSS_FG",
                    _ => string.Empty,
                };

                if (string.IsNullOrWhiteSpace(dllType) == true)
                {
                    Log.Information($"Unknown DLL: {dll}");
                    Debugger.Break();
                    continue;
                }

                var version = match.Groups["version"].Value.Trim();
                var hash = match.Groups["hash"].Value.Trim();

                if (version == "0.0.0.0")
                {
                    //Log.Information($"Unknown DLL version: {dll}");
                    //Debugger.Break();
                    continue;
                }

                // Check if it is already added.
                // If it is we update existing library/games
                // If it isn't we create it
                var existingKnownDLL = knownDLLs[dllType].Where(x => x.Hash == hash).FirstOrDefault();
                if (existingKnownDLL is null)
                {
                    existingKnownDLL = new KnownDLL()
                    {
                        DLLType = dllType,
                        Version = version,
                        Hash = hash,
                    };
                    knownDLLs[dllType].Add(existingKnownDLL);
                }
                else
                {
                    if (existingKnownDLL.DLLType != dllType)
                    {
                        Log.Information($"DLL Type mismatch: {existingKnownDLL.DLLType} != {dllType}");
                        Debugger.Break();
                    }
                    else if (existingKnownDLL.Version != version)
                    {
                        Log.Information($"Version mismatch: {existingKnownDLL.Version} != {version}");
                        Debugger.Break();
                    }
                }

                if (string.IsNullOrWhiteSpace(currentLibrary) == true)
                {
                    Log.Information($"No library for DLL: {dll}");
                    Debugger.Break();
                    continue;
                }
                if (existingKnownDLL.Sources.ContainsKey(currentLibrary) == false)
                {
                    existingKnownDLL.Sources[currentLibrary] = new List<string>();
                }

                if (existingKnownDLL.Sources[currentLibrary].Contains(currentGame, StringComparer.InvariantCultureIgnoreCase) == false)
                {
                    existingKnownDLL.Sources[currentLibrary].Add(currentGame);
                }

                addedAtLeastOne = true;
            }
            else
            {
                Log.Information($"Number: {issue.Number}");
                Log.Information($"https://github.com/beeradmoore/dlss-swapper-manifest-builder/issues/{issue.Number}");
                Log.Information($"============================");
                Log.Information(issue.Body);
                Log.Information($"============================");
                Debugger.Break();
            }
        }

        // Did not add any DLLs, needs manual review.
        if (addedAtLeastOne == false)
        {
            //Debugger.Break();
            Log.Information($"Number: {issue.Number}");
            Log.Information($"https://github.com/beeradmoore/dlss-swapper-manifest-builder/issues/{issue.Number}");
            Log.Information($"============================");
            Log.Information(issue.Body);
            Log.Information($"============================");
            continue;
        }

        // If errors were not reported from the above consider it handled.
        if (handledIssues.Contains(issue.Number) == false)
        {
            //Log.Information($"Handled issue #{issue.Number}");
            handledIssues.Add(issue.Number);
        }
    }

    int SortDLLs(KnownDLL a, KnownDLL b)
    {
        if (a.Version == b.Version)
        {
            // If version matches sort on hash. This has no real meaning but it makes it 
            // consitant between runs and updates.
            return b.Hash.CompareTo(a.Hash);
        }
        return b.Version.CompareTo(a.Version);
    }

    // Sort on DLL version
    knownDLLs["DLSS"].Sort(SortDLLs);
    knownDLLs["DLSS_G"].Sort(SortDLLs);
    knownDLLs["DLSS_D"].Sort(SortDLLs);
    knownDLLs["FSR_31_DX12"].Sort(SortDLLs);
    knownDLLs["FSR_31_VK"].Sort(SortDLLs);
    knownDLLs["XeSS"].Sort(SortDLLs);
    knownDLLs["XeLL"].Sort(SortDLLs);
    knownDLLs["XeSS_FG"].Sort(SortDLLs);
    knownDLLs["XeSS_DX11"].Sort(SortDLLs);

	// Write out the DLL source file list
	File.WriteAllText(knownDLLSourcesFile, JsonSerializer.Serialize(knownDLLs, new JsonSerializerOptions() { WriteIndented = true }));

    // Write handled issues to file
    File.WriteAllText(handledIssuesFile, JsonSerializer.Serialize(handledIssues, new JsonSerializerOptions() { WriteIndented = true }));


    // Build a known DLL hash list file
    foreach (var dllType in knownDLLs.Keys)
    {
        foreach (var knownDll in knownDLLs[dllType])
        {
            if (knownDllHashes.Contains(knownDll.Hash) == true)
            {
                continue;
            }
            knownDllHashes.Add(knownDll.Hash);
        }
    }

    File.WriteAllLines(knownDLLHashesFile, knownDllHashes);
    Log.Information($"Wrote {knownDllHashes.Count} known DLL hashes to {knownDLLHashesFile}");

    // Now that knownDLLs is written, we remove all DLLs not known to DLSS Swapper manifest.
    using (var stream = File.OpenRead("../../../../../../manifest.json"))
    {
        var manifest = JsonSerializer.Deserialize<Manifest>(stream);
        if (manifest is null)
        {
            Log.Error($"Could not load manifest.json");
            Debugger.Break();
            return;
        }

        void RemoveExisting(List<DLLRecord> manifestRecords, List<KnownDLL> knownDLLs)
        {
            var toRemoveList = new List<KnownDLL>();
            foreach (var knownDLL in knownDLLs)
            {
                if (manifestRecords.Any(x => x.MD5Hash.Equals(knownDLL.Hash, StringComparison.InvariantCultureIgnoreCase)) == true)
                {
                    toRemoveList.Add(knownDLL);
                }
            }

            foreach (var toRemove in toRemoveList)
            {
                knownDLLs.Remove(toRemove);
            }
        }

        RemoveExisting(manifest.DLSS, knownDLLs["DLSS"]);
        RemoveExisting(manifest.DLSS_G, knownDLLs["DLSS_G"]);
        RemoveExisting(manifest.DLSS_D, knownDLLs["DLSS_D"]);
        RemoveExisting(manifest.FSR_31_DX12, knownDLLs["FSR_31_DX12"]);
        RemoveExisting(manifest.FSR_31_VK, knownDLLs["FSR_31_VK"]);
        RemoveExisting(manifest.XeSS, knownDLLs["XeSS"]);
        RemoveExisting(manifest.XeSS_FG, knownDLLs["XeSS_FG"]);
        RemoveExisting(manifest.XeSS_DX11, knownDLLs["XeSS_DX11"]);
		RemoveExisting(manifest.XeLL, knownDLLs["XeLL"]);
    }

    // Write it out.
    File.WriteAllText(knownDLLSourcesMissingFile, JsonSerializer.Serialize(knownDLLs, new JsonSerializerOptions() { WriteIndented = true }));

}
catch (Exception ex)
{
    Log.Error(ex, "Error");
}