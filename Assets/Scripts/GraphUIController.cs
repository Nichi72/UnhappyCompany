using UnityEngine;
using UnityEngine.UI;

public class GraphUIController : MonoBehaviour
{
    public GraphVisualizer graphVisualizer;
    public Button dfsButton;
    public Button bfsButton;

    void Start()
    {
        dfsButton.onClick.AddListener(OnDFSButtonClick);
        bfsButton.onClick.AddListener(OnBFSButtonClick);
    }

    void OnDFSButtonClick()
    {
        graphVisualizer.StartDFS();
    }

    void OnBFSButtonClick()
    {
        graphVisualizer.StartBFS();
    }
} 