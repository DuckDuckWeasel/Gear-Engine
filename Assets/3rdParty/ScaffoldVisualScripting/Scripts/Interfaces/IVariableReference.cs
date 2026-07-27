
namespace Scaffold
{
    /// <summary>
    /// Interface for indicating that the class holds a reference to a scaffold variable, used primarily in editor.
    /// </summary>
    public interface IVariableReference : IStringLocationIdentifier
    {
        bool HasReference(Variable variable);
    }
}