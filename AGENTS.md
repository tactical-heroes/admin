Keep scope narrow. Run full solution checks for shared API contracts, build files, CI, or cross-project changes.

Use WOFF2 for web fonts.

Optimize raster images before shipping:

- Resize images for their rendered dimensions and intended device pixel ratio. Provide mobile and desktop variants when display size or composition differs; do not serve full-resolution source artwork unnecessarily.
- Prefer AVIF with a WebP fallback, selected through native CSS `image-set()` for decorative backgrounds or `<picture>` for content images. Compare file sizes and visual quality first; keep WebP as the primary format if AVIF offers no benefit. Quality numbers are not equivalent across codecs.
- Use lossy compression when it preserves acceptable visual quality at the target display sizes. Check text, thin edges, dark gradients and transparency; use higher quality or lossless encoding where artifacts remain visible. Encode each format from the original source, not from an already lossy export.
- Keep module-specific assets in that module's `wwwroot/images/<asset>/`, with `desktop` and `mobile` filenames where needed. Shared host assets belong in the host's `wwwroot/images/`.
- Record before/after byte sizes in the PR. Verify the published page loads only the selected size and format, preserves accessibility, and serves correct MIME types and cache headers. Check the whole page payload, including fonts and framework downloads, rather than image sizes alone.

The admin is one Blazor Web App composed from module Razor Class Libraries:

- `TacticalHeroes.Admin` owns the ASP.NET Core host, BFF concerns, and deployment.
- `TacticalHeroes.Admin.Client` owns the application shell, routing, global layouts, providers, and explicit module composition.
- `Modules/*` owns cohesive business UI areas. A module is not a separate SPA and must not reference another module directly.
- Keep each module project directly under `src/Modules` and its test projects directly under `tests/Modules`; do not add a redundant per-module wrapper directory.
- `TacticalHeroes.Admin.Api` owns generated Kiota code and transport-level API primitives.
- `TacticalHeroes.Admin.Shared` owns reusable presentation primitives and must not contain domain-specific models or API adapters.
- Keep the dependency direction `Host -> Client/Modules`, `Client -> Modules/Api/Shared`, `Modules -> Api/Shared`. `Api` and `Shared` must not depend on application or module projects.
- Register module assemblies, navigation, and services explicitly in the client composition root. Do not use reflection-based module discovery.

Use Feature-Sliced Design (FSD), adapted to Blazor, inside `TacticalHeroes.Admin.Client` and every module Razor Class Library:

- `App` exists only in the client shell and owns application composition, routing, global layouts, and providers.
- `Pages` contains route-level UI and page-local behavior. Keep non-reused state, models, forms, and API adapters inside the page slice.
- Keep independently routed list, create, and update flows in separate page slices when they own different data flows. A page slice must not depend on a sibling page slice.
- `Widgets` contains large reusable page sections that compose features, entities, and shared UI without owning routes or domain use cases.
- `Features` contains reusable user-facing use cases, actions, and forms.
- `Entities` contains domain concepts reused by multiple features, widgets, or pages, including their models and API adapters.
- The shared projects contain reusable UI, API, and model primitives and must not depend on higher layers.
- Higher layers may depend only on layers to their right: `App -> Pages -> Widgets -> Features -> Entities -> Api/Shared`. Do not create reverse dependencies or use existing violations as precedent; correct them locally when the affected area is already in scope.
- FSD layers are optional. Do not create a layer or wrapper component solely to preserve the full layer chain.
- Keep slices cohesive and do not introduce alternative top-level architecture folders without an explicit architectural reason.
