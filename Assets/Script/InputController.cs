using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using JPBotelho;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public class InputController : MonoBehaviour
{
    public Camera m_camera;
    public LayerMask m_mask;
    public LayerMask m_editMask;

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
    //private MeshShape m_shape;
    private MeshFilter m_currentMeshFilter;
    private List<Vector3> m_listOfNormal = new List<Vector3>();
    public float m_width= .2f;
    private List<IInteractable> m_interactableElements;
    public GameObject m_interactableElementPrefab;
    public static UnityEvent OnActivateEditModeEvent;
    public static UnityEvent OnDeactivateEditModeEvent;
    private bool m_isEditing;

    public MeshCreator m_meshCreator;
    private void Awake()
    {
        OnActivateEditModeEvent = new UnityEvent();
        OnDeactivateEditModeEvent = new UnityEvent();
    }

    

    public void ReactToPress(KeyCode _keyCode)
    {
        if (_keyCode == KeyCode.Space)
        {
            m_isDrawingMesh = true;
            InitializeDraw();
        }

        if (_keyCode == KeyCode.LeftCommand || _keyCode == KeyCode.LeftControl)
        {
            OnActivateEditModeEvent.Invoke();
            m_isEditing = true;
        }
        
    }

    private void InitializeDraw()
    {
        var newObject = new GameObject("mesh");
        var mr = newObject.AddComponent<MeshRenderer>();
        mr.materials = new[] {m_material};
        m_currentMeshFilter = newObject.AddComponent<MeshFilter>();
        m_interactableElements = new List<IInteractable>();

    }

    public void ReactToRelease(KeyCode _keyCode)
    {
        if (_keyCode == KeyCode.Space)
        {
            m_isDrawingMesh = false;
            m_listOfNormal = new List<Vector3>();
            m_listOfPosition = new List<Vector3>();
            m_previouslyHit = false;
        }
        
        if (_keyCode == KeyCode.LeftCommand || _keyCode == KeyCode.LeftControl)
        {
            OnDeactivateEditModeEvent.Invoke();
            m_isEditing = false;
        }

    }

    private void Update()
    {
        var mousePosition = Input.mousePosition;
        
        if (m_isEditing)
        {
            var ray = m_camera.ScreenPointToRay(mousePosition);
            DrawMesh();
            if (Input.GetMouseButtonDown(0) == false) return;
            
            if (Physics.Raycast(ray, out var clickHit, 10,m_editMask))
            {
                var point = clickHit.point;
                var colliders = Physics.OverlapSphere(point,.2f);
                foreach (var nearbyCollider in colliders)
                {
                    var script = nearbyCollider.GetComponent<InteractablePlane>();
                    if(script) script.Grow();
                }
            }
            

            
        }
        if (m_isDrawingMesh)
        {
            
            
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
        
        var shape = m_interactableElements[0].GetShape();
        var path = m_currentSpline.GetPoints();
        UpdateShapes(path);
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
            var interactableShape = interactableElement.GetComponent<IInteractable>();
            interactableShape.CreateShape(_path[i]);
            m_interactableElements.Add(interactableShape);
        }
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
}

public class MeshCreator : MonoBehaviour
{
}