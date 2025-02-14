using UnityEngine;
using System.Collections.Generic;

namespace SaveAndLoadPrefabsInstantiatedAtRuntime
{
    /*
     * 이 클래스는 CreateRandomPrefab() 메서드가 호출될 때마다 무작위 위치에 프리팹을 인스턴스화합니다.
     * SavePrefabInstances() 메서드가 호출되면, 이 프리팹 인스턴스들이 저장됩니다.
     * LoadPrefabInstances() 메서드가 호출되면, 프리팹 인스턴스들이 복원됩니다 (같은 참조 ID를 가진 인스턴스가 이미 존재하지 않는 경우에만).
     */
    public class PrefabManager : MonoBehaviour
    {
        // 우리가 인스턴스화하고자 하는 프리팹들입니다.
        // 이 배열에 프리팹을 추가하기 전에, 프리팹을 우클릭하고 Easy Save 3 > Enable Easy Save for Prefab을 선택해야 합니다.
        // 또한, Tools > Easy Save 3 > Add Manager to Scene을 통해 Easy Save 3 매니저를 장면에 추가해야 합니다.
        public GameObject[] prefabs;
        // 프리팹을 인스턴스화할 때 이 리스트에 추가합니다.
        private List<GameObject> prefabInstances = new List<GameObject>();

        // 무작위 위치에 무작위 프리팹을 인스턴스화합니다.
        public void CreateRandomPrefab()
        {
            // prefabs 배열에서 무작위로 프리팹을 선택합니다.
            var prefab = prefabs[Random.Range(0, prefabs.Length)];
            // (0,0)에서 5 유닛 이내의 무작위 위치에 프리팹을 인스턴스화합니다.
            var prefabInstance = Instantiate(prefab, Random.insideUnitSphere * 5, Random.rotation);
            // prefabInstances 리스트에 프리팹 인스턴스를 추가합니다.
            prefabInstances.Add(prefabInstance);
        }

        public void SavePrefabInstances()
        {
            // "prefabInstances"라는 고유 키를 사용하여 prefabInstances 리스트를 파일에 저장합니다.
            ES3.Save("prefabInstances", prefabInstances);
        }

        public void LoadPrefabInstances()
        {
            // 저장할 때 사용한 것과 동일한 고유 키를 사용하여 prefabInstances 리스트를 로드합니다.
            // 저장된 데이터가 없으면 빈 리스트를 반환합니다.
            // 이러한 참조 ID를 가진 프리팹 인스턴스가 여전히 존재하는 경우.
            prefabInstances = ES3.Load("prefabInstances", new List<GameObject>());
        }
    }
}