using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 어트리뷰트 기반 고급 치트 시스템
/// </summary>
public class AdvancedCheatSystem : MonoBehaviour
{
    public static AdvancedCheatSystem Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject cheatPanel;
    [SerializeField] private TMP_InputField commandInput;
    [SerializeField] private Button executeButton;
    [SerializeField] private ScrollRect outputScrollRect;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private Transform suggestionParent;
    [SerializeField] private GameObject suggestionItemPrefab;
    
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote; // ` 키
    [SerializeField] private int maxOutputLines = 100;
    [SerializeField] private Transform initEnemyPoint;
    
    [Header("Current Status")]
    [SerializeField] private bool isConsoleOpen = false;
    
    [Header("Suggestion Settings")]
    [SerializeField] private int maxSuggestions = 5;
    [SerializeField] private bool showDetailedSuggestions = true;
    
    // 제안 관련 변수들
    private int selectedSuggestionIndex = -1;
    private List<string> currentSuggestions = new List<string>();
    private List<CommandInfo> currentSuggestionCommands = new List<CommandInfo>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetupUI();
        if (cheatPanel != null)
            cheatPanel.SetActive(false);
        
        // Unity 로그를 UI로 리다이렉트
        Application.logMessageReceived += OnLogMessage;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessage;
    }

    void Update()
    {
        // 콘솔 토글
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleCheatPanel();
        }

        // 콘솔이 열려있을 때만 키 입력 처리
        if (isConsoleOpen && commandInput != null)
        {
            HandleInputKeys();
        }
    }

    private void SetupUI()
    {
        if (executeButton != null)
            executeButton.onClick.AddListener(ExecuteCommand);

        if (commandInput != null)
        {
            commandInput.onSubmit.AddListener(OnCommandSubmit);
            commandInput.onValueChanged.AddListener(OnInputChanged);
        }
    }

    private void HandleInputKeys()
    {
        if (!commandInput.isFocused) return;

        // 제안 목록이 있을 때 키보드 탐색
        if (currentSuggestions.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateSuggestions(-1);
                return;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateSuggestions(1);
                return;
            }
            else if (Input.GetKeyDown(KeyCode.Return) && selectedSuggestionIndex >= 0)
            {
                ApplySuggestion(selectedSuggestionIndex);
                return;
            }
        }

        // 히스토리 탐색 (제안이 없을 때만)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            string prevCommand = CommandProcessor.Instance?.GetPreviousCommand();
            if (!string.IsNullOrEmpty(prevCommand))
            {
                commandInput.text = prevCommand;
                commandInput.caretPosition = commandInput.text.Length;
                UpdateSuggestions();
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            string nextCommand = CommandProcessor.Instance?.GetNextCommand();
            commandInput.text = nextCommand;
            commandInput.caretPosition = commandInput.text.Length;
            UpdateSuggestions();
        }
        // 자동 완성
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (currentSuggestions.Count > 0 && selectedSuggestionIndex >= 0)
            {
                ApplySuggestion(selectedSuggestionIndex);
            }
            else
            {
                AutoComplete();
            }
        }
        // ESC로 콘솔 닫기
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCheatPanel();
        }
    }

    private void OnCommandSubmit(string command)
    {
        if (!string.IsNullOrWhiteSpace(command))
        {
            ExecuteCommand();
        }
    }

    private void OnInputChanged(string text)
    {
        UpdateSuggestions();
    }

    private void UpdateSuggestions()
    {
        if (CommandProcessor.Instance == null || suggestionParent == null) return;

        // 기존 제안 삭제
        foreach (Transform child in suggestionParent)
        {
            Destroy(child.gameObject);
        }

        // 제안 데이터 초기화
        currentSuggestions.Clear();
        currentSuggestionCommands.Clear();
        selectedSuggestionIndex = -1;

        string input = commandInput.text.Trim();
        if (string.IsNullOrEmpty(input))
        {
            // 입력이 없으면 제안 패널 숨기기
            if (suggestionParent != null && suggestionParent.parent != null)
            {
                suggestionParent.parent.gameObject.SetActive(false);
            }
            return;
        }

        // 명령어 제안 가져오기
        var suggestions = CommandProcessor.Instance.GetCommandSuggestions(input);
        if (suggestions.Count == 0) 
        {
            // 제안이 없으면 패널 숨기기
            if (suggestionParent != null && suggestionParent.parent != null)
            {
                suggestionParent.parent.gameObject.SetActive(false);
            }
            return;
        }

        // 제안이 있으면 패널 보이기
        if (suggestionParent != null && suggestionParent.parent != null)
        {
            suggestionParent.parent.gameObject.SetActive(true);
        }

        // 상세 명령어 정보 가져오기 (CommandProcessor에서)
        var detailedSuggestions = GetDetailedSuggestions(input);

        // 제안 UI 생성
        int count = Mathf.Min(suggestions.Count, maxSuggestions);
        for (int i = 0; i < count; i++)
        {
            string suggestion = suggestions[i];
            currentSuggestions.Add(suggestion);
            
            if (suggestionItemPrefab != null)
            {
                GameObject item = Instantiate(suggestionItemPrefab, suggestionParent);
                
                // 텍스트 설정 (입력된 부분 하이라이트)
                TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string highlightedText = HighlightMatchingText(suggestion, input);
                    
                    if (showDetailedSuggestions && detailedSuggestions.ContainsKey(suggestion))
                    {
                        var cmdInfo = detailedSuggestions[suggestion];
                        text.text = $"{highlightedText} <color=#888888>- {cmdInfo.description}</color>";
                    }
                    else
                    {
                        text.text = highlightedText;
                    }
                }

                // 버튼 이벤트 설정
                Button button = item.GetComponent<Button>();
                if (button != null)
                {
                    int index = i; // 클로저를 위한 지역 변수
                    button.onClick.AddListener(() => ApplySuggestion(index));
                }

                // 선택 표시를 위한 이미지 컴포넌트 가져오기
                Image bgImage = item.GetComponent<Image>();
                if (bgImage != null)
                {
                    // 기본 상태로 설정
                    bgImage.color = new Color(0, 0, 0, 0.1f);
                }
            }
        }
    }

    /// <summary>
    /// 상세한 명령어 정보 가져오기
    /// </summary>
    private Dictionary<string, CommandInfo> GetDetailedSuggestions(string input)
    {
        if (CommandProcessor.Instance != null)
        {
            return CommandProcessor.Instance.GetDetailedCommandSuggestions(input);
        }
        
        return new Dictionary<string, CommandInfo>();
    }

    /// <summary>
    /// 제안 목록에서 키보드로 탐색
    /// </summary>
    private void NavigateSuggestions(int direction)
    {
        if (currentSuggestions.Count == 0) return;

        // 이전 선택 해제
        if (selectedSuggestionIndex >= 0 && selectedSuggestionIndex < suggestionParent.childCount)
        {
            Image prevImage = suggestionParent.GetChild(selectedSuggestionIndex).GetComponent<Image>();
            if (prevImage != null)
                prevImage.color = new Color(0, 0, 0, 0.1f);
        }

        // 새 인덱스 계산
        selectedSuggestionIndex += direction;
        if (selectedSuggestionIndex < 0)
            selectedSuggestionIndex = currentSuggestions.Count - 1;
        else if (selectedSuggestionIndex >= currentSuggestions.Count)
            selectedSuggestionIndex = 0;

        // 새 선택 표시
        if (selectedSuggestionIndex >= 0 && selectedSuggestionIndex < suggestionParent.childCount)
        {
            Image newImage = suggestionParent.GetChild(selectedSuggestionIndex).GetComponent<Image>();
            if (newImage != null)
                newImage.color = new Color(0.3f, 0.5f, 1f, 0.5f);
        }
    }

    /// <summary>
    /// 선택된 제안 적용
    /// </summary>
    private void ApplySuggestion(int index)
    {
        if (index < 0 || index >= currentSuggestions.Count) return;

        string suggestion = currentSuggestions[index];
        commandInput.text = suggestion + " ";
        commandInput.caretPosition = commandInput.text.Length;
        commandInput.ActivateInputField();
        
        // 제안 목록 업데이트
        UpdateSuggestions();
    }

    private void AutoComplete()
    {
        if (CommandProcessor.Instance == null) return;

        string input = commandInput.text.Trim();
        string[] parts = input.Split(' ');
        
        if (parts.Length == 1)
        {
            // 명령어 자동완성
            var suggestions = CommandProcessor.Instance.GetCommandSuggestions(input);
            
            if (suggestions.Count == 1)
            {
                string completedCommand = suggestions[0];
                var cmdInfo = CommandProcessor.Instance.GetCommandInfo(completedCommand);
                
                if (cmdInfo != null && cmdInfo.parameters.Length > 0)
                {
                    // 매개변수가 있는 경우 힌트 표시
                    commandInput.text = completedCommand + " ";
                    ShowParameterHint(cmdInfo);
                }
                else
                {
                    // 매개변수가 없는 경우 바로 완성
                    commandInput.text = completedCommand + " ";
                }
                commandInput.caretPosition = commandInput.text.Length;
            }
            else if (suggestions.Count > 1)
            {
                // 공통 접두사 찾기
                string commonPrefix = GetCommonPrefix(suggestions);
                if (commonPrefix.Length > input.Length)
                {
                    commandInput.text = commonPrefix;
                    commandInput.caretPosition = commonPrefix.Length;
                }
            }
        }
        else
        {
            // 매개변수 자동완성 (향후 확장 가능)
            string commandName = parts[0];
            var cmdInfo = CommandProcessor.Instance.GetCommandInfo(commandName);
            if (cmdInfo != null)
            {
                ShowParameterHint(cmdInfo);
            }
        }
    }

    /// <summary>
    /// 매개변수 힌트 표시
    /// </summary>
    private void ShowParameterHint(CommandInfo cmdInfo)
    {
        if (cmdInfo == null || cmdInfo.parameters.Length == 0) return;
        
        string hint = cmdInfo.GetUsage();
        AddToOutput($"<color=cyan>사용법: {hint}</color>");
        
        // 매개변수 상세 정보
        foreach (var param in cmdInfo.parameters)
        {
            string paramType = param.ParameterType.Name;
            string defaultValue = param.HasDefaultValue ? $" (기본값: {param.DefaultValue})" : "";
            AddToOutput($"<color=yellow>  {param.Name} ({paramType}){defaultValue}</color>");
        }
    }

    /// <summary>
    /// 입력된 텍스트와 일치하는 부분을 하이라이트
    /// </summary>
    private string HighlightMatchingText(string fullText, string inputText)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(fullText))
            return $"<color=white>{fullText}</color>";

        if (fullText.StartsWith(inputText, System.StringComparison.OrdinalIgnoreCase))
        {
            string matchingPart = fullText.Substring(0, inputText.Length);
            string remainingPart = fullText.Substring(inputText.Length);
            return $"<color=#4FC3F7>{matchingPart}</color><color=white>{remainingPart}</color>";
        }

        return $"<color=white>{fullText}</color>";
    }

    private string GetCommonPrefix(List<string> strings)
    {
        if (strings.Count == 0) return "";
        
        string prefix = strings[0];
        for (int i = 1; i < strings.Count; i++)
        {
            while (!strings[i].StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                prefix = prefix.Substring(0, prefix.Length - 1);
                if (prefix == "") return "";
            }
        }
        return prefix;
    }

    private void ExecuteCommand()
    {
        string command = commandInput.text.Trim();
        if (string.IsNullOrEmpty(command)) return;

        // 명령어 출력
        AddToOutput($"> {command}");
        
        // 명령어 실행
        if (CommandProcessor.Instance != null)
        {
            CommandProcessor.Instance.ExecuteCommand(command);
        }
        else
        {
            AddToOutput("<color=red>CommandProcessor가 초기화되지 않았습니다.</color>");
        }

        // 입력 필드 초기화
        commandInput.text = "";
        commandInput.ActivateInputField();
        
        // 제안 초기화
        UpdateSuggestions();
    }

    private void OnLogMessage(string logString, string stackTrace, LogType type)
    {
        string colorTag = type switch
        {
            LogType.Error => "<color=red>",
            LogType.Warning => "<color=yellow>",
            LogType.Log => "<color=white>",
            _ => "<color=white>"
        };

        AddToOutput($"{colorTag}{logString}</color>");
    }

    private void AddToOutput(string message)
    {
        if (outputText == null) return;

        outputText.text += message + "\n";

        // 최대 라인 수 제한
        string[] lines = outputText.text.Split('\n');
        if (lines.Length > maxOutputLines)
        {
            outputText.text = string.Join("\n", lines.Skip(lines.Length - maxOutputLines));
        }

        // 스크롤을 맨 아래로 (안전하게 다음 프레임에서 실행)
        if (outputScrollRect != null)
        {
            StartCoroutine(ScrollToBottomNextFrame());
        }
    }

    /// <summary>
    /// 다음 프레임에서 스크롤을 맨 아래로 이동 (Canvas 업데이트 충돌 방지)
    /// </summary>
    private System.Collections.IEnumerator ScrollToBottomNextFrame()
    {
        yield return null; // 다음 프레임까지 대기
        if (outputScrollRect != null)
        {
            // LayoutRebuilder를 사용하여 안전하게 레이아웃 업데이트
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(outputScrollRect.content);
            outputScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void ToggleCheatPanel()
    {
        if (cheatPanel == null) return;

        isConsoleOpen = !cheatPanel.activeInHierarchy;
        cheatPanel.SetActive(isConsoleOpen);

        if (isConsoleOpen)
        {
            commandInput?.ActivateInputField();
            // 너무 복잡해서 주석처리
            // AddToOutput("=== 고급 치트 시스템 활성화 ===");
            // AddToOutput("<color=cyan>💡 사용법:</color>");
            // AddToOutput("<color=yellow>• 명령어 입력 시 실시간 제안 표시</color>");
            // AddToOutput("<color=yellow>• ↑↓ 키로 제안 탐색, Enter로 선택</color>");
            // AddToOutput("<color=yellow>• Tab 키로 자동완성</color>");
            // AddToOutput("<color=yellow>• help 명령어로 전체 도움말 확인</color>");
            // AddToOutput("<color=white>명령어를 입력해보세요...</color>");
        }
    }

    public void ClearOutput()
    {
        if (outputText != null)
            outputText.text = "";
    }

    #region 기본 치트 명령어들

    [CheatCommand("clear", "콘솔 출력을 지웁니다", "System")]
    private void ClearConsole()
    {
        ClearOutput();
        Debug.Log("콘솔 출력이 지워졌습니다.");
    }

    [CheatCommand("spawn_eggs", "모든 방에 적 알을 스폰합니다", "Enemy")]
    private void SpawnEggs()
    {
        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.SpawnEggsInEachRoom();
            Debug.Log("모든 방에 적 알 스폰 완료");
        }
        else
        {
            Debug.LogError("EnemyManager를 찾을 수 없습니다");
        }
    }

    [CheatCommand("spawn_enemy", "지정된 위치에 적을 스폰합니다", "Enemy")]
    private void SpawnEnemy(int count = 1)
    {
        if (EnemyManager.instance != null && initEnemyPoint != null)
        {
            for (int i = 0; i < count; i++)
            {
                EnemyManager.instance.SpawnEnemy(initEnemyPoint);
            }
            Debug.Log($"{count}마리의 적 스폰 완료");
        }
        else
        {
            Debug.LogError("EnemyManager 또는 스폰 포인트를 찾을 수 없습니다");
        }
    }

    [CheatCommand("random_enemy", "랜덤 위치에 적을 스폰합니다", "Enemy")]
    private void SpawnRandomEnemy()
    {
        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.RandomSpawnEnemy();
            Debug.Log("랜덤 위치에 적 스폰 완료");
        }
        else
        {
            Debug.LogError("EnemyManager를 찾을 수 없습니다");
        }
    }

    [CheatCommand("init_schedule", "기본 적 스케줄을 초기화합니다", "Schedule")]
    private void InitSchedule()
    {
        if (RegionManager.instance != null)
        {
            RegionManager.instance.InitDefaultSchedule();
            Debug.Log("기본 스케줄 초기화 완료");
        }
        else
        {
            Debug.LogError("RegionManager를 찾을 수 없습니다");
        }
    }

    [CheatCommand("fast_forward", "가장 가까운 스케줄 시간으로 빠르게 이동합니다", "Time")]
    private void FastForwardTime()
    {
        if (TimeManager.instance != null)
        {
            TimeManager.instance.FastForwardToNearestSchedule();
            Debug.Log("시간 빠르게 이동 완료");
        }
        else
        {
            Debug.LogError("TimeManager를 찾을 수 없습니다");
        }
    }

    [CheatCommand("hatch_eggs", "스폰된 알들을 즉시 부화시킵니다", "Enemy")]
    private void HatchEggs()
    {
        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.CheatEggHatchIntoEnemy();
            Debug.Log("모든 알 부화 완료");
        }
        else
        {
            Debug.LogError("EnemyManager를 찾을 수 없습니다");
        }
    }

    [CheatCommand("teleport", "플레이어를 지정된 위치로 이동시킵니다", "Player")]
    private void TeleportPlayer(float x, float y, float z)
    {
        if (GameManager.instance?.currentPlayer != null)
        {
            Vector3 newPosition = new Vector3(x, y, z);
            GameManager.instance.currentPlayer.transform.position = newPosition;
            Debug.Log($"플레이어를 {newPosition}로 이동시켰습니다");
        }
        else
        {
            Debug.LogError("플레이어를 찾을 수 없습니다");
        }
    }

    [CheatCommand("god_mode", "플레이어 무적 모드를 토글합니다", "Player")]
    private void ToggleGodMode(bool enabled = true)
    {
        Debug.Log($"무적 모드: {(enabled ? "활성화" : "비활성화")}");
        // TODO: 실제 무적 모드 구현 필요
    }

    [CheatCommand("fps", "현재 FPS를 표시합니다", "Debug")]
    private void ShowFPS()
    {
        float fps = 1.0f / Time.deltaTime;
        Debug.Log($"현재 FPS: {fps:F1}");
    }

    [CheatCommand("version", "게임 정보를 표시합니다", "System")]
    private void ShowVersion()
    {
        Debug.Log($"Unity 버전: {Application.unityVersion}");
        Debug.Log($"플랫폼: {Application.platform}");
        Debug.Log($"게임 버전: {Application.version}");
    }

    [CheatCommand("refresh", "명령어 목록을 새로고침합니다", "System")]
    private void RefreshCommands()
    {
        if (CommandProcessor.Instance != null)
        {
            CommandProcessor.Instance.RefreshCommands();
            Debug.Log("명령어 목록이 새로고침되었습니다");
        }
    }

    #endregion
}



