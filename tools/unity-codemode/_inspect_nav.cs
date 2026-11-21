using System.Text;
using UnityEngine;
using UnityEngine.AI;

var sb = new StringBuilder();
sb.AppendLine($"settings: {NavMesh.GetSettingsCount()}");
for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
{
    var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
    var s = NavMesh.GetSettingsByID(id);
    sb.AppendLine($"  agent {id}: r={s.agentRadius} h={s.agentHeight} climb={s.agentClimb} slope={s.agentSlope}");
}

var pts = new (string, Vector3)[]
{
    ("chao", new Vector3(0, 1, 0)),
    ("chao_oeste", new Vector3(-25, 1, -10)),
    ("tampo_norte", new Vector3(26, 13.2f, -20)),
    ("tampo_leste", new Vector3(34, 13.2f, 5)),
    ("nicho_A4", new Vector3(34, 23.2f, 3)),
    ("nicho_A2", new Vector3(27, 23.2f, -35)),
    ("ponte_A1A2", new Vector3(15, 23.3f, -31)),
    ("mesa", new Vector3(0, 12, 10)),
    ("subsolo", new Vector3(-25, 1, -35))
};
foreach (var (n, p) in pts)
{
    bool ok = NavMesh.SamplePosition(p, out var hit, 1.2f, NavMesh.AllAreas);
    sb.AppendLine($"{n} {p} -> {(ok ? hit.position.ToString() : "FORA")}");
}

var path = new NavMeshPath();
bool has = NavMesh.CalculatePath(new Vector3(0, 0.5f, 0), new Vector3(26, 13.2f, -20), NavMesh.AllAreas, path);
sb.AppendLine($"path chao->tampo: {has} status={path.status} corners={path.corners.Length}");
var p2 = new NavMeshPath();
bool has2 = NavMesh.CalculatePath(new Vector3(-25, 0.5f, -10), new Vector3(-25, 0.5f, -35), NavMesh.AllAreas, p2);
sb.AppendLine($"path chao->subsolo: {has2} status={p2.status}");
sb.ToString()
