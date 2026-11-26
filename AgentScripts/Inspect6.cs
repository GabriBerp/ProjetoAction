using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

public static class Inspect6
{
    static TextWriterLike w;
    class TextWriterLike { public StringBuilder sb = new StringBuilder(); }

    public static string All()
    {
        var sb = new StringBuilder();
        var path = new NavMeshPath();

        var pairs = new (string name, Vector3 from, Vector3 to)[]
        {
            ("ponte_y18 -> chao",   new Vector3(-124.57f, 18.16f, 8.14f),  new Vector3(-124f, 0.5f, 10f)),
            ("ponte_y12 -> chao",   new Vector3(-115.22f, 12.31f, 28.2f),  new Vector3(-115f, 0.5f, 20f)),
            ("chao -> ponte_y18",   new Vector3(-124f, 0.5f, 10f),         new Vector3(-124.57f, 18.16f, 8.14f)),
            ("mesa -> tampo_leste", new Vector3(0f, 11.5f, 10f),           new Vector3(34f, 13.2f, 5f)),
            ("chao -> subsolo",     new Vector3(-25f, 0.2f, -10f),         new Vector3(-25f, 1.34f, -35f)),
            ("chao -> neve",        new Vector3(0f, 0.2f, 0f),             new Vector3(-130f, 0.6f, 30f)),
        };

        foreach (var (name, from, to) in pairs)
        {
            sb.AppendLine($"\n### {name}");
            if (!NavMesh.SamplePosition(from, out var a, 3f, NavMesh.AllAreas)) { sb.AppendLine("  from FORA"); continue; }
            if (!NavMesh.SamplePosition(to, out var b, 3f, NavMesh.AllAreas)) { sb.AppendLine("  to FORA"); continue; }

            bool calc = NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path);
            sb.AppendLine($"  calc={calc} status={(calc ? path.status.ToString() : "-")} corners={(path.corners?.Length ?? 0)}");
            if (path.corners != null)
            {
                for (int i = 0; i < path.corners.Length; i++)
                    sb.AppendLine($"    c{i}={path.corners[i].ToString("F2")}");
            }

            var last = path.corners != null && path.corners.Length > 0 ? path.corners[path.corners.Length - 1] : a.position;
            sb.AppendLine($"  --- objetos perto do ponto onde para ({last.ToString("F1")}) ---");
            var cols = Physics.OverlapSphere(last, 3.5f, ~0, QueryTriggerInteraction.Ignore);
            var names = new HashSet<string>();
            foreach (var c in cols)
                names.Add($"{c.name}[y {c.bounds.min.y:F1}..{c.bounds.max.y:F1}, x {c.bounds.size.x:F1} z {c.bounds.size.z:F1}]");
            foreach (var n in names) sb.AppendLine("    " + n);
        }

        return sb.ToString();
    }
}
