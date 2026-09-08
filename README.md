# Atlas

A native Coil prototype that turns Git snapshots and saved working-tree changes into an interactive island world. All application logic, analyzers, Git access, threading, geometry, and controls are Coil. C libraries are imported directly with in-source `cimport`; there is no C application or wrapper layer.

The prototype runs, but is still under development. Its graphics do **not yet match** the supplied reference's fidelity.

The discovered Coil C-aggregate overread was fixed upstream in commit `0ec70ea` and the corrected compiler installed globally. The original no-window reproduction and a 180-frame Atlas live-observation run now pass AddressSanitizer. The reproduction remains in `tests/repros/color_abi.coil`; the report and resolution are in `coil-bugs` under `atlas-aarch64-color-overread`. No Atlas wrapper, padding, or sanitizer suppression was needed.

## Folder geography

Placement grows a two-dimensional frontier guided by a spatially correlated terrain field. Building settlements follow broad ridges instead of filling disks. Terrain is sampled from a world-coordinate, domain-warped heightfield, with explicit flat foundations for buildings and the town hall. The same function produces fine and coarse meshes. Shared noise and overlapping translucent shelf influences connect neighboring islands visually through shallow water. Conservative circular bounds remain collision guards, not coastline definitions.

Territory dividers are marching-triangle isolines of occupied reservations. They can have concavities, holes and disconnected groups; they no longer draw center-to-child bridges across empty ocean. Borders are prepared with the layout, copied with explicit ownership on live publication, and reused while seeking history. Folder focus fits the islands visible in the current revision, rather than including absent historical outliers. Terrain techniques and visual references are credited in [CREDITS.md](CREDITS.md).

Files with observed declarations are islands; directories are nested, disjoint archipelago reservations. Borders follow that hierarchy and expose smaller territories as you zoom. The town hall identifies its file, while declarations retain their analyzer-defined module in the inspector. File moves keep their comparison identity but occupy the appropriate folder in each revision.

The left tree is a directories-first, alphabetical view of paths present at the selected revision, including unsupported files. Those files appear in the tree without fabricated structures. Folder inspectors summarize current subtree files, structures, and changes. Visibility choices persist across scrubbing and apply to new live descendants; hiding a subtree removes its terrain, halls, structures, labels, links, picking targets, and minimap markers. Expanding/collapsing only changes the tree, not the layout. Selecting a building reveals its file in the tree.

All reachable history shares prepacked geography and prepared folder censuses. Historical seeks do not analyze, pack, or generate border geometry. Live growth preserves unaffected sibling reservations and translates a relocated subtree rigidly. Dependency links start off for a readable folder map; press G to show them. Borders use neutral blue-gray so change colors remain distinct.

## Run

Currently validated on Apple Silicon macOS, with Coil, raylib 5.5, libgit2 1.9.1, and `pkg-config` installed. Header imports currently use `/opt/homebrew/include`. The shaders require OpenGL 3.3 or newer and are embedded in the executable.

```sh
coil build
./build/release/atlas
```

No arguments creates a six-commit example repository at `build/demo-v2`, then opens its timeline. Subsequent launches preserve that repository and any edits you make there.

```sh
./build/release/atlas /path/to/repository
./build/release/atlas /path/to/repository HEAD~3
```

An explicit repository opens at its latest revision. Atlas indexes every commit reachable from HEAD, or from the optional revision argument, including merged branches. This excludes unreachable commits and unmerged branches outside that ancestry. Each commit is compared with its actual first parent, not an adjacent timeline entry. Atlas reads external repositories; it does not check out commits or modify their working trees. `ATLAS_REPO` is an alternative to the repository argument.

History is uncapped by default. Set `ATLAS_HISTORY_LIMIT=128` to explicitly cap the index at 128 commits; the value must be a positive decimal integer. All indexed snapshots are prepared before playback. Analysis is shared by Git blob and source path; immutable source/field metadata is borrowed across snapshots instead of reparsed and copied. Actual-parent comparisons, layout positions, impact graphs and module-route resolution are prepared up front. Seeking switches resident state: it does not analyze files, relocate islands or generate/upload terrain. Opening a large repository has a visible preparation phase and significant memory cost; this cache is session-local, not persisted to disk. The compiler benchmark indexed 438 commits from 5,007 unique file analyses. Close the preparation window to cancel.

With nothing selected, the inspector shows the active commit title and author. Selecting an island replaces the overview with that file and its revision-specific structure list, including separately counted removal ghosts. Selecting a structure shows only its declaration details, observed dependencies, and fields or declaration source. The footer gives the one-based position in the reachable history index. Live mode instead explains saved-worktree observation and does not attribute edits to a commit author or an identified agent.

