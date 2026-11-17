#!/usr/bin/env node
// Code Mode entry point for the ProjetoAction Unity project.
//
//   node tools/unity-codemode/run.mjs --describe
//   node tools/unity-codemode/run.mjs --search "console"
//   node tools/unity-codemode/run.mjs --code "return unity.status()"
//   node tools/unity-codemode/run.mjs --file plan.js
//   echo "return unity.status()" | node tools/unity-codemode/run.mjs
//
// Prints the Code Mode output envelope as JSON.

import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { createCodemodeRuntime } from "./runtime.mjs";
import { createUnityConnector } from "./connectors/unity.mjs";

const HERE = path.dirname(fileURLToPath(import.meta.url));
const PROJECT_ROOT = path.resolve(HERE, "..", "..");

function parseArgs(argv) {
  const options = { timeoutMs: 180_000, allowDestructive: false };

  for (let i = 0; i < argv.length; i += 1) {
    const arg = argv[i];
    const next = () => {
      i += 1;
      if (i >= argv.length) throw new Error(`Missing value for ${arg}`);
      return argv[i];
    };

    switch (arg) {
      case "--code":
      case "-c":
        options.code = next();
        break;
      case "--file":
      case "-f":
        options.file = next();
        break;
      case "--timeout":
        options.timeoutMs = Number(next());
        break;
      case "--allow-destructive":
        options.allowDestructive = true;
        break;
      case "--describe":
        options.describe = true;
        break;
      case "--search":
        options.search = next();
        break;
      case "--project":
        options.project = next();
        break;
      case "--help":
      case "-h":
        options.help = true;
        break;
      default:
        if (arg.startsWith("-")) throw new Error(`Unknown option ${arg}`);
        options.code ??= arg;
    }
  }

  return options;
}

function printHelp() {
  process.stdout.write(
    `unity-codemode — Code Mode runtime for the ProjetoAction Unity Editor

Usage:
  node tools/unity-codemode/run.mjs --code "<javascript>"
  node tools/unity-codemode/run.mjs --file plan.js
  node tools/unity-codemode/run.mjs --describe
  node tools/unity-codemode/run.mjs --search "play mode"

Options:
  -c, --code <js>        Program body or async arrow function to execute
  -f, --file <path>      Read the program from a file
      --describe         Print the tool description (connector TypeScript declarations)
      --search <query>   Find connector methods by keyword
      --timeout <ms>     Execution budget (default 180000)
      --allow-destructive Allow commands marked destructive by the Editor catalog
      --project <path>   Unity project root (default: this repository)
  -h, --help             Show this help
`,
  );
}

async function readStdin() {
  if (process.stdin.isTTY) return "";
  const chunks = [];
  for await (const chunk of process.stdin) chunks.push(chunk);
  return Buffer.concat(chunks).toString("utf8");
}

async function main() {
  const options = parseArgs(process.argv.slice(2));
  if (options.help) {
    printHelp();
    return 0;
  }

  const cwd = options.project ? path.resolve(options.project) : PROJECT_ROOT;
  const connector = createUnityConnector({ cwd, allowDestructive: options.allowDestructive });
  const runtime = createCodemodeRuntime({
    connectors: [connector],
    timeoutMs: options.timeoutMs,
  });

  if (options.describe) {
    const tool = runtime.tool();
    process.stdout.write(`${JSON.stringify({ name: tool.name, description: tool.description, types: runtime.describeAll() }, null, 2)}\n`);
    return 0;
  }

  if (options.search) {
    process.stdout.write(`${JSON.stringify(runtime.search(options.search), null, 2)}\n`);
    return 0;
  }

  const code = options.code ?? (options.file ? fs.readFileSync(path.resolve(options.file), "utf8") : await readStdin());

  if (!String(code).trim()) {
    printHelp();
    return 2;
  }

  const output = await runtime.execute(code);
  process.stdout.write(`${JSON.stringify(output, null, 2)}\n`);
  return output.status === "completed" ? 0 : 1;
}

main()
  .then((code) => process.exit(code))
  .catch((error) => {
    process.stdout.write(
      `${JSON.stringify({ status: "error", executionId: null, error: String(error?.message ?? error) }, null, 2)}\n`,
    );
    process.exit(1);
  });
