using UnityEngine;
using System.Collections.Generic;

public class BuildSystem : MonoBehaviour
{
    public GameObject objectToPlace; // ��ġ�� ��ü
    public LayerMask groundLayer; // ��ġ ������ ���̾�
    public Material previewMaterial; // ��ġ �̸������ ������ ����

    private GameObject currentObject; // ���� ��ġ ���� ��ü
    private bool isPlacing = false; // ��ġ ��� ����
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>(); // ���� ���� �����

    void Update()
    {
        if (isPlacing)
        {
            MoveObjectToMouse(); // ���콺�� ���� ��ü �̵�
            RotateObject(); // ��ü ȸ��

            if (Input.GetMouseButtonDown(0))
            {
                PlaceObject(); // ��ġ Ȯ��
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement(); // ��ġ ���
            }
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            StartPlacing(); // ��ġ ��� ����
        }
    }

    // ��ġ ��� ���� - ��ü�� �����ϰ� �̸����� ���� ����
    void StartPlacing()
    {
        //objectToPlace = GetBuildObject();
        if (objectToPlace != null)
        {
            currentObject = Instantiate(objectToPlace); // ��ġ�� ��ü �ν��Ͻ�ȭ
            //currentObject.layer = 
            SetPreviewMaterial(currentObject); // �̸����� ���� ����

            // ��ü�� Rigidbody�� �ִٸ� ��ġ ��� ���� ��Ȱ��ȭ
            Rigidbody rb = currentObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            isPlacing = true; // ��ġ ��� Ȱ��ȭ
        }
    }

    // ���콺�� ���� ��ü �̵� - Raycast�� ����Ͽ� ���� ��ġ ���
    void MoveObjectToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // ���콺 ��ġ���� Ray ����
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            currentObject.transform.position = hit.point; // ��ü ��ġ�� Ray �浹 �������� ����
        }
    }

    // ��ü ȸ�� - R Ű�� ���� ��ü�� ȸ����Ŵ
    void RotateObject()
    {
        if (Input.GetKey(KeyCode.R))
        {
            currentObject.transform.Rotate(Vector3.up, 100 * Time.deltaTime); // Y�� �������� ȸ��
        }
    }

    // ��ü ��ġ Ȯ�� - �̸����� ���� ���� �� Rigidbody ����
    void PlaceObject()
    {
        if (currentObject.GetComponent<Rigidbody>() != null)
        {
            Rigidbody rb = currentObject.GetComponent<Rigidbody>();
            rb.isKinematic = false; // Rigidbody ���� Ȱ��ȭ
            rb.useGravity = true; // �߷� Ȱ��ȭ
        }
        RemovePreviewMaterial(currentObject); // �̸����� ���� ����
        currentObject = null;
        isPlacing = false; // ��ġ ��� ��Ȱ��ȭ
    }

    // ��ġ ��� - ���� ��ü ����
    void CancelPlacement()
    {
        Destroy(currentObject); // ���� ��ġ ���� ��ü ����
        currentObject = null;
        isPlacing = false; // ��ġ ��� ��Ȱ��ȭ
    }

    // �̸����� ���� ���� - ��ġ�� ��ü�� ������ ���� ����
    void SetPreviewMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(); // ��ü�� ��� Renderer ��������
        foreach (Renderer renderer in renderers)
        {
            originalMaterials[renderer] = renderer.material; // ���� ���� ���
            renderer.material = previewMaterial; // �� Renderer�� �̸����� ���� ����
        }
    }

    // �̸����� ���� ���� - ��ġ Ȯ�� �� ���� ������ ����
    void RemovePreviewMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(); // ��ü�� ��� Renderer ��������
        foreach (Renderer renderer in renderers)
        {
            if (originalMaterials.ContainsKey(renderer))
            {
                renderer.material = originalMaterials[renderer]; // ���� ������ ����
            }
        }
        originalMaterials.Clear(); // ��� ���� ���� ����
    }

    GameObject GetBuildObject()
    {
        return null;
    }
}
