using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PG2 : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    [SerializeField] private Slider agentSlider;
    
    // Start is called before the first frame update
    void Start()
    {
        DrawPolygon();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    public void DrawPolygon()
    {
        int sides = (int)(agentSlider.value);
        Debug.Log(sides);
        Debug.Log("hi");
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
        mesh.RecalculateNormals();
    }

    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir  = point - pivot;
        dir = Quaternion.Euler(angles) * dir;
        Vector3 dest = dir + pivot;
        return dest;
    }
}