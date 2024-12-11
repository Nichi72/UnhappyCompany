/// <summary>
/// IState 인터페이스는 AI의 상태 클래스들이 구현해야 하는 기본 메서드들을 정의합니다.
/// 각 상태는 Enter, ExecuteMorning, ExecuteAfternoon, Exit 메서드를 구현해야 합니다.
/// </summary>
public interface IState
{
    /// <summary>
    /// 상태가 시작될 때 호출되는 메서드.
    /// </summary>
    void Enter();
    /// <summary>
    /// 오전 동안 실행되는 Execute 메서드.
    /// </summary>
    void ExecuteMorning();
    /// <summary>
    /// 오후 동안 실행되는 Execute 메서드.
    /// </summary>
    void ExecuteAfternoon();
    // FixedUpdate에서 실행할 메서드
    void ExecuteFixedMorning();    // 아침에 FixedUpdate에서 호출될 메서드
    void ExecuteFixedAfternoon();  // 오후에 FixedUpdate에서 호출될 메서드
    void Exit();
} 