## Controls

WASD panning is normalized in screen space: diagonal key chords do not accelerate movement, and camera tilt does not slow forward motion relative to sideways motion. Its speed is 55% of the visible vertical span per second, so zooming in gives finer world-space control. Middle-button dragging retains its cursor-matched displacement. `tests/keyboard-pan.rep` exercises diagonal travel, reset, selection and focus.

| Input | Action |
| --- | --- |
| WASD / left- or middle-button drag | Pan; dragging does not select on release |
| Right-button drag | Orbit / tilt |
| Wheel over the world | Smooth zoom anchored under the cursor |
| Click a building | Select and inspect |
| Click a town-center building | Select its entire island |
| F | Focus the selected building or island |
| R | Fit the world |
| Folder name / tactical map | Focus a folder territory / navigate |
| Tree arrow | Expand or collapse children without moving geography |
| Tree checkbox | Show/hide the subtree; partial parents show a dash |
| Backspace | Navigate to the parent folder |
| B / G, or the left-panel overlay controls | Toggle territory borders / dependency links |
| Wheel over a side panel | Scroll its contents |
| Space / play button | Play or pause history; restart at the end |
| Left / right arrow | Step revisions |
| Click or drag the timeline | Scrub resident history |
| `[` / `]` | Halve / double playback speed, from 0.125× to 16× |
| L | Toggle live saved-file observation |
| H | Controls overlay |
| V / click source header | Expand or collapse source at close-up zoom |
| `/` / click search | Search declaration names, paths, and modules |
| Up / down, Enter (search open) | Choose a result and focus it |
| Escape | Dismiss one layer: search, help, source drawer, member selection, building selection, island selection, then folder selection. Exit when none remains. |

The top lenses select exploration, observed dependencies, changes, or ingestion health. Verify does not claim to run builds or tests. A close-up selection shows a collapsed source header; click it or press V to reveal declaration source. The drawer retains your open/closed choice across selections and revisions. Only its visible area blocks map mouse input, and search consumes its keyboard shortcut.

The focused inspector retains declaration fields at every zoom level. Declarations without observed fields show their scrollable declaration source. Coil structs provide field names, exact source signatures and source line numbers, including generic and nested type syntax. Scroll the inspector for longer lists; V still opens the full declaration. Complete empty lists are distinguished from unavailable or partial metadata. The source reader handles mixed brackets and quoted delimiters and stops safely at incomplete or more than 256-level nested forms. Signatures are observed syntax, not typechecked or resolved types. Enum variants, function parameters and C-family fields are not yet extracted.

The selected declaration also exposes one 3D cell per observed field. Click a cell or inspector row to select the same field in both views; a parchment roof rim marks selection without replacing its field-change color. Rendering and ray picking share per-cell bounds, with gaps between cells and a separately selectable foundation. The source-order lattice is symbolic, not a claim about memory layout or field size. Unsupported declarations retain their original glyph. Partial metadata shows only observed fields, labeled partial in the inspector. Revision/live snapshot changes clear field selection to avoid selecting a different field at an old index.

Member comparisons use the commit's actual first parent, or the previous accepted live snapshot: unchanged fields stay graphite, modified fields are amber, added fields mint, and removed fields remain selectable coral wire ghosts. The inspector labels each state; removed rows display their previous signature and source line. Current field counts exclude ghosts. Current entries are followed by removed entries; field positions are not durable across reordering or grid-size changes. Complete empty declarations can therefore show removal ghosts, or just the foundation if no previous fields existed. Incomplete metadata and ambiguous duplicate names produce an unavailable comparison instead of inferred changes. Coil field fingerprints ignore formatting/comments while preserving quoted tokens; these are syntax comparisons, not resolved type equivalence. Field renames are represented as removal plus addition.

`tests/member-focus.rep` checks close-up fields, scroll and click isolation. `tests/member-history.rep` steps backward while retaining Order selection and shows its field list change from four fields to three.

Changed selections show side-by-side BEFORE and AFTER declaration excerpts. Additions and removals explicitly mark the absent side; a first snapshot without a loaded baseline shows only source. Comparison text is owned by the new snapshot, including during live arena recycling, and always refers to the immediately preceding reconciled snapshot. Excerpts wrap at UTF-8 boundaries and mark continuation beyond the panel; this is not yet a scrollable or line-highlighted diff viewer. The existing font still lacks full Unicode coverage.

