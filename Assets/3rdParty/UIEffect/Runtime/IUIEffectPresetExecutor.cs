namespace Coffee.UIEffects
{
    /// <summary>
    /// Lets a UIEffectPreset provide its own application behavior.
    /// </summary>
    public interface IUIEffectPresetExecutor
    {
        void Execute(UIEffect target, bool append);
    }
}
