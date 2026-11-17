---
title: Healing Potion
labels: [ready-for-agent]
status: implemented, pending review
created: 2026-11-16
---

## Problem Statement

Once the dragon fight starts, a player can only attack or retreat. Health lost to the dragon's claws never comes back, so a fight that turns against the player can only end in defeat or retreat. The Merchant's goods only strengthen a character before the fight, and the South path offers nothing after the first trip.

## Solution

Add the **Healing Potion**: a single-use drink that gives back health lost in combat, up to the character's **Maximum Health**. A character can carry up to 3 at a time. They get them in two ways:

- **Buying** them from the Merchant for 20 Gold each, during the Merchant Encounter. Potions do not count toward the one-magical-item limit.
- **Finding** one, once, on the second trip down the South path.

During combat, a player carrying at least one potion can choose to drink one at the attack/retreat prompt. Drinking restores 8 health (never above Maximum Health) and uses up the player's turn: the dragon attacks again before the next prompt.

## User Stories

1. As a player, I want to buy Healing Potions from the Merchant, so that I can prepare to recover health during the dragon fight.
2. As a player, I want to buy potions as well as Magical Armor or a Magical Weapon, so that potions aren't a worse choice competing with those items.
3. As a player, I want to buy potions before or after buying a magical item, in the same encounter, so that the order I choose in doesn't cost me anything.
4. As a player, I want the Merchant menu to stay open after each purchase until I choose to leave, so that I can buy several things.
5. As a player, I want a magical item to disappear from the menu once I've bought one, so that I'm not offered something I can't buy.
6. As a player, I want to be told when I can't afford a potion, or am already carrying 3, so that I understand why it wasn't bought.
7. As a player, I want a confirmation after buying a potion that shows how many I carry and how much Gold I have left, so that I know the purchase worked.
8. As a player going south a second time, I want to find a Healing Potion, so that the trip is worth making.
9. As a player who is already carrying 3 potions on that trip, I want the potion to stay where it is so that I can come back for it, so that I don't lose it for having a full load.
10. As a player going south after I've picked up the potion, I want the existing South path story, so that I can't collect free potions forever.
11. As a player in combat who is carrying a potion, I want a `d`/`drink` option at the attack/retreat prompt, so that I can heal when the fight turns against me.
12. As a player carrying no potions, I want the prompt to show only attack and retreat, so that I'm not offered something I can't do.
13. As a player who drinks a potion, I want to see how much I healed and my new health, so that I know it worked.
14. As a player at Maximum Health, I want drinking to be refused without using a potion or my turn, so that I don't waste either by mistake.
15. As a player, I want drinking to use up my turn, so that choosing when to drink is a real decision.
16. As a player, I want healing to stop at my Maximum Health, so that potions recover lost health and don't stand in for Magical Armor.
17. As a player who bought Magical Armor, I want its 10 health included in my Maximum Health, so that potions can restore armor health I lost.
18. As a player, I want to see how many Healing Potions I carry in the combat stats and Character Summary.
19. As a player, I want the combat stats to show my Maximum Health, so that I know how much a potion can restore.
20. As a player brought to 0 health, I want to be defeated even if I'm carrying potions, so that potions are only drunk when I choose to.
21. As a Spanish-, French- or Italian-speaking player, I want all potion text in my language, without accented letters.
22. As a player whose input closes at the combat prompt, I want that to count as retreating, as it does now.

## Implementation Decisions

- **Character changes:**
  - A Maximum Health value, set when character creation ends (after health is rolled).
  - Magical Armor adds its 10 health to both current health and Maximum Health.
  - The number of Healing Potions carried, starting at 0, with a named constant for the carry limit of 3.
  - A setting that records whether this character has picked up the potion found on the South path.
