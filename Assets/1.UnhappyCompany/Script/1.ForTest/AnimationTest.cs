using UnityEngine;

public class AnimationTest : MonoBehaviour
{
    public Animator animator1;
    public Animator animator2;
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            animator1.SetTrigger("test");
            animator2.SetTrigger("test");
        }
    }
}
