---
title: Inventory display
labels: [ready-for-agent]
status: implemented, pending review
created: 2026-11-16
---

## Problem Statement

A player can only see what their character is carrying in the Character Summary, and it's offered only once, right after character creation. Later, a player can't check their Inventory: after the Merchant Encounter, after picking up the Healing Potion on the South path, or in the middle of the dragon fight. They have no way to see which weapon they hold, whether they wear Magical Armor, how many Healing Potions they carry, or how much Gold is left. To decide whether to go south again, head north, or drink a potion now, they have to remember it all themselves.

## Solution

Add the **Inventory**: a short view of everything a character is carrying or wearing. That is their weapon, any Magical Armor, their Healing Potions and their Gold. The player opens it by typing `i` or `inventory` at the crossroads or at the combat prompt. The game shows the Inventory and then asks the same question again. In combat, checking the Inventory is free: the player keeps their turn and the dragon does not attack. The Inventory is available in all four supported languages.

```
----- Inventory -----
Weapon: magical long sword (Maximum Damage: 16)
Armor: none
Healing Potions: 2 of 3
Gold: 20
```

## User Stories

1. As a player at the crossroads, I want to type `i` to see my Inventory, so that I can check what I have before choosing a Path.
2. As a player at the crossroads, I want to type `inventory` as well as `i`, so that the full word works too.
3. As a player, I want the command to ignore case and extra spaces, as the other commands do, so that `I` or ` INVENTORY ` still works.
4. As a player at the crossroads, I want the Path prompt shown again after the Inventory, so that I can choose a Path straight away.
5. As a player at the crossroads, I want checking my Inventory never to count as an invalid choice, so that I'm not told off for using a real command.
6. As a player coming back from the Merchant Encounter, I want to check my Inventory, so that I can confirm what I bought and how much Gold is left.
7. As a player who has gone south a second time, I want to check my Inventory, so that I can see the Healing Potion I picked up.
8. As a player in combat, I want to type `i` or `inventory` at the attack prompt to see my Inventory, so that I can decide whether to drink a Healing Potion.
9. As a player in combat, I want checking my Inventory to leave my turn unused, so that looking never costs me anything.
10. As a player in combat, I want the dragon not to attack when I check my Inventory, so that looking is never dangerous.
11. As a player in combat, I want the attack prompt shown again after the Inventory, so that I can carry on fighting.
12. As a player in combat carrying no Healing Potions, I want to check my Inventory anyway, so that the command works the same in every fight.
13. As a player, I want checking my Inventory never to change my character, so that it's safe to check at any time.
14. As a player, I want my weapon shown by name, so that I know what I'm fighting with.
15. As a player, I want my weapon's Maximum Damage shown, so that I can tell a Magical Weapon from the ordinary one.
16. As a player who bought a Magical Weapon, I want its magical name and higher damage shown, so that I can see the purchase took effect.
17. As a player wearing Magical Armor, I want it listed, so that I know I'm wearing it.
18. As a player without Magical Armor, I want to see `Armor: none`, so that I can see at a glance what I'm missing.
19. As a player, I want my Healing Potions shown as "N of 3", so that I know how many more I can carry before the next trip south.
20. As a player carrying no Healing Potions, I want to see "0 of 3", so that the line is always there in the same place.
21. As a player, I want my Gold shown, so that I know what I could still afford.
22. As a player, I want every line shown every time, in the same order, so that the Inventory always reads the same way.
23. As a player, I want the crossroads prompt to mention `i`, so that I know the Inventory exists.
24. As a player, I want the crossroads invalid-choice message to list `i`, so that I'm reminded of every valid option.
25. As a player in combat, I want the attack prompt to mention `i`, with or without the drink option, so that I know I can check my Inventory mid-fight.
26. As a Spanish-, French- or Italian-speaking player, I want the Inventory and the updated prompts in my language, so that the whole game reads in one language.
27. As a player in any language, I want no accented letters, so that the text shows correctly in every Windows terminal.
28. As a player in any language, I want to type `i`/`inventory` in English, as I do every other command, so that commands work the same in every language.
29. As a player after character creation, I want the Character Summary unchanged, so that the familiar full view stays the same.

