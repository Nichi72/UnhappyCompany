using UnityEngine;

public static class UtilityRaycast
{
    /// <summary>
    /// Raycast�� �����ϴ� �޼���
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="hit"></param>
    /// <param name="maxDistance"></param>
    /// <param name="layerMask"></param>
    /// <returns></returns>
    public static bool PerformRaycast(Ray ray, out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
    {
        return Physics.Raycast(ray, out hit, maxDistance, layerMask);
    }


    /// <summary>
    /// �±׸� ������� Raycast�� �����ϴ� �޼���
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="hit"></param>
    /// <param name="targetTag"></param>
    /// <param name="maxDistance"></param>
    /// <param name="layerMask"></param>
    /// <returns></returns>
    public static bool PerformRaycastWithTag(Ray ray, out RaycastHit hit, string targetTag, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
    {
        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            return hit.collider.CompareTag(targetTag);
        }
        return false;
    }

    /// <summary>
    /// RaycastHit ó�� �޼���
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="targetTag"></param>
    public static void HandleRaycastHit(RaycastHit hit, string targetTag)
    {
        if (hit.collider.CompareTag(targetTag))
        {
            Debug.Log(targetTag + " ������Ʈ�� �����߽��ϴ�!");
        }
        else
        {
            Debug.Log("�ٸ� ������Ʈ�� �����߽��ϴ�: " + hit.collider.tag);
        }
    }
}

public static class VectorLerpUtility
{
    // �� ���� ���� ���̸� Lerp�� �����̴� �޼���
    public static Vector3 LerpBetweenVectors(Vector3 start, Vector3 end, float time)
    {
        return Vector3.Lerp(start, end, time);
    }
    public static Vector3 FastToSlowLerp(Vector3 start, Vector3 end, float time)
    {
        time = 1f - Mathf.Pow(1f - time, 3); // �ʹݿ� ������ ���� �������� ���ӵ� ����
        return Vector3.Lerp(start, end, time);
    }
}