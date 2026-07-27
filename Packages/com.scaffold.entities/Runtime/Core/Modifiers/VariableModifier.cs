namespace Scaffold.Entities
{
    /// <summary>
    /// Base class for variable modifiers that transform a float value at runtime.
    /// Extend this to create custom modifier behaviors (add, multiply, override, etc.).
    /// </summary>
    public abstract class VariableModifier
    {
        /// <summary>
        /// The order in which this modifier is applied. Lower values are applied first.
        /// </summary>
        public virtual int Order => 0;

        /// <summary>
        /// Apply this modifier to the given base value and return the result.
        /// </summary>
        public abstract float Apply(float baseValue);
    }

    /// <summary>
    /// A modifier that adds a flat value to the base variable.
    /// </summary>
    public sealed class FloatAddModifier : VariableModifier
    {
        private readonly float amount;

        public FloatAddModifier(float amount)
        {
            this.amount = amount;
        }

        public override float Apply(float baseValue) => baseValue + amount;
    }
}
