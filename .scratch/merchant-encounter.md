---
title: Merchant Encounter on the South path
labels: [ready-for-agent]
status: implemented, pending review
created: 2026-11-16
---

## Problem Statement

The South path is a dead end. A player who goes south reads that nothing happens, that they are bored and return home, and is then sent back to the crossroads. The only meaningful choice in the game is whether to fight the dragon in the north, and a character goes into that fight with whatever health and weapon they were given at creation. There is nothing a player can do to prepare, and no reason to ever go south.

## Solution

On their first journey down the South path, the player meets the **Merchant**, a travelling trader who sells magical goods for **Gold**. Every new character starts with a purse of 100 Gold. In the **Merchant Encounter**, the player may buy exactly one item, or leave with nothing:

- **Magical Armor** (60 Gold): the character gains 10 health points when they put it on.
- **Magical Weapon** (80 Gold): the character's occupation weapon is enchanted, raising its maximum damage by 4.

After the encounter, the player returns to the crossroads with their improved character and can head north to face the dragon. If they go south again, the Merchant has moved on. The whole encounter is available in all four supported languages.

## User Stories

1. As a player, I want my new character to start with a purse of Gold, so that I have something to spend on my adventure.
2. As a player, I want to see how much Gold my character has in the Character Summary, so that I know what I can afford.
3. As a player, I want the South path to lead somewhere interesting, so that choosing it is a real decision and not a wasted turn.
4. As a player, I want to meet a Merchant the first time I go south, so that I have a chance to prepare before facing the dragon.
5. As a player, I want the Merchant to tell me how much Gold I have before I choose, so that I can decide without looking it up.
6. As a player, I want each item's price and effect listed in the menu, so that I can compare Magical Armor with a Magical Weapon.
7. As a player, I want to buy Magical Armor, so that my character has more health points going into the dragon fight.
8. As a player, I want to buy a Magical Weapon, so that my character can deal more damage to the dragon.
9. As a Fighter, I want my long sword enchanted into a magical long sword, so that my upgrade fits my occupation.
10. As a Magician, I want my lightning bolt spell to become a greater lightning bolt spell, so that I am not handed a physical weapon that makes no sense for a Magician.
11. As a Thief, I want my dagger enchanted into a magical dagger, so that my upgrade fits my occupation.
12. As an Archer, I want my long bow enchanted into a magical long bow, so that my upgrade fits my occupation.
13. As a player, I want my Magical Weapon to keep its familiar ASCII art with a visible enchantment mark, so that I can see in combat that my weapon is magical.
14. As a player, I want to choose by number (`1`, `2`, `3`) or by word (`armor`, `weapon`, `leave`), so that the menu works the same way as the path prompt.
15. As a player, I want my choice to be accepted regardless of capital letters or extra spaces, so that small typing differences don't count as mistakes.
16. As a player, I want to leave the Merchant without buying anything, so that I keep my Gold if I don't want either item.
17. As a player, I want an invalid choice to show a clear message and the menu again, so that a typo doesn't end the encounter or buy the wrong item.
18. As a player, I want to be told when I can't afford an item and then choose again, so that I can pick something I can afford instead.
19. As a player, I want a confirmation after buying that shows my new health or weapon damage and my remaining Gold, so that I know the purchase worked.
20. As a player, I want the Merchant to sell me only one item, so that choosing between armor and weapon is a real trade-off.
21. As a player, I want to go back to the crossroads after the encounter, so that I can go north with my improved character.
22. As a player who goes south a second time, I want to be told the Merchant has moved on, so that it's clear why there's nothing to buy.
23. As a player who left without buying anything, I want the Merchant to be gone on later trips too, so that the rules are the same whether or not I bought something.
24. As a player who creates a new character from the main menu, I want that new character to meet the Merchant, so that every character gets the same chance.
25. As a player who bought a Magical Weapon, I want its name and maximum damage shown in the combat stats, so that I can see my advantage over the dragon.
26. As a player who bought Magical Armor, I want my higher health points shown in the combat stats, so that I can see my advantage over the dragon.
27. As a player who bought Magical Armor, I want "Armor: Magical Armor" shown in my Character Summary, so that I know my character owns it.
28. As a Spanish-, French- or Italian-speaking player, I want the whole Merchant Encounter shown in my language, including the magical weapon names, so that the game doesn't switch to English partway through.
29. As a player on a terminal without full UTF-8 support, I want the new text to avoid accented letters like the rest of the game does, so that it doesn't show up garbled.
30. As a player whose input closes in the middle of the encounter, I want the game to treat that as leaving, so that the game doesn't crash or loop forever.
31. As a maintainer, I want a test that fails if an occupation weapon has no magical version, so that adding an occupation later can't silently break the Merchant.
32. As a maintainer, I want a test that fails if any Merchant text is missing from any language, so that a missing translation is caught before players see a `[key_name]` placeholder.

## Implementation Decisions

- **New Merchant module** with two parts:
  - A pure purchase operation. It takes a character and an item (Magical Armor or Magical Weapon), applies the purchase if the rules allow it, and returns one of four results: `Bought`, `NotEnoughGold`, `AlreadyBought` or `NoWeapon`. It never reads or writes the console. When a purchase is refused, the character is left unchanged.
  - A thin console encounter that shows the menu, reads input, calls the purchase operation and prints the result.
