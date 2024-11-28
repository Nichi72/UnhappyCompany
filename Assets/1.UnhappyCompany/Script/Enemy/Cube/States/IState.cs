/// <summary>
/// IState 인터페이스는 AI의 상태 클래스들이 구현해야 하는 기본 메서드들을 정의합니다.
/// 각 상태는 Enter, Execute, Exit 메서드를 구현해야 합니다.
/// </summary>
public interface IState
{
    /// <summary>
    /// 상태가 시작될 때 호출되는 메서드.
    /// </summary>
    void Enter();

    /// <summary>
    /// 상태가 활성화된 동안 매 프레임마다 호출되는 메서드.
    /// </summary>
    void Execute();

    /// <summary>
    /// 상태가 종료될 때 호출되는 메서드.
    /// </summary>
    void Exit();
} 