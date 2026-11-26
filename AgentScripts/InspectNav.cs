using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.SceneManagement;

public static class InspectNav
{
    public static string All()
    {
        var sb = new StringBuilder();
        var scene = SceneManager.GetActiveScene();
        sb.AppendLine($"scene: {scene.name} path={scene.path} loaded={scene.isLoaded}");

        // ---- superfícies de navegação ----
        var surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);
        sb.AppendLine($"NavMeshSurfaces: {surfaces.Length}");
        foreach (var s in surfaces)
        {
            sb.AppendLine($"  '{s.name}' enabled={s.enabled} agentTypeID={s.agentTypeID} " +
                          $"collect={s.collectObjects} useGeometry={s.useGeometry} " +
                          $"defaultArea={s.defaultArea} " +
                          $"size={s.size} center={s.center} " +
                          $"tile={s.tileSize} voxel={s.voxelSize}");
        }

        // ---- settings de agente ----
        for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
        {
            var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
            var st = NavMesh.GetSettingsByID(id);
            sb.AppendLine($"agentType {id} '{NavMesh.GetSettingsNameFromID(id)}' r={st.agentRadius} h={st.agentHeight} " +
                          $"climb={st.agentClimb} slope={st.agentSlope} " +
                          $"voxel={st.voxelSize} minRegion={st.minRegionArea}");
        }

        // ---- componentes conexos do navmesh (ilhas) ----
        var tri = NavMesh.CalculateTriangulation();
        sb.AppendLine($"navmesh verts={tri.vertices.Length} tris={tri.indices.Length / 3} areas={tri.areas.Length}");
        int islands = CountIslands(tri, out var islandInfo);
        sb.AppendLine($"islands: {islands}");
        foreach (var line in islandInfo) sb.AppendLine(line);

        // ---- nós de patrulha e inimigos ----
        var nodes = Object.FindObjectsByType<PatrolNode>(FindObjectsSortMode.None);
        sb.AppendLine($"PatrolNodes: {nodes.Length}");
        foreach (var n in nodes)
        {
            bool on = NavMesh.SamplePosition(n.transform.position, out var hit, 2f, NavMesh.AllAreas);
            sb.AppendLine($"  node '{n.name}' pos={n.transform.position} onNav={on}" +
                          (on ? $" snapped={hit.position} dy={n.transform.position.y - hit.position.y:F2}" : ""));
        }

        var enemies = Object.FindObjectsByType<EnemyNavmesh>(FindObjectsSortMode.None);
        sb.AppendLine($"Enemies: {enemies.Length}");
        foreach (var e in enemies)
        {
            var a = e.GetComponent<NavMeshAgent>();
            sb.AppendLine($"  enemy '{e.name}' size={e.Size} pos={e.transform.position} scale={e.transform.localScale} " +
                          $"agentType={a.agentTypeID} r={a.radius} h={a.height} climb={a.baseOffset} " +
                          $"speed={a.speed} onNav={a.isOnNavMesh} area={a.areaMask}");
        }

        return sb.ToString();
    }

    static int CountIslands(NavMeshTriangulation tri, out List<string> info)
    {
        info = new List<string>();
        int n = tri.vertices.Length;
        int[] parent = new int[n];
        for (int i = 0; i < n; i++) parent[i] = i;

        int Find(int x) { while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; } return x; }
        void Union(int a, int b) { int ra = Find(a), rb = Find(b); if (ra != rb) parent[rb] = ra; }

        for (int i = 0; i + 2 < tri.indices.Length; i += 3)
        {
            Union(tri.indices[i], tri.indices[i + 1]);
            Union(tri.indices[i], tri.indices[i + 2]);
        }

        var map = new Dictionary<int, (int tris, Bounds bounds, int area)>();
        for (int i = 0; i + 2 < tri.indices.Length; i += 3)
        {
            int root = Find(tri.indices[i]);
            var v0 = tri.vertices[tri.indices[i]];
            var v1 = tri.vertices[tri.indices[i + 1]];
            var v2 = tri.vertices[tri.indices[i + 2]];

            if (!map.TryGetValue(root, out var e))
            {
                var b = new Bounds(v0, Vector3.zero);
                b.Encapsulate(v1); b.Encapsulate(v2);
                map[root] = (1, b, tri.areas[i / 3]);
            }
            else
            {
                e.bounds.Encapsulate(v0); e.bounds.Encapsulate(v1); e.bounds.Encapsulate(v2);
                map[root] = (e.tris + 1, e.bounds, e.area);
            }
        }

        foreach (var kv in map)
            info.Add($"  island root={kv.Key} tris={kv.Value.tris} area={kv.Value.area} " +
                     $"min={kv.Value.bounds.min.ToString("F1")} max={kv.Value.bounds.max.ToString("F1")} " +
                     $"center={kv.Value.bounds.center.ToString("F1")}");

        return map.Count;
    }
}
