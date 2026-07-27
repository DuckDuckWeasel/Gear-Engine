using System;

namespace Scaffold
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class CommandInfoAttribute : Attribute
    {
        public CommandInfoAttribute(
            string category,
            string commandName,
            string helpText,
            int priority = 0)
        {
            Category = category;
            CommandName = commandName;
            HelpText = helpText;
            Priority = priority;
        }

        public string Category { get; set; }

        public string CommandName { get; set; }

        public string HelpText { get; set; }

        public int Priority { get; set; }
    }
}
