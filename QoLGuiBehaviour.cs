using System;
using UnityEngine;

namespace QoLCaptainOfIndustry;

public sealed class QoLGuiBehaviour : MonoBehaviour
{
    private const int WindowId = 1594320;

    private readonly string[] m_tabs = { "General", "Vehicles", "Terrain", "Storage", "Weather" };
    private QoLCaptainOfIndustryCommands m_commands;
    private Rect m_windowRect = new Rect(120f, 80f, 760f, 760f);
    private Vector2 m_scrollPosition;
    private bool m_isVisible;
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

    private GUIStyle m_titleStyle;
    private GUIStyle m_subtitleStyle;
    private GUIStyle m_cardStyle;
    private GUIStyle m_primaryButtonStyle;
    private GUIStyle m_secondaryButtonStyle;
    private GUIStyle m_statusStyle;
    private GUIStyle m_statStyle;
    private GUIStyle m_tabStyle;
    private GUIStyle m_tabSelectedStyle;

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
        if (!m_isVisible || m_commands == null)
        {
            return;
        }

        EnsureStyles();
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
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.Label(m_commands.GetSelectedStorageScopeText(), m_subtitleStyle);
        GUILayout.Space(10f);
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
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.Label(m_commands.GetSelectedMineTowerScopeText(), m_subtitleStyle);
        GUILayout.Space(10f);
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
        m_cardStyle.normal.background = MakeTexture(new Color(0.12f, 0.15f, 0.19f, 0.96f));
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
        m_statusStyle.normal.background = MakeTexture(new Color(0.10f, 0.18f, 0.24f, 0.96f));
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
