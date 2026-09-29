# CLAUDE.md — Hearth & Havoc: Goblin Legacy

These rules govern how Claude works in this repository. They apply to every session and override any default habits.

## The one rule

**No code enters this game unless the developer designed it, understands it, and explicitly approved it.**

## How we work

1. **Design first, code second.** Conversations start at a high level: goals, design choices, trade-offs. No code is written until the design is settled and the developer has approved it.
2. **Code exactly what was designed.** Implement the agreed design and nothing more. No extra features, "while I'm here" refactors, cleanups, renames, or reformatting.
3. **Ask before generating new code.** If Claude thinks something new is needed that wasn't part of the agreed design (a helper class, a fix, a new file, a dependency), it stops and discusses it first, explaining what it is and why it's needed.
4. **Nothing is modified without explicit approval.** This includes existing code, generated files, project/config files, assets, and this file. Reading the repo is always fine; changing it is not, until approved.
5. **Teach as you go.** For every change, explain what the code does and why it's written that way, including the reasoning behind any C#, MonoGame or algorithm choices. The developer should be able to explain every line in the codebase.
6. **No hidden changes.** Every change is described plainly. Nothing is slipped into a commit that wasn't discussed.
7. **Suggest helpers, don't add them silently.** When Claude sees code that could be deduplicated into a shared helper, or a helper method that would make code easier to read, it recommends it to the developer, who approves or declines it before it is written.

## Git workflow

- Never commit directly to the default branch (`main`).
- Each approved piece of work goes on its own branch and is opened as a pull request into `main` for the developer to code review.
- Branch names follow `MM-DD-YY/short-description`:
  - The date is the day the branch is created, zero-padded (e.g. `09-27-26`, `10-03-26`).
  - The description is at most 3 words, lowercase, joined by hyphens, and says what the branch contains.
  - Example: `09-27-26/adding-claude-md`
- One PR per approved change, kept small enough to review comfortably.
- PR descriptions list every file touched and summarize what changed and why.
- Claude does not merge PRs. The developer reviews and merges.

## When in doubt

Ask. A question costs less than code the developer didn't want or doesn't understand.
