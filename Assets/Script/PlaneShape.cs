using UnityEngine;

public class PlaneShape : MeshShape
{
    private float m_width;
    private float m_offset;

    public PlaneShape(float _width,float _offset =-.01f)
    {
        m_width = _width;
        m_offset = _offset;
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

    public override void Expand()
    {
        m_width += .2f*0.01f;
        m_vertices = new[]
        {
            new Vector2(-m_width * .5f, m_offset),
            new Vector2(m_width * .5f, m_offset),
        };
    }
}