Wheel zoom keeps its original cursor anchor while easing, even if the pointer moves away. The target correction uses each frame's actual scale change. Focus, fit, pan, orbit, resize, and sidebar/minimap navigation cancel that anchor. `tests/zoom-pick.rep` zooms over a roof, moves the pointer away during easing, then selects the same roof at the original pixel and focuses it.

The inspector lists changed declarations, including removal ghosts, with source paths and state colors. Click a row to select and focus it; scroll the inspector to reach longer lists. `tests/change-list.rep` exercises the first-row interaction in the demo.

Search ranks exact names before prefixes, name substrings, paths, and modules. It includes removal ghosts from the displayed transition. Search consumes navigation and playback input while open. Editing preserves whole UTF-8 codepoints; case-insensitive matching currently covers ASCII, and the bundled font loading does not provide full Unicode glyph coverage.

## Data and identity

Each file with observed declarations has a reserved island site, sized from the union of the indexed history. An island is rendered only while that file exists in the displayed revision and its subtree is enabled. Absent islands leave no terrain, coastal water, town, minimap silhouette or floating removal ghosts. Surviving islands retain their history-wide terrain footprint to preserve building coordinates; their coastlines do not yet shrink with structure count. A declaration's initial identity uses its path and name, not its line number. Token fingerprints exclude comments and whitespace. Unambiguous same-shape renames carry their earlier identity forward; ambiguous matches are additions/removals. Removed entities remain as coral ghosts for that transition.

Each file island retains reference-guided local placement and a 2.3-world-unit minimum between building centers. Coast-aware scoring limits unnecessary growth; historical reservations remain occupied. New files receive separate islands inside their containing folder rather than sharing an analyzer module's island.

Initial packing recursively reserves disjoint disks for sibling folders and file islands without renumbering their identities. Bounded eroded coastlines, bent strata, and a tessellated plateau share layout clearances; terrain around each registered foundation stays level. A shared radial profile keeps the seven narrow coastal terraces, rock seating, plateau seam, and buildable footprint consistent. Plateau normals are smooth, while cliff and rock faces retain hard edges.

Fit-to-world uses a conservative projected cylinder per island: horizontal radius 1.35 times the layout radius, foreshortened ground extent, and vertical clearance from -0.5 to 4.8 world units. This replaces the former screen-circle approximation that omitted roof height. The fit keeps its 12% viewport margin and current camera orientation. It does not fit annotation cards or the entire outer water shelf, and width-limited worlds still retain unused vertical space. Replay click coordinates are calibrated to the resulting camera center.

Coastline seeds vary elongation, headlands and two localized inlets. Smooth saturation keeps the existing radial clearance envelope without hard-clipped arcs. The terrain, coastal shelf, contours and tactical silhouette use this same deterministic profile; it does not relocate declarations or change module identities.

New buildings are placed near already registered declaration references, with source-file peers providing a weaker neighborhood preference. A deterministic expanding search checks actual occupied sites through a spatial index and penalizes unnecessary island growth. It preserves the minimum 2.3-unit site separation for 1.05-unit foundations, including historical reservations. Existing buildings never repack when later declarations or references arrive. This is an incremental lexical-reference heuristic, not global graph optimization or compiler-semantic clustering; history-window and discovery order still affect the initial map.

Growth scoring and island sizing account for the local coastline around each full foundation disk. The bound covers the disk angular extent plus neighboring mesh endpoints, subtracts a proven coast-slope allowance between samples, and accounts for polygon chords. Buildings can occupy headlands without sizing the whole island as though every site were beside the deepest inlet. Candidates within the guaranteed inscribed disk skip the more expensive coast calculation. The renderer and sizing code share shoreline segment limits. Tests compare full foundation boundaries with exact rendered-polygon ray intersections, including radius growth that changes tessellation. Initial layouts differ from earlier builds; stable local sites remain unchanged during a running history/live session.

For a read-only report of the existing demo map, including radii and projected changed-struct roofs for replay calibration:

```sh
coil build tests/repros/layout_report.coil -o build/atlas-layout-report
./build/atlas-layout-report
```

Plateau foundation queries use a balanced spatial index, checked against linear nearest-site queries. Live terrain geometry is generated on the analysis worker. Mesh preparation/upload remains on the rendering thread; initial startup generation is synchronous.

A cached translucent coastal shelf adds a shallow-to-deep-water transition beneath the bathymetric lines. Its geometry is generated alongside live terrain, uploaded on the rendering thread, and replaced with the terrain when island footprints change; it is not rebuilt every frame.

Bathymetric rings are shaded directly on that shelf using radial coastline UV coordinates. Screen derivatives soften line edges and fade rings when their spacing becomes subpixel; the old per-frame 3D hairline loops are gone. Eleven contours occupy a narrow coastal band and fade at its outer edge. These are symbolic coastline-relative rings, not measured depths or a merged seabed: shelves from sufficiently close islands can still overlap. Mesh tests cover coordinate-to-coast agreement, the closed angular seam, and neutral UVs on ordinary geometry.

