namespace Scaffold.VisualScripting
{
    public interface ITextSubstitutionService
    {
        string Substitute(string input, BlackboardVariableSet variables);
    }
}
