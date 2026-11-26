using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

public static class Inspect4
{
    public static string All()
    {
        var sb = new StringBuilder();
        var verts = NavMesh.CalculateTriangulation().vertices;
        var idx = NavMesh.CalculateTriangulation().indices;

        Vector3[] anchors =
        {
            new Vector3(-124.57f, 18.16f, 8.14f),   // Enemy (5)  ponte y18
            new Vector3(-115.22f, 12.31f, 28.20f),  // Enemy (6)  ponte y12
            new Vector3(-79.36f, 12.31f, 6.60f),    // Enemy      ponte y12
            new Vector3(-189.44f, 5.56f, -0.96f),   // Enemy (13) ground y5.6
        };

        foreach (var anchor in anchors)
        {
            sb.AppendLine($"\n### ancora {anchor.ToString("F2")}");

            // triangulos do navmesh a menos de 14m
            var local = new List<int>();
            for (int t = 0; t + 2 < idx.Length; t += 3)
            {
                Vector3 a = verts[idx[t]], b = verts[idx[t + 1]], c = verts[idx[t + 2]];
                Vector3 ctr = (a + b + c) / 3f;
                if ((ctr - anchor).sqrMagnitude < 14f * 14f) local.Add(t);
            }
            var bb = new Bounds(anchor, Vector3.zero);
            foreach (var t in local)
                for (int k = 0; k < 3; k++) bb.Encapsulate(verts[idx[t + k]]);
            sb.AppendLine($"  tris<=14m={local.Count} bbox={bb.min.ToString("F1")}..{bb.max.ToString("F1")}");

            // a partir da ancora, procurar em aneis o navmesh alcancavel (PathComplete)
            var path = new NavMeshPath();
            bool found = false;
            for (float r = 1.5f; r <= 20f && !found; r += 1.5f)
            {
                for (int i = 0; i < 24; i++)
                {
                    float ang = i * 15f * Mathf.Deg2Rad;
                    Vector3 wanted = anchor + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * r;
                    // procura navmesh perto desse ponto, em varios planos de altura
                    for (float dy = -3f; dy <= 6f; dy += 1f)
                    {
                        Vector3 probe = new Vector3(wanted.x, anchor.y + dy, wanted.z);
                        if (!NavMesh.SamplePosition(probe, out var hit, 1.2f, NavMesh.AllAreas)) continue;
                        if ((hit.position - anchor).sqrMagnitude < 1.0f) continue;
                        if (!NavMesh.CalculatePath(anchor, hit.position, NavMesh.AllAreas, path)) continue;
                        if (path.status != NavMeshPathStatus.PathComplete) continue;

                        sb.AppendLine($"  ALCANCA r={r:F1} dir={(ang * Mathf.Rad2Deg):F0}deg dy={dy:F1} " +
                                      $"ponto={hit.position.ToString("F2")} alturaRel={(hit.position.y - anchor.y):F2} " +
                                      $"dist={Vector3.Distance(anchor, hit.position):F2}");
                        found = true;
                        break;
                    }
                    if (found) break;
                }
            }
            if (!found) sb.AppendLine("  NADA ALCANCAVEL num raio de 20m (ilha isolada)");
        }

        return sb.ToString();
    }
}
