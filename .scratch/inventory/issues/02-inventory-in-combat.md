# 02: Inventory in combat

**What to build:** At the combat prompt, a player can type `i` or `inventory` to see the same Inventory shown at the crossroads, then the combat prompt again. This works whether or not the drink option is offered. Checking the Inventory is free: the player keeps their turn, the dragon does not attack, and nothing about the character or the fight changes. Both forms of the combat prompt (with and without the drink option) mention `i` in English, Spanish, French and Italian, without accented letters. See the parent spec: `.scratch/inventory.md`.

**Blocked by:** 01: Inventory at the crossroads (reuses the Inventory view and its text).

**Status:** done

- [x] `i` at the combat prompt prints the Inventory and shows the prompt again, with 0 Healing Potions and with some
- [x] Checking the Inventory gives the dragon no extra attack and does not use the player's turn
- [x] Checking the Inventory never triggers the invalid-choice message and leaves the Healing Potion count unchanged
- [x] Both forms of the combat prompt mention `'i'` in all four languages
- [x] The full test suite passes