The ocean and shelf share a world-space water shader with warped wave slopes, subdued sky reflection and derivative-filtered small ripples. Water uses the simulation clock, including pause and playback speed. This is a stylized surface material, not fluid simulation or reflected scene geometry; bathymetric contours remain cartographic overlays.

The north-up tactical map uses the terrain coastline profile and current declaration positions, including amber/mint/coral change markers and the selected declaration. Its camera footprint accounts for yaw, pitch, zoom and viewport proportions on the horizontal focus plane (not terrain occlusion). Clicking uses the inverse of the same map projection. Large footprints are clipped at the map boundary.

Coastal rocks use closed, layered boulder meshes with irregular shoulders and broad fractured crowns rather than pointed pyramids. Their deterministic footprints remain inside the existing placement radius, preserving building clearances. This detail is baked into terrain at startup or during live geometry preparation, not regenerated each frame.

Island silhouettes separate broad seeded headlands and inlets from fine erosion. High-contrast smooth saturation gives the broad coastline more pronounced bays without clamping it into circular arcs, while the fine detail remains visible at headland tips. The combined radial profile retains the shared 0.805–1.17 bounds used for layout clearance; terrain, shelves, contours, and the tactical map use the same profile. Building coordinates are unchanged by this shaping.

Large outcrops use uneven seeded groups across several terraces, with independent heights and capped radii. This breaks up repeated perimeter rows and prevents oversized boulders on growing modules. Candidate rocks still pass the foundation-clearance check before being emitted.

Cliff beds have shared piecewise-linear fracture joints and three faceted courses per face. Bed-edge offsets taper to zero at the plateau and waterline, preserving those joins and radial ordering. Relief is capped in world units so large modules do not grow oversized fractures. The geometry is baked into the same terrain mesh used by lighting and shadows; this adds mesh preparation/upload cost during live expansion.

Offshore outcrops have height-relative submerged bases and exposed crowns. A narrow, irregular wet-stone band darkens lower cliffs and rocks at the waterline, with a tighter surface highlight; this is an authored splash zone, not simulated tides.

Terrain uses a dark, slightly cool stone palette with ochre weathering on upward-facing coastal shelves below the plateau. The weathering follows geometric surface orientation and world height, with broad noise variation; it is an authored material mask, not erosion simulation. Both coarse and fine bump gradients are attenuated by their screen footprint before perturbing normals, so unresolved relief contributes less to lighting at distant zoom. Mineral fractures, moss, wetness, geometry and foundation clearances are unchanged. This filtering is approximate and does not establish alias-free rendering at every camera position.

Enum glyphs use a symmetric upright fan with nine closed, folded leaves. Each leaf has two non-coplanar faces and real thickness, giving the fan a pleated silhouette and lighting response. Solid and construction/removal wireframes share the same vertices. Leaf count is a glyph design, not a count of parsed enum variants.

Queue glyphs have three open lanes with low side walls, recessed bays, a loading gate and exterior supports. All passes share the same part geometry. Lane and bay counts are visual conventions, not runtime queue occupancy.

Map glyphs use eight closed courtyard-cell shells around an open center. Their profiles include stepped feet, projecting cornices, beveled metal rims, inner walls and recessed floors. The same profile supplies solid geometry and change-state wireframes; picking bounds are derived from the generated mesh. Five material-state models are cached at startup and unloaded at shutdown. Cell counts are glyph conventions, not runtime map contents. `tests/courtyard-focus.rep` opens OrderHistory in the demo.

An in-map legend explains change colors, selection and dependency links. The source preview sits above it; both panels block world picking and mouse camera gestures. Each island has an authored three-dimensional civic hall and paved town square, with a portico, side wings, tiered roof, lantern tower and lamps. The building casts shadows and uses shared geometry bounds for picking. Its scale follows the current structure count (0.8–2.4×), within a reserved seven-unit town-center clearance. Click the building to select the island; its file name remains beside it. The selected island remains named in a persistent top breadcrumb even at close zoom; without explicit selection, the breadcrumb identifies the island containing the camera target. Distant towns use a simpler three-dimensional silhouette; close views retain the detailed mesh. Towns are instanced in both color and shadow passes. Island selection survives revision changes while the file remains present and visible; disappearing or hidden selections are cleared. Left-drag currently pans the camera; it does not physically reposition an island. The older collision-placed annotation implementation remains available internally but is no longer drawn in the normal UI.

