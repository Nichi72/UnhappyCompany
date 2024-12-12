using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnemyStateDebugUI : MonoBehaviour
{
    [Header("References")]
    public MeshRenderer stateIndicator;    // Inspector에서 프리팹의 MeshRenderer 컴포넌트 할당
    public TMP_Text stateText;    // Inspector에서 프리팹의 TMP 컴포넌트 할당
    private Transform targetEnemy;
    
    [Header("State Colors")]
    public Color patrolColor;
    public Color chaseColor;
    public Color attackColor;

    public void Initialize(Transform enemy, Color patrolGizmoColor, Color chaseGizmoColor, Color attackGizmoColor)
    {
        targetEnemy = enemy;
        transform.position = targetEnemy.position + Vector3.up * 2f;

        // 색상 초기화
        patrolColor = patrolGizmoColor;
        chaseColor = chaseGizmoColor;
        attackColor = attackGizmoColor;
    }

    private void LateUpdate()
    {
        if (targetEnemy != null)
        {
            // UI가 항상 적 위에 위치하도록 업데이트
            transform.position = targetEnemy.position + Vector3.up * 2f;
            // UI가 항상 카메라를 향하도록 설정
            transform.rotation = Camera.main.transform.rotation;
        }
    }

    public void UpdateState(string stateName)
    {
        if (stateText != null)
        {
            stateText.text = stateName;
        }
        
        if (stateIndicator != null)
        {
            // 상태에 따라 색상 변경
            switch (stateName.ToLower())
            {
                case "patrol":
                    stateIndicator.material.color = patrolColor;
                    break;
                case "chase":
                    stateIndicator.material.color = chaseColor;
                    break;
                case "attack":
                    stateIndicator.material.color = attackColor;
                    break;
                default:
                    stateIndicator.material.color = Color.white;
                    break;
            }
        }
    }
} 