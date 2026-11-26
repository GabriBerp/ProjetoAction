using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public static class LiveNav
{
    public static async Task<string> Run()
    {
        var sb = new StringBuilder();
        if (!Application.isPlaying) return "NAO ESTA EM PLAY MODE";

        var enemies = Object.FindObjectsByType<EnemyNavmesh>(FindObjectsInactive.Exclude);
        sb.AppendLine($"=== PLAY MODE: {enemies.Length} inimigos ===");

        var start = new Dictionary<EnemyNavmesh, Vector3>();
        foreach (var e in enemies) start[e] = e.transform.position;

        int frame0 = Time.frameCount;
        await Task.Delay(15000);
        float dt = (Time.frameCount - frame0) / 60f;

        int onNav = 0, moving = 0, withPath = 0, invalid = 0, partial = 0, stopped = 0, idle = 0;

        foreach (var e in enemies)
        {
            var a = e.GetComponent<NavMeshAgent>();
            if (a == null) continue;
            float moved = Vector3.Distance(start[e], e.transform.position);
            bool on = a.isOnNavMesh;
            bool hasPath = a.hasPath;
            string ps = !a.pathPending ? a.pathStatus.ToString() : "pending";

            if (on) onNav++;
            if (a.velocity.sqrMagnitude > 0.05f) moving++;
            if (hasPath) withPath++;
            if (a.pathStatus == NavMeshPathStatus.PathInvalid) invalid++;
            if (a.pathStatus == NavMeshPathStatus.PathPartial) partial++;
            if (a.isStopped) stopped++;
            if (moved < 0.3f) idle++;

            sb.AppendLine($"'{e.name}' size={e.Size} st={e.State} onNav={on} hasPath={hasPath} path={ps} " +
                          $"stopped={a.isStopped} speed={a.speed:F2} vel={a.velocity.magnitude:F2} " +
                          $"moved={moved:F2} rem={a.remainingDistance:F1} pos={e.transform.position.ToString("F1")}");
        }

        sb.AppendLine($"--- resumo ({dt:F0}s) onNav={onNav}/{enemies.Length} moving={moving} withPath={withPath} " +
                      $"invalid={invalid} partial={partial} stopped={stopped} parados(moved<0.3)={idle}");

        // testes dirigidos de subida: agente temporario do chao ate pontos altos
        sb.AppendLine("--- testes de ROTA (agente temporario) ---");
        var go = new GameObject("__navprobe");
        var probe = go.AddComponent<NavMeshAgent>();
        probe.agentTypeID = 1;
        probe.radius = 0.24f;
        probe.height = 0.5f;
        probe.speed = 4f;
        probe.updatePosition = false;
        probe.updateRotation = false;

        var routes = new (string name, Vector3 from, Vector3 to)[]
        {
            ("chao -> tampo_norte", new Vector3(0, 0.2f, 0), new Vector3(26, 13.2f, -20)),
            ("chao -> tampo_leste", new Vector3(0, 0.2f, 0), new Vector3(34, 13.2f, 5)),
            ("chao -> mesa (y12)", new Vector3(0, 0.2f, 0), new Vector3(0, 12f, 10)),
            ("chao -> subsolo", new Vector3(-25, 0.2f, -10), new Vector3(-25, 1.34f, -35)),
            ("chao -> nicho_A4", new Vector3(0, 0.2f, 0), new Vector3(34, 23.2f, 3)),
            ("chao -> ponte_A1A2", new Vector3(0, 0.2f, 0), new Vector3(15, 23.3f, -31)),
            ("mesa -> tampo_leste", new Vector3(0, 11.5f, 10), new Vector3(34, 13.2f, 5)),
            ("cozinha -> neve", new Vector3(0, 0.2f, 0), new Vector3(-130, 0.6f, 30)),
            ("cozinha -> fogao", new Vector3(0, 0.2f, 0), new Vector3(-200, 5.6f, 20)),
        };

        foreach (var (name, from, to) in routes)
        {
            bool okF = NavMesh.SamplePosition(from, out var a1, 3f, NavMesh.AllAreas);
            bool okT = NavMesh.SamplePosition(to, out var a2, 3f, NavMesh.AllAreas);
            if (!okF || !okT)
            {
                sb.AppendLine($"{name}: sample from={okF} to={okT} -> INCONCLUSIVO");
                continue;
            }
            var p = new NavMeshPath();
            bool calc = NavMesh.CalculatePath(a1.position, a2.position, NavMesh.AllAreas, p);
            sb.AppendLine($"{name}: calc={calc} status={p.status} corners={p.corners.Length} " +
                          $"dist={(p.status == NavMeshPathStatus.PathComplete ? PathLength(p).ToString("F1") : "-")}");
        }

        Object.Destroy(go);
        return sb.ToString();
    }

    static float PathLength(NavMeshPath p)
    {
        float d = 0f;
        for (int i = 1; i < p.corners.Length; i++) d += Vector3.Distance(p.corners[i - 1], p.corners[i]);
        return d;
    }
}
