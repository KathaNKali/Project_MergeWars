# Copilot / Claude Instructions — Merge Wars (Generator Variant)

Purpose of this file: **control how the AI develops this Unity project.**
The user remains the final authority over what task is active, what gets
implemented, which suggested task comes next, and when implementation
begins. The AI's job is to understand the documentation, discuss
architecture, identify dependencies, implement only the approved task,
verify its work, keep documentation synchronized, and suggest next steps
without autonomously executing them.

---

## 1. Scope discipline (read this first)

- Before writing or editing any code, read only the files inside
  `/GameDocs/`. Do NOT scan, index, or infer conventions from the rest of
  the repository unless a file in `/GameDocs/` explicitly points you to
  specific existing scripts to extend.
- Source of truth lives in `/GameDocs/`: `PROJECT_INDEX.md`,
  `GAME_DESIGN.md` (+ variant file), `GAMEPLAY_LOOPS.md`,
  `SYSTEM_MAP.md`, `Systems/SYS_*.md`,
  `Implementation/IMPLEMENTATION_PLAN.md`,
  `Implementation/CURRENT_TASK.md`, `Implementation/PROGRESS_LOG.md`,
  `DECISIONS_LOG.md`. If information needed isn't in these files, stop and
  ask rather than inventing it or pulling it from elsewhere in the
  codebase.
- Never assume the wider project's existing code patterns apply unless the
  active `SYS_*.md` file says so.
- New scripts follow the existing per-domain folder convention, e.g.
  `Assets/Scripts/<Domain>/` (`Grid/`, `Pooling/`, `Economy/`, `Heroes/`,
  `Generator/`, `Merge/`) and editor-only tools go in `Assets/Editor/`.
  Match this pattern instead of inventing a new layout.

---

## 2. The workflow contract (MANDATORY for every request)

Every request — implementation, discussion, or investigation — follows
this sequence, no exceptions:

1. **Discuss first, in points.** Break down the request, relevant
   architecture/dependencies, tradeoffs, assumptions, and UNKNOWNs as a
   bulleted/numbered list.
2. **Ask clarifying questions** whenever something is ambiguous,
   underspecified, or has multiple valid approaches, instead of guessing.
3. **Wait for explicit go-ahead.** Only the literal word **`Ayyappa`**
   authorizes moving into implementation. Nothing else — not "yes", "ok",
   "go ahead", "implement this", "do it", or "proceed" — counts.
4. **Only after `Ayyappa`**, implement the discussed and agreed-upon plan.
5. This applies to every request going forward, by default.

Read-only investigation (searching/reading files, dependency analysis,
asking questions) is always allowed before `Ayyappa`. Only actual code
edits/implementation, and edits to `CURRENT_TASK.md` to reflect a newly
chosen task, must wait for it.

During discussion (steps 1–3): do not write or edit implementation code,
do not call file-editing tools, and do not run implementation-affecting
commands.

---

## 3. User-controlled task lifecycle

`Implementation/CURRENT_TASK.md` is the authoritative definition of the
one task currently in scope. Its `Active task` and `Active variant`
fields drive everything below.

Rules:
- Always read `CURRENT_TASK.md` before implementation.
- The user decides what task is in `CURRENT_TASK.md`. Never replace it
  with another task because that task seems easier, more important, or
  more logical, and never silently reorder `IMPLEMENTATION_PLAN.md`.
- Never autonomously select, start, or begin the next task — not even
  immediately after completing the current one.
- Implement only what `CURRENT_TASK.md` scopes ("Out of scope" sections
  are binding). Do not build ahead into unscoped systems, refactor
  unrelated code, improve unrelated systems, replace working architecture
  without discussion, or add speculative functionality — even if it looks
  easy or related.
- If implementation reveals another system genuinely must change to
  complete the current task, explain why, identify the dependency, and
  discuss the scope expansion before implementing it.
- Confirm the `Active variant` before touching gameplay rules, systems,
  balancing, data, terminology, or mechanics — this project has parallel
  design variants (original vs. generator/continuous-conquest) and they
  must never be mixed unless explicitly documented.

When the current task is completed:
1. State clearly that it is complete.
2. Summarize what was implemented and what was verified.
3. Identify remaining issues, TODOs, UNKNOWNs, or follow-up work.
4. Do NOT start another task.
5. Suggest up to **3** sensible next tasks (drawn from
   `IMPLEMENTATION_PLAN.md`, system dependencies, unfinished documented
   work, or blockers just discovered), each with its dependency and
   whether that dependency is confirmed or inferred. These are
   suggestions only — never auto-start one.
6. Ask the user which one to work on next.

Updating `CURRENT_TASK.md`:
- Update it only to reflect a task the user has explicitly selected.
- Never invent a new task, silently replace a completed one, or assume
  the next `IMPLEMENTATION_PLAN.md` entry is what the user wants.
- If the user manually edits `CURRENT_TASK.md`, treat its new contents as
  authoritative and preserve the user's wording/intent when practical.
