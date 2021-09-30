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
    private int firstAgentVertext = -1;
    private MoveType moveType = MoveType.BORDER;
    private Vector3[] midways = null;
    private float toRotate = 180f;
    [SerializeField] private Slider agentSlider;

    [SerializeField] private GameObject agent;

    private Color[] colors = new Color[]
    {
        Color.cyan, Color.magenta, Color.red, Color.yellow, Color.black, 
        Color.blue, Color.green, Color.white, Color.gray, new Color(100,200, 0), 
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
        move();
    }

    void UpdateMesh()
    {

    }

    public void DrawPolygon()
    {
        int sides = (int)(agentSlider.value);
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
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
        mesh.RecalculateNormals();
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

        firstAgentVertext = -1;
        moveType = MoveType.BORDER;
    }

    void moveBorder()
    {
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        if (agents == null || vertices == null)
        {
            return;
        }

        if (firstAgentVertext == -1)
        {
            firstAgentVertext = agents.Length - 1;
        }
        
        for(int i =0;i<agents.Length; i++)
        {
            GameObject thisAgent = agents[i];
            int targetVertex = (firstAgentVertext + i)%agents.Length;
            Vector3 target = vertices[targetVertex];
            target = GetComponent<MeshFilter>().transform.TransformPoint(target);
            if (thisAgent.transform.position == target)
            {
                firstAgentVertext = (firstAgentVertext + (agents.Length / 2)) % agents.Length;
                setMidways();
                moveType = MoveType.CENTER;
            }
            else
            {
                thisAgent.transform.position = Vector3.MoveTowards(thisAgent.transform.position, target, Time.deltaTime);                
            }
        }        
    }

    void setMidways()
    {
        midways = new Vector3[agents.Length];
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        if (agents == null || vertices == null)
        {
            return;
        }

        Vector3 target = GetComponent<MeshFilter>().transform.position;
        for(int i =0;i<agents.Length; i++)
        {
            GameObject thisAgent = agents[i];
            Vector3 thisAgentPosition = thisAgent.transform.position;
            //midways[i] = Vector3.Lerp(thisAgentPosition, target, 0.7f);
            float radius = thisAgent.GetComponent<SphereCollider>().radius * thisAgent.transform.transform.localScale.x;
            float innerRadius = (agents.Length * radius ) / (float)Math.PI;
            Vector3 direction = (thisAgentPosition - target).normalized;
            midways[i] = direction * innerRadius;
        }        
    }

    void moveCenter()
    {
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        if (agents == null || vertices == null)
        {
            return;
        }        
        for(int i =0;i<agents.Length; i++)
        {
            GameObject thisAgent = agents[i];
            Vector3 target = midways[i];
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

    void move()
    {
        Debug.Log(moveType);
        if (moveType == MoveType.BORDER)
        {
            moveBorder();
        }
        else if (moveType == MoveType.CENTER)
        {
            moveCenter();
        }
        else if (moveType == MoveType.ROTATE)
        {
            moveRotation();
        }
        else//CORNER
        {
            moveCorner();
        }
    }

    void moveRotation()
    {
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        Vector3 pivot = GetComponent<MeshFilter>().transform.position;
        if (agents == null || vertices == null)
        {
            return;
        }

        if (toRotate <= 0)
        {
            moveType = MoveType.CORNER;
            return;
        }
        
        for(int i =0;i<agents.Length; i++)
        {
            GameObject thisAgent = agents[i];
            Vector3 thisAgentPosition = thisAgent.transform.position;
            Vector3 angle = new Vector3(0, 0, 0.2f);
            Vector3 direction = RotatePointAroundPivot(thisAgentPosition, pivot, angle);
            thisAgent.transform.position = Vector3.MoveTowards(thisAgentPosition, direction, Time.deltaTime*10);  
        }

        toRotate = toRotate - 0.2f;
    }
    
    void moveCorner()
    {
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        if (agents == null || vertices == null)
        {
            return;
        }

        for(int i =0;i<agents.Length; i++)
        {
            GameObject thisAgent = agents[i];
            int targetVertex = (firstAgentVertext + i)%agents.Length;
            Vector3 target = vertices[targetVertex];
            target = GetComponent<MeshFilter>().transform.TransformPoint(target);
            if (thisAgent.transform.position == target)
            {
                firstAgentVertext = firstAgentVertext - 1;
                if (firstAgentVertext < 0)
                {
                    firstAgentVertext = agents.Length - 1;
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

    static int randomColorInt()
    {
        return (int) (new Random().Next(255));
    }
}