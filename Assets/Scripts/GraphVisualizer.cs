using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class GraphVisualizer : MonoBehaviour
{
    [System.Serializable]
    public class Node
    {
        public GameObject nodeObject;
        public List<Node> neighbors = new List<Node>();
        public bool visited = false;
        public Material defaultMaterial;
        public Material visitedMaterial;
        public Material currentMaterial;
    }

    public GameObject nodePrefab;
    public Material defaultNodeMaterial;
    public Material visitedNodeMaterial;
    public Material currentNodeMaterial;
    public float nodeSpacing = 2f;
    public float visualizationDelay = 1f;

    private List<Node> nodes = new List<Node>();
    private bool isVisualizing = false;

    void Start()
    {
        CreateGraph();
    }

    void CreateGraph()
    {
        // 예시 그래프 생성 (6개의 노드로 구성된 그래프)
        for (int i = 0; i < 6; i++)
        {
            CreateNode(new Vector3(i % 3 * nodeSpacing, i / 3 * nodeSpacing, 0));
        }

        // 노드 간 연결 ���정
        ConnectNodes(0, new int[] { 1, 2 });
        ConnectNodes(1, new int[] { 0, 3, 4 });
        ConnectNodes(2, new int[] { 0, 4 });
        ConnectNodes(3, new int[] { 1, 5 });
        ConnectNodes(4, new int[] { 1, 2, 5 });
        ConnectNodes(5, new int[] { 3, 4 });
    }

    Node CreateNode(Vector3 position)
    {
        GameObject nodeObj = Instantiate(nodePrefab, position, Quaternion.identity);
        nodeObj.transform.SetParent(transform);
        
        Node node = new Node
        {
            nodeObject = nodeObj,
            defaultMaterial = defaultNodeMaterial,
            visitedMaterial = visitedNodeMaterial,
            currentMaterial = currentNodeMaterial
        };
        
        nodes.Add(node);
        return node;
    }

    void ConnectNodes(int nodeIndex, int[] neighborIndices)
    {
        foreach (int neighborIndex in neighborIndices)
        {
            nodes[nodeIndex].neighbors.Add(nodes[neighborIndex]);
            DrawLine(nodes[nodeIndex].nodeObject.transform.position, 
                    nodes[neighborIndex].nodeObject.transform.position);
        }
    }

    void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("Line");
        line.transform.SetParent(transform);
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    public void StartDFS()
    {
        if (!isVisualizing)
        {
            ResetNodes();
            isVisualizing = true;
            StartCoroutine(DFS(nodes[0]));
        }
    }

    public void StartBFS()
    {
        if (!isVisualizing)
        {
            ResetNodes();
            isVisualizing = true;
            StartCoroutine(BFS(nodes[0]));
        }
    }

    IEnumerator DFS(Node node)
    {
        node.visited = true;
        node.nodeObject.GetComponent<Renderer>().material = node.currentMaterial;
        yield return new WaitForSeconds(visualizationDelay);
        
        node.nodeObject.GetComponent<Renderer>().material = node.visitedMaterial;

        foreach (Node neighbor in node.neighbors)
        {
            if (!neighbor.visited)
            {
                yield return StartCoroutine(DFS(neighbor));
            }
        }

        if (node == nodes[0])
        {
            isVisualizing = false;
        }
    }

    IEnumerator BFS(Node startNode)
    {
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(startNode);
        startNode.visited = true;

        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();
            node.nodeObject.GetComponent<Renderer>().material = node.currentMaterial;
            yield return new WaitForSeconds(visualizationDelay);
            
            node.nodeObject.GetComponent<Renderer>().material = node.visitedMaterial;

            foreach (Node neighbor in node.neighbors)
            {
                if (!neighbor.visited)
                {
                    neighbor.visited = true;
                    queue.Enqueue(neighbor);
                }
            }
        }

        isVisualizing = false;
    }

    void ResetNodes()
    {
        foreach (Node node in nodes)
        {
            node.visited = false;
            node.nodeObject.GetComponent<Renderer>().material = node.defaultMaterial;
        }
    }
} 