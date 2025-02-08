using NewDLLHandler;
using Octokit;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;


var knownDllHashes = new List<string>();
var handledIssues = new List<int>();
var knownDLLs = new Dictionary<string, List<KnownDLL>>();


var knownDLLHashesFile = "../../../../../../known_dll_hashes.txt";
var handledIssuesFile = "../../../../handled_issues.json";
var knownDLLSourcesFile = "../../../../../../known_dll_sources.json";
var githubIssuesFile = "github_issues.json";

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
            Console.WriteLine($"ERROR: Could not load {handledIssuesFile}");
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
            Console.WriteLine($"ERROR: Could not load {knownDLLSourcesFile}");
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

try
{
    var issues = new List<LocalIssue>();

    // Cache issues data as we are using unauthenticated requests.
    if (File.Exists(githubIssuesFile))
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
    else
    {
        var client = new GitHubClient(new ProductHeaderValue("dlss-swapper-manifest-builder"));

        var owner = "beeradmoore";
        var repository = "dlss-swapper-manifest-builder";

        var issueRequest = new RepositoryIssueRequest()
        {
            State = ItemStateFilter.All,
            SortProperty = IssueSort.Created,
            SortDirection = SortDirection.Descending,
            Since = new DateTimeOffset(2025, 02, 07, 0, 0, 0, TimeSpan.Zero), // Last updateed
        };

        var newIssues = await client.Issue.GetAllForRepository(owner, repository, issueRequest);
        File.WriteAllText(githubIssuesFile, JsonSerializer.Serialize(newIssues, new JsonSerializerOptions() { WriteIndented = true } ));
        
        foreach (var issue in newIssues)
        {
            issues.Add(LocalIssue.FromIssue(issue));
        }
    }

    var dllVersionRegex = new Regex(@"^-- (?<dll_name>.*), Version: (?<version>.*), Hash: (?<hash>([A-F0-9]*))$");
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
            Console.WriteLine($"Issue #{issue.Number} has a null body");
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
                      currentLibrary != "UbisoftConnect")
                {
                    Console.WriteLine($"Unknown library: {currentLibrary}");
                    Debugger.Break();
                    continue;
                }

                continue;
            }

            if (currentLibrary == string.Empty)
            {
                Debugger.Break();
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
                    "libxess_fg.dll" => "XeSS_FG",
                    "nvngx_dlss.dll.dlsss" => "DLSS",
                    "nvngx_dlssg.dll.dlsss" => "DLSS_G",
                    "nvngx_dlssd.dll.dlsss" => "DLSS_D",
                    "amd_fidelityfx_dx12.dll.dlsss" => "FSR_31_DX12",
                    "amd_fidelityfx_vk.dll.dlsss" => "FSR_31_VK",
                    "libxess.dll.dlsss" => "XeSS",
                    "libxell.dll.dlsss" => "XeLL",
                    "libxess_fg.dll.dlsss" => "XeSS_FG",
                    _ => string.Empty,
                };

                if (string.IsNullOrWhiteSpace(dllType) == true)
                {
                    Console.WriteLine($"Unknown DLL: {dll}");
                    Debugger.Break();
                    continue;
                }

                var version = match.Groups["version"].Value.Trim();
                var hash = match.Groups["hash"].Value.Trim();

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
                        Console.WriteLine($"DLL Type mismatch: {existingKnownDLL.DLLType} != {dllType}");
                        Debugger.Break();
                    }
                    else if (existingKnownDLL.Version != version)
                    {
                        Console.WriteLine($"Version mismatch: {existingKnownDLL.Version} != {version}");
                        Debugger.Break();
                    }
                }

                if (string.IsNullOrWhiteSpace(currentLibrary) == true)
                {
                    Console.WriteLine($"No library for DLL: {dll}");
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
                Debugger.Break();
            }
        }

        // Did not add any DLLs, needs manual review.
        if (addedAtLeastOne == false)
        {
            //Debugger.Break();
            Console.WriteLine($"Number: {issue.Number}");
            Console.WriteLine($"https://github.com/beeradmoore/dlss-swapper-manifest-builder/issues/{issue.Number}");
            Console.WriteLine($"============================");
            Console.WriteLine(issue.Body);
            Console.WriteLine($"============================");
            continue;
        }

        // If errors were not reported from the above consider it handled.
        if (handledIssues.Contains(issue.Number) == false)
        {
            //Console.WriteLine($"Handled issue #{issue.Number}");
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
    Console.WriteLine($"Wrote {knownDllHashes.Count} known DLL hashes to {knownDLLHashesFile}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}