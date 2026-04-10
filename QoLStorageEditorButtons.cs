using Mafi;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Input;
using Mafi.Unity.UiToolkit.Library.ObjectEditor;

namespace QoLCaptainOfIndustry;

public static class QoLStorageEditorButtons
{
    public static void Install(ObjEditorsRegistry registry, IInputScheduler scheduler)
    {
        RegisterModeAction(
            registry,
            scheduler,
            Storage.StorageCheatMode.None,
            "QoL Off",
            "Turns the selected storage cheat mode off.");

        RegisterModeAction(
            registry,
            scheduler,
            Storage.StorageCheatMode.KeepFull,
            "QoL Keep Full",
            "Keeps the selected storage full.");

        RegisterModeAction(
            registry,
            scheduler,
            Storage.StorageCheatMode.KeepEmpty,
            "QoL Keep Empty",
            "Keeps the selected storage empty.");
    }

    private static void RegisterModeAction(
        ObjEditorsRegistry registry,
        IInputScheduler scheduler,
        Storage.StorageCheatMode targetMode,
        string label,
        string description)
    {
        registry.RegisterActionPerType<Storage>(
            storage =>
            {
                scheduler.ScheduleInputCmd(new StorageSetCheatModeCmd(storage, targetMode));
                Log.Info($"QoLCaptainOfIndustry: queued storage mode change for {storage.Id} to {targetMode}");
                return storage;
            },
            label,
            description);
    }
}
