using UnityEngine;

public class MeshShape
{
    public Vector2[] m_vertices;
    public int[] m_us;
    public Vector2[] m_normals;
    public int[] m_lines;

    public virtual void Expand()
    {
        Debug.Log("Expand");
    }
}