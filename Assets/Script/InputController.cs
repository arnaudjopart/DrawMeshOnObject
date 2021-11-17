
using System;
using System.Collections.Generic;
using UnityEngine;
using JPBotelho;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class InputController : MonoBehaviour
{
    [Header("Cameras & Masks")]
    public Camera m_camera;
    public LayerMask m_mask;
    public LayerMask m_editMask;

    [FormerlySerializedAs("m_DrawVirtualCamera")] public GameObject m_drawVirtualCamera;

    [Header("Settings")]
    [SerializeField]
    private float m_validationThreshold=.2f;
    public enum STATE
    {
        IDLE,
        DRAWING,
        EDITING
    };

    private STATE m_currentState = STATE.IDLE;

    private RaycastHit m_previousHit;
    private bool m_previouslyHit;
    
    public static UnityEvent OnActivateEditModeEvent;
    public static UnityEvent OnDeactivateEditModeEvent;
    

    public MeshCreator m_meshCreator;
    private void Awake()
    {
        OnActivateEditModeEvent = new UnityEvent();
        OnDeactivateEditModeEvent = new UnityEvent();
    }

    public void ReactToPress(KeyCode _keyCode)
    {
        switch (_keyCode)
        {
            case KeyCode.Space:
                m_currentState = STATE.DRAWING;
                m_drawVirtualCamera.SetActive(true);
                InitializeDraw();
                break;
            case KeyCode.LeftCommand:
            case KeyCode.LeftControl:
                m_drawVirtualCamera.SetActive(true);
                OnActivateEditModeEvent.Invoke();
                m_currentState = STATE.EDITING;
                break;
        }
    }

    private void InitializeDraw()
    {
        m_meshCreator.SetupNewMesh();
    }

    public void ReactToRelease(KeyCode _keyCode)
    {
        switch (_keyCode)
        {
            case KeyCode.Space:
                m_previouslyHit = false;
                m_drawVirtualCamera.SetActive(false);
                m_meshCreator.CompleteMeshCreation();
                m_currentState = STATE.IDLE;
                break;
            case KeyCode.LeftCommand:
            case KeyCode.LeftControl:
                m_drawVirtualCamera.SetActive(false);
                OnDeactivateEditModeEvent.Invoke();
                m_currentState = STATE.IDLE;
                break;
        }
    }

    private void Update()
    {
        switch (m_currentState)
        {
            case STATE.IDLE:
                break;
            case STATE.DRAWING:
                ManageMeshDrawing();
                break;
            case STATE.EDITING:
                ManageMeshEdition();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ManageMeshEdition()
    {
        var mousePosition = Input.mousePosition;
        
        var ray = m_camera.ScreenPointToRay(mousePosition);
        m_meshCreator.DrawMesh();
            
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

    private void ManageMeshDrawing()
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
                    m_meshCreator.AddPointToMeshBaseData(hit.point, hit.normal);
                    m_meshCreator.DrawMesh();
                }
                    
            }
            else
            {
                //First Hit
                m_previouslyHit = true;
                m_previousHit = hit;
                m_meshCreator.AddPointToMeshBaseData(hit.point, hit.normal);
                    
            }
                
        }
        m_meshCreator.DrawSpline();
    }
}