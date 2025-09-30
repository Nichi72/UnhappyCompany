using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RoomOrganizing : MonoBehaviour
{
    [ReadOnly] [SerializeField] private List<string> wallKeyword = new List<string>(){"wall" , "Wall"};
    [ReadOnly] [SerializeField] private List<string> groundKeyword = new List<string>(){"floor" , "Floor"};
    [ReadOnly] [SerializeField] private List<string> floorKeyword = new List<string>(){"roundCeiling" , "pillar_B" , "pillar"};
    [ReadOnly] [SerializeField] private List<string> doorsKeyword = new List<string>(){"door" , "Door"};
    [ReadOnly] [SerializeField] private List<string> reflectionProbeKeyword = new List<string>(){"reflectionProbe" , "ReflectionProbe"};
    [ReadOnly] [SerializeField] private List<string> lightKeyword = new List<string>(){"light" , "Light"};

    private List<string> parentObjectNames = new List<string>() { "Wall", "Ground", "Floor", "OutDoor", "InnerDoor", "InteractionObj", "OtherObj", "ReflectionProbe", "Light" };

    private GameObject wallParent;
    private GameObject groundParent;
    private GameObject floorParent;
    private GameObject outDoorParent;
    private GameObject innerDoorParent;
    private GameObject interactionObjParent;
    private GameObject otherObjParent;
    private GameObject reflectionProbeParent;
    private GameObject lightParent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // OrganizeRoomObjects();
    }

    public void OrganizeRoomObjects()
    {
        // 부모 오브젝트 생성
        wallParent = CreateParentObject("Wall");
        groundParent = CreateParentObject("Ground");
        floorParent = CreateParentObject("Floor");
        outDoorParent = CreateParentObject("OutDoor");
        innerDoorParent = CreateParentObject("InnerDoor");
        interactionObjParent = CreateParentObject("InteractionObj");
        otherObjParent = CreateParentObject("OtherObj");
        reflectionProbeParent = CreateParentObject("ReflectionProbe");
        lightParent = CreateParentObject("Light");

        // 모든 자식 오브젝트 가져오기
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        
        foreach (Transform child in allChildren)
        {
            if (child == transform) continue; // 자기 자신은 건너뛰기
            if (parentObjectNames.Contains(child.gameObject.name)) continue; // 부모 오브젝트는 건너뛰기

            string childName = child.gameObject.name;
            bool isOrganized = false;

            // Wall 체크
            if (ContainsKeyword(childName, wallKeyword))
            {
                child.SetParent(wallParent.transform);
                isOrganized = true;
            }
            // Ground 체크
            else if (ContainsKeyword(childName, groundKeyword))
            {
                child.SetParent(groundParent.transform);
                isOrganized = true;
            }
            // Floor 체크
            else if (ContainsKeyword(childName, floorKeyword))
            {
                child.SetParent(floorParent.transform);
                isOrganized = true;
            }
            // Door 체크
            else if (ContainsKeyword(childName, doorsKeyword))
            {
                child.SetParent(outDoorParent.transform);
                isOrganized = true;
            }
            // ReflectionProbe 체크
            else if (ContainsKeyword(childName, reflectionProbeKeyword))
            {
                child.SetParent(reflectionProbeParent.transform);
                isOrganized = true;
            }
            // Light 체크
            else if (ContainsKeyword(childName, lightKeyword))
            {
                child.SetParent(lightParent.transform);
                isOrganized = true;
            }

            // 정리되지 않은 오브젝트는 OtherObj로 이동
            if (!isOrganized)
            {
                child.SetParent(otherObjParent.transform);
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(child.gameObject);
#endif
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
#endif
    }

    private GameObject CreateParentObject(string name)
    {
        GameObject parent = new GameObject(name);
        parent.transform.SetParent(transform);
        parent.transform.localPosition = Vector3.zero;
#if UNITY_EDITOR
        EditorUtility.SetDirty(parent);
#endif
        return parent;
    }

    private bool ContainsKeyword(string name, List<string> keywords)
    {
        foreach (string keyword in keywords)
        {
            if (name.Contains(keyword))
            {
                return true;
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RoomOrganizing))]
public class RoomOrganizingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomOrganizing roomOrganizing = (RoomOrganizing)target;
        
        EditorGUILayout.Space();
        if (GUILayout.Button("오브젝트 정리"))
        {
            roomOrganizing.OrganizeRoomObjects();
        }
    }
}
#endif
