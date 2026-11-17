# 01: Inventory at the crossroads

**What to build:** At the crossroads, a player can type `i` or `inventory` (any case, extra spaces ignored) to see their Inventory: everything their character is carrying or wearing. The game then shows the Path prompt again, and the player can choose a Path as usual. Checking the Inventory never counts as an invalid choice and never changes the character. Every line is shown every time, in this order:

```
----- Inventory -----
Weapon: <weapon name> (Maximum Damage: N)
Armor: Magical Armor | none
Healing Potions: N of 3
Gold: N
```

A character with no weapon shows `Weapon: none`. The crossroads prompt and its invalid-choice message mention `i`. The Inventory and the updated text are in English, Spanish, French and Italian, without accented letters (ADR 0001), and the command stays `i`/`inventory` in every language. The Character Summary is unchanged. The Inventory isn't offered in the Merchant menu or the post-creation menu. See the parent spec: `.scratch/inventory.md`.

**Blocked by:** None (can start immediately). Requires the Healing Potion feature, which it displays.

**Status:** done

- [x] The character can produce its Inventory text without printing anything or changing state
- [x] The weapon line shows the translated weapon name and Maximum Damage, including a Magical Weapon's name and higher damage
- [x] The armor line shows Magical Armor when worn, or `none`
- [x] Healing Potions are shown as "N of 3", including "0 of 3"
- [x] Gold is shown, reflecting any purchases
- [x] `i` and ` INVENTORY ` at the crossroads print the Inventory, show the Path prompt again, and still return the Path chosen next, with no invalid-choice message
- [x] The crossroads prompt and invalid-choice message mention `'i'` in all four languages
- [x] All new text keys exist in all four languages, and the Inventory shows no `[key_name]` placeholder in any of them
- [x] The full test suite passes
