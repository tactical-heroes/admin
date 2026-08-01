Keep scope narrow. Run full solution checks for shared API contracts, build files, CI, or cross-project changes.

Use Feature-Sliced Design (FSD), adapted to Blazor, for all code in `src/TacticalHeroes.Admin.Client`:

- `App` owns application composition, routing, layouts, and providers.
- `Pages` contains route-level composition and delegates behavior to lower layers.
- `Widgets` contains large reusable page sections that compose features, entities, and shared UI without owning routes or domain use cases.
- `Features` contains user-facing use cases, actions, and forms.
- `Entities` contains domain models and entity-specific API adapters.
- `Shared` contains reusable UI, API, and model primitives and must not depend on higher layers.
- Higher layers may depend only on layers to their right: `App -> Pages -> Widgets -> Features -> Entities -> Shared`. Do not create reverse dependencies or use existing violations as precedent; correct them locally when the affected area is already in scope.
- Keep slices cohesive and do not introduce alternative top-level architecture folders without an explicit architectural reason.
