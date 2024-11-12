using UnityEngine;

public static class UtilityRaycast
{
    /// <summary>
    /// Raycast를 수행하는 메서드
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
    /// 태그를 기반으로 Raycast를 수행하는 메서드
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
    /// RaycastHit 처리 메서드
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="targetTag"></param>
    public static void HandleRaycastHit(RaycastHit hit, string targetTag)
    {
        if (hit.collider.CompareTag(targetTag))
        {
            Debug.Log(targetTag + " 오브젝트를 검출했습니다!");
        }
        else
        {
            Debug.Log("다른 오브젝트를 검출했습니다: " + hit.collider.tag);
        }
    }
}

public static class VectorLerpUtility
{
    // 두 개의 벡터 사이를 Lerp로 움직이는 메서드
    public static Vector3 LerpBetweenVectors(Vector3 start, Vector3 end, float time)
    {
        return Vector3.Lerp(start, end, time);
    }
    public static Vector3 FastToSlowLerp(Vector3 start, Vector3 end, float time)
    {
        time = 1f - Mathf.Pow(1f - time, 3); // 초반에 빠르고 점점 느려지는 가속도 적용
        return Vector3.Lerp(start, end, time);
    }
}