- **Rules apply in this order:**
  1. If the character already owns Magical Armor or a Magical Weapon, the result is `AlreadyBought`.
  2. For a weapon purchase, if the character has no weapon (or one with no magical version), the result is `NoWeapon`.
  3. If the character's Gold is below the price, the result is `NotEnoughGold`. Having exactly the price is enough.
- **Prices and bonuses** are named constants on the Merchant: armor costs 60 and adds 10 HP; the weapon costs 80 and adds 4 to maximum damage. The starting purse is a named constant on the character: 100.
- **Magical Weapons are looked up from the character's current weapon type, not from the occupation:** long sword → magical long sword, lightning bolt spell → greater lightning bolt spell, dagger → magical dagger, long bow → magical long bow. The new weapon is the old one with +4 maximum damage, `*` added to its ASCII art, and a flag marking it as magical.
- **Magical Armor only adds health.** Its effect is applied once, at purchase; it does not reduce damage in combat. The character records that it owns the armor so the Character Summary can show it. See the glossary entry for Magical Armor.
- **Character changes:** the character gains Gold, a flag for owning Magical Armor, and a flag for having met the Merchant. The met-the-Merchant flag is set when the encounter starts, whether or not anything is bought, and belongs to the character, so it resets when a new character is created.
- **Game changes:** the South path branch hands off to a South-path handler. On the first trip it runs the Merchant Encounter. On later trips it shows the "merchant has moved on" message and then the existing South path story. Both cases return to the crossroads.
- **Menu input:** `1`/`armor`, `2`/`weapon`, `3`/`leave`, trimmed and case-insensitive. Invalid input shows the existing `invalid` message and the menu again. Closed input (end of input) counts as leaving, matching how combat handles retreat. If the player can't afford an item, or has no weapon to enchant, the game says so and shows the menu again. There is no "are you sure?" step.
- **Text:** all player-facing text lives in the language data file, as ADR 0001 requires. 14 new message keys (merchant text, Gold and Armor labels, and the Magical Armor name) plus 4 Magical Weapon display names are added to English, Spanish, French and Italian, without accented letters.
- **Combat is unchanged.** Magical items affect combat only through the existing health and weapon stats.
- No ADR: the decisions are cheap to reverse. The domain terms are recorded in the glossary.

## Testing Decisions

- **A good test checks only what the player or the game can see:** the purchase result, the character's Gold, health, weapon and armor afterwards, and the text printed. Tests don't depend on how the code is structured inside.
- **Test seams.** These are the highest seams already in the code, and no new ones are needed:
  1. **The Merchant's purchase operation**: no console, so the purchase rules can be tested directly and predictably.
  2. **The Merchant's console encounter**, driven by redirecting console input and output.
  3. **The game's South-path handler**, for first and later trips.
  4. **The language data**, loaded through the existing Messages module.
- **Purchase cases covered:**
  - Armor adds 10 HP, costs 60 and leaves the weapon unchanged.
  - The weapon upgrade for each of the 4 base weapons (name, damage, art, magical flag, cost, health unchanged).
  - A second purchase is refused, for every pair of items.
  - One Gold short is refused; exactly the price is enough.
  - A weapon purchase with no weapon is refused.
  - Every weapon the character-creation code hands out has a magical version.
- **Encounter cases covered:**
  - Each menu choice, by number and by word, including different capitalisation and extra spaces.
  - Leaving, including closed input.
  - Invalid input followed by a valid choice.
  - Not enough Gold followed by an affordable choice.
- **South path cases covered:** first trip, then second trip (no menu, no purchase possible); and leaving without buying, then a second trip.
- **Character Summary:** Gold is always shown; Armor is shown only after buying it.
- **Languages:** every new key exists in all four languages; the magical weapon names are translated in Spanish, French and Italian; and a full encounter in each language shows no `[key_name]` placeholder. This also gives Italian its first automated text test, a follow-up ADR 0001 asked for.
- **Prior art:** the existing dragon-encounter and combat tests, which redirect console input and output in classes that don't run in parallel, and the existing tests that check message keys exist in each language.
- **Manual check:** one scripted playthrough per language (create character → south → buy → south again → north → fight) shows no `[key_name]` placeholders.

## Out of Scope

- Any other use for Gold: earning more Gold, spending leftover Gold, or adding the dragon's hoard to the purse.
- Armor that reduces damage or affects combat in any way beyond the health added at purchase.
- More than one item per encounter, more than one Merchant, or a Merchant who comes back.
- Selling, swapping or removing items.
- Gold or Armor in the combat stats screen.
- Translating typed commands (`armor`, `weapon`, `leave` stay in English in every language, as with `roll` and `n`/`s`/`e`).
- Accented letters in any language.
- Making the dice injectable for tests. The new code adds no randomness.

## Further Notes

- **Known gap:** the Character Summary can only be opened during character creation, which is before the Merchant. In a real game the player therefore never sees "Gold" or "Armor: Magical Armor" after a purchase. They see the purchase message, and for the weapon, the combat stats. This should be fixed in a follow-up, for example by adding a "view stats" choice at the crossroads.
- The South path story now appears only on later trips south, after the "merchant has moved on" message. Its wording ("you return home, poor, and still bored") is unchanged, even though the game then shows the crossroads again. This was true before the change and is left as is.
- Domain terms used here (Path, Merchant, Merchant Encounter, Gold, Magical Armor, Magical Weapon) are defined in the glossary.
