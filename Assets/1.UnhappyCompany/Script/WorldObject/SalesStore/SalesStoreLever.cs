using UnityEngine;
using System.Collections;
using MyUtility;

public class SalesStoreLever : MonoBehaviour , IInteractableF
{
    [SerializeField] private Animator animator;
    [SerializeField] private string animationNameOpen = "Open";
    [SerializeField] private SalesStore salesStore;

    private bool isAnimating = false;

    public string InteractionTextF { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "SalesStoreLever_ITR"); set => InteractionTextF = value; }

    public void HitEventInteractionF(Player rayOrigin)
    {
        if (isAnimating) return; // 애니메이션이 실행 중이면 함수 호출을 무시

        isAnimating = true; // 애니메이션 실행 중으로 설정
        animator.Play(animationNameOpen, -1, 0f);
        
        foreach(var sellObject in salesStore.sellObjects) // 예외처리: 상자를 닫으면 모두 날라가는 버그가 있음
        {
            sellObject.GetComponent<Rigidbody>().isKinematic = false;
            sellObject.GetComponent<Collider>().enabled = true;
        }
        StartCoroutine(DisableAnimatorAfterAnimation());
    }

    private IEnumerator DisableAnimatorAfterAnimation()
    {
        StartCoroutine(salesStore.salesStoreSellPoint.ToggleDoor());
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isAnimating = false; // 애니메이션이 끝나면 다시 호출 가능하게 설정
        salesStore.SellObject();
        yield return new WaitForSeconds(1f);
        StartCoroutine(salesStore.salesStoreSellPoint.ToggleDoor());
    }

}
