using Octokit;

var client = new GitHubClient(new ProductHeaderValue("dlss-swapper-manifest-builder"));

try
{
    var owner = "beeradmoore";
    var repository = "dlss-swapper-manifest-builder";

    var issueRequest = new RepositoryIssueRequest()
    {
        State = ItemStateFilter.All,
        SortProperty = IssueSort.Created,
        SortDirection = SortDirection.Descending,
    };
    
    var issues = await client.Issue.GetAllForRepository(owner, repository, issueRequest);

    foreach (var issue in issues)
    {
        Console.WriteLine($"#{issue.Number} - {issue.Title}");
        Console.WriteLine($"State: {issue.State}");
        Console.WriteLine($"Created: {issue.CreatedAt}");
        Console.WriteLine($"URL: {issue.HtmlUrl}");
        Console.WriteLine("------------------------");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}