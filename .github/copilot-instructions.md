# Copilot / Claude Instructions — Merge Wars (Generator Variant)

## Scope discipline — READ THIS FIRST
- Before writing or editing any code, read only the files inside `/GameDocs/`.
  Do NOT scan, index, or infer conventions from the rest of the repository
  unless a file in `/GameDocs/` explicitly points you to specific existing
  scripts to extend.
- Source of truth lives in `/GameDocs/`: PROJECT_INDEX.md, GAME_DESIGN.md
  (+ variant file), GAMEPLAY_LOOPS.md, SYSTEM_MAP.md, Systems/SYS_*.md,
  Implementation/IMPLEMENTATION_PLAN.md, Implementation/CURRENT_TASK.md,
  DECISIONS_LOG.md. If information needed isn't in these files, stop and ask
  rather than inventing it or pulling it from elsewhere in the codebase.
- Never assume the wider project's existing code patterns apply unless the
  active SYS_*.md file says so.

## Every task, in order
1. Read `Implementation/CURRENT_TASK.md` — this is the only task in scope.
2. Read the SYS_*.md file(s) named in that task. Read its "Does NOT know
   about" section as seriously as its "Responsibility" section — do not
   give a system access to state or systems it isn't supposed to know about.
3. Check for any ⚠️ ASSUMED PLACEHOLDER or UNKNOWN markers relevant to this
   task. If the task can't be completed correctly without resolving one,
   stop and surface it — don't guess a gameplay-defining value (mana costs,
   caps, thresholds) silently. Use a clearly marked `// TODO(design):` code
   comment plus a note back to the user for anything guessed.
4. Implement only what CURRENT_TASK.md scopes. Do not build ahead into
   systems not yet requested, even if related.
5. After implementing, update the relevant SYS_*.md file(s) if the
   implementation changed or clarified the system's actual responsibility,
   dependencies, or interface. Do not let code and docs drift.
6. End every task with a diff-style summary: what was created/changed, what
   MD files were touched, what's now stale elsewhere and needs a follow-up
   (but don't auto-update unrelated files — flag only).

## Architecture rules to preserve
- Small single-responsibility systems over large managers. If an
  implementation is growing a system beyond its documented responsibility,
  stop and flag it rather than absorbing the extra logic.
- Respect the CONFIRMED / ASSUMED / UNKNOWN distinctions in the docs.
  UNKNOWN values get a named constant with a TODO, never a hardcoded guess
  presented as final.
- This project has two parallel design variants (original + generator/
  continuous-conquest). Always confirm which variant's SYS_*.md you're
  building against — check CURRENT_TASK.md for which one is active. Do not
  mix mechanics from the two.

## Unity conventions
- Standard Unity 3D components only (MeshRenderer, BoxCollider/
  SphereCollider, Rigidbody) unless a SYS file says otherwise.
- Keep gameplay data (grid state, slot contents, base layout) as plain
  serializable data classes separate from MonoBehaviour presentation, so
  data can later be persisted (save/resume is a confirmed variant
  requirement) without refactoring.
