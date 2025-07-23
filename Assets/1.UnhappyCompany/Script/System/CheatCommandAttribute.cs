using System;

/// <summary>
/// 메서드를 치트 명령어로 등록하는 어트리뷰트
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CheatCommandAttribute : Attribute
{
    /// <summary>
    /// 명령어 이름 (예: "spawn", "teleport")
    /// </summary>
    public string CommandName { get; }
    
    /// <summary>
    /// 명령어 설명 (도움말에 표시)
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// 명령어 카테고리 (예: "Enemy", "Player", "System")
    /// </summary>
    public string Category { get; }
    
    /// <summary>
    /// 개발자 전용 명령어인지 여부
    /// </summary>
    public bool DeveloperOnly { get; }

    public CheatCommandAttribute(string commandName, string description = "", string category = "General", bool developerOnly = false)
    {
        CommandName = commandName;
        Description = description;
        Category = category;
        DeveloperOnly = developerOnly;
    }
} 