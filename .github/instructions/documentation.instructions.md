---
description: 'README consistency and Mermaid diagram conventions for Markdown documentation.'
applyTo: '**/*.md'
---

# Documentation

## README Consistency

- **Every project must have a `README.md`**: When adding a new `.csproj` project, create a `README.md` in the project directory as part of the same commit. Follow the existing pattern: Purpose → Services/Extensions → Configuration → Dependencies (NuGet packages table + Project references table).
- Every project's `README.md` must stay in sync with its implementation. During any refactoring — and **always** before creating a new PR — scan each affected project's `README.md` for inconsistencies: outdated service names, missing or removed configuration options, stale dependency tables, or inaccurate flow diagrams. Update the README as part of the same change, not as a follow-up.
- **Major refactorings** (renames, project moves, DI restructuring, model type splits): when a rename or restructure touches type names, configuration sections, or project references, update every `README.md` that mentions the old names **in the same commit**. Do not leave stale references for a follow-up.
- For large refactorings that touch multiple projects, review all impacted `README.md` files before opening the PR.
- **Markdown tables**: Table separator rows must use spaces around pipes to match the spaced style used in header and data rows (e.g. `| --- | --- |` not `|---|---|`). This prevents MD060 (table-column-style) warnings.
- **Configuration examples in library READMEs**: Library projects that expose `IAppConfig` records should include a `## Configuration Examples` section in their `README.md` with `appsettings.json` snippets progressing from minimal configuration through to fully configured. This documents the configuration surface area and provides copy-paste-ready templates for consumers.

## Mermaid Diagrams

Use Mermaid diagrams in `README.md` files to visualize complex relationships and flows. Choose the appropriate diagram type:

### Diagram Type Selection

- **`flowchart`**: Sequential processes, data flow, event flow, service orchestration, CI/CD pipelines
  - Direction: Use `TD` (top-down) for vertical flows; `LR` (left-right) for wide workflows
  - Example: Data moving from device → monitor service → broker → processor → sinks

- **`graph`**: Relationships, dependencies, hierarchies (non-sequential)
  - Direction: Use `TD` for dependency trees; `LR` for peer relationships
  - Example: NuGet package dependencies, project references, Helm chart hierarchies

- **`classDiagram`**: C# class hierarchies, inheritance, composition
  - Shows: Inheritance (`<|--`), composition (`*--`), aggregation (`o--`), association (`-->`), dependency (`..>`)
  - Example: Service class inheritance, interface implementation

- **`sequenceDiagram`**: Time-based interactions between components
  - Shows: Method calls, async operations, timing
  - Example: Request/response flows, background service timing

### Standard Headings

Use these consistent heading patterns before Mermaid diagrams:

| Heading | Use For |
| --- | --- |
| `## Data Flow` | How data moves through the system (device → service → storage) |
| `## Event Flow` | Event-driven processing (pub/sub, channels, streams) |
| `## Service Architecture` | How services interact (SignalR hubs, background services, API clients) |
| `## Dependency Graph` | Package/project dependencies, references |
| `## Class Hierarchy` | C# class structures, inheritance trees |
| `## Deployment Flow` | CI/CD pipelines, GitHub Actions workflows, Helm/K8s deployments |
| `## Configuration Hierarchy` | IAppConfig structure, nested configuration objects |

### Styling Guidelines

- **Subgraphs**: Group related components (e.g., `subgraph Monitor["MonitorBgService"]`)
- **Custom styling**: Define `classDef` for highlighting (e.g., owned vs. third-party actions)
- **Node shapes**:
  - `[ ]` rectangle (default) - services, components
  - `([ ])` stadium - entry/exit points
  - `[( )]` cylinder - databases, storage
  - `{ }` diamond - decision points
  - `(( ))` circle - events

### Synchronization

- Mermaid diagrams must stay in sync with code during refactoring
- When renaming services, update corresponding diagram nodes
- When adding/removing dependencies, update dependency graphs
- Review all `README.md` diagrams before creating PRs
