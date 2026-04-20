using System;
using System.Reflection;
using GameModule.GameApi;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModule.ModuleFetchData.Unity;
using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using GameModule.Signal;
using GameModule.Modules.Ads;
using GameModule.Modules.Currency;
using GameModule.Modules.Gold;
using GameModule.Modules.Level;
using GameModule.ModuleFetchData.Http;
using GameModule.Modules.Global;
using GameModule.Modules.Tracks;
using GameModule.Modules.Loadout;
using GameModule.Modules.Inventory;
using GameModule.Modules.Cards;

/// <summary>
/// Configures the dependency injection container for cloud code execution.
/// </summary>
public partial class ModuleConfig : ICloudCodeSetup
{
    /// <summary>
    /// Registers all scoped routines and dependencies dynamically.
    /// </summary>
    /// <param name="config">The injection container mapping properties securely.</param>
    public void Setup(ICloudCodeConfig config)
    {
        IGameApiClient gameApiClient = GameApiClient.Create();
        config.Dependencies.AddSingleton(gameApiClient);
        PushClient pushClient = PushClient.Create();
        config.Dependencies.AddSingleton(pushClient);

        RegisterScoped<IPlayerData, UnityPlayerData>(config);
        RegisterScoped<IGameState, UnityGameState>(config);
        RegisterScoped<IRemoteConfig, UnityRemoteConfig>(config);

        RegisterScoped<SignalModule>(config);

        Assembly gameApiAssembly = typeof(GameApiDispatcher).Assembly;
        config.Dependencies.AddSingleton(new GameApiRegistry(gameApiAssembly));
        RegisterScoped<GameApiDispatcher>(config);

        foreach (Type type in gameApiAssembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            foreach (Type iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType || iface.GetGenericTypeDefinition() != typeof(IGameApiHandler<,>))
                {
                    continue;
                }

                config.Dependencies.AddScoped(iface, type);
            }
        }

        RegisterModuleScoped<AdsService>(config);
        RegisterModuleScoped<GoldModule>(config);
        RegisterModuleScoped<CurrencyModule>(config);
        RegisterModuleScoped<LevelService>(config);
        RegisterModuleScoped<GlobalConfigModule>(config);
        RegisterModuleScoped<TracksModule>(config);
        RegisterModuleScoped<LoadoutModule>(config);
        RegisterModuleScoped<InventoryModule>(config);
        RegisterModuleScoped<CardsModule>(config);
    }

    private void RegisterScoped<T>(ICloudCodeConfig config) where T : class
    {
        config.Dependencies.AddScoped<T>();
    }

    private void RegisterScoped<TInterface, TImplementation>(ICloudCodeConfig config)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        config.Dependencies.AddScoped<TInterface, TImplementation>();
    }

    private void RegisterModuleScoped<T>(ICloudCodeConfig config) where T : class, IGameModule
    {
        config.Dependencies.AddScoped<IGameModule, T>();
        RegisterScoped<T>(config);
    }
}