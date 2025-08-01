using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

/// <summary>
/// 명령어 정보를 담는 클래스
/// </summary>
[System.Serializable]
public class CommandInfo
{
    public string name;
    public string description;
    public string category;
    public MethodInfo method;
    public object target;
    public ParameterInfo[] parameters;
    
    public string GetUsage()
    {
        if (parameters.Length == 0)
            return name;
            
        var paramStrings = parameters.Select(p => 
        {
            string paramStr = p.Name;
            if (p.HasDefaultValue)
                paramStr = $"[{paramStr}]";
            else
                paramStr = $"<{paramStr}>";
            return paramStr;
        });
        
        return $"{name} {string.Join(" ", paramStrings)}";
    }
}

/// <summary>
/// 어트리뷰트 기반 명령어 시스템의 핵심 프로세서
/// </summary>
public class CommandProcessor : MonoBehaviour
{
    public static CommandProcessor Instance { get; private set; }
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>();
    private List<string> commandHistory = new List<string>();
    private int historyIndex = -1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            RegisterAllCommands();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 모든 CheatCommand 어트리뷰트가 붙은 메서드들을 찾아서 등록
    /// </summary>
    private void RegisterAllCommands()
    {
        int registeredCount = 0;
        
        // 현재 씬의 모든 MonoBehaviour에서 찾기
        MonoBehaviour[] allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
        
        foreach (var mb in allMonoBehaviours)
        {
            Type type = mb.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (var method in methods)
            {
                CheatCommandAttribute attr = method.GetCustomAttribute<CheatCommandAttribute>();
                if (attr != null)
                {
                    RegisterCommand(method, attr, mb);
                    registeredCount++;
                }
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"[CommandProcessor] 총 {registeredCount}개의 치트 명령어가 등록되었습니다.");
    }

    /// <summary>
    /// 개별 명령어 등록
    /// </summary>
    private void RegisterCommand(MethodInfo method, CheatCommandAttribute attr, object target)
    {
        var commandInfo = new CommandInfo
        {
            name = attr.CommandName.ToLower(),
            description = attr.Description,
            category = attr.Category,
            method = method,
            target = target,
            parameters = method.GetParameters()
        };

        if (commands.ContainsKey(commandInfo.name))
        {
            Debug.LogWarning($"[CommandProcessor] 중복된 명령어 이름: {attr.CommandName}");
            return;
        }

        commands[commandInfo.name] = commandInfo;
        
        if (showDebugLogs)
            Debug.Log($"[CommandProcessor] 명령어 등록: {commandInfo.GetUsage()} - {attr.Description}");
    }

    /// <summary>
    /// 명령어 실행
    /// </summary>
    public bool ExecuteCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // 히스토리에 추가
        AddToHistory(input);

        // 명령어 파싱
        string[] parts = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;

        string commandName = parts[0].ToLower();

        // 도움말 명령어
        if (commandName == "help" || commandName == "?")
        {
            ShowHelp(parts.Length > 1 ? parts[1] : null);
            return true;
        }

        // 명령어 찾기
        if (!commands.ContainsKey(commandName))
        {
            Debug.LogError($"[CheatSystem] 알 수 없는 명령어: {commandName}");
            ShowSimilarCommands(commandName);
            return false;
        }

        var command = commands[commandName];

        try
        {
            // 매개변수 파싱
            object[] parameters = ParseParameters(command, parts.Skip(1).ToArray());
            
            // 명령어 실행
            command.method.Invoke(command.target, parameters);
            
            Debug.Log($"[CheatSystem] 명령어 실행 완료: {input}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CheatSystem] 명령어 실행 중 오류: {ex.Message}");
            Debug.LogError($"[CheatSystem] 사용법: {command.GetUsage()}");
            return false;
        }
    }

    /// <summary>
    /// 매개변수 파싱
    /// </summary>
    private object[] ParseParameters(CommandInfo command, string[] args)
    {
        var parameters = new object[command.parameters.Length];
        
        for (int i = 0; i < command.parameters.Length; i++)
        {
            var param = command.parameters[i];
            
            if (i < args.Length)
            {
                parameters[i] = ConvertToType(args[i], param.ParameterType);
            }
            else if (param.HasDefaultValue)
            {
                parameters[i] = param.DefaultValue;
            }
            else
            {
                throw new ArgumentException($"필수 매개변수 '{param.Name}'이 누락되었습니다.");
            }
        }
        
        return parameters;
    }

    /// <summary>
    /// 문자열을 지정된 타입으로 변환
    /// </summary>
    private object ConvertToType(string value, Type targetType)
    {
        try
        {
            if (targetType == typeof(string))
                return value;
            if (targetType == typeof(int))
                return int.Parse(value);
            if (targetType == typeof(float))
                return float.Parse(value);
            if (targetType == typeof(bool))
                return bool.Parse(value);
            if (targetType == typeof(Vector3))
            {
                var parts = value.Split(',');
                if (parts.Length == 3)
                    return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
            }
            
            throw new ArgumentException($"지원하지 않는 타입: {targetType.Name}");
        }
        catch (Exception)
        {
            throw new ArgumentException($"'{value}'를 {targetType.Name} 타입으로 변환할 수 없습니다.");
        }
    }

