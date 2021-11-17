using UnityEngine;

public class DiscShape : MeshShape
{
    public DiscShape(int _faces ,float _radius)
    {
        m_vertices = new Vector2[_faces];
        m_normals = new Vector2[_faces];
        m_us = new int[_faces];

        m_lines = new int[_faces * 2];
        
        var step = 2f / _faces * Mathf.PI;
        for (var i = 0; i < _faces; i++)
        {
            var normalisedPosition = new Vector2(Mathf.Cos(step * i),  Mathf.Sin(step * i));
            m_vertices[i] = _radius * normalisedPosition;
            m_normals[i] = normalisedPosition;
        }

        for (var i = 0; i < _faces; i++)
        {
            if (i < _faces - 1)
            {
                m_lines[i*2] = i;
                m_lines[i*2 + 1] = i + 1;
            }
            else
            {
                m_lines[i*2] = i;
                m_lines[i*2 + 1] = 0;
            }
            
        }
        
    }
}