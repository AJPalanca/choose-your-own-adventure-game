public class Combat
{
    public Player player { get; set; }
    public Dragon dragon { get; set; }
    public Messages messages { get; set; }

    public bool PlayerRetreated { get; private set; }

    public Combat(Player player, Dragon dragon, Messages messages)
    {
        this.player = player;
        this.dragon = dragon;
        this.messages = messages;
        PlayerRetreated = false;
    }

    public bool StartCombat()
    {
        Console.WriteLine(GetCombatStatsDisplay());

        bool playerAttacks = true;

        while (true)
        {
            if (playerAttacks)
            {
                PlayerAttacksDragonSequence();

                if (dragon.HealthPoints <= 0)
                    return true;
            }

            DragonAttacksPlayerSequence();

            if (player.HealthPoints <= 0)
                return false;

            CombatChoice choice = PromptForCombatChoice();

            if (choice == CombatChoice.Retreat)
            {
                PlayerRetreated = true;
                Console.WriteLine(messages.GetMessage("retreat_combat"));
                return false;
            }

            // Drinking a Healing Potion uses up the player's attack for the round.
            playerAttacks = choice == CombatChoice.Attack;
        }
    }

    private enum CombatChoice
    {
        Attack,
        Retreat,
        DrankPotion
    }

    // Keeps asking until the player attacks, retreats, or successfully drinks a Healing Potion.
    // The drink option is only offered while the player carries a potion.
    private CombatChoice PromptForCombatChoice()
    {
        while (true)
        {
            bool canDrink = player.HealingPotions > 0;
            Console.WriteLine(canDrink
                ? string.Format(messages.GetMessage("attack_prompt_with_potion"), player.HealingPotions)
                : messages.GetMessage("attack_prompt"));
            string? input = Console.ReadLine()?.Trim().ToLower();

            // null means input was closed (nothing more can be typed), so retreat.
            if (input == null || input == "r" || input == "retreat")
                return CombatChoice.Retreat;

            if (input == "a" || input == "attack")
                return CombatChoice.Attack;

            if (canDrink && (input == "d" || input == "drink"))
            {
                int healthBefore = player.HealthPoints;

                if (player.DrinkHealingPotion() == HealingResult.Drank)
                {
                    Console.WriteLine(string.Format(messages.GetMessage("combat_drank_potion"),
                        player.HealthPoints - healthBefore, player.HealthPoints, player.HealingPotions));
                    return CombatChoice.DrankPotion;
                }

                // Refusing a drink at full health costs neither the potion nor the turn.
                Console.WriteLine(messages.GetMessage("combat_already_full_health"));
                continue;
            }

            Console.WriteLine(messages.GetMessage("invalid"));
        }
    }

    public string GetCombatStatsDisplay()
    {
        var output = new List<string>();

        output.Add(messages.GetMessage("combat_stats_header"));
        output.Add(messages.GetMessage("combat_player_header"));
        output.Add(messages.GetMessage("stats_name") + player.Name);
        output.Add(messages.GetMessage("stats_strength") + player.Strength);
        output.Add(messages.GetMessage("stats_agility") + player.Agility);
        output.Add(messages.GetMessage("stats_health") + player.HealthPoints);
        output.Add(messages.GetMessage("stats_max_health") + player.MaxHealthPoints);
        output.Add(messages.GetMessage("stats_potions") + player.HealingPotions);

        if (player.Weapon != null)
        {
            output.Add(messages.GetMessage("stats_weapon") + messages.TranslateWeaponForDisplay(player.Weapon.Type));
            output.Add(messages.GetMessage("stats_damage") + player.Weapon.MaxDamage);
        }

        output.Add("");
        output.Add(messages.GetMessage("combat_dragon_header"));
        output.Add(messages.GetMessage("stats_name") + dragon.Name);
        output.Add(messages.GetMessage("stats_strength") + dragon.Strength);
        output.Add(messages.GetMessage("stats_agility") + dragon.Agility);
        output.Add(messages.GetMessage("stats_health") + dragon.HealthPoints);
        output.Add(messages.GetMessage("stats_weapon") + messages.TranslateWeaponForDisplay(dragon.Weapon.Type));
        output.Add(messages.GetMessage("stats_damage") + dragon.Weapon.MaxDamage);

        return string.Join(Environment.NewLine, output);
    }

    public void PlayerAttacksDragonSequence()
    {
        Console.WriteLine(player.Attack(dragon, messages));
    }

    public void DragonAttacksPlayerSequence()
    {
        Console.WriteLine(dragon.Attack(player, messages));
    }
}