    /// <summary>
    /// 도움말 표시
    /// </summary>
    private void ShowHelp(string category = null)
    {
        if (string.IsNullOrEmpty(category))
        {
            Debug.Log("=== 사용 가능한 명령어 ===");
            var categories = commands.Values.GroupBy(c => c.category);
            foreach (var cat in categories.OrderBy(c => c.Key))
            {
                Debug.Log($"\n[{cat.Key}]");
                foreach (var cmd in cat.OrderBy(c => c.name))
                {
                    Debug.Log($"  {cmd.GetUsage()} - {cmd.description}");
                }
            }
            Debug.Log("\n도움말: help <명령어이름> 또는 help <카테고리>");
        }
        else
        {
            var matchingCommands = commands.Values.Where(c => 
                c.name.Equals(category, StringComparison.OrdinalIgnoreCase) ||
                c.category.Equals(category, StringComparison.OrdinalIgnoreCase)
            ).ToList();
            
            if (matchingCommands.Any())
            {
                Debug.Log($"=== {category} 도움말 ===");
                foreach (var cmd in matchingCommands.OrderBy(c => c.name))
                {
                    Debug.Log($"{cmd.GetUsage()} - {cmd.description}");
                }
            }
            else
            {
                Debug.LogError($"'{category}' 명령어 또는 카테고리를 찾을 수 없습니다.");
            }
        }
    }

    /// <summary>
    /// 유사한 명령어 제안
    /// </summary>
    private void ShowSimilarCommands(string input)
    {
        var similar = commands.Keys.Where(cmd => 
            cmd.Contains(input) || 
            GetLevenshteinDistance(cmd, input) <= 2
        ).Take(3).ToList();
        
        if (similar.Any())
        {
            Debug.Log($"[CheatSystem] 유사한 명령어: {string.Join(", ", similar)}");
        }
    }

    /// <summary>
    /// 레벤슈타인 거리 계산
    /// </summary>
    private int GetLevenshteinDistance(string s1, string s2)
    {
        if (s1.Length == 0) return s2.Length;
        if (s2.Length == 0) return s1.Length;

        int[,] matrix = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= s2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = (s2[j - 1] == s1[i - 1]) ? 0 : 1;
                matrix[i, j] = Math.Min(Math.Min(
                    matrix[i - 1, j] + 1,
                    matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[s1.Length, s2.Length];
    }

    /// <summary>
    /// 히스토리 관리
    /// </summary>
    private void AddToHistory(string command)
    {
        if (commandHistory.Count == 0 || commandHistory[commandHistory.Count - 1] != command)
        {
            commandHistory.Add(command);
            if (commandHistory.Count > 50)
                commandHistory.RemoveAt(0);
        }
        historyIndex = commandHistory.Count;
    }

    public string GetPreviousCommand()
    {
        if (commandHistory.Count == 0) return "";
        historyIndex = Mathf.Max(0, historyIndex - 1);
        return commandHistory[historyIndex];
    }

    public string GetNextCommand()
    {
        if (commandHistory.Count == 0) return "";
        historyIndex = Mathf.Min(commandHistory.Count, historyIndex + 1);
        return historyIndex < commandHistory.Count ? commandHistory[historyIndex] : "";
    }

    /// <summary>
    /// 자동 완성을 위한 명령어 제안
    /// </summary>
    public List<string> GetCommandSuggestions(string partial)
    {
        return commands.Keys
            .Where(cmd => cmd.StartsWith(partial.ToLower()))
            .OrderBy(x => x)
            .ToList();
    }

    /// <summary>
    /// 상세한 명령어 정보와 함께 제안 반환
    /// </summary>
    public Dictionary<string, CommandInfo> GetDetailedCommandSuggestions(string partial)
    {
        var result = new Dictionary<string, CommandInfo>();
        
        var matchingCommands = commands.Where(kvp => 
            kvp.Key.StartsWith(partial.ToLower())
        ).OrderBy(kvp => kvp.Key);
        
        foreach (var kvp in matchingCommands)
        {
            result[kvp.Key] = kvp.Value;
        }
        
        return result;
    }

    /// <summary>
    /// 특정 명령어의 상세 정보 반환
    /// </summary>
    public CommandInfo GetCommandInfo(string commandName)
    {
        commands.TryGetValue(commandName.ToLower(), out CommandInfo info);
        return info;
    }

    /// <summary>
    /// 런타임에 새 명령어 등록 (필요 시)
    /// </summary>
    public void RefreshCommands()
    {
        commands.Clear();
        RegisterAllCommands();
    }
} 