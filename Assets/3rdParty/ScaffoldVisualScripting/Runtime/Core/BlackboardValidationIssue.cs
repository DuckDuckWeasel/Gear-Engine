using System;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardValidationIssue
    {
        public BlackboardValidationIssue(string path, string message)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string Path { get; }

        public string Message { get; }

        public override string ToString()
        {
            return $"{Path}: {Message}";
        }
    }
}
