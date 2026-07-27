using System;
using System.Globalization;

namespace Scaffold.VisualScripting
{
    public sealed class TextSubstitutionService : ITextSubstitutionService
    {
        public TextSubstitutionService(IBlackboardLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private readonly IBlackboardLogger logger;

        public string Substitute(string input, BlackboardVariableSet variables)
        {
            try
            {
                return SubstituteValues(input ?? string.Empty, variables ?? throw new ArgumentNullException(nameof(variables)));
            }
            catch (Exception exception)
            {
                logger.Error("Failed to substitute Blackboard variable values.", exception);
                throw;
            }
        }

        private string SubstituteValues(string input, BlackboardVariableSet variables)
        {
            string result = input;
            foreach (VariableCellBase cell in variables.Cells)
            {
                if (!string.IsNullOrWhiteSpace(cell.Key))
                {
                    result = result.Replace($"${{{cell.Key}}}", FormatValue(cell.UntypedValue));
                }
            }

            return result;
        }

        private string FormatValue(object value)
        {
            return value is IFormattable formattable ? formattable.ToString(null, CultureInfo.InvariantCulture) : value?.ToString() ?? string.Empty;
        }
    }
}
