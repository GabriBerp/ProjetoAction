// Minimal Code Mode runtime (https://developers.cloudflare.com/agents/tools/codemode/).
//
// Contract mirrored from the Code Mode docs:
//   input:  { code: string }
//   output: { status: "completed" | "error", executionId, result?, error?, logs? }
//
// The model writes one JavaScript program per call. Connectors become sandbox
// globals (`unity.status()`), and `codemode.search()/describe()/step()` provide
// progressive discovery plus a recordable step for nondeterministic work.

import vm from "node:vm";
import { randomUUID } from "node:crypto";

const DEFAULT_DESCRIPTION = `Run one JavaScript program that composes Unity Editor tools.

Pass the body of an async function (or an async arrow function). Connector methods
are async, so use await. Return the small value the answer needs instead of dumping
raw payloads.

Discover methods with codemode.search("query") and codemode.describe("unity.status")
before calling an unfamiliar one.`;

function tokenize(text) {
  return String(text)
    .toLowerCase()
    .split(/[^a-z0-9_]+/)
    .filter(Boolean);
}

function formatLog(args) {
  return args
    .map((value) => {
      if (typeof value === "string") return value;
      try {
        return JSON.stringify(value);
      } catch {
        return String(value);
      }
    })
    .join(" ");
}

function firstLines(error, limit = 6) {
  const stack = error?.stack ? String(error.stack).split("\n").slice(0, limit) : [];
  return stack.length > 0 ? stack.join("\n") : String(error?.message ?? error);
}

export function createCodemodeRuntime({
  connectors = [],
  timeoutMs = 180_000,
  description = DEFAULT_DESCRIPTION,
} = {}) {
  const table = new Map();

  for (const connector of connectors) {
    for (const [methodName, spec] of Object.entries(connector.methods)) {
      table.set(`${connector.name}.${methodName}`, { connector, methodName, spec });
    }
  }

  function declarations(connectorName) {
    const connector = connectors.find((item) => item.name === connectorName);
    if (!connector) throw new Error(`Unknown connector "${connectorName}".`);

    const lines = Object.entries(connector.methods).map(
      ([methodName, spec]) => `  // ${spec.description}\n  ${spec.signature.replace(/^\w+/, methodName)}`,
    );

    return `declare const ${connectorName}: {\n${lines.join("\n")}\n};`;
  }

  function describeAll() {
    return connectors
      .map((connector) => `// ${connector.instructions}\n${declarations(connector.name)}`)
      .join("\n\n");
  }

  function search(query) {
    const terms = tokenize(query);
    const results = [];

    for (const [path, entry] of table) {
      const haystack = tokenize(`${path} ${entry.spec.description} ${entry.spec.signature}`);
      let score = 0;
      for (const term of terms) {
        if (tokenize(path).some((token) => token.includes(term))) score += 3;
        else if (haystack.some((token) => token.includes(term))) score += 1;
      }
      if (score > 0 || terms.length === 0) {
        results.push({
          path,
          connector: entry.connector.name,
          method: entry.methodName,
          description: entry.spec.description,
          kind: "method",
          score,
        });
      }
    }

    results.sort((a, b) => b.score - a.score || a.path.localeCompare(b.path));
    return { results: results.slice(0, 25), total: results.length, truncated: results.length > 25 };
  }

  function describe(target) {
    if (!target) throw new Error("codemode.describe requires a path such as 'unity.status'.");
    if (!target.includes(".")) {
      return { path: target, kind: "connector", types: declarations(target) };
    }

    const entry = table.get(target);
    if (!entry) {
      throw new Error(`Unknown method "${target}". Try codemode.search("...") first.`);
    }

    return {
      path: target,
      kind: "method",
      description: entry.spec.description,
      types: `${entry.spec.signature};`,
    };
  }

  function buildSandbox({ logs, steps, deadline }) {
    const capture = (...args) => {
      logs.push(formatLog(args));
    };

    const sandboxConsole = {
      log: capture,
      info: capture,
      warn: capture,
      error: capture,
      debug: capture,
      table: capture,
    };

    const codemode = {
      search,
      describe,
      step: async (name, fn) => {
        const value = await fn();
        steps.push({ name, value });
        return value;
      },
    };

    const globals = { console: sandboxConsole, codemode };
    const context = { deadline, callCount: 0 };

    for (const connector of connectors) {
      const namespace = {};
      for (const [methodName, spec] of Object.entries(connector.methods)) {
        namespace[methodName] = async (...callArgs) => {
          context.callCount += 1;
          return spec.run(callArgs[0], context, callArgs[1]);
        };
      }
      globals[connector.name] = namespace;
    }

    return { globals, context };
  }

  function wrap(code) {
    const trimmed = String(code ?? "").trim();
    if (!trimmed) throw new Error("codemode input.code is empty.");

    if (/^(async\b[\s(]|\(|async\s*$)/.test(trimmed) && trimmed.includes("=>")) {
      return `(${trimmed})()`;
    }

    return `(async () => {\n${trimmed}\n})()`;
  }

  async function execute(code) {
    const executionId = randomUUID();
    const logs = [];
    const steps = [];
    const deadline = Date.now() + timeoutMs;
    const { globals, context } = buildSandbox({ logs, steps, deadline });

    const sandbox = vm.createContext(globals, {
      name: "unity-codemode",
      codeGeneration: { strings: false, wasm: false },
    });

    let timer;
    try {
      const script = new vm.Script(wrap(code), { filename: "codemode.js" });
      const running = script.runInContext(sandbox, { timeout: timeoutMs, breakOnSigint: true });

      const timedOut = new Promise((_resolve, reject) => {
        timer = setTimeout(() => reject(new Error(`Code Mode execution timed out after ${timeoutMs}ms.`)), timeoutMs);
        timer.unref?.();
      });

      const result = await Promise.race([Promise.resolve(running), timedOut]);

      return {
        status: "completed",
        executionId,
        result: result === undefined ? null : result,
        logs,
        ...(steps.length > 0 ? { steps } : {}),
        calls: context.callCount,
      };
    } catch (error) {
      return {
        status: "error",
        executionId,
        error: firstLines(error),
        logs,
        calls: context.callCount,
      };
    } finally {
      clearTimeout(timer);
    }
  }

  function tool() {
    const namespaceList = connectors.map((connector) => connector.name).join(", ");
    return {
      name: "codemode",
      description: `${description}\n\nConnectors: ${namespaceList}.`,
      inputSchema: {
        type: "object",
        properties: { code: { type: "string", description: "JavaScript async function body or async arrow function." } },
        required: ["code"],
        additionalProperties: false,
      },
      execute: async ({ code }) => execute(code),
    };
  }

  return { tool, execute, search, describe, describeAll };
}
