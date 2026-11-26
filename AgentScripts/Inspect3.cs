using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

public static class Inspect3
{
    public static string All()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"playing={Application.isPlaying}");

        var enemies = Object.FindObjectsByType<EnemyNavmesh>(FindObjectsInactive.Exclude);

        foreach (var e in enemies)
        {
            var a = e.GetComponent<NavMeshAgent>();
            if (a == null) continue;
            if (a.pathStatus != NavMeshPathStatus.PathPartial && a.hasPath) continue;

            sb.AppendLine($"\n=== '{e.name}' pos={e.transform.position.ToString("F2")} path={a.pathStatus} hasPath={a.hasPath} dest={a.destination.ToString("F2")}");

            if (a.path != null && a.path.corners.Length > 0)
            {
                var c = a.path.corners;
                int last = c.Length - 1;
                sb.AppendLine($"  corners={c.Length} last={c[last].ToString("F2")} " +
                              $"gapToDest={Vector3.Distance(c[last], a.destination):F2} " +
                              $"dyToDest={a.destination.y - c[last].y:F2}");
                if (c.Length > 1)
                {
                    sb.AppendLine($"  corner[1]={c[1].ToString("F2")} dy={c[1].y - e.transform.position.y:F2}  " +
                                  $"corner[last-1]={c[Mathf.Max(0, last - 1)].ToString("F2")}");
                }
            }

            // quantos nodulos de patrulha ele alcanca
            int total = 0, complete = 0, partial = 0, invalid = 0;
            var p = new NavMeshPath();
            foreach (var n in PatrolNode.All)
            {
                if (n == null) continue;
                Vector3 np = n.transform.position;
                Vector3 f = np - e.transform.position; f.y = 0f;
                if (f.magnitude > 40f) continue;
                total++;
                if (!NavMesh.CalculatePath(e.transform.position, np, NavMesh.AllAreas, p)) { invalid++; continue; }
                if (p.status == NavMeshPathStatus.PathComplete) complete++;
                else if (p.status == NavMeshPathStatus.PathPartial) partial++;
                else invalid++;
            }
            sb.AppendLine($"  nodulos<=40m: total={total} complete={complete} partial={partial} invalid={invalid}");

            // geometria embaixo/perto: o que ha ali
            var cols = Physics.OverlapSphere(e.transform.position, 3f, ~0, QueryTriggerInteraction.Ignore);
            var names = new HashSet<string>();
            foreach (var c in cols)
            {
                if (c.transform.IsChildOf(e.transform)) continue;
                names.Add($"{c.name}({c.bounds.max.y:F2})");
            }
            sb.AppendLine($"  colliders<=3m: {string.Join(", ", names)}");

            // navmesh livre acima/abaixo
            bool up = NavMesh.SamplePosition(e.transform.position + Vector3.up * 2f, out var hu, 3f, NavMesh.AllAreas);
            bool dn = NavMesh.SamplePosition(e.transform.position + Vector3.down * 2f, out var hd, 3f, NavMesh.AllAreas);
            sb.AppendLine($"  navUp={up}{(up ? $"({hu.position.y:F2})" : "")} navDown={dn}{(dn ? $"({hd.position.y:F2})" : "")}");
        }

        return sb.ToString();
    }
}
