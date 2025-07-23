using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

/// <summary>
/// 명령어 정보를 담는 클래스
/// </summary>
public class CommandInfo
{
    public string Name;
    public string Description;
    public string Category;
    public MethodInfo Method;
    public object Target;
    public ParameterInfo[] Parameters;
    public bool DeveloperOnly;
    
    public string GetUsage()
    {
        if (Parameters.Length == 0)
            return Name;
            
        var paramStrings = Parameters.Select(p => 
        {
            string paramStr = p.Name;
            if (p.HasDefaultValue)
                paramStr = $"[{paramStr}]";
            else
                paramStr = $"<{paramStr}>";
            return paramStr;
        });
        
        return $"{Name} {string.Join(" ", paramStrings)}";
    }
}

/// <summary>
/// 어트리뷰트 기반 명령어 시스템의 핵심 프로세서
/// </summary>
public class CommandProcessor : MonoBehaviour
{
    public static CommandProcessor Instance { get; private set; }
    
    private Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>();
    private List<string> commandHistory = new List<string>();
    private int historyIndex = -1;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

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
    /// 모든 어셈블리에서 CheatCommand 어트리뷰트가 붙은 메서드들을 찾아서 등록
    /// </summary>
    private void RegisterAllCommands()
    {
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        int registeredCount = 0;
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    // MonoBehaviour를 상속받은 클래스만 처리
                    if (!typeof(MonoBehaviour).IsAssignableFrom(type)) continue;
                    
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    
                    foreach (var method in methods)
                    {
                        var attr = method.GetCustomAttribute<CheatCommandAttribute>();
                        if (attr != null)
                        {
                            RegisterCommand(method, attr);
                            registeredCount++;
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 일부 어셈블리에서 타입 로드 실패 시 무시
                if (showDebugLogs)
                    Debug.LogWarning($"타입 로드 실패: {assembly.FullName}");
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"총 {registeredCount}개의 치트 명령어가 등록되었습니다.");
    }

    /// <summary>
    /// 개별 명령어 등록
    /// </summary>
    private void RegisterCommand(MethodInfo method, CheatCommandAttribute attr)
    {
        object target = null;
        
        // Static 메서드가 아닌 경우 인스턴스 찾기
        if (!method.IsStatic)
        {
            target = FindObjectOfType(method.DeclaringType);
            if (target == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"명령어 '{attr.CommandName}': {method.DeclaringType.Name}의 인스턴스를 찾을 수 없습니다.");
                return;
            }
        }

        var commandInfo = new CommandInfo
        {
            Name = attr.CommandName,
            Description = attr.Description,
            Category = attr.Category,
            Method = method,
            Target = target,
            Parameters = method.GetParameters(),
            DeveloperOnly = attr.DeveloperOnly
        };

        if (commands.ContainsKey(attr.CommandName))
        {
            Debug.LogWarning($"중복된 명령어 이름: {attr.CommandName}");
            return;
        }

        commands[attr.CommandName] = commandInfo;
        
        if (showDebugLogs)
            Debug.Log($"명령어 등록: {commandInfo.GetUsage()} - {attr.Description}");
    }

    /// <summary>
    /// 명령어 실행
    /// </summary>
    public bool ExecuteCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // 히스토리에 추가
        if (commandHistory.Count == 0 || commandHistory[commandHistory.Count - 1] != input)
        {
            commandHistory.Add(input);
            if (commandHistory.Count > 50) // 최대 50개 히스토리 유지
                commandHistory.RemoveAt(0);
        }
        historyIndex = commandHistory.Count;

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
            Debug.LogError($"알 수 없는 명령어: {commandName}");
            ShowSimilarCommands(commandName);
            return false;
        }

        var command = commands[commandName];

        try
        {
            // 매개변수 파싱
            object[] parameters = ParseParameters(command, parts.Skip(1).ToArray());
            
            // 명령어 실행
            command.Method.Invoke(command.Target, parameters);
            
            if (showDebugLogs)
                Debug.Log($"명령어 실행 완료: {input}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"명령어 실행 중 오류: {ex.Message}");
            Debug.LogError($"사용법: {command.GetUsage()}");
            return false;
        }
    }

    /// <summary>
    /// 매개변수 파싱
    /// </summary>
    private object[] ParseParameters(CommandInfo command, string[] args)
    {
        var parameters = new object[command.Parameters.Length];
        
        for (int i = 0; i < command.Parameters.Length; i++)
        {
            var param = command.Parameters[i];
            
            if (i < args.Length)
            {
                // 전달된 인수가 있는 경우
                parameters[i] = ConvertToType(args[i], param.ParameterType);
            }
            else if (param.HasDefaultValue)
            {
                // 기본값이 있는 경우
                parameters[i] = param.DefaultValue;
            }
            else
            {
                // 필수 매개변수가 없는 경우
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

    /// <summary>
    /// 도움말 표시
    /// </summary>
    private void ShowHelp(string category = null)
    {
        if (string.IsNullOrEmpty(category))
        {
            Debug.Log("=== 사용 가능한 명령어 ===");
            var categories = commands.Values.GroupBy(c => c.Category);
            foreach (var cat in categories.OrderBy(c => c.Key))
            {
                Debug.Log($"\n[{cat.Key}]");
                foreach (var cmd in cat.OrderBy(c => c.Name))
                {
                    Debug.Log($"  {cmd.GetUsage()} - {cmd.Description}");
                }
            }
            Debug.Log("\n도움말: help <명령어이름> 또는 help <카테고리>");
        }
        else
        {
            // 특정 명령어 또는 카테고리 도움말
            var matchingCommands = commands.Values.Where(c => 
                c.Name.Equals(category, StringComparison.OrdinalIgnoreCase) ||
                c.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
            ).ToList();
            
            if (matchingCommands.Any())
            {
                Debug.Log($"=== {category} 도움말 ===");
                foreach (var cmd in matchingCommands.OrderBy(c => c.Name))
                {
                    Debug.Log($"{cmd.GetUsage()} - {cmd.Description}");
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
            LevenshteinDistance(cmd, input) <= 2
        ).Take(3).ToList();
        
        if (similar.Any())
        {
            Debug.Log($"유사한 명령어: {string.Join(", ", similar)}");
        }
    }

    /// <summary>
    /// 레벤슈타인 거리 계산 (유사도 측정)
    /// </summary>
    private int LevenshteinDistance(string s1, string s2)
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
    /// 명령어 히스토리 가져오기
    /// </summary>
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
    /// 등록된 모든 명령어 목록 반환
    /// </summary>
    public List<string> GetAllCommandNames()
    {
        return commands.Keys.OrderBy(x => x).ToList();
    }

    /// <summary>
    /// 자동 완성을 위한 명령어 제안
    /// </summary>
    public List<string> GetCommandSuggestions(string partial)
    {
        return commands.Keys
            .Where(cmd => cmd.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x)
            .ToList();
    }
} 