using UnityEngine;
using System.Collections;

public class DrawStuff
{
    public static void DrawLine(Vector3 p1, Vector3 p2, Color col)
    {
        GL.Begin(GL.LINES);
        GL.Color(col);
        GL.Vertex3(p1.x, p1.y, p1.z);
        GL.Vertex3(p2.x, p2.y, p2.z);
        GL.End();
    }

    public static void DrawQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color col)
    {
        GL.Begin(GL.QUADS);
        GL.Color(col);
        GL.Vertex3(p1.x, p1.y, p1.z);
        GL.Vertex3(p2.x, p2.y, p2.z);
        GL.Vertex3(p3.x, p3.y, p3.z);
        GL.Vertex3(p4.x, p4.y, p4.z);
        GL.End();
    }

    public static void DrawFrame(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color col)
    {
        GL.Begin(GL.LINES);
        GL.Color(col);
        GL.Vertex3(p1.x, p1.y, p1.z);
        GL.Vertex3(p2.x, p2.y, p2.z);
        GL.Vertex3(p2.x, p2.y, p2.z);
        GL.Vertex3(p3.x, p3.y, p3.z);
        GL.Vertex3(p3.x, p3.y, p3.z);
        GL.Vertex3(p4.x, p4.y, p4.z);
        GL.Vertex3(p4.x, p4.y, p4.z);
        GL.Vertex3(p1.x, p1.y, p1.z);
        GL.End();
    }

    public static void AddTextMesh(GameObject parent, float x, float y, string name)
    {
        var textGO = new GameObject();
        textGO.transform.parent = parent.transform;
        textGO.transform.position = new Vector3(x + 0.5f, y + 1.0f, 0.0f);
        var textMesh = textGO.AddComponent<TextMesh>();
        textMesh.text = name;
        textMesh.color = Color.white;
        textMesh.fontSize = 10;
        textMesh.anchor = TextAnchor.MiddleLeft;
    }
}
