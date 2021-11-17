using System;
using System.Collections;
using JPBotelho;
using UnityEngine;

public class InteractablePlane : MonoBehaviour, IInteractable
{
    private Quaternion m_rotation;
    private Vector3 m_position;
    private MeshShape m_shape;
    private SphereCollider m_collider;

    private void Awake()
    {
        m_collider = GetComponent<SphereCollider>();
        m_collider.enabled = false;
    }

    private void Start()
    {
        InputController.OnActivateEditModeEvent.AddListener(ActivateCollider);
        InputController.OnDeactivateEditModeEvent.AddListener(DeactivateCollider);
    }

    private void DeactivateCollider()
    {
        m_collider.enabled = false;
    }

    private void ActivateCollider()
    {
        m_collider.enabled = true;
    }


    public void CreateShape(CatmullRom.CatmullRomPoint _catmullRomPoint)
    {
        m_shape = new PlaneShape(.12f);
        m_rotation = Quaternion.LookRotation(_catmullRomPoint.tangent, _catmullRomPoint.normal*-1);
        m_position = _catmullRomPoint.position;
    }

    public Vector3 GetVertexPosition(int _i)
    {
        return m_position + m_rotation * m_shape.m_vertices[_i];
    }

    public Vector3 GetNormal(int _i)
    {
        return m_rotation* m_shape.m_normals[_i];
    }

    public MeshShape GetShape()
    {
        return m_shape;
    }

    public void Grow()
    {
        StartCoroutine(nameof(GrowCoroutine));
    }

    private IEnumerator GrowCoroutine()
    {
        float time = 0;
        float step = Time.deltaTime;

        while (time < .5f)
        {
            time += step;
            m_shape.Expand();
            yield return null;
        }
    }
}