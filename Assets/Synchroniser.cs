using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class Synchroniser : MonoBehaviour
{
    Mesh mesh;
    int[] triangles;
    private GameObject[] agents;
    private int targetVertexForFirstAgent = -1;
    private MoveType moveType = MoveType.BORDER;
    private Vector3[] pointsOnInnerCircle = null;
    private float toRotate = 180f;
    [SerializeField] private Slider agentSlider;
    [SerializeField] private Slider innerSlider;
    [SerializeField] private GameObject agent;
    private float innerRadiusMultiplier = 1;
    
    private Color[] colors = new Color[]
    {
        Color.cyan, Color.magenta, Color.red, Color.yellow, Color.black, 
        Color.blue, Color.green, Color.white, Color.gray, new Color(0,100, 100),  
        new Color(56,200, 0), new Color(50,150, 50), new Color(100,0, 0), new Color(0,200, 0), 
        new Color(100,200, 56), new Color(0,0, 200), new Color(0,0, 255), new Color(0,200, 80),
        new Color(14,167, 55), new Color(0,156, 67), new Color(2,32, 0), new Color(32,200, 40)

    };
    
    // Start is called before the first frame update
    void Start()
    {
        DrawPolygon();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    float GetPolygonRadius()
    {
        Vector3 firstVertex = GetVertices()[0];
        Vector3 worldPt = GetComponent<MeshFilter>().transform.TransformPoint(firstVertex);        
        return Vector3.Distance(worldPt, GetCenter());
    }

    float GetAgentRadius()
    {
        return agents[0].GetComponent<SphereCollider>().radius * agents[0].transform.transform.localScale.x;        
    }

    Vector3 GetCenter()
    {
        return GetComponent<MeshFilter>().transform.position;
    }

    int GetSides()
    {
        return (int)(agentSlider.value);
    }

    Vector3[] GetVertices()
    {
        return GetComponent<MeshFilter>().mesh.vertices;
    }

    public void DrawPolygon()
    {
        int sides = GetSides(); 
        Vector3[] vertices = GetVertices();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.Clear();
        Vector3 startPoint = vertices[0];
        startPoint.z = 0;
        Vector3 pivot = transform.position;
        vertices = new Vector3[sides];
        triangles = new int[(sides - 2)*3];
        vertices[0] = startPoint;
        for (int i = 1; i < sides ; i++)
        {
            Vector3 angle = new Vector3(0, 0, -360 / sides);
            startPoint = RotatePointAroundPivot(startPoint, pivot, angle);
            vertices[i] = startPoint;
        }

        int triangleIndex = 0;
        triangles[triangleIndex++] = 0;
        triangles[triangleIndex++] = 1;
        triangles[triangleIndex++] = 2;
        
        for (int i = 3; i < sides; i++)
        {
            triangles[triangleIndex++] = 0;
            triangles[triangleIndex++] = i-1;
            triangles[triangleIndex++] = i;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        CreateAgents();
        innerSlider.minValue = 1;
        innerSlider.maxValue = (GetPolygonRadius() / GetInnerRadius()) / 2;
        innerSlider.value = innerSlider.minValue;
        mesh.RecalculateNormals();
    }

    public void AdjustBoundingBox()
    {
        innerRadiusMultiplier = innerSlider.value;
    }

    void CreateAgents()
    {
        if (agents != null)
        {
            foreach (GameObject agent in agents)
            {
                Destroy(agent);
            }            
        }

        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        agents = new GameObject[vertices.Length];
        for (int i =0; i<vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector3 worldPt = GetComponent<MeshFilter>().transform.TransformPoint(vertex);
            agents[i] = Instantiate(agent);
            agents[i].transform.position = worldPt;
            agents[i].GetComponent<Renderer>().material.color = colors[i];
            agents[i].SetActive(true);
        }

        targetVertexForFirstAgent = -1;
        moveType = MoveType.BORDER;
    }

    void MoveAlongBorder()
    {
        Vector3[] vertices = GetVertices();

        if (targetVertexForFirstAgent == -1)//This is the start
        {
            targetVertexForFirstAgent = agents.Length - 1;
        }
        
        for(int i =0;i<agents.Length; i++)
        {
            GameObject thisAgent = agents[i];
            int targetVertex = (targetVertexForFirstAgent + i)%agents.Length;
            Vector3 target = vertices[targetVertex];
            target = GetComponent<MeshFilter>().transform.TransformPoint(target);
            if (thisAgent.transform.position == target)
            {
                targetVertexForFirstAgent = (targetVertexForFirstAgent + (agents.Length / 2)) % agents.Length;
                SetBoundinBox();
                moveType = MoveType.CENTER;
            }
            else
            {
                thisAgent.transform.position = Vector3.MoveTowards(thisAgent.transform.position, target, Time.deltaTime);                
            }
        }        
    }

    void SetBoundinBox()
    {
        pointsOnInnerCircle = new Vector3[agents.Length];
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        if (agents == null || vertices == null)
        {
            return;
        }

        Vector3 target = GetCenter();
        for(int i =0;i<agents.Length; i++)
        {
            GameObject thisAgent = agents[i];
            Vector3 thisAgentPosition = thisAgent.transform.position;
            float innerRadius = GetInnerRadius() * innerRadiusMultiplier;
            Vector3 direction = (thisAgentPosition - target).normalized;
            pointsOnInnerCircle[i] = direction * innerRadius;
        }        
    }

    float GetInnerRadius()
    {
        float radius = GetAgentRadius();
        return (agents.Length * radius ) / (float)Math.PI;
    }

    void MoveTowardCenter()
    {
        Vector3[] vertices = GetVertices();
        for(int i =0;i<agents.Length; i++)
        {
            GameObject thisAgent = agents[i];
            Vector3 target = pointsOnInnerCircle[i];
            if (thisAgent.transform.position == target)
            {
                toRotate = 180f;
                moveType = MoveType.ROTATE;
            }
            else
            {
                thisAgent.transform.position = Vector3.MoveTowards(thisAgent.transform.position, target, Time.deltaTime);                
            }
        }          
    }

    void Move()
    {
        Debug.Log(moveType);
        if (agents == null || GetVertices() == null)
        {
            return;
        }        
        if (moveType == MoveType.BORDER)
        {
            MoveAlongBorder();
        }
        else if (moveType == MoveType.CENTER)
        {
            MoveTowardCenter();
        }
        else if (moveType == MoveType.ROTATE)
        {
            MoveByRotation();
        }
        else//CORNER
        {
            MoveTowardCorner();
        }
    }

    void MoveByRotation()
    {
        Vector3 pivot = GetCenter();
        float rotationPerPass = 0.2f;
        if (toRotate <= 0)
        {
            moveType = MoveType.CORNER;
            return;
        }
        
        for(int i =0;i<agents.Length; i++)
        {
            GameObject thisAgent = agents[i];
            Vector3 thisAgentPosition = thisAgent.transform.position;
            Vector3 angle = new Vector3(0, 0, rotationPerPass);
            Vector3 direction = RotatePointAroundPivot(thisAgentPosition, pivot, angle);
            thisAgent.transform.position = Vector3.MoveTowards(thisAgentPosition, direction, Time.deltaTime*10);  
        }

        toRotate = toRotate - rotationPerPass;
    }
    
    void MoveTowardCorner()
    {
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        if (agents == null || vertices == null)
        {
            return;
        }

        for(int i =0;i<agents.Length; i++)
        {
            GameObject thisAgent = agents[i];
            int targetVertex = (targetVertexForFirstAgent + i)%agents.Length;
            Vector3 target = vertices[targetVertex];
            target = GetComponent<MeshFilter>().transform.TransformPoint(target);
            if (thisAgent.transform.position == target)
            {
                targetVertexForFirstAgent = targetVertexForFirstAgent - 1;
                if (targetVertexForFirstAgent < 0)
                {
                    targetVertexForFirstAgent = agents.Length - 1;
                }    
                moveType = MoveType.BORDER;
            }
            else
            {
                thisAgent.transform.position = Vector3.MoveTowards(thisAgent.transform.position, target, Time.deltaTime);                
            }
        }          
    }    
    
    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir  = point - pivot;
        dir = Quaternion.Euler(angles) * dir;
        Vector3 dest = dir + pivot;
        return dest;
    }

}
