using System;
using System.Linq;
using IssueList = System.Collections.Generic.IReadOnlyList<Scaffold.VisualScripting.BlackboardValidationIssue>;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardValidationException : InvalidOperationException
    {
        public BlackboardValidationException(IssueList issues) : base(CreateMessage(issues))
        {
            Issues = issues ?? throw new ArgumentNullException(nameof(issues));
        }

        public IssueList Issues { get; }

        private static string CreateMessage(IssueList issues)
        {
            if (issues == null)
            {
                throw new ArgumentNullException(nameof(issues));
            }

            string issueList = string.Join(Environment.NewLine, issues.Select(issue => $"- {issue}"));
            return "Blackboard definition validation failed:" + Environment.NewLine + issueList;
        }
    }
}
