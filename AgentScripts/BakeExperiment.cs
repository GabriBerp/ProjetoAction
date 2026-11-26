using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public static class BakeExperiment
{
    static readonly (string name, Vector3 from, Vector3 to)[] Routes =
    {
        ("chao -> tampo_leste", new Vector3(0f, 0.3f, 0f), new Vector3(34f, 13.2f, 5f)),
        ("chao -> mesa", new Vector3(0f, 0.3f, 0f), new Vector3(0f, 11.5f, 10f)),
        ("chao -> subsolo", new Vector3(-25f, 0.3f, -10f), new Vector3(-25f, 1.3f, -35f)),
        ("chao -> neve", new Vector3(0f, 0.3f, 0f), new Vector3(-130f, 0.6f, 30f)),
        ("chao -> ponte_y18", new Vector3(-124f, 0.5f, 10f), new Vector3(-124.6f, 18.2f, 8.1f)),
        ("chao -> fogao", new Vector3(0f, 0.3f, 0f), new Vector3(-200f, 5.6f, 20f)),
    };

    public static string Run()
    {
        var sb = new StringBuilder();
        var surface = Object.FindFirstObjectByType<NavMeshSurface>();
        if (surface == null) return "sem NavMeshSurface";

        var baseSettings = NavMesh.GetSettingsByID(1);
        sb.AppendLine($"base: r={baseSettings.agentRadius} h={baseSettings.agentHeight} climb={baseSettings.agentClimb} slope={baseSettings.agentSlope} voxel={baseSettings.voxelSize}");

        var markups = new List<NavMeshBuildMarkup>();
        var sources = new List<NavMeshBuildSource>();
        Bounds bounds = new Bounds(surface.transform.position + surface.center, surface.size * 100f);
        NavMeshBuilder.CollectSources(surface.transform, surface.layerMask, surface.useGeometry,
                                      surface.defaultArea, markups, sources);
        sb.AppendLine($"sources coletadas: {sources.Count}");

        var variants = new (string name, float climb, float slope, float radius, float voxel, float minRegion)[]
        {
            ("atual",      1.0f, 60f, 0.24f, 0.10f, 0.10f),
            ("climb1.6",   1.6f, 60f, 0.24f, 0.10f, 0.10f),
            ("climb2.0",   2.0f, 65f, 0.24f, 0.10f, 0.10f),
            ("raiox0.14",  2.0f, 65f, 0.14f, 0.08f, 0.08f),
        };

        foreach (var v in variants)
        {
            var s = baseSettings;
            s.agentClimb = v.climb;
            s.agentSlope = v.slope;
            s.agentRadius = v.radius;
            s.voxelSize = v.voxel;
            s.minRegionArea = v.minRegion;
            s.overrideVoxelSize = true;

            NavMeshData data;
            try { data = NavMeshBuilder.BuildNavMeshData(s, sources, bounds, Vector3.zero, Quaternion.identity); }
            catch (System.Exception e) { sb.AppendLine($"{v.name}: FALHOU {e.Message}"); continue; }

            if (data == null) { sb.AppendLine($"{v.name}: data nulo"); continue; }

            NavMesh.RemoveAllNavMeshData();
            var inst = NavMesh.AddNavMeshData(data);

            sb.AppendLine($"\n--- {v.name} (climb={v.climb} slope={v.slope} r={v.radius}) ---");
            foreach (var (name, from, to) in Routes)
            {
                bool okF = NavMesh.SamplePosition(from, out var a, 4f, NavMesh.AllAreas);
                bool okT = NavMesh.SamplePosition(to, out var b, 4f, NavMesh.AllAreas);
                if (!okF || !okT) { sb.AppendLine($"  {name}: sample={okF}/{okT}"); continue; }
                var p = new NavMeshPath();
                bool calc = NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, p);
                float len = 0f;
                if (calc && p.status == NavMeshPathStatus.PathComplete)
                    for (int i = 1; i < p.corners.Length; i++) len += Vector3.Distance(p.corners[i - 1], p.corners[i]);
                sb.AppendLine($"  {name}: {p.status} corners={p.corners.Length} dist={len:F1}");
            }

            NavMesh.RemoveNavMeshData(inst);
            Object.Destroy(data);
        }

        // restaura o navmesh da cena
        NavMesh.RemoveAllNavMeshData();
        surface.BuildNavMesh();
        sb.AppendLine("\nnavmesh da cena reconstruido");
        return sb.ToString();
    }
}