The timeline has mouse buttons for previous revision, play/pause, next revision, slower and faster. They share the keyboard control state transitions and retain the 0.125x–16x speed limits. The speed label preserves the exact minimum value. Seeking is confined to the timeline track band, so clicking the revision description does not seek.

Selection follows a declaration's durable identity across history and live snapshots, including its removal ghost. The inspector's structure total and island labels count declarations present in the displayed snapshot, excluding ghosts. Town labels identify files, with overlapping labels suppressed; the underlying layout still reserves history-wide sites.

Added structures fill upward inside mint wireframes; removed structures clear downward into coral wireframes. Color and shadow passes use the same two-second simulation envelope, and wireframe ghosts cast no solid shadows. Playback pause freezes the animation clock; speed controls scale it. Manually seeking or stepping starts the selected transition, and live observation resumes the clock.

Observed module dependencies use camera-facing ribbons. Routes touching changed modules have amber halos and clock-driven moving tails; inactive dependencies stay subdued. Ribbon thickness and moving-marker radius are derived from the orthographic world span and logical viewport height, so close zoom and window resizing preserve their screen-space scale. These show dependency direction, not measured runtime traffic. Terrain and buildings can occlude them.

Both module and declaration routes choose their segment counts from a curvature bound and a 0.20-logical-pixel interpolation-error target. Inter-island height transitions have continuous slopes at their clearance joins, avoiding sharp knees near foundations. This smooths the existing paths; it does not add obstacle routing or change dependency resolution.

On-island declaration-name references use narrower raised ribbons with foundation endpoints. Changed or selected connections gain amber emphasis and source-to-target moving tails driven by the same animation clock. Their raised middle avoids the old fixed-height lines disappearing into plateau relief; buildings still occlude them. Resolution remains module/name-based lexical matching, not a compiler-verified call graph or obstacle-routed road network.

Glyphs represent records, enums, map-like containers, trees, queues, graphs/traits, and functions. Container classification currently uses declared field names. The Coil adapter extracts declaration forms and imports; the C-family adapter is a conservative lexical adapter for Rust, JavaScript/TypeScript, C/C++ headers and sources. These are **not compiler-semantic analyzers or verified call graphs**. Unknown declaration shapes fall back to a file glyph.

Functions use a low service-pavilion glyph with an open entrance, projecting roof, recessed roof panel and raised ribs. The twelve architectural parts are symbolic, not runtime equipment or parsed function internals. Five palette meshes are cached once and shared by solid color and shadow passes; transition wireframes use the same part dimensions, and picking bounds come from generated vertices. `tests/function-focus.rep` focuses the demo priority-weight function for visual inspection.

The selected declaration inspector reports direct and indirect lexical dependents. Reverse-graph breadth-first traversal counts each dependent once, including across cycles and duplicate references. Resolution prefers a unique same-module name, then a globally unique name; ambiguous or missing targets are excluded and reported in Understand mode. These are potential dependencies observed by the adapter, not verified call sites or a complete impact estimate. The current Coil adapter records uppercase type-like references only. Removed declarations show `n/a` because the current snapshot does not contain their prior reference graph. Graph storage contains indices and belongs to the snapshot arena. Git graphs are prepared during initial ingestion; changed live graphs are prepared by the worker before atomic publication. Unchanged scans retain the previously displayed snapshot and graph. The renderer caches selection summaries but never resolves names or rebuilds adjacency. Graph construction uses temporary content-keyed module/name, global-name, and source-ID hash indexes, with expected O(N+E) table operations plus string hashing. Hash collisions still require full content equality. Index storage and its borrowed keys are discarded before the immutable graph is published; the 10,000-declaration chain test checks graph construction and complete traversal, not whole-application rendering scalability. Live publication logs report worker graph time separately from geometry and render-thread upload time.

Solid glyphs use a stylized satin-metal lighting model with GGX direct highlights, correlated Smith masking, Schlick Fresnel, and world-oriented warm/cool reflection lobes. Unchanged structures share a cool graphite palette; parchment is reserved for selected unchanged structures, while amber/mint/coral retain precedence for edit states. Tangent-projected machining relief fades before becoming subpixel; reduced diffuse lighting separates the metal response from stone. The environment and material colors remain art-directed, not measured materials or scene reflections. Graph spheres supply explicit radial normals in Coil. Tree limbs, enum supports, and graph links use Coil tapered tubes with smooth side normals and separate cap normals; raylib 5.5 immediate spheres and cylinders do not supply normals for custom lighting. `tests/graph-focus.rep`, `tests/tree-focus.rep`, and `tests/material-orbit.rep` open these glyphs for visual inspection.

