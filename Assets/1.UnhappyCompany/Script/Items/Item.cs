using UnityEngine;

public abstract class Item : MonoBehaviour , InteractionF
{
    public ItemData itemData; // ��ũ���ͺ� ������Ʈ�� ���� ������ ������
    /// <summary>
    /// �������� ���忡 �����ϰ� ���� �� �÷��̾ ���� ����ĳ��xm�� �¾��� �� ȣ��Ǵ� �Լ�
    /// </summary>
    /// <param name="player"></param>
    /// <param name="raycastHit"></param>
    public virtual void HitEventInteractionF(Player player , RaycastHit raycastHit)
    {
        Debug.Log("HitEvent!");
        QuickSlotSystem.instance.AddItemToQuickSlot(itemData.prefab.GetComponent<Item>());
        PickUp();
    }

    public virtual void Use()
    {
        Debug.Log($"{itemData.itemName} USE");
    }

    public virtual void PickUp()
    {
        // �������� ������ �� ���忡�� ������Ʈ�� ����
        Debug.Log($"{itemData.itemName} picked up.");
        Destroy(gameObject);
        QuickSlotSystem.instance.UpdatePlayerSpeed();
    }

    

}
