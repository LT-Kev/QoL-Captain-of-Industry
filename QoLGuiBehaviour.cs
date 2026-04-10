using System;
using Mafi.Unity;
using UnityEngine;

namespace QoLCaptainOfIndustry;

public sealed class QoLGuiBehaviour : MonoBehaviour
{
    private const int WindowId = 1594320;
    private const string StorageHudPosXKey = "QoLCaptainOfIndustry.StorageHud.PosX";
    private const string StorageHudPosYKey = "QoLCaptainOfIndustry.StorageHud.PosY";
    private const string TowerHudPosXKey = "QoLCaptainOfIndustry.TowerHud.PosX";
    private const string TowerHudPosYKey = "QoLCaptainOfIndustry.TowerHud.PosY";
    private const string SpeedHudPosXKey = "QoLCaptainOfIndustry.SpeedHud.PosX";
    private const string SpeedHudPosYKey = "QoLCaptainOfIndustry.SpeedHud.PosY";
    private const string HudOpacityKey = "QoLCaptainOfIndustry.Hud.Opacity";
    private const string CompactHudModeKey = "QoLCaptainOfIndustry.Hud.Compact";
    private const string ShowShortLabelsKey = "QoLCaptainOfIndustry.Hud.ShortLabels";
    private const string ShowWorldMarkersKey = "QoLCaptainOfIndustry.Hud.WorldMarkers";
    private const string ShowSpeedHudKey = "QoLCaptainOfIndustry.Hud.ShowSpeed";
    private const string ShowTowerHudKey = "QoLCaptainOfIndustry.Hud.ShowTower";

    private readonly string[] m_tabs = { "General", "Vehicles", "Terrain", "Storage", "Weather", "Settings" };
    private QoLCaptainOfIndustryCommands m_commands;
    private Rect m_windowRect = new Rect(120f, 80f, 760f, 760f);
    private Rect m_storageHudAnchorRect = new Rect(18f, 190f, 198f, 42f);
    private Rect m_towerHudAnchorRect = new Rect(18f, 292f, 198f, 42f);
    private Rect m_speedHudAnchorRect = new Rect(170f, 18f, 360f, 42f);
    private Vector2 m_scrollPosition;
    private Vector2 m_storageHudDragOffset;
    private Vector2 m_towerHudDragOffset;
    private Vector2 m_speedHudDragOffset;
    private bool m_isVisible;
    private bool m_isStorageHudExpanded;
    private bool m_isTowerHudExpanded;
    private bool m_isSpeedHudExpanded;
    private bool m_isDraggingStorageHud;
    private bool m_isDraggingTowerHud;
    private bool m_isDraggingSpeedHud;
    private bool m_storageHudPositionLoaded;
    private bool m_towerHudPositionLoaded;
    private bool m_speedHudPositionLoaded;
    private bool m_settingsLoaded;
    private int m_activeTab;
    private string m_statusMessage = "F8 opens the QoL window.";

    private string m_unityAmount = "5000";
    private string m_workersAmount = "200";
    private string m_vehicleLimitAmount = "20";
    private string m_customGameSpeedAmount = "5";
    private string m_powerMwAmount = "500";
    private string m_terrainProductFilter = "dirt";
    private string m_selectedTerrainProduct = "dirt";
    private string m_productFilter = "coal";
    private string m_selectedProduct = "coal";
    private string m_weatherFilter = "";
    private string m_selectedWeather = "";

    private bool m_instantBuild;
    private bool m_ignoreMissingUnity;
    private bool m_ignoreMissingWorkers;
    private bool m_disableDiseases;
    private bool m_ignoreMissingMaintenance;
    private bool m_ignoreMissingPower;
    private bool m_ignoreMissingComputing;
    private bool m_compactHudMode;
    private bool m_showShortLabels;
    private bool m_showWorldMarkers = true;
    private bool m_showSpeedHud = true;
    private bool m_showTowerHud = true;
    private float m_hudOpacity = 0.96f;

    private GUIStyle m_titleStyle;
    private GUIStyle m_subtitleStyle;
    private GUIStyle m_cardStyle;
    private GUIStyle m_primaryButtonStyle;
    private GUIStyle m_secondaryButtonStyle;
    private GUIStyle m_statusStyle;
    private GUIStyle m_statStyle;
    private GUIStyle m_tabStyle;
    private GUIStyle m_tabSelectedStyle;
    private GUIStyle m_storageOverlayFullStyle;
    private GUIStyle m_storageOverlayEmptyStyle;
    private GUIStyle m_storageHudButtonStyle;
    private GUIStyle m_storageHudPanelStyle;
    private GUIStyle m_storageHudGripStyle;
    private GUIStyle m_storageHudTitleStyle;

