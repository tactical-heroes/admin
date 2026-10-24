import { readFile, writeFile } from "node:fs/promises";

const [lcovPath, sonarPath] = process.argv.slice(2);
const lcov = await readFile(lcovPath, "utf8");
const sonar = await readFile(sonarPath, "utf8");
const files = [];
const escape = value => value.replaceAll("&", "&amp;").replaceAll('"', "&quot;").replaceAll("<", "&lt;");

for (const record of lcov.split("end_of_record")) {
    let path;
    const lines = new Map();
    for (const entry of record.split(/\r?\n/)) {
        if (entry.startsWith("SF:")) path = entry.slice(3).replaceAll("\\", "/");
        if (!entry.startsWith("DA:") && !entry.startsWith("BRDA:")) continue;
        const values = entry.slice(entry.indexOf(":") + 1).split(",");
        const line = lines.get(values[0]) ?? { covered: false, branches: 0, coveredBranches: 0 };
        if (entry.startsWith("DA:")) line.covered = Number(values[1]) > 0;
        else {
            line.branches++;
            if (Number(values[3]) > 0) line.coveredBranches++;
        }
        lines.set(values[0], line);
    }
    if (!path) continue;
    const coverage = [...lines].sort(([a], [b]) => Number(a) - Number(b)).map(([number, line]) =>
        `<lineToCover lineNumber="${number}" covered="${line.covered}"` +
        (line.branches ? ` branchesToCover="${line.branches}" coveredBranches="${line.coveredBranches}"` : "") + "/>");
    files.push(`<file path="${escape(path)}">${coverage.join("")}</file>`);
}

if (files.length === 0 || !sonar.includes("</coverage>")) throw new Error("Coverage reports are missing or invalid.");
await writeFile(sonarPath, sonar.replace("</coverage>", files.join("") + "</coverage>"));
