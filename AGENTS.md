Keep scope narrow. Run full solution checks for shared API contracts, build files, CI, or cross-project changes.

The admin is one Blazor Web App composed from module Razor Class Libraries:

- `TacticalHeroes.Admin` owns the ASP.NET Core host, BFF concerns, and deployment.
- `TacticalHeroes.Admin.Client` owns the application shell, routing, global layouts, providers, and explicit module composition.
- `Modules/*` owns cohesive business UI areas. A module is not a separate SPA and must not reference another module directly.
- `TacticalHeroes.Admin.Api` owns generated Kiota code and transport-level API primitives.
- `TacticalHeroes.Admin.Shared` owns reusable presentation primitives and must not contain domain-specific models or API adapters.
- Keep the dependency direction `Host -> Client/Modules`, `Client -> Modules/Api/Shared`, `Modules -> Api/Shared`. `Api` and `Shared` must not depend on application or module projects.
- Register module assemblies, navigation, and services explicitly in the client composition root. Do not use reflection-based module discovery.

Use Feature-Sliced Design (FSD), adapted to Blazor, inside `TacticalHeroes.Admin.Client` and every module Razor Class Library:

- `App` exists only in the client shell and owns application composition, routing, global layouts, and providers.
- `Pages` contains route-level composition and delegates behavior to lower layers.
- `Widgets` contains large reusable page sections that compose features, entities, and shared UI without owning routes or domain use cases.
- `Features` contains user-facing use cases, actions, and forms.
- `Entities` contains domain models and entity-specific API adapters.
- The shared projects contain reusable UI, API, and model primitives and must not depend on higher layers.
- Higher layers may depend only on layers to their right: `App -> Pages -> Widgets -> Features -> Entities -> Api/Shared`. Do not create reverse dependencies or use existing violations as precedent; correct them locally when the affected area is already in scope.
- Keep slices cohesive and do not introduce alternative top-level architecture folders without an explicit architectural reason.
