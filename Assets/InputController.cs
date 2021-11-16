using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using JPBotelho;

public class InputController : MonoBehaviour
{
    public Camera m_camera;
    public LayerMask m_mask;

    public Material m_material;
    
    private bool m_isDrawingMesh;
    
    private RaycastHit m_previousHit;
    private bool m_previouslyHit;
    [SerializeField]
    private float m_validationThreshold=.2f;

    private List<Vector3> m_listOfPosition = new List<Vector3>();
    private Mesh m_currentMesh;

    private CatmullRom m_currentSpline;
    private bool m_alreadyDrawingSpline;
    private MeshShape m_shape;
    private MeshFilter m_currentMeshFilter;
    private List<Vector3> m_listOfNormal = new List<Vector3>();
    public float m_width= .2f;

    public void ReactToPress(KeyCode _keyCode)
    {
        m_isDrawingMesh = true;
        InitializeDraw();
    }

    private void InitializeDraw()
    {
        m_shape = new PlaneShape(m_width);
        
        var newObject = new GameObject("mesh");
        var mr = newObject.AddComponent<MeshRenderer>();
        mr.materials = new[] {m_material};
        m_currentMeshFilter = newObject.AddComponent<MeshFilter>();
        
    }

    public void ReactToRelease(KeyCode _keyCode)
    {
        m_isDrawingMesh = false;
    }

    private void Update()
    {
        if (m_isDrawingMesh)
        {
            var mousePosition = Input.mousePosition;
            
            var ray = m_camera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out var hit, 100, m_mask))
            {
                if (m_previouslyHit)
                {
                    if (Vector3.Distance(m_previousHit.point, hit.point) > m_validationThreshold)
                    {
                        m_previousHit = hit;
                        AddPointToMeshBaseData(hit.point, hit.normal);
                        DrawMesh();
                    }
                    
                }
                else
                {
                    //First Hit
                    m_previouslyHit = true;
                    m_previousHit = hit;
                    AddPointToMeshBaseData(hit.point, hit.normal);
                    
                }
                
            }
            else
            {
                
            }

            if (m_alreadyDrawingSpline)
            {
                m_currentSpline.DrawSpline(Color.white);
                m_currentSpline.DrawNormals(1,Color.red);
                m_currentSpline.DrawTangents(1,Color.blue);
            }
        }
    }

    private void DrawMesh()
    {
        
        if (m_alreadyDrawingSpline == false) return;
        var path = m_currentSpline.GetPoints();
        
        var verticesInShape = m_shape.m_vertices.Length;
        var nbSegments = path.Length - 1;
        var edgeLoops = path.Length;

        var nbVertices = verticesInShape * edgeLoops;
        var triCount = m_shape.m_lines.Length * nbSegments;
        var trianglesIndexCount = triCount * 3;

        var triangles = new int[trianglesIndexCount];
        var vertices = new Vector3[nbVertices];
        var uvs = new Vector2[nbVertices];
        var normals = new Vector3[nbVertices];
        
        

        for (var i = 0; i < edgeLoops; i++)
        {
            var offset = i * verticesInShape;
            for (var j = 0; j < verticesInShape; j++)
            {
                var id = offset + j;
                var rotation = Quaternion.LookRotation(path[i].tangent, path[i].normal*-1);
                vertices[id] = path[i].position + rotation * m_shape.m_vertices[j];
                normals[id] = rotation* m_shape.m_normals[j];
                uvs[id] = Vector2.zero;
            }
        }

        int ti = 0;

        for (var i = 0; i < nbSegments; i++)
        {
            int offset = i * verticesInShape;
            for (int l = 0; l < m_shape.m_lines.Length; l += 2)
            {
                var a = offset + m_shape.m_lines[l] + verticesInShape;//2
                var b = offset + m_shape.m_lines[l];//0
                var c = offset + m_shape.m_lines[l + 1];//1
                var d = offset + m_shape.m_lines[l + 1] + verticesInShape;//3

                triangles[ti] = a;
                ti++;
                triangles[ti] = b;
                ti++;
                triangles[ti] = c;
                ti++;
                triangles[ti] = c;
                ti++;
                triangles[ti] = d;
                ti++;
                triangles[ti] = a;
                ti++;
            }
        }
        // Stuff drawing mesh
        
        if (m_currentMeshFilter.sharedMesh == null)
        {
            m_currentMeshFilter.sharedMesh = new Mesh();
        }
        var mesh = m_currentMeshFilter.sharedMesh;

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        m_currentMeshFilter.mesh = mesh;

    }

    private void AddPointToMeshBaseData(Vector3 _hitInfoPoint, Vector3 _hitNormal)
    {
        m_listOfPosition.Add(_hitInfoPoint);
        m_listOfNormal.Add(_hitNormal);
        
        if (m_listOfPosition.Count > 2 && m_alreadyDrawingSpline==false)
        {
            m_alreadyDrawingSpline = true;
            m_currentSpline = new CatmullRom(m_listOfPosition.ToArray(), m_listOfNormal.ToArray(), 3, false);
        }

        if (m_alreadyDrawingSpline)
        {
            UpdateSpline();
        }
    }

    private void UpdateSpline()
    {
        m_currentSpline.Update(m_listOfPosition.ToArray(),m_listOfNormal.ToArray()) ;

    }

    private void OnDrawGizmos()
    {
        /*
        if (m_listOfPosition.Count > 0)
        {
            foreach (var VARIABLE in m_listOfPosition)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(VARIABLE,.2f);
            }

        }*/
    }
}


public class PlaneShape : MeshShape
{
    public PlaneShape(float _width,float _offset =-.01f)
    {
        m_vertices = new[]
        {
            new Vector2(-_width * .5f, _offset),
            new Vector2(_width * .5f, _offset),
        };
        m_normals = new[]
        {
            Vector2.up,
            Vector2.up,
        };

        m_us = new[]
        {
            0, 0
        };
        m_lines = new[] {0, 1};
    }
}

public class DiscShape : MeshShape
{
    public DiscShape(int _faces ,float _radius)
    {
        /*m_vertices = new[]
        {
            //new Vector2(-_width * .5f, 0),
            //new Vector2(_width * .5f, 0),
        };*/
        m_normals = new[]
        {
            Vector2.up,
            Vector2.up,
        };

        m_us = new[]
        {
            0, 0
        };
        m_lines = new[] {0, 1};
    }
}

public class MeshShape
{
    public Vector2[] m_vertices;
    public int[] m_us;
    public Vector2[] m_normals;
    public int[] m_lines;
}