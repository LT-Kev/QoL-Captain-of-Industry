using System;
using System.Linq;
using System.Reflection;
using Mafi;
using Mafi.Core;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Console;
using Mafi.Core.Environment;
using Mafi.Core.Factory.ComputingPower;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Input;
using Mafi.Core.Maintenance;
using Mafi.Core.Population;
using Mafi.Core.Population.Refugees;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Core.Simulation;
using Mafi.Core.Terrain;
using Mafi.Core.Terrain.Designation;
using Mafi.Core.Terrain.Trees;
using Mafi.Core.Utils;
using Mafi.Core.World;
using Mafi.Core.Entities;
using Mafi.Core.Vehicles;

namespace QoLCaptainOfIndustry;

public sealed class QoLCaptainOfIndustryCommands
{
    private static readonly FieldInfo s_resultField = typeof(GameCommandResult).GetField("Result", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static readonly FieldInfo s_errorField = typeof(GameCommandResult).GetField("ErrorMessage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private readonly ProtosDb m_protosDb;
    private readonly IInputScheduler m_scheduler;
    private readonly SourceSinkCheatManager m_sourceSinkCheatManager;
    private readonly ResearchManager m_researchManager;
    private readonly UpointsManager m_upointsManager;
    private readonly WorkersManager m_workersManager;
    private readonly PopsHealthManager m_popsHealthManager;
    private readonly MaintenanceManager m_maintenanceManager;
    private readonly ElectricityManager m_electricityManager;
    private readonly ComputingManager m_computingManager;
    private readonly InstaBuildManager m_instaBuildManager;
    private readonly WeatherManager m_weatherManager;
    private readonly RefugeesManager m_refugeesManager;
    private readonly EntitiesManager m_entitiesManager;
    private readonly VehiclesManager m_vehiclesManager;
    private readonly SimLoopEvents m_simLoopEvents;
    private readonly ITerrainDumpingManager m_terrainDumpingManager;
    private readonly ITerrainMiningManager m_terrainMiningManager;
    private readonly TerrainManager m_terrainManager;
    private readonly ITreesManager m_treesManager;
    private readonly ITreePlantingManager m_treePlantingManager;
    private readonly VirtualResourceManager m_virtualResourceManager;
    private readonly ICalendar m_calendar;

    public QoLCaptainOfIndustryCommands(
        ProtosDb protosDb,
        IInputScheduler scheduler,
        SourceSinkCheatManager sourceSinkCheatManager,
        ResearchManager researchManager,
        UpointsManager upointsManager,
        WorkersManager workersManager,
        PopsHealthManager popsHealthManager,
        MaintenanceManager maintenanceManager,
        ElectricityManager electricityManager,
        ComputingManager computingManager,
        InstaBuildManager instaBuildManager,
        WeatherManager weatherManager,
        RefugeesManager refugeesManager,
        EntitiesManager entitiesManager,
        VehiclesManager vehiclesManager,
        SimLoopEvents simLoopEvents,
        ITerrainDumpingManager terrainDumpingManager,
        ITerrainMiningManager terrainMiningManager,
        TerrainManager terrainManager,
        ITreesManager treesManager,
        ITreePlantingManager treePlantingManager,
        VirtualResourceManager virtualResourceManager,
        ICalendar calendar)
    {
        m_protosDb = protosDb;
        m_scheduler = scheduler;
        m_sourceSinkCheatManager = sourceSinkCheatManager;
        m_researchManager = researchManager;
        m_upointsManager = upointsManager;
        m_workersManager = workersManager;
        m_popsHealthManager = popsHealthManager;
        m_maintenanceManager = maintenanceManager;
        m_electricityManager = electricityManager;
        m_computingManager = computingManager;
        m_instaBuildManager = instaBuildManager;
        m_weatherManager = weatherManager;
        m_refugeesManager = refugeesManager;
        m_entitiesManager = entitiesManager;
        m_vehiclesManager = vehiclesManager;
        m_simLoopEvents = simLoopEvents;
        m_terrainDumpingManager = terrainDumpingManager;
        m_terrainMiningManager = terrainMiningManager;
        m_terrainManager = terrainManager;
        m_treesManager = treesManager;
        m_treePlantingManager = treePlantingManager;
        m_virtualResourceManager = virtualResourceManager;
        m_calendar = calendar;
    }

    public void EnableSourceSinks()
    {
        m_sourceSinkCheatManager.SetAreSourcesAndSinksAllowed(true);
        InvokeHidden(m_sourceSinkCheatManager, "ShowSourcesSinksInToolbar");
    }

    public bool IsSourceSinkEnabled()
    {
        return m_sourceSinkCheatManager.AreSourcesAndSinksAllowed;
    }

    public bool IsInstantBuildEnabled()
    {
        return ReadHiddenBool(m_instaBuildManager, "IsInstaBuildEnabled");
    }

    public bool IsIgnoreMissingUnityEnabled()
    {
        return m_upointsManager.IgnoreMissingUnity;
    }

    public bool IsIgnoreMissingWorkersEnabled()
    {
        return m_workersManager.IgnoreMissingWorkers;
    }

    public bool IsDiseasesDisabled()
    {
        return m_popsHealthManager.DisableDiseases;
    }

    public bool IsIgnoreMissingMaintenanceEnabled()
    {
        return ReadPublicOrHiddenBool(m_maintenanceManager, "IgnoreMissingMaintenance");
    }

    public bool IsIgnoreMissingPowerEnabled()
    {
        return ReadPublicOrHiddenBool(m_electricityManager, "IgnoreMissingPower");
    }

    public bool IsIgnoreMissingComputingEnabled()
    {
        return ReadPublicOrHiddenBool(m_computingManager, "IgnoreMissingComputing");
    }

    public string GetUnityAmountText()
    {
        return m_upointsManager.Quantity.ToString();
    }

    public string GetWorkersAmountText()
    {
        return m_workersManager.AmountOfFreeWorkersOrMissing.ToString();
    }

    public string GetVehicleCountText()
    {
        return m_vehiclesManager.AllVehicles.Count().ToString();
    }

    public string GetVehicleLimitText()
    {
        return m_vehiclesManager.MaxVehiclesLimit.ToString();
    }

    public string GetVehicleLimitLeftText()
    {
        return m_vehiclesManager.VehiclesLimitLeft.ToString();
    }

    public string GetGameSpeedText()
    {
        return m_simLoopEvents.IsSimPaused
            ? "Paused"
            : $"x{Math.Max(1, m_simLoopEvents.SimSpeedMult)}";
    }

    public string[] FindProducts(string filter, int max = 24)
    {
        var normalizedFilter = Normalize(filter);
        return m_protosDb.All<ProductProto>()
            .Select(product => product.Id.ToString())
            .Where(id => string.IsNullOrEmpty(normalizedFilter) || id.IndexOf(normalizedFilter, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(id => id)
            .Take(max)
            .ToArray();
    }

    public string[] FindWeather(string filter, int max = 24)
    {
        var normalizedFilter = Normalize(filter);
        return m_protosDb.All<WeatherProto>()
            .Select(proto => proto.Id.ToString())
            .Where(id => string.IsNullOrEmpty(normalizedFilter) || id.IndexOf(normalizedFilter, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(id => id)
            .Take(max)
            .ToArray();
    }

    public string[] FindTerrainProducts(string filter, int max = 24)
    {
        var normalizedFilter = Normalize(filter);
        return m_protosDb.All<LooseProductProto>()
            .Where(product => product.CanBeLoadedOnTruck && product.CanBeOnTerrain)
            .Select(product => product.Id.ToString())
            .Where(id => string.IsNullOrEmpty(normalizedFilter) || id.IndexOf(normalizedFilter, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(id => id)
            .Take(max)
            .ToArray();
    }

    public string RunUiAction(Func<GameCommandResult> action)
    {
        try
        {
            return DescribeResult(action());
        }
        catch (Exception ex)
        {
            return ex.InnerException?.Message ?? ex.Message;
        }
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolHelp()
    {
        var lines = new[]
        {
            "QoL Captain of Industry commands:",
            "coi_qol_enable_source_sinks",
            "coi_qol_unlock_all_research",
            "coi_qol_finish_research",
            "coi_qol_instant_build [true|false]",
            "coi_qol_add_unity [amount]",
            "coi_qol_ignore_missing_unity [true|false]",
            "coi_qol_add_workers [amount]",
            "coi_qol_add_vehicle_limit [amount]",
            "coi_qol_set_game_speed [multiplier]",
            "coi_qol_ignore_missing_workers [true|false]",
            "coi_qol_disable_diseases [true|false]",
            "coi_qol_repair_all",
            "coi_qol_ignore_missing_maintenance [true|false]",
            "coi_qol_free_power [true|false]",
            "coi_qol_set_free_power_mw [mw]",
            "coi_qol_free_computing [true|false]",
            "coi_qol_set_storage_mode <off|full|empty>",
            "coi_qol_fill_storages <productId>",
            "coi_qol_list_products [filter]",
            "coi_qol_instant_mine",
            "coi_qol_instant_dump <productId>",
            "coi_qol_change_terrain <productId>",
            "coi_qol_add_trees",
            "coi_qol_remove_selected_trees",
            "coi_qol_refill_groundwater",
            "coi_qol_refill_groundcrude",
            "coi_qol_set_weather <weatherId>",
            "coi_qol_list_weather [filter]",
            "coi_qol_finish_refugees",
            "coi_qol_repair_fleet",
            "coi_qol_finish_exploration"
        };

        return Success(string.Join(Environment.NewLine, lines));
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolEnableSourceSinks()
    {
        EnableSourceSinks();
        return Success("Source/sink cheat buildings are now enabled and should appear in the toolbar.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolUnlockAllResearch()
    {
        InvokeHidden(m_researchManager, "Cheat_UnlockAllResearch");
        return Success("All research has been unlocked.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolFinishResearch()
    {
        m_scheduler.ScheduleInputCmd(new ResearchCheatFinishCmd());
        return Success("Current research will be finished.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolInstantBuild(bool enabled = true)
    {
        InvokeHidden(m_instaBuildManager, "SetInstaBuild", enabled);
        return Success($"Instant build {(enabled ? "enabled" : "disabled")}.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolAddUnity(int amount = 5000)
    {
        if (amount <= 0)
        {
            return Error("Amount must be greater than 0.");
        }

        InvokeHidden(m_upointsManager, "Cheat_addUnityOnce", new Upoints(amount));
        return Success($"Added {amount} unity.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolIgnoreMissingUnity(bool enabled = true)
    {
        InvokeHidden(m_upointsManager, "Cheat_IgnoreMissingUnity", enabled);
        return Success($"Ignore missing unity {(enabled ? "enabled" : "disabled")}.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolAddWorkers(int amount = 100)
    {
        if (amount == 0)
        {
            return Error("Amount must not be 0.");
        }

        m_workersManager.Cheat_addWorkers(amount);
        return Success($"Adjusted workers by {amount}.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolAddVehicleLimit(int amount = 10)
    {
        if (amount == 0)
        {
            return Error("Amount must not be 0.");
        }

        m_vehiclesManager.IncreaseVehicleLimit(amount);
        return Success($"Vehicle limit increased by {amount}. New limit: {m_vehiclesManager.MaxVehiclesLimit}.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolSetGameSpeed(int multiplier = 5)
    {
        if (multiplier <= 0)
        {
            return Error("Game speed multiplier must be greater than 0.");
        }

        if (multiplier > 20)
        {
            return Error("Game speed multiplier must be 20 or lower.");
        }

        m_scheduler.ScheduleInputCmd(new GameSpeedChangeCmd(multiplier));
        return Success($"Game speed change to x{multiplier} queued.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolIgnoreMissingWorkers(bool enabled = true)
    {
        InvokeHidden(m_workersManager, "Cheat_IgnoreMissingWorkers", enabled);
        return Success($"Ignore missing workers {(enabled ? "enabled" : "disabled")}.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolDisableDiseases(bool enabled = true)
    {
        InvokeHidden(m_popsHealthManager, "SetDisableDiseases", enabled);
        return Success($"Diseases {(enabled ? "disabled" : "enabled")}.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolRepairAll()
    {
        InvokeHidden(m_maintenanceManager, "Cheat_RepairAllEntities");
        InvokeHidden(m_maintenanceManager, "Cheat_FillAllMaintenanceBuffers");
        return Success("All maintenance buffers were filled and all entities were repaired.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolIgnoreMissingMaintenance(bool enabled = true)
    {
        InvokeHidden(m_maintenanceManager, "Cheat_IgnoreMissingMaintenance", enabled);
        return Success($"Ignore missing maintenance {(enabled ? "enabled" : "disabled")}.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolFreePower(bool enabled = true)
    {
        InvokeHidden(m_electricityManager, "Cheat_IgnoreMissingPower", enabled);
        return Success($"Ignore missing power {(enabled ? "enabled" : "disabled")}.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolSetFreePowerMw(int megawatts = 500)
    {
        if (megawatts <= 0)
        {
            m_electricityManager.Cheat_ClearFreeElectricityPerTick();
            return Success("Cleared extra free electricity generation.");
        }

        m_electricityManager.Cheat_AddFreeElectricityPerTick(Electricity.FromMw(megawatts));
        return Success($"Added {megawatts} MW of free electricity per tick.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolFreeComputing(bool enabled = true)
    {
        InvokeHidden(m_computingManager, "Cheat_IgnoreMissingComputing", enabled);
        return Success($"Ignore missing computing {(enabled ? "enabled" : "disabled")}.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolSetStorageMode(string mode)
    {
        if (!TryParseStorageMode(mode, out var parsedMode))
        {
            return Error("Unknown mode. Use off, full or empty.");
        }

        var changed = 0;
        foreach (var storage in m_entitiesManager.GetAllEntitiesOfType<Storage>())
        {
            m_scheduler.ScheduleInputCmd(new StorageSetCheatModeCmd(storage, parsedMode));
            changed++;
        }

        return Success($"Applied storage mode '{mode}' to {changed} storages.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolFillStorages(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            return Error("Please provide a product id. Use coi_qol_list_products to discover ids.");
        }

        if (!m_protosDb.TryFindProtoIgnoreCase<ProductProto>(productId.Trim(), out var product))
        {
            return Error($"Unknown product '{productId}'. Use coi_qol_list_products to find a valid id.");
        }

        var changed = 0;

        foreach (var storage in m_entitiesManager.GetAllEntitiesOfType<Storage>())
        {
            if (!storage.IsProductSupported(product))
            {
                continue;
            }

            m_scheduler.ScheduleInputCmd(new StorageCheatProductCmd(storage, product));
            changed++;
        }

        if (changed == 0)
        {
            return Error($"No storage accepted product '{product.Id}'.");
        }

        return Success($"Filled {changed} storages with {product.Id}.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolListProducts(string filter = "")
    {
        var normalizedFilter = Normalize(filter);
        var products = m_protosDb.All<ProductProto>()
            .Select(product => product.Id.ToString())
            .Where(id => string.IsNullOrEmpty(normalizedFilter) || id.IndexOf(normalizedFilter, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(id => id)
            .Take(60)
            .ToArray();

        if (products.Length == 0)
        {
            return Error("No product ids matched that filter.");
        }

        return Success(string.Join(Environment.NewLine, products));
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolInstantMine()
    {
        var changed = 0;

        foreach (var designation in m_terrainMiningManager.MiningDesignations.Where(x => x.IsNotFulfilled))
        {
            designation.ForEachTile((tile, _) =>
            {
                RemoveTreeAt(tile.TileCoord);
                tile.SetHeight(designation.GetTargetHeightAt(tile.TileCoord));
            });

            changed++;
        }

        return changed > 0
            ? Success($"Completed {changed} mining designations instantly.")
            : Error("No active mining designations were found.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolInstantDump(string productId)
    {
        if (!TryFindLooseTerrainProduct(productId, out var product))
        {
            return Error("Unknown terrain product. Use a loose product that can be on terrain, such as dirt or gravel.");
        }

        var terrainMaterial = new LooseProductQuantity(product, Quantity.MaxValue).ToTerrainThickness();
        var changed = 0;

        foreach (var designation in m_terrainDumpingManager.DumpingDesignations.Where(x => x.IsNotFulfilled))
        {
            designation.ForEachTile((tile, targetHeight) =>
            {
                RemoveTreeAt(tile.TileCoord);
                m_terrainManager.DumpMaterialUpToHeight(tile.CoordAndIndex, terrainMaterial.AsSlim, targetHeight);
            });

            changed++;
        }

        return changed > 0
            ? Success($"Completed {changed} dumping designations with {product.Id}.")
            : Error("No active dumping designations were found.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolChangeTerrain(string productId)
    {
        if (!TryFindLooseTerrainProduct(productId, out var product))
        {
            return Error("Unknown terrain product. Use a loose product that can be on terrain, such as dirt or gravel.");
        }

        var terrainMaterial = new LooseProductQuantity(product, Quantity.MaxValue).ToTerrainThickness();
        var changed = 0;

        foreach (var designation in m_terrainDumpingManager.DumpingDesignations)
        {
            designation.ForEachTile((TerrainTile tile, HeightTilesF _) =>
            {
                m_terrainManager.ConvertMaterialInFirstLayer(
                    tile.CoordAndIndex,
                    terrainMaterial.Material.SlimId,
                    ThicknessTilesF.One,
                    ThicknessTilesF.One);
            });

            changed++;
        }

        return changed > 0
            ? Success($"Changed the top terrain layer for {changed} dumping designations to {product.Id}.")
            : Error("No dumping designations were found to use as terrain markers.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolAddTrees()
    {
        var treeProto = m_protosDb.All<TreeProto>()
            .FirstOrDefault(proto => proto.Id.ToString().IndexOf("fir", StringComparison.OrdinalIgnoreCase) >= 0)
            ?? m_protosDb.All<TreeProto>().FirstOrDefault();

        if (treeProto == null)
        {
            return Error("No tree prototype was found in the current game data.");
        }

        var planted = 0;
        foreach (var designation in m_terrainDumpingManager.DumpingDesignations)
        {
            designation.ForEachTile((TerrainTile tile, HeightTilesF _) =>
            {
                if (m_treePlantingManager.TryAddManualTree(treeProto, tile.TileCoord, m_simLoopEvents.CurrentStep))
                {
                    planted++;
                }
            });
        }

        return planted > 0
            ? Success($"Queued {planted} trees on dumping designations.")
            : Error("No valid dumping tiles were found for tree placement.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolRemoveSelectedTrees()
    {
        var removed = 0;
        foreach (var treeId in m_treesManager.EnumerateSelectedTrees().ToArray())
        {
            if (m_treesManager.TryRemoveTree(treeId, false, true))
            {
                removed++;
            }
        }

        return removed > 0
            ? Success($"Removed {removed} selected trees.")
            : Error("No selected trees were found.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolRefillGroundwater()
    {
        return RefillVirtualResource("ground", "water", "groundwater");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolRefillGroundcrude()
    {
        return RefillVirtualResource("crude", null, "ground crude");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolSetWeather(string weatherId)
    {
        if (string.IsNullOrWhiteSpace(weatherId))
        {
            return Error("Please provide a weather id. Use coi_qol_list_weather to discover ids.");
        }

        return m_weatherManager.Cheat_TrySetWeatherFixed(weatherId.Trim())
            ? Success($"Weather set to '{weatherId.Trim()}'.")
            : Error($"Unknown weather '{weatherId}'. Use coi_qol_list_weather to discover ids.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolListWeather(string filter = "")
    {
        var normalizedFilter = Normalize(filter);
        var weather = m_protosDb.All<WeatherProto>()
            .Select(proto => proto.Id.ToString())
            .Where(id => string.IsNullOrEmpty(normalizedFilter) || id.IndexOf(normalizedFilter, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(id => id)
            .Take(40)
            .ToArray();

        if (weather.Length == 0)
        {
            return Error("No weather ids matched that filter.");
        }

        return Success(string.Join(Environment.NewLine, weather));
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolFinishRefugees()
    {
        m_refugeesManager.Cheat_FinishCurrentDiscovery();
        return Success("Finished the current refugees/beacon discovery.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolRepairFleet()
    {
        m_scheduler.ScheduleInputCmd(new FleetRepairCheatCmd());
        return Success("Fleet repair cheat queued.");
    }

    [ConsoleCommand(true, false, "", "")]
    public GameCommandResult coiQolFinishExploration()
    {
        m_scheduler.ScheduleInputCmd(new ExploreFinishCheatCmd());
        return Success("Fleet exploration finish cheat queued.");
    }

    private static string Normalize(string value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static bool TryParseStorageMode(string mode, out Storage.StorageCheatMode parsedMode)
    {
        switch (Normalize(mode).ToLowerInvariant())
        {
            case "off":
            case "none":
                parsedMode = Storage.StorageCheatMode.None;
                return true;
            case "full":
            case "keepfull":
            case "keep_full":
                parsedMode = Storage.StorageCheatMode.KeepFull;
                return true;
            case "empty":
            case "keepempty":
            case "keep_empty":
                parsedMode = Storage.StorageCheatMode.KeepEmpty;
                return true;
            default:
                parsedMode = Storage.StorageCheatMode.None;
                return false;
        }
    }

    private bool TryFindLooseTerrainProduct(string productId, out LooseProductProto product)
    {
        product = null;
        if (string.IsNullOrWhiteSpace(productId))
        {
            return false;
        }

        if (!m_protosDb.TryFindProtoIgnoreCase<LooseProductProto>(productId.Trim(), out product))
        {
            return false;
        }

        return product.CanBeLoadedOnTruck && product.CanBeOnTerrain;
    }

    private GameCommandResult RefillVirtualResource(string requiredIdPart, string requiredSecondaryIdPart, string displayName)
    {
        var resourceProto = m_protosDb.All<VirtualResourceProductProto>()
            .FirstOrDefault(proto =>
            {
                var id = proto.Id.ToString();
                var matchesPrimary = id.IndexOf(requiredIdPart, StringComparison.OrdinalIgnoreCase) >= 0;
                var matchesSecondary = string.IsNullOrEmpty(requiredSecondaryIdPart)
                    || id.IndexOf(requiredSecondaryIdPart, StringComparison.OrdinalIgnoreCase) >= 0;
                return matchesPrimary && matchesSecondary;
            });

        if (resourceProto == null)
        {
            return Error($"Could not find a virtual resource for {displayName}.");
        }

        var refilled = 0;
        foreach (var resource in m_virtualResourceManager.GetAllResourcesFor(resourceProto))
        {
            resource.AddAsMuchAs(resource.Capacity);
            refilled++;
        }

        return refilled > 0
            ? Success($"Refilled {refilled} {displayName} reserves.")
            : Error($"No {displayName} reserves were found on this map.");
    }

    private void RemoveTreeAt(Tile2i tile)
    {
        m_treesManager.TryRemoveTreeAt(tile, false, true);
    }

    private static GameCommandResult Success(string message)
    {
        return GameCommandResult.Success(message, false);
    }

    private static GameCommandResult Error(string message)
    {
        return GameCommandResult.Error(message, false);
    }

    private static object InvokeHidden(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            throw new MissingMethodException(target.GetType().FullName, methodName);
        }

        return method.Invoke(target, args);
    }

    private static bool ReadHiddenBool(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property == null)
        {
            throw new MissingMemberException(target.GetType().FullName, propertyName);
        }

        return (bool)property.GetValue(target);
    }

    private static bool ReadPublicOrHiddenBool(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property == null)
        {
            return false;
        }

        return (bool)property.GetValue(target);
    }

    private static string DescribeResult(GameCommandResult result)
    {
        var errorOption = s_errorField?.GetValue(result);
        if (TryReadOptionValue(errorOption, out var errorValue) && errorValue != null)
        {
            return errorValue.ToString();
        }

        var resultOption = s_resultField?.GetValue(result);
        if (TryReadOptionValue(resultOption, out var resultValue) && resultValue != null)
        {
            return resultValue.ToString();
        }

        return "Done.";
    }

    private static bool TryReadOptionValue(object option, out object value)
    {
        value = null;
        if (option == null)
        {
            return false;
        }

        var optionType = option.GetType();
        var hasValueProperty = optionType.GetProperty("HasValue", BindingFlags.Instance | BindingFlags.Public);
        var valueProperty = optionType.GetProperty("ValueOrNull", BindingFlags.Instance | BindingFlags.Public);

        if (hasValueProperty == null || valueProperty == null)
        {
            return false;
        }

        var hasValue = (bool)hasValueProperty.GetValue(option);
        if (!hasValue)
        {
            return false;
        }

        value = valueProperty.GetValue(option);
        return true;
    }
}
