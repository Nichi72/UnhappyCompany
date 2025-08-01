using System;

/// <summary>
/// 메서드를 치트 명령어로 등록하는 어트리뷰트
/// 사용법: [CheatCommand("spawn", "적을 스폰합니다")]
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

    public CheatCommandAttribute(string commandName, string description = "", string category = "General")
    {
        CommandName = commandName;
        Description = description;
        Category = category;
    }
} 