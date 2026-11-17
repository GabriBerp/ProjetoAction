// Unity connector for the Code Mode runtime.
//
// Each method is a typed sandbox method (`unity.status()`, `unity.command()`, ...).
// Every call shells out to the Unity Pipeline CLI (`unity`), so the connector never
// depends on MCP transport: one CLI invocation per tool call, composed by the code
// the model writes.

import { execFile } from "node:child_process";
import path from "node:path";

const GLOBAL_FLAGS = ["--json", "--no-banner", "--non-interactive", "--no-color"];
const MAX_BUFFER = 32 * 1024 * 1024;
const DEFAULT_TIMEOUT_MS = 180_000;

function cliPath() {
  if (process.env.UNITY_CLI) return process.env.UNITY_CLI;
  return process.platform === "win32" ? "unity.exe" : "unity";
}

function runCli(args, { cwd, timeoutMs = DEFAULT_TIMEOUT_MS } = {}) {
  return new Promise((resolve) => {
    execFile(
      cliPath(),
      args,
      {
        cwd,
        timeout: Math.max(1_000, Math.min(timeoutMs, DEFAULT_TIMEOUT_MS)),
        windowsHide: true,
        maxBuffer: MAX_BUFFER,
        env: { ...process.env, UNITY_NO_BANNER: "1", UNITY_NON_INTERACTIVE: "1" },
      },
      (error, stdout, stderr) => {
        resolve({
          exitCode: error ? (typeof error.code === "number" ? error.code : 1) : 0,
          timedOut: Boolean(error && error.killed),
          stdout: String(stdout ?? ""),
          stderr: String(stderr ?? ""),
        });
      },
    );
  });
}