The scene render target owns RGBA16F color and a sampleable depth texture. Half-float color preserves values above 1.0 through bloom and hue-preserving tone mapping; the former RGBA8 target clipped those values before post-processing could recover them. Color attachment storage is now 8 bytes per pixel instead of 4. Post-processing reconstructs view-space positions from the actual orthographic projection and adds 24-sample contact obscurance within a 0.70-world-unit radius, before amber bloom and UI composition. Depth discontinuity rejection and world-space bias limit silhouette halos and self-occlusion. This is screen-space shading of the already-lit scene, not global illumination; hidden surfaces cannot contribute. Color and depth attachments are released on resize and shutdown.

The native GPU regression below checks three target sizes, reads back HDR values as floats, and runs the actual post shader to verify preserved color ratios. It requires a graphics context and is separate from the headless unit suite:

```sh
coil build tests/repros/hdr_target.coil --sanitize=address -o build/atlas-hdr-probe-asan
./build/atlas-hdr-probe-asan
```

The metal environment includes a broad lower-hemisphere bounce and a low, world-oriented fill reflection. These reveal vertical walls and folded enum faces while preserving the brighter upper key reflection. They are authored studio lighting, not traced ground bounce or scene reflections; stone and ocean lighting are unchanged. A separate native GPU probe renders the production material fragment shader on controlled front, side, and roof normals in all four state colors. It reads floating-point HDR output and checks vertical-face readability, retained key/fill contrast, and color ordering:

```sh
coil build tests/repros/material_environment.coil --sanitize=address -o build/atlas-material-environment-asan
./build/atlas-material-environment-asan
```

The 17 tree branch variants are uploaded once and reused in both shadow and color passes. Each retains all 192 tube segments; selection tint, world translation, and construction clipping are applied per instance. Branches use a pale neutral satin finish to distinguish their fine silhouettes from graphite buildings; selected unchanged branches become warm parchment. Amber, mint and coral override that finish without the channel clipping caused by the former 1.85 brightness multiplier. Foundations retain the common glyph palette. Wireframe trees share the same branch generator and branch-color function. CPU staging arrays are freed after upload, and cached models are unloaded at shutdown. The earlier 327-entity growth capture measured 52 FPS with this cache versus 37 before it; these are individual historical runs, not frame-time percentiles or a current benchmark. Other glyph families and tree foundation/ring details still use immediate geometry.

Stacked slab buildings are cached across nine heights and five material states, preserving per-face colors rather than approximating them with a single tint. Their recessed opaque facade core and ten continuous beveled mullions are part of the same cached mesh, replacing the former immediate plain cube. Posts meet the foundation and extend through the floor solids; slab overhangs and picking bounds are unchanged. These are architectural details, not additional fields or runtime activity. Foundations remain immediate geometry. Each change-state pass submits immediate geometry together, followed by cached meshes, avoiding a batch flush for every building. An earlier growth capture reached 57 FPS with the slab and tree caches, with color-pass glyph submission samples of 4.7–5.4 ms; this predates the facade detail and is not a current benchmark. Thin roof bevels are bounded by panel thickness so opposing bevel rings cannot cross. `tests/slab-facade.rep` focuses Order and lowers the camera for facade inspection.

Live observation runs on a worker thread, sampling about every 750 ms. It respects Git ignore rules and skips symlinks. The worker stages the analyzed snapshot, reconciled identities, layout and any required CPU terrain geometry. The renderer keeps the old map until that package is ready, then uploads the mesh and adopts the layout together. Two arenas alternate ownership; retained layout data is copied into long-lived storage. Unchanged scans do not regenerate geometry. Failed scans retain the last complete map. The animated activity marker represents observed source changes, not an identified agent or inferred intent.

When an existing island grows beyond its available clearance, only that growing island relocates. Buildings retain their local slots, unchanged islands remain anchored, and loaded historical snapshots use the repaired geography. New islands are packed afterward. Relocation currently happens at publication rather than animating smoothly; press R to fit an expanded world.

Activity pulses appear only when the displayed snapshot contains an observed declaration change. Unchanged and empty snapshots clear the previous activity target validity. Marker motion, ring expansion, and lifetime share simulation time, so pause and speed controls apply consistently; marker radius stays at 2.4 logical pixels across zoom levels. Motion uses an exponential trajectory from the transition origin, independent of render frame subdivision.

## Plug in an analyzer

Implement `atlas.analyzer/Analyzer`:

