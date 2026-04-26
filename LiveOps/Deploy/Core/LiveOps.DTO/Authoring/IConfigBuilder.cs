namespace LiveOps.DTO.Authoring
{
    /// <summary>
    /// Contract implemented by Remote Config authoring builders (Unity ScriptableObjects)
    /// and mirrored by <c>Scaffold.LiveOps.Authoring.ConfigBuilderSO&lt;TConfig&gt;</c>.
    /// </summary>
    public interface IConfigBuilder<TConfig>
        where TConfig : class, new()
    {
        TConfig Build();

        void Apply(TConfig pulled);
    }
}