- After updating it for a newly selected task, wait for `Ayyappa` before
  implementing — the gate applies independently to every task.

Lifecycle: `USER CHOOSES TASK → CURRENT_TASK.md → AI READS TASK → DISCUSS
→ "Ayyappa" → IMPLEMENT → VERIFY → TASK COMPLETE → SUGGEST UP TO 3 NEXT
TASKS → USER CHOOSES → CURRENT_TASK.md UPDATED → WAIT FOR "Ayyappa" →
IMPLEMENT NEXT TASK`. Never skip the user-selection step.

---

## 4. Reading a task before implementing

For the active task:
1. Read `Implementation/CURRENT_TASK.md` and its `Reference docs` list —
   read only those referenced docs, not the whole `/GameDocs/` tree.
2. Read the "Responsibility" section of each relevant `SYS_*.md`.
3. Read the "Does NOT know about" section just as seriously as
   "Responsibility" — never give a system access to state or systems it
   isn't supposed to know about.
4. Check for CONFIRMED / ASSUMED / UNKNOWN / TODO / BLOCKED markers
   relevant to the task.
5. Resolve important ambiguities before implementation. Do not silently
   convert an UNKNOWN gameplay-defining value (mana costs, caps,
   thresholds, balancing numbers) into an arbitrary constant — use a
   clearly marked `// TODO(design):` comment and surface it back to the
   user, or ask first if it blocks correctness.

---

## 5. Dependency analysis (read-only mode)

When asked for dependencies of a task, feature, system, or proposed
implementation:
1. Read the relevant `/GameDocs/` documentation; inspect only the source
   files that documentation explicitly references.
2. Identify required systems, data, prefabs/assets, interfaces, events,
   configuration, and scene objects, plus upstream dependencies,
   downstream systems affected, and potential blockers.
3. Label each as **Confirmed**, **Likely**, **Optional**, or **Unknown**
   dependency. If the documentation doesn't establish a dependency, don't
   present it as confirmed.
4. Do not implement anything, do not modify `CURRENT_TASK.md`, and do not
   turn the analysis into an implementation plan unless asked.

Example output shape:

```
Task: Automated Combat
Required: Unit Registry, Target Selection, Damage Resolution
Optional: Combat VFX, Hit Reactions
Blocked by: Target Selection is not implemented.
Affected systems: Unit Controller, Health System, Battle State
Unknown: Whether attacks resolve immediately or via an animation event.
```

---

## 6. Architecture rules

Preserve: small single-responsibility systems, clear ownership of state,
minimal coupling, explicit dependencies, composition over unnecessary
inheritance.

Avoid: God Managers, unnecessary global state, speculative frameworks,
unnecessary abstractions, event buses, DI frameworks, service locators,
large manager hierarchies, or complex design patterns — unless one
solves a documented problem or is explicitly approved.

Prefer the simplest architecture that satisfies the documented
requirement while leaving reasonable room for extension.

Approved-but-unintegrated packages (per `PROJECT_INDEX.md` Tech Stack
section): **TopDown Engine, FEEL, DoTween, Cinemachine**. These may be
referenced/wired in when a task calls for them without asking for
re-approval — but confirm the integration point per-system if it's
genuinely unclear, and don't wire them in speculatively outside task
scope. Any other new package or dependency still requires explicit
approval.

---

## 7. Unity engineering rules

**Lifecycle** — `Awake()` for internal init; `OnEnable`/`OnDisable` for
event subscription symmetry; `Start()` when init depends on other
objects' `Awake()`; `FixedUpdate()` for physics; use `Update`/`LateUpdate`
only when appropriate. Avoid expensive per-frame work, repeated scene
searches, unnecessary component lookups, hidden initialization, and
assumptions about lifecycle order.

**MonoBehaviours** — keep them focused; separate gameplay rules from
presentation where practical; no God Managers; composition over deep
inheritance; `[SerializeField] private` over public mutable fields.

**Dependencies** — prefer explicit/serialized references and interfaces
where useful. Avoid unnecessary singletons, static global state,
`FindObjectOfType`/`FindFirstObjectByType`, `GameObject.Find`, tag
searches during gameplay, and circular dependencies.

**Gameplay data** — separate gameplay state, configuration, presentation,
and scene objects. Prefer serializable C# classes for runtime state. Use
ScriptableObjects for authored/config data unless a system explicitly
defines another use, and never store mutable per-instance runtime state
inside a shared ScriptableObject.

**Prefabs/Inspector** — reusable prefabs for repeated entities; no hidden
scene-specific dependencies; private serialized fields; headers/tooltips
where they help designers; no magic numbers for designer-configurable
values.

**Object creation** — pool frequently created/destroyed objects (units,
projectiles, VFX, floating numbers, temp gameplay objects) via the
project's `PoolManager`. Pooled objects must correctly reset state,
references, events, coroutines, timers, and animations. Don't force
pooling where it has no benefit.

