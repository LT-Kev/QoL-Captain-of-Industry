using System;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Game;
using Mafi.Core.Mods;

namespace QoLCaptainOfIndustry;

public sealed class QoLCaptainOfIndustryMod : IMod
{
    private QoLGuiBridge m_guiBridge;

    public QoLCaptainOfIndustryMod(ModManifest manifest)
    {
        Manifest = manifest;
        JsonConfig = new ModJsonConfig(this);
        ModConfig = default;
        Log.Info("QoLCaptainOfIndustry: constructed");
    }

    public ModManifest Manifest { get; }

    public bool IsUiOnly => false;

    public Option<IConfig> ModConfig { get; }

    public ModJsonConfig JsonConfig { get; }

    public void RegisterPrototypes(ProtoRegistrator registrator)
    {
    }

    public void RegisterDependencies(DependencyResolverBuilder depBuilder, Mafi.Core.Prototypes.ProtosDb protosDb, bool wasLoaded)
    {
        depBuilder.RegisterDependency<QoLCaptainOfIndustryCommands>().AsSelf();
    }

    public void EarlyInit(DependencyResolver resolver)
    {
    }

    public void Initialize(DependencyResolver resolver, bool gameWasLoaded)
    {
        var commands = resolver.Resolve<QoLCaptainOfIndustryCommands>();
        TryInstallStorageEditorButtons(resolver);

        if (JsonConfig.GetBool("auto_enable_source_sinks", true))
        {
            commands.EnableSourceSinks();
            Log.Info("QoLCaptainOfIndustry: source/sink tools enabled");
        }

        m_guiBridge = QoLGuiBridge.Install(commands);
        Log.Info("QoLCaptainOfIndustry: GUI bridge installed");
    }

    public void MigrateJsonConfig(VersionSlim savedVersion, Dict<string, object> savedValues)
    {
    }

    private static void TryInstallStorageEditorButtons(DependencyResolver resolver)
    {
        try
        {
            QoLStorageEditorButtons.Install(
                resolver.Resolve<Mafi.Unity.UiToolkit.Library.ObjectEditor.ObjEditorsRegistry>(),
                resolver.Resolve<Mafi.Core.Input.IInputScheduler>());
            Log.Info("QoLCaptainOfIndustry: storage inspector button installed");
        }
        catch (Exception ex)
        {
            Log.Warning($"QoLCaptainOfIndustry: storage inspector button unavailable in this API setup: {ex.Message}");
        }
    }

    public void Dispose()
    {
        m_guiBridge?.Dispose();
        m_guiBridge = null;
    }
}
