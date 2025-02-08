using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewDLLHandler;

internal class LocalIssue
{
    public int Number { get; set; } = 0;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public LocalIssue()
    {

    }

    internal static LocalIssue FromIssue(Issue issue)
    {
        return new LocalIssue()
        {
            Number = issue.Number,
            Title = issue.Title,
            Body = issue.Body
        };
    }
}
