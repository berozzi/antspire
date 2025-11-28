using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class AdvancedDebugConsole : MonoBehaviour
{
    public static event Action<bool> OnConsoleStateChanged; // true = otwarta, false = zamknięta

    [Header("Console Settings")]
    public bool showConsole = false;
    public KeyCode toggleModifier = KeyCode.LeftShift; // Modyfikator do otwierania konsoli
    public KeyCode toggleKey = KeyCode.Tab;            // Klawisz otwierania/zamykania konsoli
    public KeyCode closeKey = KeyCode.Escape;     // Klawisz zamykania konsoli
    public KeyCode executeKey = KeyCode.Backspace;   // Klawisz wykonywania komend
    public int maxMessageHistory = 50;
    private bool isPausedInDebug = false;
    public bool opencursorWhenOpen = true;
    
    private float previousTimeScale = 1f;
    private bool wasCursorVisible;
    private CursorLockMode previousCursorLockState;
    private bool wasPlayerInputEnabled = true;

    [Header("Ant Settings")]
    public GameObject antPrefab;
    public Transform defaultSpawnPoint;

    private string inputBuffer = "";
    private Vector2 scrollPosition;
    private List<ConsoleMessage> messageHistory = new List<ConsoleMessage>();
    private Dictionary<string, CommandHandler> commands = new Dictionary<string, CommandHandler>();

    public enum MessageType
    {
        Info,
        Warning,
        Error,
        Success,
        System
    }

    [System.Serializable]
    public class ConsoleMessage
    {
        public string message;
        public MessageType type;
        public string timestamp;

        public ConsoleMessage(string msg, MessageType msgType)
        {
            message = msg;
            type = msgType;
            timestamp = DateTime.Now.ToString("HH:mm:ss");
        }

        public string GetFormattedMessage()
        {
            string color = type switch
            {
                MessageType.Info => "white",
                MessageType.Warning => "yellow",
                MessageType.Error => "red",
                MessageType.Success => "green",
                MessageType.System => "cyan",
                _ => "white"
            };

            return $"[{timestamp}] <color={color}>{message}</color>";
        }
    }

    public delegate void CommandHandler(string[] args);

    void Start()
    {
        RegisterCommands();
        LogSystem($"Debug Console Ready - Press {toggleKey} to open, {closeKey} to close");
    }

    #region Game State Management
    private void SetConsoleState(bool consoleActive)
    {
        showConsole = consoleActive;
        OnConsoleStateChanged?.Invoke(showConsole);
        if (consoleActive == true)
        {
            // ZAPISZ OBECNY STAN PRZED OTWARCIEM KONSOLI
            wasCursorVisible = Cursor.visible;
            previousCursorLockState = Cursor.lockState;

            // WŁĄCZ KURSOR I ODŁUŻ GO
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // WYŁĄCZ INPUT GRACZA (dostosuj do swojego systemu)
            DisablePlayerInput();
        } 
        else
        {
            // PRZYWRÓĆ POPRZEDNI STAN PO ZAMKNIĘCIU KONSOLI
            Cursor.visible = wasCursorVisible;
            Cursor.lockState = previousCursorLockState;
            Debug.Log("Cursor state restored");
            // WŁĄCZ INPUT GRACZA
            EnablePlayerInput();
        }

        Debug.Log($"After SetConsoleState: showConsole = {showConsole}");
    }

    private void DisablePlayerInput()
    {
        isPausedInDebug = true;
        Time.timeScale = 0f;
        Debug.Log("Player input disabled - console active");
    }

    private void EnablePlayerInput()
    {
        isPausedInDebug = false;
        Time.timeScale = 1f;
        Debug.Log("Player input enabled - console closed");
    }
    #endregion
    


    void Update()
    {
        // DEBUG - sprawdź czy klawisz jest w ogóle wykrywany
        if (Input.GetKeyDown(toggleKey) && IsShiftPressed())
        {
            Debug.Log($"Shift+Tab pressed! Current showConsole: {showConsole}");
        }

        // ESC zawsze zamyka gdy konsola jest otwarta
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (showConsole)
            {
                SetConsoleState(false);
                Debug.Log("Console closed with ESC");
            }
            return;
        }

        // Toggle konsoli - Shift+Tab otwiera/zamyka
        if (Input.GetKeyDown(toggleKey) && IsShiftPressed())
        {
            ToggleConsole();
        }

        // Wykonywanie komendy - tylko gdy konsola jest otwarta
        if (showConsole && Input.GetKeyDown(executeKey) && !string.IsNullOrEmpty(inputBuffer))
        {
            ExecuteCommand(inputBuffer);
            inputBuffer = "";
        }
    }

    // Sprawdza czy Shift jest wciśnięty (lewy lub prawy)
    private bool IsShiftPressed()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private void ToggleConsole()
    {
        
        SetConsoleState(!showConsole);
        Debug.Log($"Console toggled: {showConsole}");

        if (showConsole)
            inputBuffer = "";
    }


    private float lastGuiUpdate;
    private float guiUpdateInterval = 0.1f; // 10 FPS dla GUI

    void OnGUI()
    {
        {
            if (!showConsole) return;

            // OBSŁUGA KLAWISZY - NA SAMYM POCZĄTKU OnGUI
            if (Event.current.isKey && Event.current.type == EventType.KeyDown)
            {
                // ESC ZAMYKA KONSOLĘ
                if (Event.current.keyCode == KeyCode.Escape)
                {
                    showConsole = false;
                    Event.current.Use();
                    return;
                }

                // RETURN/ENTER WYKONUJE KOMENDĘ
                if ((Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                    && !string.IsNullOrEmpty(inputBuffer))
                {
                    ExecuteCommand(inputBuffer);
                    inputBuffer = "";
                    Event.current.Use();
                    return;
                }
            }
            DrawConsole();
        }
    }
    void OnDestroy()
    {
        // Wyczyść kolekcje
        messageHistory?.Clear();
        commands?.Clear();

        // Wymuś garbage collection
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();

        Debug.Log("Debug Console cleaned up");
    }
    #region Command System
    private void RegisterCommands()
    {
        // Ant commands
        RegisterCommand("summon", SummonAnt);
        RegisterCommand("summon_ant", SummonAnt);
        RegisterCommand("ants_clear", ClearAnts);

        // System commands
        RegisterCommand("help", ShowHelp);
        RegisterCommand("clear", ClearConsole);
        RegisterCommand("echo", Echo);
        RegisterCommand("keys", ShowKeys);
       

        // GameObject commands
        RegisterCommand("destroy", DestroyObject);
        RegisterCommand("list_objects", ListObjects);

        // Time commands
        RegisterCommand("timescale", SetTimeScale);
        RegisterCommand("pause", PauseGame);

        
    }

    public void RegisterCommand(string command, CommandHandler handler)
    {
        if (commands.ContainsKey(command))
        {
            commands[command] = handler;
            Debug.Log($"Command '{command}' updated");
        }
        else
        {
            commands.Add(command, handler);
            Debug.Log($"Command '{command}' registered");
        }
        
    }

    private void ExecuteCommand(string input)
    {
        if (string.IsNullOrEmpty(input)) return;

        LogInput(input);

        string[] parts = input.Split(' ');
        string command = parts[0].ToLower();
        string[] args = parts.Skip(1).ToArray();

        if (commands.ContainsKey(command))
        {
            try
            {
                commands[command].Invoke(args);
            }
            catch (System.Exception e)
            {
                LogError($"Error executing command '{command}': {e.Message}");
            }
        }
        else
        {
            LogError($"Unknown command: '{command}'. Type 'help' for available commands.");
        }
    }
    #endregion

    #region Command Handlers
    private void ShowKeys(string[] args)
    {
        LogSystem("=== CURRENT KEY BINDINGS ===");
        LogSystem($"Toggle Console: {toggleKey}");
        LogSystem($"Execute Command: {executeKey}");
        LogSystem("Use 'set_key open/execute KeyCode' to change bindings");
    }

    
    private void SummonAnt(string[] args)
    {
        if (antPrefab == null)
        {
            LogError("Ant prefab is not assigned in the inspector!");
            return;
        }

        Vector3 spawnPosition = defaultSpawnPoint != null ? defaultSpawnPoint.position : Vector3.zero;

        if (args.Length >= 3)
        {
            if (float.TryParse(args[0], out float x) && float.TryParse(args[1], out float y) && float.TryParse(args[2], out float z))
            {
                spawnPosition = new Vector3(x, 0, z);
            }
        }

        GameObject newAnt = Instantiate(antPrefab, spawnPosition, Quaternion.identity);
        newAnt.name = $"Ant_{DateTime.Now.Ticks}";

        LogSuccess($"Summoned ant at: {spawnPosition}");
    }

    private void ClearAnts(string[] args)
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int antCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Ant_"))
            {
                Destroy(obj);
                antCount++;
            }
        }

        LogSuccess($"Cleared {antCount} ants");
    }

    private void ShowHelp(string[] args)
    {
        LogSystem("=== AVAILABLE COMMANDS ===");
        LogSystem("help - Show this help");
        LogSystem("keys - Show current key bindings");
        LogSystem("set_key - Change key bindings");
        LogSystem("clear - Clear console");
        LogSystem("test - Test command");
        LogSystem("summon [x] [y] [z] - Spawn ant");
        LogSystem("ants_clear - Clear all ants");
        LogSystem("echo <text> - Echo text");
    }

    private void ClearConsole(string[] args)
    {
        messageHistory.Clear();
        LogSystem("Console cleared");
    }

    private void Echo(string[] args)
    {
        if (args.Length > 0)
        {
            LogInfo("ECHO: " + string.Join(" ", args));
        }
    }

    //private void ToggleFPS(string[] args) { LogSystem("FPS toggled"); }
    private void DestroyObject(string[] args) { LogSystem("Destroy command"); }
    private void ListObjects(string[] args) { LogSystem("List objects command"); }
    private void SetTimeScale(string[] args) { LogSystem("Timescale command"); }
    private void PauseGame(string[] args) { LogSystem("Pause command"); }
    #endregion

    #region Console UI
    private void DrawConsole()
    {
        float consoleHeight = Screen.height * 0.4f;

        // Tło konsoli
        GUI.Box(new Rect(0, 0, Screen.width, consoleHeight), "");

        // Historia wiadomości
        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, consoleHeight - 50));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(consoleHeight - 60));

        foreach (ConsoleMessage msg in messageHistory)
        {
            GUILayout.Label(msg.GetFormattedMessage());
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        // Pole inputu
        GUILayout.BeginArea(new Rect(10, consoleHeight - 30, Screen.width - 20, 30));
        GUILayout.BeginHorizontal();

        GUI.SetNextControlName("ConsoleInput");
        inputBuffer = GUILayout.TextField(inputBuffer, GUILayout.ExpandWidth(true));

        // Przycisk wysyłania
        if (GUILayout.Button("SEND", GUILayout.Width(60)))
        {
            if (!string.IsNullOrEmpty(inputBuffer))
            {
                ExecuteCommand(inputBuffer);
                inputBuffer = "";
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // AUTO-FOCUS NA INPUT
        if (GUI.GetNameOfFocusedControl() != "ConsoleInput")
        {
            GUI.FocusControl("ConsoleInput");
        }

        // OBSŁUGA ENTER W GUI
        if (Event.current.type == EventType.KeyDown &&
            Event.current.keyCode == executeKey &&
            !string.IsNullOrEmpty(inputBuffer) &&
            GUI.GetNameOfFocusedControl() == "ConsoleInput")
        {
            ExecuteCommand(inputBuffer);
            inputBuffer = "";
            Event.current.Use();
        }

        // OBSŁUGA ESC W GUI - dodajemy tu również
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == closeKey)
        {
            showConsole = false;
            Event.current.Use();
        }
    }
    #endregion

    #region Logging Methods
    public void LogInfo(string message)
    {
        AddMessage(new ConsoleMessage(message, MessageType.Info));
    }

    public void LogWarning(string message)
    {
        AddMessage(new ConsoleMessage(message, MessageType.Warning));
    }

    public void LogError(string message)
    {
        AddMessage(new ConsoleMessage(message, MessageType.Error));
    }

    public void LogSuccess(string message)
    {
        AddMessage(new ConsoleMessage(message, MessageType.Success));
    }

    public void LogSystem(string message)
    {
        AddMessage(new ConsoleMessage(message, MessageType.System));
    }

    private void LogInput(string message)
    {
        AddMessage(new ConsoleMessage($"> {message}", MessageType.Info));
    }

    private void AddMessage(ConsoleMessage message)
    {
        messageHistory.Add(message);
        if (messageHistory.Count > maxMessageHistory)
        {
            messageHistory.RemoveAt(0);
        }
        scrollPosition.y = Mathf.Infinity;
    }
    #endregion
}