- **Healing:** a pure operation that drinks one potion, if the character carries one and is below Maximum Health. It restores `min(8, Maximum Health - health)` and reports the outcome: `Drank` (with the amount healed), `NoPotion`, or `AlreadyAtFullHealth`. If the drink is refused, the character is not changed. The 8 is a named constant.
- **Merchant purchase rules:**
  - Add `HealingPotion` to the list of Merchant items, priced at 20 Gold (a named constant).
  - The `AlreadyBought` rule applies only to Magical Armor and Magical Weapon. Buying a potion never triggers it, and owning a potion never blocks a magical item.
  - New result for potions: `CarryingTooMany` when the character already carries 3. Rule order for a potion: carry limit first, then Gold.
- **Merchant Encounter menu:** `1`/`armor`, `2`/`weapon`, `3`/`potion`, `4`/`leave`. The encounter ends only when the player chooses leave or input closes. After a magical item is bought, both magical options are hidden, but the numbers of the other options don't change. Choosing a hidden option counts as invalid input. Existing tests that use `3` to leave must be updated to use `4`.
- **South path:** first trip, the Merchant Encounter. Later trips: if the character hasn't picked up the potion yet, they get the "merchant has moved on" message and then the potion. If they carry fewer than 3, they pick it up and the setting is recorded. If they already carry 3, they're told it stays where it is, and the setting is not recorded. Once the potion has been picked up, they get the "merchant has moved on" message and the existing South path story. Every case returns to the crossroads.
- **Combat:** the prompt offers `d`/`drink` only while the character carries a potion. Typing `d` with no potion counts as invalid input.
  - When the player drinks, the game prints the result. Then the dragon attacks. If the player survives, the prompt appears again. The player doesn't attack that round.
  - If the drink is refused (full health), the game prints a message and shows the prompt again. No turn is used.
  - Potions are never drunk automatically.
- **Stats displays:** combat stats show Maximum Health and the number of Healing Potions carried. The Character Summary shows the number of Healing Potions carried.
- **Text:** all new text goes in the language data file for English, Spanish, French and Italian, without accented letters, as ADR 0001 requires. Typed commands (`potion`, `d`/`drink`) stay in English in every language.

## Testing Decisions

Follow the existing seams and style from the Merchant Encounter spec: test only what the player or the game can see. Redirect console input and output where needed. Use the existing classes that don't run in parallel.

- **Healing:** restores 8 when 8 or more below the maximum; restores only up to the maximum when closer; refused at full health (potion count and health unchanged); refused with no potion; Maximum Health includes the armor bonus.
- **Purchase:** a potion costs 20 and adds 1 to the count; a 4th is refused (`CarryingTooMany`) and Gold is unchanged; one Gold short is refused, while having exactly 20 is enough; potions before and after a magical item both work; owning potions doesn't block a magical item.
- **Encounter:** buying a potion keeps the menu open; magical options disappear after one is bought, but potion and leave still work; `4`/`leave` and closed input leave; choosing a hidden option counts as invalid.
- **South path:** second trip finds the potion; third trip shows the story; with 3 potions carried, the potion is left and is picked up on a later trip once there's room.
- **Combat:** the drink option appears only with a potion; drinking skips the player's attack and the dragon attacks; a refused drink at full health doesn't use a turn; reaching 0 health with potions is still a defeat. Dice aren't injectable, so tests check what happens around a drink (potion count, whether health is capped, which prompts appear), not exact combat rolls.
- **Languages:** every new key exists in all four languages; no `[key_name]` placeholder shows up in a scripted run through each language.

## Out of Scope

- Using potions outside combat, including "after combat". This needs retreat to return the player to the crossroads, which would be a separate feature.
- Any other potion types, or randomly rolled healing amounts.
- Selling potions, or more than one potion found per character.
- Showing Character Summary stats after the Merchant (the known gap from the Merchant Encounter spec still applies).

## Further Notes

- Domain terms (Healing Potion, Maximum Health, Merchant Encounter) are defined in the glossary. The Merchant Encounter definition now allows one magical item plus potions.
- No ADR: every decision here is cheap to reverse.
