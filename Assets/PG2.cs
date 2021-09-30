using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PG2 : MonoBehaviour
{
    private float time = 0.0f;
    Mesh mesh;
    int[] triangles;
    private GameObject[] agents;
    private int firstAgentVertext = -1;

    [SerializeField] private Slider agentSlider;

    [SerializeField] private GameObject agent;
    // Start is called before the first frame update
    void Start()
    {
        DrawPolygon();
    }

    // Update is called once per frame
    void Update()
    {
        moveAcrossBorder();
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
            agents[i].SetActive(true);
        }

        firstAgentVertext = -1;
    }

    void moveAcrossBorder()
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
                firstAgentVertext--;
                if (firstAgentVertext < 0)
                {
                    firstAgentVertext = agents.Length - 1;
                }
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
