using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

public static class Inspect5
{
    public static string All()
    {
        var sb = new StringBuilder();
        var path = new NavMeshPath();

        var names = new List<string>();
        var seen = new HashSet<string>();
        foreach (var t in Object.FindObjectsByType<Transform>())
        {
            string n = t.name.ToLowerInvariant();
            if (!(n.Contains("rampa") || n.Contains("ladeir"))) continue;
            if (seen.Add(t.name)) names.Add(t.name);
        }

        int ok = 0, partial = 0, invalid = 0, nosample = 0;
        var prob = new List<string>();

        foreach (var name in names)
        {
            var all = Object.FindObjectsByType<Transform>();
            var objs = new List<Transform>();
            foreach (var candidate in all) if (candidate.name == name) objs.Add(candidate);
            foreach (var t in objs)
            {
                var r = t.GetComponent<Renderer>();
                if (r == null) continue;
                var b = r.bounds;
                // eixo horizontal mais longo = comprimento da rampa
                Vector3 size = b.size;
                bool alongX = size.x >= size.z;
                Vector3 lowEnd, highEnd;

                // descobre qual das pontas é baixa olhando navmesh verticalmente
                Vector3 e1 = alongX
                    ? new Vector3(b.min.x + 0.05f, b.max.y + 2f, b.center.z)
                    : new Vector3(b.center.x, b.max.y + 2f, b.min.z + 0.05f);
                Vector3 e2 = alongX
                    ? new Vector3(b.max.x - 0.05f, b.max.y + 2f, b.center.z)
                    : new Vector3(b.center.x, b.max.y + 2f, b.max.z - 0.05f);

                bool s1 = NavMesh.SamplePosition(e1, out var h1, 3f, NavMesh.AllAreas);
                bool s2 = NavMesh.SamplePosition(e2, out var h2, 3f, NavMesh.AllAreas);

                if (!s1 || !s2) { nosample++; prob.Add($"  SEM NAVMESH nas pontas: '{name}' {b.min.ToString("F1")}..{b.max.ToString("F1")} s1={s1} s2={s2}"); continue; }
                if (h1.position.y <= h2.position.y) { lowEnd = h1.position; highEnd = h2.position; }
                else { lowEnd = h2.position; highEnd = h1.position; }

                float rise = highEnd.y - lowEnd.y;
                bool calc = NavMesh.CalculatePath(lowEnd, highEnd, NavMesh.AllAreas, path);
                string st = calc ? path.status.ToString() : "CALCFAIL";

                if (st == "PathComplete") ok++;
                else if (st == "PathPartial") partial++;
                else invalid++;

                if (st != "PathComplete")
                    prob.Add($"  '{name}' {b.min.ToString("F1")}..{b.max.ToString("F1")} rise={rise:F2} str={NavMesh.GetSettingsByID(1).agentSlope} -> {st} corners={(path.corners?.Length ?? 0)}");
            }
        }

        sb.AppendLine($"rampas testadas: ok={ok} partial={partial} invalid={invalid} semNavmesh={nosample}");
        sb.AppendLine("--- problemas ---");
        foreach (var p in prob) sb.AppendLine(p);

        return sb.ToString();
    }
}
