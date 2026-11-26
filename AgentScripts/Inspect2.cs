using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class Inspect2
{
    public static string All()
    {
        var sb = new StringBuilder();
        string[] keys = { "ramp", "rampa", "escada", "stair", "step", "degrau", "ponte", "bridge", "ladeir", "decliv" };

        var all = Object.FindObjectsByType<Transform>();
        var hits = new List<Transform>();
        foreach (var t in all)
        {
            string n = t.name.ToLowerInvariant();
            foreach (var k in keys)
                if (n.Contains(k)) { hits.Add(t); break; }
        }

        sb.AppendLine($"objects com nome de rampa/ponte/degrau: {hits.Count}");
        foreach (var t in hits)
        {
            var r = t.GetComponent<Renderer>();
            string b = r != null ? $" bounds={r.bounds.min.ToString("F2")}..{r.bounds.max.ToString("F2")}" : "";
            Vector3 p = t.position;
            bool on = NavMesh.SamplePosition(p, out var h1, 1.5f, NavMesh.AllAreas);
            bool onTop = NavMesh.SamplePosition(p + Vector3.up * 2f, out var h2, 2.5f, NavMesh.AllAreas);
            sb.AppendLine($"  '{t.name}' pos={p.ToString("F2")}{b} navHere={on}" +
                          (on ? $"({h1.position.y:F2})" : "") +
                          $" navTop={onTop}" + (onTop ? $"({h2.position.y:F2})" : ""));
        }

        // bbox do navmesh
        var tri = NavMesh.CalculateTriangulation();
        var bb = new Bounds(tri.vertices[0], Vector3.zero);
        foreach (var v in tri.vertices) bb.Encapsulate(v);
        sb.AppendLine($"navmesh bbox min={bb.min.ToString("F1")} max={bb.max.ToString("F1")}");

        return sb.ToString();
    }
}
