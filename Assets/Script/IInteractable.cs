using JPBotelho;
using UnityEngine;

internal interface IInteractable
{
    void CreateShape(CatmullRom.CatmullRomPoint _catmullRomPoint);
    Vector3 GetVertexPosition(int _i);
    Vector3 GetNormal(int _i);
    MeshShape GetShape();
}