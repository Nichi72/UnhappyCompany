using UnityEngine;

public class EmotionWheelController : MonoBehaviour
{
    public GameObject emotionWheelUI;
    public Player player;
   
    void Start()
    {
       player = GameManager.instance.currentPlayer;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            bool isActive = emotionWheelUI.activeSelf;
            player.firstPersonController._input.SetCursorLock(isActive);
            emotionWheelUI.SetActive(!isActive);
        }
    }
}
