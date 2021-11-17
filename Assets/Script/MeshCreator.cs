using System.Collections.Generic;
using JPBotelho;
using UnityEngine;

public class MeshCreator : MonoBehaviour
{
    public Material m_material;
    private MeshFilter m_currentMeshFilter;
    private List<IInteractable> m_interactableElements;
    public GameObject m_interactableElementPrefab;
    private List<Vector3> m_listOfNormal;
    private List<Vector3> m_listOfPosition;
    private CatmullRom m_currentSpline;
    private bool m_alreadyDrawingSpline;

    private GameObject m_newObject;
    private int m_index;

    public void SetupNewMesh()
    {
        m_newObject = new GameObject("mesh_" + m_index);
        m_index++;
        var mr = m_newObject.AddComponent<MeshRenderer>();
        mr.materials = new[] {m_material};
        m_currentMeshFilter = m_newObject.AddComponent<MeshFilter>();
        m_interactableElements = new List<IInteractable>();
        
        m_listOfNormal = new List<Vector3>();
        m_listOfPosition = new List<Vector3>();
    }

    public void CompleteMeshCreation()
    {
        m_listOfNormal = new List<Vector3>();
        m_listOfPosition = new List<Vector3>();
    }

    public void DrawMesh()
    {
        
        var path = m_currentSpline.GetPoints();
        UpdateShapes(path);
        var shape = m_interactableElements[0].GetShape();
        var verticesInShape = shape.m_vertices.Length;
        var nbSegments = path.Length - 1;
        var edgeLoops = path.Length;

        var nbVertices = verticesInShape * edgeLoops;
        var triCount = shape.m_lines.Length * nbSegments;
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
                
                vertices[id] = m_interactableElements[i].GetVertexPosition(j);
                normals[id] = m_interactableElements[i].GetNormal(j);
                uvs[id] = Vector2.zero;
            }
        }

        int ti = 0;

        for (var i = 0; i < nbSegments; i++)
        {
            int offset = i * verticesInShape;
            for (int l = 0; l < shape.m_lines.Length; l += 2)
            {
                var a = offset + shape.m_lines[l] + verticesInShape;//2
                var b = offset + shape.m_lines[l];//0
                var c = offset + shape.m_lines[l + 1];//1
                var d = offset + shape.m_lines[l + 1] + verticesInShape;//3

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

    private void UpdateShapes(CatmullRom.CatmullRomPoint[] _path)
    {
        var missingElements = _path.Length - m_interactableElements.Count;
        for (var i = _path.Length-missingElements; i < _path.Length; i++)
        {
            var interactableElement = Instantiate(m_interactableElementPrefab, _path[i].position, Quaternion.identity);
            interactableElement.transform.SetParent(m_newObject.transform);
            
            var interactableShape = interactableElement.GetComponent<IInteractable>();
            interactableShape.CreateShape(_path[i]);
            m_interactableElements.Add(interactableShape);
        }
    }

    public void AddPointToMeshBaseData(Vector3 _hitInfoPoint, Vector3 _hitNormal)
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

    public void DrawSpline()
    {
        if (m_alreadyDrawingSpline == false) return;
        m_currentSpline.DrawSpline(Color.white);
        m_currentSpline.DrawNormals(1,Color.red);
        m_currentSpline.DrawTangents(1,Color.blue);
    }
}