## Implementation Decisions

- **Domain terms:** **Inventory** and **Character Summary** are now defined in the glossary. The Inventory is everything a character carries or wears, Gold included. The Character Summary is the full description of a character, and it contains their Inventory.
- **Character: Inventory view.** A pure operation on the character takes the current language's messages and returns the Inventory text. It prints nothing and changes nothing. Its lines, in order:
  - a header;
  - the weapon's translated name and Maximum Damage, or `none` if the character has no weapon (this can't happen in normal play);
  - the armor: Magical Armor, or `none`;
  - the Healing Potions as "N of 3", where 3 is the existing carry-limit constant;
  - the Gold.
- **Game: crossroads.** The Path prompt is now public and takes the character as well as the messages, so it can show the Inventory. On `i`/`inventory`, it prints the Inventory and shows the prompt again. It still returns only `north`, `south` or `exit`.
- **Combat: attack prompt.** On `i`/`inventory`, the prompt prints the Inventory and shows itself again. This works whether or not the drink option is offered. No turn is used, the dragon does not attack, and the combat result is unchanged.
- **Where the Inventory isn't available:** the Merchant menu (it already shows Gold, and a numbered fifth option didn't fit) and the post-creation menu (it already offers the Character Summary).
- **Character Summary:** unchanged. It still leaves out the armor line when none is worn.
- **Text** (in the language data file, for all four languages):
  - New keys: `inventory_header`, `inventory_weapon` (placeholders for the weapon name and Maximum Damage), `inventory_none` and `inventory_potions` (placeholders for the count and the limit).
  - Reused: the existing armor, Magical Armor and Gold labels.
  - Updated to mention `i`: the crossroads prompt, the crossroads invalid-choice message, and both forms of the attack prompt.
  - No accented letters, and typed commands stay in English, as ADR 0001 requires.
- **Validators:** the unused path- and combat-choice validators on Game are left as they are. They already don't accept `d`, and they aren't part of the game flow.

## Testing Decisions

- **What makes a good test:** test only what the player or the game can see: the printed text, the prompts that appear, the choice returned, and the character's state afterwards. Don't test how the view is put together. Redirect console input and output as needed, and keep test classes non-parallel, because they share the console.
- **Seams (confirmed with the user):**
  1. **Inventory view on the character** (new). Exact English output for a new Fighter; a Magical Weapon's name and damage after buying it from the Merchant; a Healing Potion bought; Gold after purchases; Magical Armor worn; `Weapon: none` for a character with no weapon.
  2. **The crossroads Path prompt** (now public). `i`, and `inventory` in any case with extra spaces, print the Inventory, show the prompt again, and still return the Path chosen next. They never trigger the invalid-choice message.
  3. **Starting combat** (existing). With 0 and with 2 Healing Potions, `i` then `r` prints the Inventory; the dragon attacks only once; no invalid-choice message appears; the potion count is unchanged.
- **Languages:** every new key exists in all four languages; the Inventory shows no `[key_name]` placeholder in any language; all four updated prompts mention `'i'` in every language.
- **Prior art:** the Healing Potion tests. They provide the console-redirect helper, the harmless-fight setup (a weaponless player and a Strength-0 dragon, so every round reaches the prompt whatever the dice roll), counting the dragon's attack intro to check turns, and the per-language checks for missing keys.
- Dice aren't injectable, so the combat tests check what happens around the command (prompts, counts, state), not exact rolls.

## Out of Scope

- The Inventory in the Merchant menu or the post-creation menu.
- Any change to the Character Summary, including showing `Armor: none` there.
- Translating typed commands into other languages.
- Showing Health or Maximum Health in the Inventory. They describe the character, not what they carry; combat stats already show both.
- Updating the unused path- and combat-choice validators.

## Further Notes

- Builds on the Healing Potion feature (it shows the potion count and limit), so it should be merged after it.
- No ADR: every decision here is cheap to reverse.
- Implemented and verified: the full test suite passes (176 tests), and a scripted Spanish playthrough showed the Inventory at the crossroads and mid-combat with the prompt returning each time.