```clojure
(defstruct MyAnalyzer [(version i64)])
(impl analyzer/Analyzer MyAnalyzer
  (analyze [(self (ptr MyAnalyzer)) (sink (ptr m/Snapshot))
            (path (slice u8)) (source (slice u8))] (-> i64)
    ;; Parse source; emit actual declarations and relationships into sink.
    ;; Return the number of declarations emitted.
    ...))
```

Construct a registry with `analyzer/new-registry` and a fallback adapter, or extend `analyzer/defaults`. Register your adapter with `analyzer/register!`, supplying an extension such as `.py`. Registrations are tested in order; the first matching suffix wins. The registry constructed at the start of `atlas.main/run` is shared by Git traversal and live traversal, so new extensions require no separate filesystem or Git switch.

`m/emit!` appends a declaration; set its exact source excerpt, module, and rename-shape fingerprint as appropriate. Append observed module dependencies to `sink.edges`, and declaration references to `sink.references`. Allocate or copy retained text into `sink.alc`; do not return pointers into parser-temporary storage. Registry adapters must remain alive throughout the run and be reentrant: the live worker must not race with mutable shared parser state. See the registry-dispatch test for a small functioning implementation.

Adapters can append `m/MemberDetail` records (`name`, `signature`, `line`, `fingerprint`) to an entity `details` array and set `details-known` when the complete declared field list was observed. Supply a normalized syntax fingerprint, or zero to compare exact signature text. Leave `details-known` false for partial/unavailable metadata. The default empty array is allocated with the snapshot allocator; all appended text must also belong to that snapshot. Reconciliation owns a separate `previous-details` array and index-based `detail-changes`; adapters should not populate those. Retained entities and removal ghosts deep-copy both detail arrays and comparison indices, so comparisons survive source-arena recycling. The existing numeric `members` field still stores a semantic-token count used for symbolic glyph sizing; it is not the real field count. Complete field metadata supplies the inspector count instead.

For record glyphs with complete field metadata, stack height uses `clamp(2 + field_count, 3, 11)` tiers. The base and minimum silhouette are architectural, not additional fields; nine or more fields saturate the eleven-tier overview. This makes adding a field visible even when its type has a short spelling, while generic/nested type syntax does not inflate a record. Partial or unavailable metadata retains the token-size fallback `clamp(2 + sqrt(max(tokens, 0)) / 1.45, 3, 11)`, rounded down. The exact field count remains available in the inspector and uncapped member view. Solid geometry, shadows, ghost outlines, and picking derive from the same height, without changing site positions or terrain clearance.

This is an in-process Coil trait boundary, not a dynamically loaded binary-plugin ABI.

## Verification and captures

```sh
coil test
ATLAS_CAPTURE=build/atlas.png ./build/release/atlas
ATLAS_REPLAY=tests/live.rep ATLAS_CAPTURE=build/live.png ./build/release/atlas
```

Captures exit after 90 frames by default; set `ATLAS_CAPTURE_FRAMES` to override. `ATLAS_REPLAY` consumes raylib automation events for application-level input testing. Generated captures and fixture repositories are under ignored `build/`.

Set `ATLAS_PROFILE=1` to log CPU wall-clock samples for the first three frames and every 60 frames thereafter: shadow, world, post-processing/UI/presentation, terrain, glyphs, water, declaration references, and import routes. Presentation includes frame pacing and driver waits; these are not GPU timestamps or statistical frame-time summaries. A short terrain submission interval does not measure the GPU cost of shading terrain.