function parseJson(text) {
  const trimmed = String(text ?? "").trim();
  if (!trimmed) return undefined;
  try {
    return JSON.parse(trimmed);
  } catch {
    // The CLI may print a header or a wrapped envelope; try the outermost JSON block.
  }

  const start = trimmed.search(/[[{]/);
  if (start < 0) return undefined;

  for (const closer of ["}", "]"]) {
    const end = trimmed.lastIndexOf(closer);
    if (end <= start) continue;
    try {
      return JSON.parse(trimmed.slice(start, end + 1));
    } catch {
      // keep trying the other closer
    }
  }

  return undefined;
}

function toFlagName(key) {
  return `--${key
    .replace(/([a-z0-9])([A-Z])/g, "$1-$2")
    .replace(/_/g, "-")
    .toLowerCase()}`;
}

function toFlags(input) {
  const flags = [];
  if (!input || typeof input !== "object") return flags;

  for (const [key, value] of Object.entries(input)) {
    if (value === undefined || value === null) continue;
    const flag = toFlagName(key);
    if (Array.isArray(value)) {
      for (const item of value) flags.push(flag, String(item));
    } else if (typeof value === "boolean") {
      flags.push(flag, value ? "true" : "false");
    } else if (typeof value === "object") {
      flags.push(flag, JSON.stringify(value));
    } else {
      flags.push(flag, String(value));
    }
  }

  return flags;
}

function describeFailure(subject, result) {
  const payload = parseJson(result.stdout) ?? parseJson(result.stderr);
  const messages = Array.isArray(payload?.errors)
    ? payload.errors.map((entry) => entry?.message ?? entry?.detail ?? JSON.stringify(entry)).filter(Boolean)
    : [];

  const detail = messages.length > 0 ? messages.join(" | ") : result.stderr.trim() || result.stdout.trim();
  const head = detail.length > 1_500 ? `${detail.slice(0, 1_500)}…` : detail;
  const reason = result.timedOut ? "timed out" : `exit ${result.exitCode}`;
  return `Unity ${subject} failed (${reason})${head ? `: ${head}` : ""}`;
}

function assertOk(subject, result) {
  if (result.exitCode === 0 && !result.timedOut) return result;
  throw new Error(describeFailure(subject, result));
}

function unwrapEnvelope(json) {
  if (!json || typeof json !== "object" || Array.isArray(json)) return json;
  if ("data" in json && ("success" in json || "command" in json)) return json.data;
  if ("result" in json) return json.result;
  return json;
}

export function createUnityConnector({ cwd, allowDestructive = false } = {}) {
  let catalogPromise;

  // The Pipeline server answers one Editor command at a time: concurrent CLI calls
  // fail with "Editor busy". Plans may still use Promise.all; calls are queued here.
  let queue = Promise.resolve();
  function serial(task) {
    const run = queue.then(task, task);
    queue = run.then(
      () => undefined,
      () => undefined,
    );
    return run;
  }

  const enqueue = (args, opts) => serial(() => runCli(args, opts));

  async function catalog(ctx) {
    catalogPromise ??= (async () => {
      const listed = await listCommands({ limit: 1000 }, ctx);
      const entries = Array.isArray(listed) ? listed : (listed?.commands ?? listed?.result?.commands ?? []);
      return Array.isArray(entries) ? entries : [];
    })();

    try {
      return await catalogPromise;
    } catch (error) {
      catalogPromise = undefined;
      throw error;
    }
  }

  async function findCommand(commandName, ctx) {
    const entries = await catalog(ctx);
    return entries.find((entry) => entry?.command === commandName);
  }

  async function listCommands(input = {}, ctx) {
    const flags = toFlags({
      query: input.query,
      tag: input.tag,
      limit: input.limit,
      offset: input.offset,
      groupBy: input.groupBy,
      detail: input.detail,
    });

    const result = await enqueue(["command", ...GLOBAL_FLAGS, ...flags], {
      cwd,
      timeoutMs: remaining(ctx),
    });    assertOk("command listing", result);
    const json = parseJson(result.stdout);
    return json?.commands ?? json?.result?.commands ?? json ?? [];
  }

  function remaining(ctx, fallback = DEFAULT_TIMEOUT_MS) {
    if (!ctx?.deadline) return fallback;
    const left = ctx.deadline - Date.now();
    if (left <= 0) throw new Error("Code Mode execution exceeded its time budget.");
    return Math.min(left, fallback);
  }

  async function invoke(commandName, params, ctx, { resultOnly = true } = {}) {
    if (!allowDestructive) {
      let entry;
      try {
        entry = await findCommand(commandName, ctx);
      } catch {
        entry = undefined;
      }
      if (entry?.destructive === true) {
        throw new Error(
          `Unity command "${commandName}" is destructive (${entry.description.trim().split("\n")[0]}). ` +
            "Re-run the runtime with --allow-destructive after explicit user approval.",
        );
      }
    }

    // CLI options must precede the command name; anything after it is forwarded to
    // the Editor command as its own parameter (and rejected as unknown).
    const args = ["command", ...GLOBAL_FLAGS, ...(resultOnly ? ["--result-only"] : []), commandName, ...toFlags(params)];

    const result = await enqueue(args, { cwd, timeoutMs: remaining(ctx) });
    assertOk(`command "${commandName}"`, result);

    const json = parseJson(result.stdout);
    if (json !== undefined) return resultOnly ? unwrapEnvelope(json) : json;
    return { stdout: result.stdout.trim() };
  }

  const methods = {
    status: {
      description: "Live state of every connected Unity Editor (port, project, version, pid, state).",
      signature: "status(): Promise<EditorInstance[]>",
      run: async (_input, ctx) => {
        const result = await enqueue(["status", ...GLOBAL_FLAGS], { cwd, timeoutMs: remaining(ctx) });
        assertOk("status", result);
        const data = unwrapEnvelope(parseJson(result.stdout));
        if (data === undefined) return { stdout: result.stdout.trim() };
        return data?.instances ?? data;
      },
    },

    pipeline: {
      description: "List the Unity Pipeline servers/packages reachable for this project.",
      signature: "pipeline(): Promise<unknown>",
      run: async (_input, ctx) => {
        const result = await enqueue(["pipeline", "list", ...GLOBAL_FLAGS], { cwd, timeoutMs: remaining(ctx) });
        assertOk("pipeline list", result);
        return parseJson(result.stdout) ?? { stdout: result.stdout.trim() };
      },
    },

    commands: {
      description:
        "List/search Editor commands (196 in this project). Use before calling an unfamiliar command.",
      signature:
        "commands(input?: { query?: string; tag?: string; limit?: number; offset?: number; groupBy?: 'flat' | 'package' | 'tag' | 'group'; detail?: 'compact' | 'full' }): Promise<EditorCommand[]>",
      run: async (input, ctx) => listCommands(input ?? {}, ctx),
    },

    command: {
      description:
        "Execute one Unity Editor command, e.g. command('scene get_hierarchy', { path: 'Assets/Scenes/SampleScene.unity' }). Destructive commands are blocked unless the runtime allows them.",
      signature:
        "command(name: string, args?: Record<string, string | number | boolean | unknown[]>): Promise<unknown>",
      run: async (input, ctx, extra) => {
        const name = typeof input === "string" ? input : input?.command;
        if (!name) throw new Error("unity.command requires a command name.");
        const params = typeof input === "string" ? (extra ?? {}) : (input?.args ?? {});
        return invoke(name.trim(), params, ctx);
      },
    },

    recompile: {
      description: "Force a script recompile and wait for the compile result.",
      signature: "recompile(input?: { path?: string }): Promise<unknown>",
      run: async (input, ctx) => {
        const args = ["recompile", ...GLOBAL_FLAGS, ...toFlags({ path: input?.path })];
        const result = await enqueue(args, { cwd, timeoutMs: remaining(ctx) });
        assertOk("recompile", result);
        return parseJson(result.stdout) ?? { stdout: result.stdout.trim() };
      },
    },

    recompileStatus: {
      description: "Status of the last recompile: idle | triggered | compiling | completed | up_to_date.",
      signature: "recompileStatus(): Promise<unknown>",
      run: async (_input, ctx) => invoke("script recompile_status", {}, ctx),
    },

    console: {
      description: "Read captured Unity console output (tail, level filter, compile-failure flag).",
      signature: "console(input?: { tail?: number; level?: string }): Promise<unknown>",
      run: async (input, ctx) => invoke("console get", input ?? {}, ctx),
    },

    consoleStatus: {
      description: "Console counters and compile-failure flag without pulling entries.",
      signature: "consoleStatus(): Promise<unknown>",
      run: async (_input, ctx) => invoke("console status", {}, ctx),
    },

    tests: {
      description: "List available EditMode/PlayMode tests without running them.",
      signature: "tests(input?: { mode?: 'editor' | 'playmode' | 'all' }): Promise<unknown>",
      run: async (input, ctx) => invoke("test list", input ?? {}, ctx),
    },

    playmode: {
      description: "Control play mode: start, stop, or pause.",
      signature: "playmode(action: 'start' | 'stop' | 'pause'): Promise<unknown>",
      run: async (action, ctx) => {
        const verb = typeof action === "string" ? action : action?.action;
        if (!["start", "stop", "pause"].includes(verb)) {
          throw new Error(`unity.playmode expects 'start' | 'stop' | 'pause', received ${JSON.stringify(verb)}.`);
        }
        return invoke(`editor.playmode ${verb}`, {}, ctx);
      },
    },

    saveAll: {
      description: "Save all open scenes that have unsaved changes.",
      signature: "saveAll(): Promise<unknown>",
      run: async (_input, ctx) => invoke("editor save_all", {}, ctx),
    },

    setAutoTick: {
      description:
        "Keep the editor ticking while unfocused; recommended before scripted/headless automation.",
      signature: "setAutoTick(input?: { enabled?: boolean }): Promise<unknown>",
      run: async (input, ctx) => invoke("editor set_auto_tick", { enabled: input?.enabled ?? true }, ctx),
    },

    cwd: {
      description: "Project root the connector runs commands in.",
      signature: "cwd(): string",
      run: () => path.resolve(cwd),
    },
  };

  return {
    name: "unity",
    instructions:
      "Use for anything inside the open Unity Editor: status, Editor commands, recompiling, console, tests, play mode. Compose calls in one program instead of one round trip per call.",
    methods,
  };
}