**Coroutines/async** — avoid duplicate coroutines; track them when
cancellation matters; clean up time-based operations; no threads for
ordinary gameplay; never touch Unity objects from background threads.

**Events** — subscribe/unsubscribe symmetrically; avoid uncontrolled
static events and hidden event chains; keep ownership clear; events
should communicate meaningful state changes.

**State machines** — use explicit states for meaningful mutually
exclusive states rather than piling up interacting booleans.

**Randomness** — centralize gameplay-critical randomness; separate it
from presentation randomness; never use random values as hidden
balancing decisions; use deterministic seeds when required.

**Input** — use the project's existing Unity Input System only; keep
Input → Action → Gameplay Result separated; no gameplay rules inside
touch/pointer callbacks directly.

**UI** — UI displays gameplay state, it does not own it; no gameplay
rules in UI scripts; update UI on state change, don't rebuild every
frame.

**Physics** — use physics only where required; don't casually mix
transform movement and Rigidbody movement; use the matching update loop.

**Scene management** — no casual `DontDestroyOnLoad`; clear ownership of
persistent systems; prevent duplicate managers; clean up scene-transition
subscriptions and temporary state.

**Asset loading** — no new `Resources.Load()` or Addressables usage
unless already used or explicitly approved; follow the project's
existing strategy.

**Serialization** — respect Unity serialization behavior; be careful
renaming serialized fields (use `[FormerlySerializedAs]`); don't casually
change a serialized field's type; don't assume constructors run for
Unity-deserialized objects.

---

## 8. Performance

Avoid unnecessary per-frame allocations, LINQ in hot paths, repeated
scene searches/component lookups, excessive Instantiate/Destroy,
unnecessary UI rebuilding, and unnecessary physics queries. Don't
prematurely optimize. When performance is an explicit requirement,
identify the suspected bottleneck and how it can be measured.

---

## 9. Error handling, debugging, and honesty

Use contextual logs during development; avoid excessive production
logging; never log continuously in hot loops unless actively debugging.
Don't swallow exceptions or silently invent fallback gameplay behavior
for invalid configuration. Use assertions to catch programmer/config
errors. Remove temporary debugging code once the task is complete unless
told to keep it.

**Never claim code was compiled, tested, profiled, executed, or
validated unless that actually happened.**

---

## 10. Testing / verification

Before declaring a task complete, consider: initialization, normal
gameplay, invalid input, missing references, repeated execution,
reset/restart, disable/destroy, scene transitions, save/load, and edge
cases. If runtime testing in Unity is unavailable, explicitly state what
was verified, what was not, and what still needs manual testing.

---

## 11. Save / load

Persisted data represents gameplay state, not presentation state. Never
persist GameObjects, Components, coroutines, or transient Unity
references — reconstruct runtime objects from saved state instead. Never
silently overwrite valid player data.

---

## 12. Git / repository safety

Keep changes focused; don't modify unrelated files or reformat unrelated
code; don't overwrite user changes; don't modify project-wide settings
outside task scope; preserve `.meta` files; avoid unnecessary
package/configuration changes.

---

## 13. AI implementation discipline

Never invent Unity APIs, package APIs, project systems, gameplay rules,
balancing values, dependencies, or architecture. Never assume a package
is installed beyond what `PROJECT_INDEX.md` confirms. Never generate
speculative code or rewrite working systems unnecessarily.

When uncertain: **stop, identify the uncertainty, and ask or report it.**
Prefer the smallest implementation that satisfies the confirmed
requirement.

---

## 14. Documentation synchronization

After implementation, update the docs that are actually affected:
- Relevant `SYS_*.md` file(s) if the system's responsibility, interface,
  dependency, or behavior changed.
- `PROJECT_INDEX.md`'s systems-status table and "Open Unknowns" if a
  system's build status changed or a new ASSUMED/UNKNOWN value was
  introduced.
- `SYSTEM_MAP.md` if dependency edges between systems changed.

Do not rewrite unrelated documentation, and do not silently change the
documented design to match an implementation that contradicts it — report
the conflict instead.

---

## 15. Task completion report

Every completed task ends with:

```
## Completed
## Files Changed
- Created:
- Modified:
- Deleted:
## Verification
## Not Verified
## Documentation
## Remaining Issues
## Suggested Next Tasks
(up to 3, each with: task name, why useful, dependencies, confirmed/inferred)
```

Never automatically start any suggested task — wait for the user to
choose.

---

## 16. Definition of done

A task is complete only when:
1. The implementation matches `CURRENT_TASK.md`.
2. Relevant system boundaries ("Does NOT know about") are respected.
3. No undocumented gameplay assumptions were introduced.
4. No unrelated functionality was changed.
5. References, events, coroutines, and runtime state are handled
   correctly.
6. Obvious compile and Unity lifecycle issues were checked.
7. Relevant documentation was updated where necessary.
8. Temporary debugging code was removed.
9. Verification status is honestly reported.
10. The user is given control over what happens next.
