// CYOATests/HealingPotionTests.cs
[TestClass]
[DoNotParallelize]
public class HealingPotionTests
{
    private static Player Wounded(int health, int maxHealth, int potions) => new Player
    {
        Name = "Test",
        Occupation = "Fighter",
        HealthPoints = health,
        MaxHealthPoints = maxHealth,
        HealingPotions = potions
    };

    private TextReader? _originalIn;
    private TextWriter? _originalOut;

    [TestInitialize]
    public void Setup()
    {
        _originalIn = Console.In;
        _originalOut = Console.Out;
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_originalIn != null) Console.SetIn(_originalIn);
        if (_originalOut != null) Console.SetOut(_originalOut);
    }

    private static Messages GetMessages(string language = "English")
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage(language);
        messages.ReadDictionary();
        return messages;
    }

    // Runs the given action with typed input and returns everything printed.
    private static string RunWithInput(string typedInput, Action action)
    {
        Console.SetIn(new StringReader(typedInput));
        using StringWriter writer = new StringWriter();
        Console.SetOut(writer);
        action();
        return writer.ToString();
    }

    // --- Character creation ---

    [TestMethod]
    public void NewCharacter_StartsWithNoPotions_AndMaximumHealthEqualToRolledHealth()
    {
        Player player = new Player();
        Assert.AreEqual(0, player.HealingPotions);

        RunWithInput("Human\nTest\nFighter\nroll\nroll\nroll\n2\n",
            () => player.CreateCharacter(GetMessages()));

        Assert.IsTrue(player.HealthPoints > 0);
        Assert.AreEqual(player.HealthPoints, player.MaxHealthPoints);
    }

    // --- Buying from the Merchant ---

    [TestMethod]
    public void BuyArmor_RaisesMaximumHealthBy10()
    {
        Player player = Wounded(health: 15, maxHealth: 15, potions: 0);

        new Merchant().TryBuy(player, MerchantItem.MagicalArmor);

        Assert.AreEqual(25, player.HealthPoints);
        Assert.AreEqual(25, player.MaxHealthPoints);
    }

    [TestMethod]
    public void BuyPotion_Costs20Gold_AddsOnePotion()
    {
        Player player = Wounded(health: 15, maxHealth: 15, potions: 0);

        PurchaseResult result = new Merchant().TryBuy(player, MerchantItem.HealingPotion);

        Assert.AreEqual(PurchaseResult.Bought, result);
        Assert.AreEqual(80, player.Gold);
        Assert.AreEqual(1, player.HealingPotions);
    }

    [TestMethod]
    public void BuyPotion_WhenCarrying3_IsRefused_GoldUnchanged()
    {
        Player player = Wounded(health: 15, maxHealth: 15, potions: 3);

        PurchaseResult result = new Merchant().TryBuy(player, MerchantItem.HealingPotion);

        Assert.AreEqual(PurchaseResult.CarryingTooMany, result);
        Assert.AreEqual(100, player.Gold);
        Assert.AreEqual(3, player.HealingPotions);
    }

    [DataTestMethod]
    [DataRow(19, PurchaseResult.NotEnoughGold, 19, 0)]
    [DataRow(20, PurchaseResult.Bought, 0, 1)]
    public void BuyPotion_NeedsAtLeast20Gold(int gold, PurchaseResult expected, int goldAfter, int potionsAfter)
    {
        Player player = Wounded(health: 15, maxHealth: 15, potions: 0);
        player.Gold = gold;

        PurchaseResult result = new Merchant().TryBuy(player, MerchantItem.HealingPotion);

        Assert.AreEqual(expected, result);
        Assert.AreEqual(goldAfter, player.Gold);
        Assert.AreEqual(potionsAfter, player.HealingPotions);
    }

    [TestMethod]
    public void Potions_AndOneMagicalItem_CanBeBoughtInEitherOrder()
    {
        Merchant merchant = new Merchant();

        Player potionFirst = Wounded(health: 15, maxHealth: 15, potions: 0);
        Assert.AreEqual(PurchaseResult.Bought, merchant.TryBuy(potionFirst, MerchantItem.HealingPotion));
        Assert.AreEqual(PurchaseResult.Bought, merchant.TryBuy(potionFirst, MerchantItem.MagicalArmor));
        Assert.AreEqual(20, potionFirst.Gold);

        Player armorFirst = Wounded(health: 15, maxHealth: 15, potions: 0);
        Assert.AreEqual(PurchaseResult.Bought, merchant.TryBuy(armorFirst, MerchantItem.MagicalArmor));
        Assert.AreEqual(PurchaseResult.Bought, merchant.TryBuy(armorFirst, MerchantItem.HealingPotion));
        Assert.AreEqual(PurchaseResult.Bought, merchant.TryBuy(armorFirst, MerchantItem.HealingPotion));
        Assert.AreEqual(0, armorFirst.Gold);
        Assert.AreEqual(2, armorFirst.HealingPotions);
    }

    // --- Merchant Encounter menu ---

    private static Player NewFighter() => new Player
    {
        Name = "Test",
        Occupation = "Fighter",
        HealthPoints = 15,
        MaxHealthPoints = 15,
        Weapon = new Weapon("long sword", 12, "-)=====>")
    };

    [DataTestMethod]
    [DataRow("3\n3\n4\n")]
    [DataRow("potion\n POTION \nleave\n")]
    public void Encounter_BuyingPotions_KeepsMenuOpenUntilLeave(string input)
    {
        Player player = NewFighter();
        Messages messages = GetMessages();

        string output = RunWithInput(input, () => new Merchant().RunEncounter(player, messages));

        Assert.AreEqual(2, player.HealingPotions);
        Assert.AreEqual(60, player.Gold);
        StringAssert.Contains(output, string.Format(messages.GetMessage("merchant_bought_potion"), 2, 60));
        StringAssert.Contains(output, messages.GetMessage("merchant_leave"));
    }

    [TestMethod]
    public void Encounter_AfterBuyingMagicalItem_MenuStaysOpen_WithMagicalOptionsHidden()
    {
        Player player = NewFighter();
        Messages messages = GetMessages();
        string weaponOffer = string.Format(messages.GetMessage("merchant_offer_weapon"),
            Merchant.MagicalWeaponDamageBonus, Merchant.MagicalWeaponPrice);

        // Armor, then a potion, then the (now hidden) weapon option, then leave.
        string output = RunWithInput("1\n3\n2\n4\n", () => new Merchant().RunEncounter(player, messages));

        Assert.IsTrue(player.HasMagicalArmor);
        Assert.AreEqual(1, player.HealingPotions);
        Assert.AreEqual(20, player.Gold);
        Assert.IsFalse(player.Weapon!.IsMagical);
        Assert.AreEqual(1, CountOf(output, weaponOffer), "Weapon offer should only appear before the armor purchase.");
        StringAssert.Contains(output, messages.GetMessage("invalid"));
        StringAssert.Contains(output, messages.GetMessage("merchant_leave"));
    }

    private static int CountOf(string text, string value)
    {
        int count = 0, index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        return count;
    }

    // --- South path ---

    private static string GoSouth(Player player, Messages messages, string typedInput = "") =>
        RunWithInput(typedInput, () => new Game().HandleSouthPath(player, messages));

    [TestMethod]
    public void SouthPath_SecondTrip_FindsPotion_ThirdTrip_ShowsStory()
    {
        Player player = NewFighter();
        Messages messages = GetMessages();

        GoSouth(player, messages, "4\n"); // meet the Merchant, buy nothing
        string second = GoSouth(player, messages);
        string third = GoSouth(player, messages);

        Assert.AreEqual(1, player.HealingPotions);
        StringAssert.Contains(second, messages.GetMessage("merchant_gone"));
        StringAssert.Contains(second, string.Format(messages.GetMessage("south_found_potion"), 1));
        Assert.IsFalse(second.Contains(messages.GetMessage("south_path_narrative")));
        StringAssert.Contains(third, messages.GetMessage("south_path_narrative"));
        Assert.AreEqual(1, player.HealingPotions, "Only one potion is ever found.");
    }

    [TestMethod]
    public void SouthPath_CarryingThree_LeavesPotion_UntilALaterTripWithRoom()
    {
        Player player = NewFighter();
        Messages messages = GetMessages();

        GoSouth(player, messages, "3\n3\n3\n4\n"); // buy 3 potions at the Merchant
        string full = GoSouth(player, messages);

        Assert.AreEqual(3, player.HealingPotions);
        StringAssert.Contains(full, messages.GetMessage("south_potion_left"));

        player.HealingPotions = 2; // as if one were drunk in a fight
        string withRoom = GoSouth(player, messages);

        Assert.AreEqual(3, player.HealingPotions);
        StringAssert.Contains(withRoom, string.Format(messages.GetMessage("south_found_potion"), 3));
    }

    // --- Combat ---
    // A weaponless player can't hurt the dragon, and a Strength-0 dragon can't hit the player,
    // so every round reaches the prompt whatever the dice roll.

    private const string NoWeaponAttack = "You have no weapon to attack with.";

    private static (Player player, Dragon dragon) HarmlessFight(int health, int maxHealth, int potions)
    {
        Player player = Wounded(health, maxHealth, potions);
        Dragon dragon = new Dragon(new Die()) { Strength = 0 };
        return (player, dragon);
    }

    private static string Fight(Player player, Dragon dragon, Messages messages, string typedInput) =>
        RunWithInput(typedInput, () => new Combat(player, dragon, messages).StartCombat());

    private static string DragonAttackIntro(Player player, Dragon dragon, Messages messages) =>
        string.Format(messages.GetMessage("dragon_attack_intro"), dragon.Name, player.Name,
            messages.TranslateWeaponForDisplay(dragon.Weapon.Type));

    [DataTestMethod]
    [DataRow("d\nr\n")]
    [DataRow(" DRINK \nr\n")]
    public void Combat_Drinking_Heals_SkipsPlayerAttack_DragonStillAttacks(string input)
    {
        var (player, dragon) = HarmlessFight(health: 5, maxHealth: 20, potions: 1);
        Messages messages = GetMessages();

        string output = Fight(player, dragon, messages, input);

        Assert.AreEqual(13, player.HealthPoints);
        Assert.AreEqual(0, player.HealingPotions);
        StringAssert.Contains(output, string.Format(messages.GetMessage("combat_drank_potion"), 8, 13, 0));
        Assert.AreEqual(1, CountOf(output, NoWeaponAttack), "The player should not attack in the round they drink.");
        Assert.AreEqual(2, CountOf(output, DragonAttackIntro(player, dragon, messages)));
    }

    [TestMethod]
    public void Combat_DrinkingAtFullHealth_IsRefused_CostsNoPotionAndNoTurn()
    {
        var (player, dragon) = HarmlessFight(health: 20, maxHealth: 20, potions: 1);
        Messages messages = GetMessages();

        string output = Fight(player, dragon, messages, "d\nr\n");

        Assert.AreEqual(1, player.HealingPotions);
        StringAssert.Contains(output, messages.GetMessage("combat_already_full_health"));
        Assert.AreEqual(1, CountOf(output, DragonAttackIntro(player, dragon, messages)),
            "A refused drink must not give the dragon another attack.");
    }

    [TestMethod]
    public void Combat_WithNoPotion_DrinkIsNotOffered_AndCountsAsInvalid()
    {
        var (player, dragon) = HarmlessFight(health: 5, maxHealth: 20, potions: 0);
        Messages messages = GetMessages();

        string output = Fight(player, dragon, messages, "d\nr\n");

        StringAssert.Contains(output, messages.GetMessage("attack_prompt"));
        Assert.IsFalse(output.Contains(string.Format(messages.GetMessage("attack_prompt_with_potion"), 0)));
        StringAssert.Contains(output, messages.GetMessage("invalid"));
        Assert.AreEqual(5, player.HealthPoints);
    }

    [TestMethod]
    public void Combat_ReachingZeroHealth_IsDefeat_EvenWhenCarryingPotions()
    {
        var (player, dragon) = HarmlessFight(health: 1, maxHealth: 20, potions: 2);
        player.Agility = 0;    // can never block
        dragon.Strength = 20;  // always hits
        Messages messages = GetMessages();

        bool won = false;
        string output = RunWithInput("d\nd\n", () => won = new Combat(player, dragon, messages).StartCombat());

        Assert.IsFalse(won);
        Assert.AreEqual(0, player.HealthPoints);
        Assert.AreEqual(2, player.HealingPotions, "Potions are never drunk automatically.");
    }

    // --- Stats displays ---

    [TestMethod]
    public void CombatStats_ShowMaximumHealth_AndPotionsCarried()
    {
        var (player, dragon) = HarmlessFight(health: 12, maxHealth: 20, potions: 2);
        Messages messages = GetMessages();

        string stats = new Combat(player, dragon, messages).GetCombatStatsDisplay();

        StringAssert.Contains(stats, messages.GetMessage("stats_max_health") + 20);
        StringAssert.Contains(stats, messages.GetMessage("stats_potions") + 2);
    }

    [TestMethod]
    public void CharacterSummary_ShowsPotionsCarried()
    {
        Player player = Wounded(health: 12, maxHealth: 20, potions: 3);
        Messages messages = GetMessages();

        string output = RunWithInput("", () => player.DisplayStats(messages));

        StringAssert.Contains(output, messages.GetMessage("stats_potions") + 3);
    }

    // --- Language data ---

    private static readonly string[] Languages = { "English", "Spanish", "French", "Italian" };

    private static readonly string[] PotionKeys =
    {
        "merchant_offer_potion", "merchant_bought_potion", "merchant_carrying_too_many",
        "south_found_potion", "south_potion_left",
        "attack_prompt_with_potion", "combat_drank_potion", "combat_already_full_health",
        "stats_max_health", "stats_potions"
    };

    [TestMethod]
    public void EveryPotionKey_ExistsInEveryLanguage()
    {
        foreach (string language in Languages)
        {
            Messages messages = GetMessages(language);
            foreach (string key in PotionKeys)
                Assert.AreNotEqual($"[{key}]", messages.GetMessage(key), $"{key} missing in {language}");
        }
    }

    [DataTestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void FullPotionPlaythrough_ShowsNoMissingTextPlaceholder(string language)
    {
        Messages messages = GetMessages(language);
        Player player = NewFighter();
        player.HealthPoints = 5; // wounded, so the potion can be drunk

        // Merchant: potion, armor, hidden weapon (invalid), 3 more potions (one too many), leave.
        string output = GoSouth(player, messages, "3\n1\n2\n3\n3\n3\n4\n");
        output += GoSouth(player, messages); // potion left behind: carrying 3
        player.HealingPotions = 2;
        output += GoSouth(player, messages); // potion found
        output += GoSouth(player, messages); // story

        Dragon dragon = new Dragon(new Die()) { Strength = 0 };
        player.Weapon = null;
        output += Fight(player, dragon, messages, "d\nd\nd\nd\nr\n");
        output += RunWithInput("", () => player.DisplayStats(messages));

        var missing = System.Text.RegularExpressions.Regex.Matches(output, @"\[[a-z_0-9]+\]");
        Assert.AreEqual(0, missing.Count, string.Join(", ", missing.Select(m => m.Value)));
    }

    // --- Drinking ---

    [TestMethod]
    public void Drink_WellBelowMaximum_Restores8_UsesOnePotion()
    {
        Player player = Wounded(health: 5, maxHealth: 20, potions: 2);

        HealingResult result = player.DrinkHealingPotion();

        Assert.AreEqual(HealingResult.Drank, result);
        Assert.AreEqual(13, player.HealthPoints);
        Assert.AreEqual(1, player.HealingPotions);
    }

    [TestMethod]
    public void Drink_CloseToMaximum_HealsOnlyUpToMaximum()
    {
        Player player = Wounded(health: 17, maxHealth: 20, potions: 1);

        HealingResult result = player.DrinkHealingPotion();

        Assert.AreEqual(HealingResult.Drank, result);
        Assert.AreEqual(20, player.HealthPoints);
        Assert.AreEqual(0, player.HealingPotions);
    }

    [TestMethod]
    public void Drink_AtMaximum_IsRefused_NothingChanges()
    {
        Player player = Wounded(health: 20, maxHealth: 20, potions: 1);

        HealingResult result = player.DrinkHealingPotion();

        Assert.AreEqual(HealingResult.AlreadyAtFullHealth, result);
        Assert.AreEqual(20, player.HealthPoints);
        Assert.AreEqual(1, player.HealingPotions);
    }

    [TestMethod]
    public void Drink_WithNoPotion_IsRefused_NothingChanges()
    {
        Player player = Wounded(health: 5, maxHealth: 20, potions: 0);

        HealingResult result = player.DrinkHealingPotion();

        Assert.AreEqual(HealingResult.NoPotion, result);
        Assert.AreEqual(5, player.HealthPoints);
        Assert.AreEqual(0, player.HealingPotions);
    }
}
