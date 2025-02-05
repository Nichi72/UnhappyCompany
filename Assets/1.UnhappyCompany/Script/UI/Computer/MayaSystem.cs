using System.Collections.Generic;
using UnityEngine;

public class MayaSystem : MonoBehaviour
{
    public GameObject mayaImage;
    // public Dictionary<GameObject, GameObject> mayaImages = new Dictionary<GameObject, GameObject>();
    [SerializeField] private ComputerMayaPosition mainMayaPosition;
    [SerializeField] private ComputerMayaPosition[] computerMayaPositions;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetMayaPosition();
    }

    public void SetMayaPosition()
    {
        foreach (var mayaPosition in computerMayaPositions)
        {
            if (mayaPosition.computerView.activeSelf)
            {
                // var temp = 
                // Vector3 offset = new Vector3(mayaPosition.mayaPosition.GetComponent<RectTransform>().rect.width / 2, mayaPosition.mayaPosition.GetComponent<RectTransform>().rect.height / 2, 0);
                // MoveMayaImage(mayaPosition.mayaPosition.position - offset);
                MoveMayaImage(mayaPosition.mayaPosition.position);

                return;
            }
        }
        // 여기까지 온거면 켜져있는게 없기 때문에 MainMayaPosition이라는 뜻
        MoveMayaImage(mainMayaPosition.mayaPosition.position);

        void MoveMayaImage(Vector3 targetPosition)
        {
            mayaImage.transform.position = Vector3.Lerp(mayaImage.transform.position, targetPosition, Time.deltaTime * 5f);
        }
    }
}
[System.Serializable]
public class ComputerMayaPosition
{
    public string name;
    public GameObject computerView;
    public RectTransform mayaPosition;
    
}