    public void Initialize(QoLCaptainOfIndustryCommands commands)
    {
        m_commands = commands;
        RefreshState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            m_isVisible = !m_isVisible;
            if (m_isVisible)
            {
                RefreshState();
            }
        }
    }

    private void OnGUI()
    {
        if (m_commands == null)
        {
            return;
        }

        EnsureSettingsLoaded();
        EnsureStyles();
        DrawStorageModeOverlays();
        DrawStorageHudWidget();
        DrawMineTowerHudWidget();
        DrawSpeedHudWidget();

        if (!m_isVisible)
        {
            return;
        }

        DrawBackdrop();
        m_windowRect = GUI.Window(WindowId, m_windowRect, DrawWindow, "QoL Captain of Industry");
    }

    private void DrawWindow(int windowId)
    {
        GUILayout.BeginVertical();
        DrawHeader();
        DrawTabs();

        m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, false, true, GUILayout.ExpandHeight(true));
        switch (m_activeTab)
        {
            case 0:
                DrawGeneralTab();
                break;
            case 1:
                DrawVehiclesTab();
                break;
            case 2:
                DrawTerrainTab();
                break;
            case 3:
                DrawStorageTab();
                break;
            case 4:
                DrawWeatherTab();
                break;
            case 5:
                DrawSettingsTab();
                break;
        }
        GUILayout.EndScrollView();

        GUILayout.Space(6f);
        GUILayout.Label(m_statusMessage, m_statusStyle, GUILayout.Height(54f));

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
    }

    private void DrawGeneralTab()
    {
        BeginCard("Quick Actions", "The high-impact cheats you probably want most often.");
        DrawButtonGrid(
            ("Enable Source/Sinks", () => RunAction(() => m_commands.coiQolEnableSourceSinks())),
            ("Unlock All Research", () => RunAction(() => m_commands.coiQolUnlockAllResearch())),
            ("Finish Research", () => RunAction(() => m_commands.coiQolFinishResearch())),
            ("Repair All", () => RunAction(() => m_commands.coiQolRepairAll())),
            ("Finish Refugees", () => RunAction(() => m_commands.coiQolFinishRefugees())),
            ("Repair Fleet", () => RunAction(() => m_commands.coiQolRepairFleet())),
            ("Finish Exploration", () => RunAction(() => m_commands.coiQolFinishExploration()))
        );
        EndCard();

        BeginCard("Presets", "Safe grouped presets for common sandbox sessions. They only touch options already exposed by the mod.");
        DrawButtonGrid(
            ("Builder", () => RunBatchAction(
                () => m_commands.coiQolInstantBuild(true),
                () => m_commands.coiQolIgnoreMissingWorkers(true),
                () => m_commands.coiQolIgnoreMissingMaintenance(true),
                () => m_commands.coiQolFreePower(true),
                () => m_commands.coiQolFreeComputing(true),
                () => m_commands.coiQolSetGameSpeed(5))),
            ("Sandbox", () => RunBatchAction(
                () => m_commands.coiQolEnableSourceSinks(),
                () => m_commands.coiQolInstantBuild(true),
                () => m_commands.coiQolIgnoreMissingUnity(true),
                () => m_commands.coiQolIgnoreMissingWorkers(true),
                () => m_commands.coiQolIgnoreMissingMaintenance(true),
                () => m_commands.coiQolFreePower(true),
                () => m_commands.coiQolFreeComputing(true),
                () => m_commands.coiQolDisableDiseases(true))),
            ("Logistics", () => RunBatchAction(
                () => m_commands.coiQolAddVehicleLimit(100),
                () => m_commands.coiQolRepairFleet(),
                () => m_commands.coiQolSetGameSpeed(10))),
            ("Reset Toggles", () => RunBatchAction(
                () => m_commands.coiQolInstantBuild(false),
                () => m_commands.coiQolIgnoreMissingUnity(false),
                () => m_commands.coiQolIgnoreMissingWorkers(false),
                () => m_commands.coiQolIgnoreMissingMaintenance(false),
                () => m_commands.coiQolFreePower(false),
                () => m_commands.coiQolFreeComputing(false),
                () => m_commands.coiQolDisableDiseases(false),
                () => m_commands.coiQolSetGameSpeed(1)))
        );
        EndCard();

        DrawSelectedEntityActionsCard();

        BeginCard("Game Speed", "Push the simulation beyond the stock x3 cap. Higher values can stress large saves.");
        GUILayout.BeginHorizontal();
        DrawStatChip("Current Speed", m_commands.GetGameSpeedText());
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);
        DrawButtonGrid(
            ("x1", () => RunAction(() => m_commands.coiQolSetGameSpeed(1))),
            ("x2", () => RunAction(() => m_commands.coiQolSetGameSpeed(2))),
            ("x3", () => RunAction(() => m_commands.coiQolSetGameSpeed(3))),
            ("x5", () => RunAction(() => m_commands.coiQolSetGameSpeed(5))),
            ("x10", () => RunAction(() => m_commands.coiQolSetGameSpeed(10))),
            ("x15", () => RunAction(() => m_commands.coiQolSetGameSpeed(15)))
        );
        GUILayout.Space(10f);
        DrawIntActionRow("Custom Speed", ref m_customGameSpeedAmount, 5, value => m_commands.coiQolSetGameSpeed(value));
        EndCard();

        BeginCard("Toggles", "Persistent switches for sandbox-style play.");
        DrawToggle("Instant Build", ref m_instantBuild, value => m_commands.coiQolInstantBuild(value));
        DrawToggle("Ignore Missing Unity", ref m_ignoreMissingUnity, value => m_commands.coiQolIgnoreMissingUnity(value));
        DrawToggle("Ignore Missing Workers", ref m_ignoreMissingWorkers, value => m_commands.coiQolIgnoreMissingWorkers(value));
        DrawToggle("Disable Diseases", ref m_disableDiseases, value => m_commands.coiQolDisableDiseases(value));
        DrawToggle("Ignore Missing Maintenance", ref m_ignoreMissingMaintenance, value => m_commands.coiQolIgnoreMissingMaintenance(value));
        DrawToggle("Ignore Missing Power", ref m_ignoreMissingPower, value => m_commands.coiQolFreePower(value));
        DrawToggle("Ignore Missing Computing", ref m_ignoreMissingComputing, value => m_commands.coiQolFreeComputing(value));
        EndCard();

        BeginCard("Resource Boosts", "Fast numeric cheats for the current save.");
        DrawIntActionRow("Add Unity", ref m_unityAmount, 5000, value => m_commands.coiQolAddUnity(value));
        DrawIntActionRow("Add Workers", ref m_workersAmount, 200, value => m_commands.coiQolAddWorkers(value));
        DrawIntActionRow("Set Free Power MW", ref m_powerMwAmount, 500, value => m_commands.coiQolSetFreePowerMw(value));
        EndCard();
    }

    private void DrawVehiclesTab()
    {
        BeginCard("Vehicle Capacity", "Increase the global vehicle cap for your island.");
        GUILayout.BeginHorizontal();
        DrawStatChip("Current Vehicles", m_commands.GetVehicleCountText());
        DrawStatChip("Vehicle Limit", m_commands.GetVehicleLimitText());
        DrawStatChip("Slots Left", m_commands.GetVehicleLimitLeftText());
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);
        DrawIntActionRow("Add Vehicle Limit", ref m_vehicleLimitAmount, 20, value => m_commands.coiQolAddVehicleLimit(value));
        EndCard();

        BeginCard("Notes", "This uses the live Update 4 vehicle manager instead of older reflection-heavy approaches.");
        GUILayout.Label("If the old vehicle limit button did nothing before, this panel is the replacement for it.", m_subtitleStyle);
        EndCard();
    }

    private void DrawStorageTab()
    {
        BeginCard("Selected Storage", "Select a storage building in the normal game UI to change only that specific storage.");
        GUILayout.BeginHorizontal();
        DrawStatChip("Selected Storage", m_commands.GetSelectedStorageText());
        DrawStatChip("Selection Mode", m_commands.IsStoragePinned() ? "Pinned" : "Live/Cached");
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.Label(m_commands.GetSelectedStorageScopeText(), m_subtitleStyle);
        GUILayout.Space(10f);
        DrawButtonGrid(
            (m_commands.IsStoragePinned() ? "Unpin Storage" : "Pin Storage", () => RunAction(() => m_commands.coiQolPinSelectedStorage(!m_commands.IsStoragePinned()))),
            ("Clear All Pins", () => RunAction(() => m_commands.coiQolClearSelectionPins()))
        );
        GUILayout.Space(8f);
        DrawButtonGrid(
            ("Selected Off", () => RunAction(() => m_commands.coiQolSetSelectedStorageMode("off"))),
            ("Selected Keep Full", () => RunAction(() => m_commands.coiQolSetSelectedStorageMode("full"))),
            ("Selected Keep Empty", () => RunAction(() => m_commands.coiQolSetSelectedStorageMode("empty")))
        );
        GUILayout.Space(8f);
        DrawActionButton("Fill Selected Storage", () =>
        {
            if (string.IsNullOrWhiteSpace(m_selectedProduct))
            {
                m_statusMessage = "Please select a product first.";
                return;
            }

            RunAction(() => m_commands.coiQolFillSelectedStorage(m_selectedProduct));
        }, true);
        EndCard();

        BeginCard("Storage Modes", "Apply one mode to every compatible storage on the island.");
        DrawButtonGrid(
            ("Mode Off", () => RunAction(() => m_commands.coiQolSetStorageMode("off"))),
            ("Keep Full", () => RunAction(() => m_commands.coiQolSetStorageMode("full"))),
            ("Keep Empty", () => RunAction(() => m_commands.coiQolSetStorageMode("empty")))
        );
        EndCard();

        BeginCard("Fill Storages", "Search a product id, select it, then fill all matching storages.");
        GUILayout.Label("Product Search", m_subtitleStyle);
        m_productFilter = GUILayout.TextField(m_productFilter ?? string.Empty);

        DrawSelectableList(m_commands.FindProducts(m_productFilter), ref m_selectedProduct, "No matching products.");

        GUILayout.Space(8f);
        GUILayout.Label($"Selected Product: {m_selectedProduct}", m_subtitleStyle);
        DrawActionButton("Fill Matching Storages", () =>
        {
            if (string.IsNullOrWhiteSpace(m_selectedProduct))
            {
                m_statusMessage = "Please select a product first.";
                return;
            }

            RunAction(() => m_commands.coiQolFillStorages(m_selectedProduct));
        }, true);
        EndCard();
    }

    private void DrawTerrainTab()
    {
        BeginCard("Selected Mine Tower", "Select a Mine Tower in the normal game UI to run tower-scoped terrain actions only for that tower.");
        GUILayout.BeginHorizontal();
        DrawStatChip("Selected Tower", m_commands.GetSelectedMineTowerText());
        DrawStatChip("Selection Mode", m_commands.IsMineTowerPinned() ? "Pinned" : "Live/Cached");
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.Label(m_commands.GetSelectedMineTowerScopeText(), m_subtitleStyle);
        GUILayout.Space(10f);
        DrawButtonGrid(
            (m_commands.IsMineTowerPinned() ? "Unpin Tower" : "Pin Tower", () => RunAction(() => m_commands.coiQolPinSelectedMineTower(!m_commands.IsMineTowerPinned()))),
            ("Clear All Pins", () => RunAction(() => m_commands.coiQolClearSelectionPins()))
        );
        GUILayout.Space(8f);
        DrawButtonGrid(
            ("Instant Mine Selected", () => RunAction(() => m_commands.coiQolInstantMineSelected())),
            ("Instant Dump Selected", () => RunTerrainProductAction(() => m_commands.coiQolInstantDumpSelected(m_selectedTerrainProduct))),
            ("Change Terrain Selected", () => RunTerrainProductAction(() => m_commands.coiQolChangeTerrainSelected(m_selectedTerrainProduct))),
            ("Add Trees Selected", () => RunAction(() => m_commands.coiQolAddTreesSelected()))
        );
        EndCard();

        BeginCard("Terrain Material", "Pick a dumpable loose material for terrain cheats like instant dump and surface conversion.");
        GUILayout.Label("Terrain Product Search", m_subtitleStyle);
        m_terrainProductFilter = GUILayout.TextField(m_terrainProductFilter ?? string.Empty);
        DrawSelectableList(m_commands.FindTerrainProducts(m_terrainProductFilter), ref m_selectedTerrainProduct, "No matching terrain products.");
        GUILayout.Space(8f);
        GUILayout.Label($"Selected Terrain Product: {m_selectedTerrainProduct}", m_subtitleStyle);
        EndCard();

        BeginCard("Terrain Actions", "Fast terrain operations for all active designations in the current save.");
        DrawButtonGrid(
            ("Instant Mine", () => RunAction(() => m_commands.coiQolInstantMine())),
            ("Instant Dump", () => RunTerrainProductAction(() => m_commands.coiQolInstantDump(m_selectedTerrainProduct))),
            ("Change Terrain", () => RunTerrainProductAction(() => m_commands.coiQolChangeTerrain(m_selectedTerrainProduct))),
            ("Add Trees", () => RunAction(() => m_commands.coiQolAddTrees()))
        );
        EndCard();

        BeginCard("Terrain Utilities", "Extra map helpers pulled over from the old terrain tab.");
        DrawButtonGrid(
            ("Remove Selected Trees", () => RunAction(() => m_commands.coiQolRemoveSelectedTrees())),
            ("Fill Groundwater", () => RunAction(() => m_commands.coiQolRefillGroundwater())),
            ("Fill Ground Crude", () => RunAction(() => m_commands.coiQolRefillGroundcrude()))
        );
        GUILayout.Space(8f);
        GUILayout.Label("This Update 4 port intentionally skips the old tower-ignore and terrain-physics toggles for now, so the tab stays stable.", m_subtitleStyle);
        EndCard();
    }

    private void DrawWeatherTab()
    {
        BeginCard("Weather Control", "Pick a fixed weather profile from the game's current weather ids.");
        GUILayout.Label("Weather Search", m_subtitleStyle);
        m_weatherFilter = GUILayout.TextField(m_weatherFilter ?? string.Empty);

        var weather = m_commands.FindWeather(m_weatherFilter);
        DrawSelectableWeatherList(weather);

        GUILayout.Space(8f);
        GUILayout.Label($"Selected Weather: {m_selectedWeather}", m_subtitleStyle);
        EndCard();
    }

    private void DrawActionButton(string label, Action action, bool isPrimary = false)
    {
        var style = isPrimary ? m_primaryButtonStyle : m_secondaryButtonStyle;
        if (GUILayout.Button(label, style, GUILayout.Height(32f)))
        {
            action();
        }
    }

    private void DrawToggle(string label, ref bool currentValue, Func<bool, Mafi.Core.Console.GameCommandResult> action)
    {
        var newValue = GUILayout.Toggle(currentValue, label);
        if (newValue != currentValue)
        {
            currentValue = newValue;
            RunAction(() => action(newValue));
        }
    }

    private void DrawIntActionRow(string label, ref string textValue, int fallback, Func<int, Mafi.Core.Console.GameCommandResult> action)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, m_subtitleStyle, GUILayout.Width(180f));
        textValue = GUILayout.TextField(textValue ?? string.Empty, GUILayout.Width(110f), GUILayout.Height(26f));
        if (GUILayout.Button("Apply", m_primaryButtonStyle, GUILayout.Width(96f), GUILayout.Height(28f)))
        {
            var value = fallback;
            if (!int.TryParse(textValue, out value))
            {
                value = fallback;
            }

            RunAction(() => action(value));
        }
        GUILayout.EndHorizontal();
    }

    private void RunAction(Func<Mafi.Core.Console.GameCommandResult> action)
    {
        m_statusMessage = m_commands.RunUiAction(action);
        RefreshState();
    }

    private void RunTerrainProductAction(Func<Mafi.Core.Console.GameCommandResult> action)
    {
        if (string.IsNullOrWhiteSpace(m_selectedTerrainProduct))
        {
            m_statusMessage = "Please select a terrain product first.";
            return;
        }

        RunAction(action);
    }

    private void RefreshState()
    {
        if (m_commands == null)
        {
            return;
        }

        m_instantBuild = m_commands.IsInstantBuildEnabled();
        m_ignoreMissingUnity = m_commands.IsIgnoreMissingUnityEnabled();
        m_ignoreMissingWorkers = m_commands.IsIgnoreMissingWorkersEnabled();
        m_disableDiseases = m_commands.IsDiseasesDisabled();
        m_ignoreMissingMaintenance = m_commands.IsIgnoreMissingMaintenanceEnabled();
        m_ignoreMissingPower = m_commands.IsIgnoreMissingPowerEnabled();
        m_ignoreMissingComputing = m_commands.IsIgnoreMissingComputingEnabled();

        if (string.IsNullOrWhiteSpace(m_selectedProduct))
        {
            m_selectedProduct = "coal";
        }

        if (string.IsNullOrWhiteSpace(m_selectedTerrainProduct))
        {
            m_selectedTerrainProduct = "dirt";
        }
    }

    private void RunBatchAction(params Func<Mafi.Core.Console.GameCommandResult>[] actions)
    {
        var lastMessage = "Done.";
        foreach (var action in actions)
        {
            lastMessage = m_commands.RunUiAction(action);
        }

        m_statusMessage = lastMessage;
        RefreshState();
    }

    private void DrawSelectedEntityActionsCard()
    {
        BeginCard("Selected Entity", "Quick context actions for whatever you currently have selected. Storages and mine towers get the richest shortcuts.");
        GUILayout.BeginHorizontal();
        DrawStatChip("Current", m_commands.GetSelectedEntityText());
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.Label(m_commands.GetSelectedEntityScopeText(), m_subtitleStyle);
        GUILayout.Space(10f);

        if (m_commands.HasSelectedStorage())
        {
            DrawButtonGrid(
                (m_commands.IsStoragePinned() ? "Unpin Storage" : "Pin Storage", () => RunAction(() => m_commands.coiQolPinSelectedStorage(!m_commands.IsStoragePinned()))),
                ("Fill Selected", () =>
                {
                    if (string.IsNullOrWhiteSpace(m_selectedProduct))
                    {
                        m_statusMessage = "Please select a product first in the Storage tab.";
                        return;
                    }

                    RunAction(() => m_commands.coiQolFillSelectedStorage(m_selectedProduct));
                }),
                ("Keep Full", () => RunAction(() => m_commands.coiQolSetSelectedStorageMode("full"))),
                ("Keep Empty", () => RunAction(() => m_commands.coiQolSetSelectedStorageMode("empty"))),
                ("Storage Off", () => RunAction(() => m_commands.coiQolSetSelectedStorageMode("off"))),
                ("Clear Pins", () => RunAction(() => m_commands.coiQolClearSelectionPins()))
            );
        }
        else if (m_commands.HasSelectedMineTower())
        {
            DrawButtonGrid(
                (m_commands.IsMineTowerPinned() ? "Unpin Tower" : "Pin Tower", () => RunAction(() => m_commands.coiQolPinSelectedMineTower(!m_commands.IsMineTowerPinned()))),
                ("Instant Mine", () => RunAction(() => m_commands.coiQolInstantMineSelected())),
                ("Instant Dump", () => RunTerrainProductAction(() => m_commands.coiQolInstantDumpSelected(m_selectedTerrainProduct))),
                ("Change Terrain", () => RunTerrainProductAction(() => m_commands.coiQolChangeTerrainSelected(m_selectedTerrainProduct))),
                ("Add Trees", () => RunAction(() => m_commands.coiQolAddTreesSelected())),
                ("Clear Pins", () => RunAction(() => m_commands.coiQolClearSelectionPins()))
            );
        }
        else
        {
            GUILayout.Label("No direct quick actions for this entity type yet. The new context layer is intentionally conservative so it stays stable.", m_subtitleStyle);
            DrawActionButton("Clear Pins", () => RunAction(() => m_commands.coiQolClearSelectionPins()));
        }

        EndCard();
    }

    private void DrawSettingsTab()
    {
        BeginCard("HUD Style", "Tune the small helper widgets without touching the rest of the mod.");
        DrawSliderSetting("HUD Opacity", ref m_hudOpacity, 0.45f, 1f, "{0:0.00}");
        DrawToggleSetting("Compact HUD Layout", ref m_compactHudMode);
        DrawToggleSetting("Short Labels", ref m_showShortLabels);
        DrawToggleSetting("World Storage Markers", ref m_showWorldMarkers);
        DrawToggleSetting("Show Speed HUD", ref m_showSpeedHud);
        DrawToggleSetting("Show Mine Tower HUD", ref m_showTowerHud);
        EndCard();

        BeginCard("HUD Positions", "All helper widgets are draggable. These buttons snap them back to a clean default if needed.");
        DrawButtonGrid(
            ("Reset Storage HUD", ResetStorageHudPosition),
            ("Reset Tower HUD", ResetTowerHudPosition),
            ("Reset Speed HUD", ResetSpeedHudPosition),
            ("Reset All HUDs", ResetAllHudPositions)
        );
        EndCard();

        BeginCard("Notes", "The normal game UI stays in charge. These HUDs sit on top as light helpers, so they are easier to keep stable across game patches.");
        GUILayout.Label("If you ever want the widgets cleaner or closer to the vanilla HUD style, we can now iterate on that safely from here.", m_subtitleStyle);
        EndCard();
    }

    private void DrawToggleSetting(string label, ref bool value)
    {
        var newValue = GUILayout.Toggle(value, label);
        if (newValue != value)
        {
            value = newValue;
            SaveSettings();
        }
    }

    private void DrawSliderSetting(string label, ref float value, float min, float max, string format)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, m_subtitleStyle, GUILayout.Width(180f));
        var newValue = GUILayout.HorizontalSlider(value, min, max);
        GUILayout.Label(string.Format(format, newValue), m_subtitleStyle, GUILayout.Width(56f));
        GUILayout.EndHorizontal();

        if (Mathf.Abs(newValue - value) > 0.001f)
        {
            value = newValue;
            SaveSettings();
        }
    }

    private void EnsureSettingsLoaded()
    {
        if (m_settingsLoaded)
        {
            return;
        }

        m_hudOpacity = PlayerPrefs.GetFloat(HudOpacityKey, m_hudOpacity);
        m_compactHudMode = PlayerPrefs.GetInt(CompactHudModeKey, 0) == 1;
        m_showShortLabels = PlayerPrefs.GetInt(ShowShortLabelsKey, 0) == 1;
        m_showWorldMarkers = PlayerPrefs.GetInt(ShowWorldMarkersKey, 1) == 1;
        m_showSpeedHud = PlayerPrefs.GetInt(ShowSpeedHudKey, 1) == 1;
        m_showTowerHud = PlayerPrefs.GetInt(ShowTowerHudKey, 1) == 1;
        m_settingsLoaded = true;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(HudOpacityKey, m_hudOpacity);
        PlayerPrefs.SetInt(CompactHudModeKey, m_compactHudMode ? 1 : 0);
        PlayerPrefs.SetInt(ShowShortLabelsKey, m_showShortLabels ? 1 : 0);
        PlayerPrefs.SetInt(ShowWorldMarkersKey, m_showWorldMarkers ? 1 : 0);
        PlayerPrefs.SetInt(ShowSpeedHudKey, m_showSpeedHud ? 1 : 0);
        PlayerPrefs.SetInt(ShowTowerHudKey, m_showTowerHud ? 1 : 0);
        PlayerPrefs.Save();
        InvalidateStyles();
    }

    private void InvalidateStyles()
    {
        m_titleStyle = null;
        m_subtitleStyle = null;
        m_cardStyle = null;
        m_primaryButtonStyle = null;
        m_secondaryButtonStyle = null;
        m_statusStyle = null;
        m_statStyle = null;
        m_tabStyle = null;
        m_tabSelectedStyle = null;
        m_storageOverlayFullStyle = null;
        m_storageOverlayEmptyStyle = null;
        m_storageHudButtonStyle = null;
        m_storageHudPanelStyle = null;
        m_storageHudGripStyle = null;
        m_storageHudTitleStyle = null;
    }

    private void EnsureStyles()
    {
        if (m_titleStyle != null)
        {
            return;
        }

        m_titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.95f, 0.96f, 0.98f) }
        };

        m_subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            wordWrap = true,
            normal = { textColor = new Color(0.73f, 0.78f, 0.84f) }
        };

        m_cardStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(14, 14, 12, 14),
            margin = new RectOffset(0, 0, 0, 12)
        };
        m_cardStyle.normal.background = MakeTexture(new Color(0.12f, 0.15f, 0.19f, Mathf.Clamp01(m_hudOpacity)));
        m_cardStyle.normal.textColor = Color.white;

        m_primaryButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };
        m_primaryButtonStyle.normal.background = MakeTexture(new Color(0.19f, 0.45f, 0.31f, 1f));
        m_primaryButtonStyle.normal.textColor = Color.white;
        m_primaryButtonStyle.hover.background = MakeTexture(new Color(0.22f, 0.54f, 0.37f, 1f));

        m_secondaryButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12
        };
        m_secondaryButtonStyle.normal.background = MakeTexture(new Color(0.20f, 0.24f, 0.30f, 1f));
        m_secondaryButtonStyle.normal.textColor = new Color(0.92f, 0.94f, 0.97f);
        m_secondaryButtonStyle.hover.background = MakeTexture(new Color(0.26f, 0.31f, 0.38f, 1f));

        m_statusStyle = new GUIStyle(GUI.skin.box)
        {
            wordWrap = true,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(12, 12, 10, 10),
            fontSize = 12
        };
        m_statusStyle.normal.background = MakeTexture(new Color(0.10f, 0.18f, 0.24f, Mathf.Clamp01(m_hudOpacity)));
        m_statusStyle.normal.textColor = new Color(0.92f, 0.96f, 1f);

        m_statStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(0, 8, 0, 0)
        };
        m_statStyle.normal.background = MakeTexture(new Color(0.18f, 0.22f, 0.27f, 1f));
        m_statStyle.normal.textColor = new Color(0.94f, 0.95f, 0.97f);

        m_tabStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold
        };
        m_tabStyle.normal.background = MakeTexture(new Color(0.17f, 0.19f, 0.23f, 1f));
        m_tabStyle.normal.textColor = new Color(0.82f, 0.86f, 0.90f);

        m_tabSelectedStyle = new GUIStyle(m_tabStyle);
        m_tabSelectedStyle.normal.background = MakeTexture(new Color(0.28f, 0.36f, 0.18f, 1f));
        m_tabSelectedStyle.normal.textColor = Color.white;

        m_storageOverlayFullStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(6, 6, 4, 4)
        };
        m_storageOverlayFullStyle.normal.background = MakeTexture(new Color(0.12f, 0.50f, 0.26f, Mathf.Clamp(m_hudOpacity, 0.35f, 1f)));
        m_storageOverlayFullStyle.normal.textColor = Color.white;

        m_storageOverlayEmptyStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(6, 6, 4, 4)
        };
        m_storageOverlayEmptyStyle.normal.background = MakeTexture(new Color(0.70f, 0.34f, 0.10f, Mathf.Clamp(m_hudOpacity, 0.35f, 1f)));
        m_storageOverlayEmptyStyle.normal.textColor = Color.white;

        m_storageHudButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(12, 12, 10, 10)
        };
        m_storageHudButtonStyle.normal.background = MakeTexture(new Color(0.10f, 0.16f, 0.21f, Mathf.Clamp01(m_hudOpacity)));
        m_storageHudButtonStyle.normal.textColor = Color.white;
        m_storageHudButtonStyle.hover.background = MakeTexture(new Color(0.15f, 0.23f, 0.30f, Mathf.Clamp01(m_hudOpacity + 0.03f)));

        m_storageHudPanelStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(12, 12, 12, 12)
        };
        m_storageHudPanelStyle.normal.background = MakeTexture(new Color(0.08f, 0.11f, 0.16f, Mathf.Clamp01(m_hudOpacity)));
        m_storageHudPanelStyle.normal.textColor = Color.white;

        m_storageHudGripStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(0, 0, 10, 10)
        };
        m_storageHudGripStyle.normal.background = MakeTexture(new Color(0.24f, 0.31f, 0.39f, Mathf.Clamp01(m_hudOpacity + 0.02f)));
        m_storageHudGripStyle.normal.textColor = new Color(0.92f, 0.95f, 0.98f);

        m_storageHudTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.94f, 0.97f, 1f) }
        };
    }

    private void DrawStorageModeOverlays()
    {
        if (!m_showWorldMarkers)
        {
            return;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        var pulse = 0.82f + (Mathf.Sin(Time.realtimeSinceStartup * 3.3f) * 0.12f);

        foreach (var info in m_commands.GetStorageModeOverlayInfos())
        {
            var worldPosition = info.CenterTile.CenterTile3f.ToVector3();
            worldPosition.y += 0.15f;

            var screenPosition = camera.WorldToScreenPoint(worldPosition);
            if (screenPosition.z <= 0f)
            {
                continue;
            }

            var label = info.Mode == Mafi.Core.Buildings.Storages.Storage.StorageCheatMode.KeepEmpty
                ? (m_showShortLabels ? "E" : "EMPTY")
                : (m_showShortLabels ? "F" : "FULL");
            var style = info.Mode == Mafi.Core.Buildings.Storages.Storage.StorageCheatMode.KeepEmpty
                ? m_storageOverlayEmptyStyle
                : m_storageOverlayFullStyle;

            var width = m_showShortLabels ? 32f : 92f;
            var height = 26f;
            var verticalJitter = ((info.EntityId % 3) - 1) * 10f;
            var rect = new Rect(
                Mathf.Round(screenPosition.x - (width * 0.5f)),
                Mathf.Round(Screen.height - screenPosition.y - 44f + verticalJitter),
                width,
                height);

            var previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.22f * pulse);
            GUI.Box(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), string.Empty, style);
            GUI.color = new Color(1f, 1f, 1f, pulse);
            GUI.Box(rect, label, style);
            GUI.color = previousColor;
        }
    }

    private void DrawStorageHudWidget()
    {
        if (!m_commands.HasSelectedStorage())
        {
            m_isStorageHudExpanded = false;
            m_isDraggingStorageHud = false;
            return;
        }

        EnsureStorageHudPositionLoaded();

        var currentModeText = m_commands.GetSelectedStorageHudText();
        var anchorRect = m_storageHudAnchorRect;
        var mainButtonRect = new Rect(anchorRect.x, anchorRect.y, anchorRect.width - 28f, anchorRect.height);
        var gripRect = new Rect(anchorRect.xMax - 24f, anchorRect.y, 24f, anchorRect.height);
        var panelRect = new Rect(anchorRect.x, anchorRect.yMax + 8f, 220f, m_compactHudMode ? 182f : 234f);
        HandleStorageHudOutsideClick(mainButtonRect, gripRect, panelRect);
        HandleStorageHudDragging(gripRect, panelRect);

        var buttonText = m_showShortLabels ? currentModeText.Replace("Storage: ", "S: ") : currentModeText;
        if (GUI.Button(mainButtonRect, buttonText, m_storageHudButtonStyle))
        {
            m_isStorageHudExpanded = !m_isStorageHudExpanded;
        }

        GUI.Box(gripRect, ":::", m_storageHudGripStyle);

        if (!m_isStorageHudExpanded)
        {
            return;
        }

        GUILayout.BeginArea(panelRect, m_storageHudPanelStyle);
        GUILayout.Label("Selected Storage Controls", m_storageHudTitleStyle);
        GUILayout.Label(m_commands.GetSelectedStorageText(), m_subtitleStyle);
        GUILayout.Label(m_commands.GetSelectedStorageScopeText(), m_subtitleStyle);
        GUILayout.Space(8f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(m_commands.IsStoragePinned() ? "Unpin" : "Pin", m_secondaryButtonStyle, GUILayout.Height(28f)))
        {
            RunAction(() => m_commands.coiQolPinSelectedStorage(!m_commands.IsStoragePinned()));
        }

        if (GUILayout.Button("Fill", m_primaryButtonStyle, GUILayout.Height(28f)))
        {
            if (string.IsNullOrWhiteSpace(m_selectedProduct))
            {
                m_statusMessage = "Please pick a product in the Storage tab first.";
            }
            else
            {
                RunAction(() => m_commands.coiQolFillSelectedStorage(m_selectedProduct));
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(4f);

        if (GUILayout.Button(m_showShortLabels ? "Off" : "Mode Off", m_secondaryButtonStyle, GUILayout.Height(30f)))
        {
            RunAction(() => m_commands.coiQolSetSelectedStorageMode("off"));
        }

        if (GUILayout.Button(m_showShortLabels ? "Full" : "Keep Full", m_primaryButtonStyle, GUILayout.Height(30f)))
        {
            RunAction(() => m_commands.coiQolSetSelectedStorageMode("full"));
        }

        if (GUILayout.Button(m_showShortLabels ? "Empty" : "Keep Empty", m_secondaryButtonStyle, GUILayout.Height(30f)))
        {
            RunAction(() => m_commands.coiQolSetSelectedStorageMode("empty"));
        }

        if (!m_compactHudMode)
        {
            GUILayout.Space(6f);
            GUILayout.Label($"Selected Product: {m_selectedProduct}", m_subtitleStyle);
            if (GUILayout.Button("Open F8 Storage Tab", m_secondaryButtonStyle, GUILayout.Height(28f)))
            {
                m_isVisible = true;
                m_activeTab = 3;
            }
        }

        GUILayout.EndArea();
    }

    private void DrawMineTowerHudWidget()
    {
        if (!m_showTowerHud || !m_commands.HasSelectedMineTower())
        {
            m_isTowerHudExpanded = false;
            m_isDraggingTowerHud = false;
            return;
        }

        EnsureTowerHudPositionLoaded();

        var currentText = m_commands.GetSelectedMineTowerHudText();
        var anchorRect = m_towerHudAnchorRect;
        var mainButtonRect = new Rect(anchorRect.x, anchorRect.y, anchorRect.width - 28f, anchorRect.height);
        var gripRect = new Rect(anchorRect.xMax - 24f, anchorRect.y, 24f, anchorRect.height);
        var panelRect = new Rect(anchorRect.x, anchorRect.yMax + 8f, 236f, m_compactHudMode ? 188f : 240f);
        HandleTowerHudOutsideClick(mainButtonRect, gripRect, panelRect);
        HandleTowerHudDragging(gripRect, panelRect);

        var buttonText = m_showShortLabels ? currentText.Replace("Tower ", "T ") : currentText;
        if (GUI.Button(mainButtonRect, buttonText, m_storageHudButtonStyle))
        {
            m_isTowerHudExpanded = !m_isTowerHudExpanded;
        }

        GUI.Box(gripRect, ":::", m_storageHudGripStyle);

        if (!m_isTowerHudExpanded)
        {
            return;
        }

        GUILayout.BeginArea(panelRect, m_storageHudPanelStyle);
        GUILayout.Label("Selected Mine Tower", m_storageHudTitleStyle);
        GUILayout.Label(m_commands.GetSelectedMineTowerText(), m_subtitleStyle);
        GUILayout.Label(m_commands.GetSelectedMineTowerScopeText(), m_subtitleStyle);
        GUILayout.Space(8f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(m_commands.IsMineTowerPinned() ? "Unpin" : "Pin", m_secondaryButtonStyle, GUILayout.Height(28f)))
        {
            RunAction(() => m_commands.coiQolPinSelectedMineTower(!m_commands.IsMineTowerPinned()));
        }

        if (GUILayout.Button("Mine", m_primaryButtonStyle, GUILayout.Height(28f)))
        {
            RunAction(() => m_commands.coiQolInstantMineSelected());
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(4f);

        if (GUILayout.Button(m_showShortLabels ? "Dump" : "Instant Dump", m_primaryButtonStyle, GUILayout.Height(30f)))
        {
            RunTerrainProductAction(() => m_commands.coiQolInstantDumpSelected(m_selectedTerrainProduct));
        }

        if (GUILayout.Button(m_showShortLabels ? "Terrain" : "Change Terrain", m_secondaryButtonStyle, GUILayout.Height(30f)))
        {
            RunTerrainProductAction(() => m_commands.coiQolChangeTerrainSelected(m_selectedTerrainProduct));
        }

        if (GUILayout.Button(m_showShortLabels ? "Trees" : "Add Trees", m_secondaryButtonStyle, GUILayout.Height(30f)))
        {
            RunAction(() => m_commands.coiQolAddTreesSelected());
        }

        if (!m_compactHudMode)
        {
            GUILayout.Space(6f);
            GUILayout.Label($"Terrain Product: {m_selectedTerrainProduct}", m_subtitleStyle);
            if (GUILayout.Button("Open F8 Terrain Tab", m_secondaryButtonStyle, GUILayout.Height(28f)))
            {
                m_isVisible = true;
                m_activeTab = 2;
            }
        }

        GUILayout.EndArea();
    }

    private void DrawSpeedHudWidget()
    {
        if (!m_showSpeedHud)
        {
            m_isSpeedHudExpanded = false;
            m_isDraggingSpeedHud = false;
            return;
        }

        EnsureSpeedHudPositionLoaded();

        var anchorRect = m_speedHudAnchorRect;
        var mainButtonRect = new Rect(anchorRect.x, anchorRect.y, anchorRect.width - 28f, anchorRect.height);
        var gripRect = new Rect(anchorRect.xMax - 24f, anchorRect.y, 24f, anchorRect.height);
        var panelRect = new Rect(anchorRect.x, anchorRect.yMax + 8f, m_compactHudMode ? 256f : 318f, m_compactHudMode ? 88f : 126f);
        HandleSpeedHudOutsideClick(mainButtonRect, gripRect, panelRect);
        HandleSpeedHudDragging(gripRect, panelRect);

        if (GUI.Button(mainButtonRect, $"Speed {m_commands.GetGameSpeedText()}", m_storageHudButtonStyle))
        {
            m_isSpeedHudExpanded = !m_isSpeedHudExpanded;
        }

        GUI.Box(gripRect, ":::", m_storageHudGripStyle);

        if (!m_isSpeedHudExpanded)
        {
            return;
        }

        GUILayout.BeginArea(panelRect, m_storageHudPanelStyle);
        GUILayout.Label("Game Speed", m_storageHudTitleStyle);
        GUILayout.Label("Extra speed buttons outside the F8 window.", m_subtitleStyle);
        GUILayout.Space(8f);
        DrawWidgetSpeedButtons(1, 2, 3, 5, 10, 15);
        if (!m_compactHudMode)
        {
            GUILayout.Space(8f);
            DrawWidgetSpeedButtons(20);
        }
        GUILayout.EndArea();
    }

    private void HandleStorageHudOutsideClick(Rect mainButtonRect, Rect gripRect, Rect panelRect)
    {
        if (!m_isStorageHudExpanded || m_isDraggingStorageHud)
        {
            return;
        }

        var currentEvent = Event.current;
        if (currentEvent == null || currentEvent.type != EventType.MouseDown || currentEvent.button != 0)
        {
            return;
        }

        var clickedInsideWidget = mainButtonRect.Contains(currentEvent.mousePosition)
            || gripRect.Contains(currentEvent.mousePosition)
            || panelRect.Contains(currentEvent.mousePosition);

        if (!clickedInsideWidget)
        {
            m_isStorageHudExpanded = false;
        }
    }

    private void HandleTowerHudOutsideClick(Rect mainButtonRect, Rect gripRect, Rect panelRect)
    {
        if (!m_isTowerHudExpanded || m_isDraggingTowerHud)
        {
            return;
        }

        var currentEvent = Event.current;
        if (currentEvent == null || currentEvent.type != EventType.MouseDown || currentEvent.button != 0)
        {
            return;
        }

        var clickedInsideWidget = mainButtonRect.Contains(currentEvent.mousePosition)
            || gripRect.Contains(currentEvent.mousePosition)
            || panelRect.Contains(currentEvent.mousePosition);

        if (!clickedInsideWidget)
        {
            m_isTowerHudExpanded = false;
        }
    }

    private void HandleSpeedHudOutsideClick(Rect mainButtonRect, Rect gripRect, Rect panelRect)
    {
        if (!m_isSpeedHudExpanded || m_isDraggingSpeedHud)
        {
            return;
        }

        var currentEvent = Event.current;
        if (currentEvent == null || currentEvent.type != EventType.MouseDown || currentEvent.button != 0)
        {
            return;
        }

        var clickedInsideWidget = mainButtonRect.Contains(currentEvent.mousePosition)
            || gripRect.Contains(currentEvent.mousePosition)
            || panelRect.Contains(currentEvent.mousePosition);

        if (!clickedInsideWidget)
        {
            m_isSpeedHudExpanded = false;
        }
    }

    private void EnsureStorageHudPositionLoaded()
    {
        if (m_storageHudPositionLoaded)
        {
            return;
        }

        m_storageHudAnchorRect.x = PlayerPrefs.GetFloat(StorageHudPosXKey, m_storageHudAnchorRect.x);
        m_storageHudAnchorRect.y = PlayerPrefs.GetFloat(StorageHudPosYKey, m_storageHudAnchorRect.y);
        m_storageHudAnchorRect = ClampContextHudAnchor(m_storageHudAnchorRect, 250f);
        m_storageHudPositionLoaded = true;
    }

    private void EnsureTowerHudPositionLoaded()
    {
        if (m_towerHudPositionLoaded)
        {
            return;
        }

        m_towerHudAnchorRect.x = PlayerPrefs.GetFloat(TowerHudPosXKey, m_towerHudAnchorRect.x);
        m_towerHudAnchorRect.y = PlayerPrefs.GetFloat(TowerHudPosYKey, m_towerHudAnchorRect.y);
        m_towerHudAnchorRect = ClampContextHudAnchor(m_towerHudAnchorRect, 260f);
        m_towerHudPositionLoaded = true;
    }

    private void EnsureSpeedHudPositionLoaded()
    {
        if (m_speedHudPositionLoaded)
        {
            return;
        }

        m_speedHudAnchorRect.x = PlayerPrefs.GetFloat(SpeedHudPosXKey, m_speedHudAnchorRect.x);
        m_speedHudAnchorRect.y = PlayerPrefs.GetFloat(SpeedHudPosYKey, m_speedHudAnchorRect.y);
        m_speedHudAnchorRect = ClampSpeedHudAnchor(m_speedHudAnchorRect);
        m_speedHudPositionLoaded = true;
    }

    private void HandleStorageHudDragging(Rect gripRect, Rect expandedPanelRect)
    {
        var currentEvent = Event.current;
        if (currentEvent == null)
        {
            return;
        }

        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && gripRect.Contains(currentEvent.mousePosition))
        {
            m_isDraggingStorageHud = true;
            m_storageHudDragOffset = currentEvent.mousePosition - new Vector2(m_storageHudAnchorRect.x, m_storageHudAnchorRect.y);
            currentEvent.Use();
            return;
        }

        if (m_isDraggingStorageHud && currentEvent.type == EventType.MouseDrag)
        {
            m_storageHudAnchorRect.position = currentEvent.mousePosition - m_storageHudDragOffset;
            m_storageHudAnchorRect = ClampContextHudAnchor(m_storageHudAnchorRect, 250f);
            currentEvent.Use();
            return;
        }

        if (m_isDraggingStorageHud && (currentEvent.type == EventType.MouseUp || currentEvent.rawType == EventType.MouseUp))
        {
            m_isDraggingStorageHud = false;
            SaveStorageHudPosition();
            currentEvent.Use();
        }
    }

    private void HandleTowerHudDragging(Rect gripRect, Rect expandedPanelRect)
    {
        var currentEvent = Event.current;
        if (currentEvent == null)
        {
            return;
        }

        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && gripRect.Contains(currentEvent.mousePosition))
        {
            m_isDraggingTowerHud = true;
            m_towerHudDragOffset = currentEvent.mousePosition - new Vector2(m_towerHudAnchorRect.x, m_towerHudAnchorRect.y);
            currentEvent.Use();
            return;
        }

        if (m_isDraggingTowerHud && currentEvent.type == EventType.MouseDrag)
        {
            m_towerHudAnchorRect.position = currentEvent.mousePosition - m_towerHudDragOffset;
            m_towerHudAnchorRect = ClampContextHudAnchor(m_towerHudAnchorRect, 260f);
            currentEvent.Use();
            return;
        }

        if (m_isDraggingTowerHud && (currentEvent.type == EventType.MouseUp || currentEvent.rawType == EventType.MouseUp))
        {
            m_isDraggingTowerHud = false;
            SaveTowerHudPosition();
            currentEvent.Use();
        }
    }

    private void HandleSpeedHudDragging(Rect gripRect, Rect expandedPanelRect)
    {
        var currentEvent = Event.current;
        if (currentEvent == null)
        {
            return;
        }

        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && gripRect.Contains(currentEvent.mousePosition))
        {
            m_isDraggingSpeedHud = true;
            m_speedHudDragOffset = currentEvent.mousePosition - new Vector2(m_speedHudAnchorRect.x, m_speedHudAnchorRect.y);
            currentEvent.Use();
            return;
        }

        if (m_isDraggingSpeedHud && currentEvent.type == EventType.MouseDrag)
        {
            m_speedHudAnchorRect.position = currentEvent.mousePosition - m_speedHudDragOffset;
            m_speedHudAnchorRect = ClampSpeedHudAnchor(m_speedHudAnchorRect);
            currentEvent.Use();
            return;
        }

        if (m_isDraggingSpeedHud && (currentEvent.type == EventType.MouseUp || currentEvent.rawType == EventType.MouseUp))
        {
            m_isDraggingSpeedHud = false;
            SaveSpeedHudPosition();
            currentEvent.Use();
        }
    }

    private Rect ClampContextHudAnchor(Rect rect, float panelHeight)
    {
        var maxX = Mathf.Max(12f, Screen.width - rect.width - 12f);
        var maxY = Mathf.Max(12f, Screen.height - rect.height - panelHeight);
        rect.x = Mathf.Clamp(rect.x, 12f, maxX);
        rect.y = Mathf.Clamp(rect.y, 96f, maxY);
        return rect;
    }

    private Rect ClampSpeedHudAnchor(Rect rect)
    {
        var maxX = Mathf.Max(12f, Screen.width - rect.width - 12f);
        var maxY = Mathf.Max(12f, Screen.height - rect.height - 170f);
        rect.x = Mathf.Clamp(rect.x, 12f, maxX);
        rect.y = Mathf.Clamp(rect.y, 12f, maxY);
        return rect;
    }

    private void SaveStorageHudPosition()
    {
        PlayerPrefs.SetFloat(StorageHudPosXKey, m_storageHudAnchorRect.x);
        PlayerPrefs.SetFloat(StorageHudPosYKey, m_storageHudAnchorRect.y);
        PlayerPrefs.Save();
    }

    private void SaveTowerHudPosition()
    {
        PlayerPrefs.SetFloat(TowerHudPosXKey, m_towerHudAnchorRect.x);
        PlayerPrefs.SetFloat(TowerHudPosYKey, m_towerHudAnchorRect.y);
        PlayerPrefs.Save();
    }

    private void SaveSpeedHudPosition()
    {
        PlayerPrefs.SetFloat(SpeedHudPosXKey, m_speedHudAnchorRect.x);
        PlayerPrefs.SetFloat(SpeedHudPosYKey, m_speedHudAnchorRect.y);
        PlayerPrefs.Save();
    }

    private void ResetStorageHudPosition()
    {
        m_storageHudAnchorRect = new Rect(18f, 190f, 198f, 42f);
        SaveStorageHudPosition();
    }

    private void ResetTowerHudPosition()
    {
        m_towerHudAnchorRect = new Rect(18f, 292f, 198f, 42f);
        SaveTowerHudPosition();
    }

    private void ResetSpeedHudPosition()
    {
        m_speedHudAnchorRect = new Rect(170f, 18f, 360f, 42f);
        SaveSpeedHudPosition();
    }

    private void ResetAllHudPositions()
    {
        ResetStorageHudPosition();
        ResetTowerHudPosition();
        ResetSpeedHudPosition();
    }

    private void DrawWidgetSpeedButtons(params int[] speeds)
    {
        GUILayout.BeginHorizontal();
        foreach (var speed in speeds)
        {
            if (GUILayout.Button($"x{speed}", speed >= 5 ? m_primaryButtonStyle : m_secondaryButtonStyle, GUILayout.Height(28f)))
            {
                RunAction(() => m_commands.coiQolSetGameSpeed(speed));
            }
        }
        GUILayout.EndHorizontal();
    }

    private void DrawBackdrop()
    {
        var previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.32f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void DrawHeader()
    {
        BeginCard("QoL Captain of Industry", "F8 toggles the window. The goal here is quick, dependable sandbox controls for Update 4.");
        GUILayout.BeginHorizontal();
        DrawStatChip("Unity", m_commands.GetUnityAmountText());
        DrawStatChip("Free Workers", m_commands.GetWorkersAmountText());
        DrawStatChip("Vehicles", m_commands.GetVehicleCountText());
        DrawStatChip("Vehicle Limit", m_commands.GetVehicleLimitText());
        DrawStatChip("Speed", m_commands.GetGameSpeedText());
        GUILayout.EndHorizontal();
        EndCard();
    }

    private void DrawTabs()
    {
        GUILayout.BeginHorizontal();
        for (var i = 0; i < m_tabs.Length; i++)
        {
            var style = i == m_activeTab ? m_tabSelectedStyle : m_tabStyle;
            if (GUILayout.Button(m_tabs[i], style, GUILayout.Height(34f)))
            {
                m_activeTab = i;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);
    }

    private void BeginCard(string title, string description)
    {
        GUILayout.BeginVertical(m_cardStyle);
        GUILayout.Label(title, m_titleStyle);
        if (!string.IsNullOrWhiteSpace(description))
        {
            GUILayout.Label(description, m_subtitleStyle);
            GUILayout.Space(8f);
        }
    }

    private void EndCard()
    {
        GUILayout.EndVertical();
    }

    private void DrawStatChip(string label, string value)
    {
        GUILayout.Label($"{label}\n{value}", m_statStyle, GUILayout.Height(54f), GUILayout.ExpandWidth(true));
    }

    private void DrawButtonGrid(params (string label, Action action)[] buttons)
    {
        const int columns = 2;
        for (var i = 0; i < buttons.Length; i += columns)
        {
            GUILayout.BeginHorizontal();
            for (var j = 0; j < columns; j++)
            {
                var index = i + j;
                if (index < buttons.Length)
                {
                    var button = buttons[index];
                    DrawActionButton(button.label, button.action, true);
                }
                else
                {
                    GUILayout.Space(4f);
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    private void DrawSelectableList(string[] values, ref string selectedValue, string emptyText)
    {
        if (values.Length == 0)
        {
            GUILayout.Label(emptyText, m_subtitleStyle);
            return;
        }

        foreach (var value in values)
        {
            var isSelected = value == selectedValue;
            if (GUILayout.Button(isSelected ? $"> {value}" : value, isSelected ? m_primaryButtonStyle : m_secondaryButtonStyle, GUILayout.Height(28f)))
            {
                selectedValue = value;
            }
        }
    }

    private void DrawSelectableWeatherList(string[] values)
    {
        if (values.Length == 0)
        {
            GUILayout.Label("No matching weather ids.", m_subtitleStyle);
            return;
        }

        foreach (var weatherId in values)
        {
            var isSelected = weatherId == m_selectedWeather;
            if (GUILayout.Button(isSelected ? $"> {weatherId}" : weatherId, isSelected ? m_primaryButtonStyle : m_secondaryButtonStyle, GUILayout.Height(28f)))
            {
                m_selectedWeather = weatherId;
                RunAction(() => m_commands.coiQolSetWeather(weatherId));
            }
        }
    }

    private static Texture2D MakeTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
