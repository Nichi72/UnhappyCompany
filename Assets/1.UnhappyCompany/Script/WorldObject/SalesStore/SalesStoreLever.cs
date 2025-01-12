using UnityEngine;
using System.Collections;

public class SalesStoreLever : MonoBehaviour , IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private string animationNameOpen = "Open";

    private bool isAnimating = false;

    public void HitEventInteractionF(Player rayOrigin)
    {
        if (isAnimating) return; // 애니메이션이 실행 중이면 함수 호출을 무시

        isAnimating = true; // 애니메이션 실행 중으로 설정
        animator.Play(animationNameOpen, -1, 0f);
        StartCoroutine(DisableAnimatorAfterAnimation());
    }

    private IEnumerator DisableAnimatorAfterAnimation()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isAnimating = false; // 애니메이션이 끝나면 다시 호출 가능하게 설정
    }
}
