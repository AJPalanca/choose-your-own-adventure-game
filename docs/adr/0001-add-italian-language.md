# ADR 0001: Add Italian as a Supported Language

- **Status:** Accepted
- **Date:** 2026-11-09
- **Decision maker:** Adam Palanca

## Context

The Choose Your Own Adventure game is a C# console app. It already supported three
languages: English, Spanish, and French. The player picks a language at the start,
and all game text then appears in that language.

The game's design makes adding languages straightforward:

- **All player-facing text lives in `language_data.json`**, not in the C# code. Each
  language has its own section with the same six parts:
  - `dictionary`: every sentence the game shows, looked up by a key such as `"welcome"`.
  - `raceMap` / `occupationMap`: what a player may *type* (for example `"elfo"`), mapped to
    the internal English name (`"Elf"`).
  - `displayRaceMap` / `displayOccupationMap` / `displayWeaponMap`: internal English
    names mapped to what is *shown* on screen.
- **`Messages.cs` loads whichever language section it is given by name.** It has no
  per-language code.
- **`Game.cs` has the language menu**, which turns the player's number choice into a
  language section name.

We wanted to widen the game's audience to Italian speakers and to show that the
translation system can grow without rewriting game logic.

## Decision

Add **Italian** as a fourth language, selectable as option **4** in the language menu.

### How it was implemented

1. **`language_data.json`**: added a new `"Italian"` section after `"French"`, with all six
   parts and the same keys as English (69 messages, 4 races, 4 occupations, 5 weapons).
   Existing languages were not modified.
2. **`Game.cs`**: two small changes to the language menu:
   - Added `Scegli la lingua` to the heading and `4. Italiano` to the options.
   - Added `"4" => "Italian"` to the `switch` that picks the language. Unknown input still
     falls back to English.
3. **No changes to `Messages.cs`** were needed.

### Translation choices

| Topic | Choice | Reason |
|---|---|---|
| Races | Umano, Elfo, Nano, **Mezzuomo** | *Mezzuomo* is the common Italian fantasy term for "Halfling." |
| Occupations | Guerriero, Mago, Ladro, Arciere | Standard Italian words. |
| Weapons | spada lunga, incantesimo del fulmine, pugnale, arco lungo, artigli | Direct translations. |
| Accented letters | Not used (`e` for `è`, `piu` for `più`) | Matches the existing convention ("Espanol", "Francais") and avoids garbled text in some Windows terminals. |
| Typed commands | Still `roll`, `n`, `s`, `e`, `a`, `r` | These are hard-coded in C#. Italian prompts tell the player to type these exact letters. |
| Weapon sentence | "Come {0}, impugni la tua arma ({1}) e attacchi il drago." | Italian articles depend on gender (*la* spada, *il* pugnale). Putting the weapon in parentheses avoids picking the wrong article. |

## Alternatives Considered

1. **Do not add Italian.** Simplest, but it misses the goal of reaching more players and
   showing that the design can grow.
2. **Put Italian in its own file (`italian.json`).** Rejected: `Messages.cs` reads one file,
   so this would need code changes and would break the "one file for all text" pattern.
3. **Use proper Italian accents (è, à, più).** More correct Italian, but inconsistent with the
   other languages and risky on terminals without UTF-8. Could be revisited for all languages
   together.
4. **Translate the typed commands (for example `tira` instead of `roll`).** Rejected for now:
   it would mean moving command words from C# into the JSON for every language, which is a
   larger change than this decision covers.

## Consequences

### Positive

- Italian speakers can play the whole game in their language.
- Shows that a new language needs **only JSON plus one menu line**, with no game-logic changes.
- Low risk: existing languages were untouched, and all 94 existing tests still pass.

### Negative / trade-offs

- **More to maintain.** Every new message key must now be added in **four** places. A
  missing Italian key shows on screen as `[key_name]` instead of crashing, but it still looks
  broken.
- **No automated Italian tests yet.** The existing tests cover Spanish and French but not Italian.
  Recommended follow-up: copy the French tests in `CYOATests/MessagesTests.cs` and
  `CYOATests/GameTests.cs` for Italian, especially `ReadDictionary_AllRequiredKeysExistInFrench`.
- **Italian without accents** reads slightly unnatural to native speakers.
- **Mixed-language commands:** Italian players type English words such as `roll`.

### Checklist for adding a future language

1. Copy an existing language section in `language_data.json` and translate every value.
   Keep all keys and `{0}` / `{1}` placeholders unchanged.
2. Add the option to the menu text and the `switch` in `Game.StartGame()` (`Game.cs`).
3. Add tests for the new language.
4. Play through once in the new language and check for `[key_name]` markers.

## Verification

- JSON check: the Italian section has exactly the same message and display keys as English.
- `dotnet build`: 0 warnings, 0 errors.
- `dotnet test`: 94 passed, 0 failed.
- Scripted Italian playthroughs covered every path: menu, invalid input, all races and
  occupations, stats, the south, north, and exit paths, retreat, victory, and defeat. No missing
  keys or leftover English text were found.

## Related

During Italian testing, two language-independent bugs were found and fixed separately
(not part of this decision):

- Halflings could start with 0 or negative health or strength (`Player.cs`).
- Invalid input during combat still triggered an attack (`Combat.cs`).
