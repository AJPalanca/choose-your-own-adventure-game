---
title: Inventory display
labels: [ready-for-agent]
status: implemented, pending review
created: 2026-11-16
---

## Problem Statement

A player can only see what their character is carrying in the Character Summary, which is offered once, right after character creation. After the Merchant Encounter, after finding a Healing Potion on the South path, or in the middle of the dragon fight, the player has no way to check their weapon, whether they wear Magical Armor, how many Healing Potions they carry, or how much Gold they have left.

## Solution

Add the **Inventory**: a short view of everything a character is carrying or wearing (their weapon, any Magical Armor, their Healing Potions, and their Gold). The player opens it by typing `i` or `inventory` at the crossroads or at the combat prompt. The game shows the Inventory and then asks the same question again. In combat, checking the Inventory is free: it doesn't use the player's turn.

```
----- Inventory -----
Weapon: magical long sword (Maximum Damage: 16)
Armor: none
Healing Potions: 2 of 3
Gold: 20
```

## User Stories

1. As a player at the crossroads, I want to type `i`/`inventory` to see what I'm carrying, so that I can decide whether to go south again or face the dragon.
2. As a player in combat, I want to type `i`/`inventory` to see what I'm carrying, so that I can decide whether to drink a potion.
3. As a player in combat, I want checking my Inventory to leave my turn unused, so that looking never costs me anything.
4. As a player, I want the same prompt again after the Inventory, so that I can carry on where I was.
5. As a player, I want every Inventory line shown every time, including `Armor: none`, so that I can see what I'm missing.
6. As a player, I want my weapon's Maximum Damage shown, so that I can tell a Magical Weapon from the ordinary one.
7. As a player, I want potions shown as "N of 3", so that I know how many more I can carry.
8. As a player, I want the crossroads and combat prompts to tell me about `i`, so that I know the command exists.
9. As a Spanish-, French- or Italian-speaking player, I want the Inventory in my language, without accented letters.

## Implementation Decisions

- **Inventory view:** a pure method on the character that returns the Inventory text with no console output. Lines: a header, then the weapon with its Maximum Damage (`none` if the character has no weapon), the armor (Magical Armor, or `none`), the Healing Potions as "N of 3", and the Gold.
- **Crossroads:** the path prompt accepts `i`/`inventory`, prints the Inventory, then shows the path prompt again.
- **Combat:** the attack prompt accepts `i`/`inventory` whether or not the player carries a potion. It prints the Inventory and shows the prompt again. No turn is used and the dragon does not attack.
- **Not available** in the Merchant menu or the post-creation menu.
- **Character Summary:** unchanged.
- **Text:** new keys `inventory_header`, `inventory_weapon`, `inventory_none` and `inventory_potions` in all four languages. The existing `stats_armor`, `magical_armor` and `stats_gold` are reused. The `path_prompt_full`, `path_invalid`, `attack_prompt` and `attack_prompt_with_potion` messages now mention `i`. No accented letters (ADR 0001). The command stays in English in every language.

## Testing Decisions

Follow the seams from the Healing Potion spec: test only what the player sees, redirect console input and output, and use non-parallel test classes.

- **View:** an ordinary weapon, a Magical Weapon, worn armor and `none`, potions as "N of 3", Gold, and no weapon shown as `none`.
- **Crossroads:** `i` and `inventory` (any case) print the Inventory and show the prompt again; the player can still choose a path afterwards.
- **Combat:** `i` prints the Inventory, the dragon attacks no extra time, and it works both with and without potions.
- **Languages:** every new key exists in all four languages; no `[key_name]` placeholder appears.

## Out of Scope

- The Inventory in the Merchant menu or the post-creation menu.
- Changes to the Character Summary.
- Translating typed commands.
