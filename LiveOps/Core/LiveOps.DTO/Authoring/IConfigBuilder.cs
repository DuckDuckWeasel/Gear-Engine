namespace LiveOps.DTO.Authoring
{
    /// <summary>
    /// Contract for Unity-side config builders that compile ScriptableObject data into a remote DTO (<see cref="Build()"/>)
    /// and optionally pull asset-independent fields from a deserialized DTO (<see cref="Apply(TConfig)"/>).
    /// </summary>
    public interface IConfigBuilder<TConfig> where TConfig : class, new()
    {
        string ConfigKey { get; }

        TConfig Build();

        void Apply(TConfig pulled);
    }
}