Large maps use per-island terrain meshes, conservative viewport culling, and a coarse terrain mesh above 0.20 world units per logical pixel. Detailed terrain remains available when zooming in. Below an eight-pixel declaration footprint, unselected buildings use instanced solid silhouettes; selected buildings retain their full geometry and inspection. Scrubbing updates compact transforms instead of rebuilding GPU meshes. Minimap coast triangles are cached per layout, and module-route resolution is cached per snapshot. The instancing implementation follows [raylib 5.5’s matrix-attribute contract](https://github.com/raysan5/raylib/blob/5.5/src/rmodels.c#L1515); the native GPU regression in `tests/repros/instances_render.coil` compares instanced and ordinary draws and checks state restoration. This is rendering level-of-detail, not the experimental grouped-landmark mode: declarations, history, selection identities, and analyzers are unchanged.

With links enabled, import routes connect their actual source file to resolved target-module files; declaration reference ribbons appear at building zoom or for the selected declaration. Links with neither endpoint near the viewport are omitted. Routes share a graphics batch and use piecewise curvature-bounded tessellation. Minimap terrain and change-colored entity markers are cached until the snapshot/layout changes; selection and camera footprint remain live. Island names appear above an 80-pixel footprint, with census labels above 180 pixels. Crowded label fallback uses an exact lattice occupancy calculation instead of repeated obstacle searches.

`tests/compiler-camera.rep` exercises full-repository overview, wheel zoom, diagonal keyboard pan, orbit, zoom-out, and reset. Use it with the compiler repository and `ATLAS_CAPTURE_FRAMES=300` to reproduce the performance interaction check without reducing the history limit.

In Change and Understand modes, selecting a declaration marks its lexical dependents with blue ground rings: solid for one reference hop, dashed for two or more. These markers do not change amber/mint/coral edit states or imply confirmed runtime impact. Marker distances and inspector counts share one cached breadth-first traversal, invalidated before rendering after selection or snapshot changes. The selected declaration itself and unreachable declarations are not marked. Ring width follows viewport scale within bounded world-space limits; buildings and terrain can occlude them. `tests/dependents-focus.rep` selects LineItem and zooms out to show four direct dependents and one indirect dependent in the demo.

Atlas additionally accepts replay event type 1000 with a Unicode codepoint in parameter 0, routed through the same text-input handler as normal character input. This tests application input handling, not operating-system keyboard delivery. `tests/search.rep` checks search alongside attempted camera/playback shortcuts; `tests/search-focus.rep` focuses a match and dismisses a reopened search without closing the application.

The mouse-focus, drag-pick, and zoom-pick recordings target the current settlement layout. Pixel-targeted replays are layout-specific; historical captures from the earlier circular layout are not current integration validation.

`tests/selection.rep` exercises focus across two demo revisions; `tests/pause-animation.rep` pauses an in-flight transition. `sh tests/create-transition-fixture.sh` creates a separate two-commit repository covering additions and removals of every glyph family and prints its path. Open that path with Atlas to inspect construction and ghost rendering without modifying the interactive demo.

`tests/mouse-focus.rep` clicks the default demo Order roof and focuses it with F. Struct selection uses a height-aware bounding box shared with the render-level calculation. Other glyphs use cached whole-building bounds, including the folded fan and all 17 generated tree variants; queue bounds follow the shared part geometry. `tests/fan-pick.rep` deselects Priority and then clicks its upper leaves. Selection envelopes include internal gaps and the full construction/removal silhouette; picking is not triangle-exact and does not yet account for terrain occlusion.

`tests/drag-pick.rep` drags the map by 80×40 pixels, clicks Order at its translated screen position, and focuses it. Middle-button movement is scaled by the orthographic zoom, viewport height, and pitch rather than fixed world units per pixel.

`tests/member-world.rep` selects Order, enters Member zoom, selects a field in the inspector, then selects the deadline cell in the world. Capture frame 150 verifies field index 3; frame 190 steps to the previous revision, retaining Order selection while clearing the field selection and showing its three earlier fields.

`tests/escape-dismiss.rep` focuses Order, opens its source drawer, and presses Escape twice. At frame 150 the application remains open with the drawer closed and selection cleared. `tests/escape-exit.rep` adds a third Escape at frame 160, which exits before a frame-220 capture. Escape is dispatched after replay injection through the same dismissal handler used by physical keyboard input.

## Remaining work

- System-level landmarks are available in a separate, non-interactive rendering harness (`tests/repros/overview_render.coil`), **not in the normal app controls**. Grouping, terrain clearance, and scaled rendering/shadows are implemented. Group drill-down, zoom transitions, camera fitting for enlarged symbols, and live-publication integration remain unfinished. The usable app keeps the individual-declaration view and its existing controls.
- Richer terrain, material treatment, glyph silhouettes, and change animation to meet the reference.
- Smooth island-relocation animation and camera continuity during major live expansion.
- Broader frame-time profiling and bounded live publication work. Historical seeking no longer publishes terrain. Live terrain generation remains off-thread with staged uploads; old model disposal and layout adoption can still hitch on large live changes.
- Compiler-backed analyzers, resolved dependency semantics, broader member metadata, stable field placement and richer before/after field inspection.
- Persistent prepared-history storage, a branch picker, lower startup memory, and persisted layout across launches. Historical playback is fully resident; startup preparation is not yet cached across launches.
- Richer source/diff inspection, build/test integration, and agent telemetry.
- Broader platform validation and deployment packaging.

Development findings and images are indexed in [the project pad](pad://codebase-visual).

Empty snapshots use a valid 40-unit camera span and display an empty-map message with history/live guidance. R resets the empty camera after navigation. Empty maps do not emit an origin change pulse. `tests/empty-world.rep` exercises compact-window resize, pan, reset and live mode; an empty-commit repository with an untracked Coil file can also verify the first structure appearing when live mode starts.
