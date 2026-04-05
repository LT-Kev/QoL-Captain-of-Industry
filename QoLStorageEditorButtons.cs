using Mafi;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Input;
using Mafi.Unity.UiToolkit.Library.ObjectEditor;

namespace QoLCaptainOfIndustry;

public static class QoLStorageEditorButtons
{
    public static void Install(ObjEditorsRegistry registry, IInputScheduler scheduler)
    {
        registry.RegisterActionPerType<Storage>(
            storage =>
            {
                var nextMode = GetNextMode(storage.CheatMode);
                scheduler.ScheduleInputCmd(new StorageSetCheatModeCmd(storage, nextMode));
                Log.Info($"QoLCaptainOfIndustry: queued storage mode change for {storage.Id} to {nextMode}");
                return storage;
            },
            "Storage Mode",
            "Cycles storage mode for the selected storage: Off -> Keep Full -> Keep Empty.");
    }

    private static Storage.StorageCheatMode GetNextMode(Storage.StorageCheatMode currentMode)
    {
        switch (currentMode)
        {
            case Storage.StorageCheatMode.None:
                return Storage.StorageCheatMode.KeepFull;
            case Storage.StorageCheatMode.KeepFull:
                return Storage.StorageCheatMode.KeepEmpty;
            default:
                return Storage.StorageCheatMode.None;
        }
    }
}
