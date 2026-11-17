// CYOATests/MerchantTests.cs
[TestClass]
[DoNotParallelize]
public class MerchantTests
{
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

    private static Player NewFighter() => new Player
    {
        Name = "Test",
        Occupation = "Fighter",
        HealthPoints = 15,
        Weapon = new Weapon("long sword", 12, "-)=====>")
    };

    // Runs the encounter with the given typed lines and returns everything printed.
    private static string RunEncounter(Player player, string typedInput, Messages? messages = null)
    {
        Console.SetIn(new StringReader(typedInput));
        using StringWriter writer = new StringWriter();
        Console.SetOut(writer);
        new Merchant().RunEncounter(player, messages ?? GetMessages());
        return writer.ToString();
    }

    // --- Character defaults ---

    [TestMethod]
    public void NewPlayer_StartsWith100Gold_NoArmor_HasNotMetMerchant()
    {
        Player player = new Player();
        Assert.AreEqual(100, player.Gold);
        Assert.IsFalse(player.HasMagicalArmor);
        Assert.IsFalse(player.HasMetMerchant);
    }

    // --- TryBuy: armor ---

    [TestMethod]
    public void TryBuy_Armor_AddsTenHealth_Costs60Gold_SetsArmor()
    {
        Player player = NewFighter();

        PurchaseResult result = new Merchant().TryBuy(player, MerchantItem.MagicalArmor);

        Assert.AreEqual(PurchaseResult.Bought, result);
        Assert.AreEqual(25, player.HealthPoints);
        Assert.AreEqual(40, player.Gold);
        Assert.IsTrue(player.HasMagicalArmor);
        Assert.AreEqual("long sword", player.Weapon!.Type, "Buying armor must not change the weapon.");
    }

    // --- TryBuy: weapon, one case per occupation ---

    [DataTestMethod]
    [DataRow("long sword", 12, "-)=====>", "magical long sword", 16)]
    [DataRow("lightning bolt spell", 12, "zap~~~~~~", "greater lightning bolt spell", 16)]
    [DataRow("dagger", 6, "-)==>", "magical dagger", 10)]
    [DataRow("long bow", 8, "}    -->", "magical long bow", 12)]
    public void TryBuy_Weapon_ReplacesWithMagicalVersion(
        string baseType, int baseDamage, string art, string magicalType, int magicalDamage)
    {
        Player player = new Player { HealthPoints = 15, Weapon = new Weapon(baseType, baseDamage, art) };

        PurchaseResult result = new Merchant().TryBuy(player, MerchantItem.MagicalWeapon);

        Assert.AreEqual(PurchaseResult.Bought, result);
        Assert.AreEqual(magicalType, player.Weapon!.Type);
        Assert.AreEqual(magicalDamage, player.Weapon.MaxDamage);
        Assert.AreEqual(art + "*", player.Weapon.AsciiArt);
        Assert.IsTrue(player.Weapon.IsMagical);
        Assert.AreEqual(20, player.Gold);
        Assert.AreEqual(15, player.HealthPoints, "Buying a weapon must not change health.");
        Assert.IsFalse(player.HasMagicalArmor);
    }

    [TestMethod]
    public void PlayerCreatedWeapons_AllHaveAMagicalVersion()
    {
        // Guards against a new occupation weapon being added without a magical version.
        foreach (string occupation in new[] { "Fighter", "Magician", "Thief", "Archer" })
        {
            Player player = new Player { Occupation = occupation };
            typeof(Player).GetMethod("AssignWeaponByOccupation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(player, null);

            Assert.AreEqual(PurchaseResult.Bought,
                new Merchant().TryBuy(player, MerchantItem.MagicalWeapon), occupation);
        }
    }

    // --- TryBuy: edge cases ---

    [DataTestMethod]
    [DataRow(MerchantItem.MagicalArmor, MerchantItem.MagicalArmor)]
    [DataRow(MerchantItem.MagicalArmor, MerchantItem.MagicalWeapon)]
    [DataRow(MerchantItem.MagicalWeapon, MerchantItem.MagicalArmor)]
    [DataRow(MerchantItem.MagicalWeapon, MerchantItem.MagicalWeapon)]
    public void TryBuy_SecondPurchase_IsRefused_AndChangesNothing(MerchantItem first, MerchantItem second)
    {
        Player player = NewFighter();
        player.Gold = 1000;
        Merchant merchant = new Merchant();
        merchant.TryBuy(player, first);

        int gold = player.Gold, health = player.HealthPoints, damage = player.Weapon!.MaxDamage;
        bool armor = player.HasMagicalArmor;

        Assert.AreEqual(PurchaseResult.AlreadyBought, merchant.TryBuy(player, second));
        Assert.AreEqual(gold, player.Gold);
        Assert.AreEqual(health, player.HealthPoints);
        Assert.AreEqual(damage, player.Weapon!.MaxDamage);
        Assert.AreEqual(armor, player.HasMagicalArmor);
    }

    [DataTestMethod]
    [DataRow(MerchantItem.MagicalArmor, 59)]
    [DataRow(MerchantItem.MagicalWeapon, 79)]
    public void TryBuy_NotEnoughGold_IsRefused_AndChangesNothing(MerchantItem item, int gold)
    {
        Player player = NewFighter();
        player.Gold = gold;

        Assert.AreEqual(PurchaseResult.NotEnoughGold, new Merchant().TryBuy(player, item));
        Assert.AreEqual(gold, player.Gold);
        Assert.AreEqual(15, player.HealthPoints);
        Assert.AreEqual("long sword", player.Weapon!.Type);
        Assert.IsFalse(player.HasMagicalArmor);
    }

    [DataTestMethod]
    [DataRow(MerchantItem.MagicalArmor, 60)]
    [DataRow(MerchantItem.MagicalWeapon, 80)]
    public void TryBuy_ExactPrice_IsEnough(MerchantItem item, int gold)
    {
        Player player = NewFighter();
        player.Gold = gold;

        Assert.AreEqual(PurchaseResult.Bought, new Merchant().TryBuy(player, item));
        Assert.AreEqual(0, player.Gold);
    }

    [TestMethod]
    public void TryBuy_Weapon_WithNoWeapon_IsRefused_AndChangesNothing()
    {
        Player player = new Player { HealthPoints = 15, Weapon = null };

        Assert.AreEqual(PurchaseResult.NoWeapon, new Merchant().TryBuy(player, MerchantItem.MagicalWeapon));
        Assert.AreEqual(100, player.Gold);
        Assert.IsNull(player.Weapon);
    }

    // --- RunEncounter: console flow ---

    [DataTestMethod]
    [DataRow("1\n")]
    [DataRow("armor\n")]
    [DataRow(" ARMOR \n")]
    public void RunEncounter_ArmorChoice_BuysArmor(string input)
    {
        Player player = NewFighter();
        Messages messages = GetMessages();

        string output = RunEncounter(player, input, messages);

        Assert.IsTrue(player.HasMagicalArmor);
        Assert.IsTrue(player.HasMetMerchant);
        StringAssert.Contains(output, messages.GetMessage("merchant_intro"));
        StringAssert.Contains(output, string.Format(messages.GetMessage("merchant_bought_armor"), 25, 40));
    }

    [DataTestMethod]
    [DataRow("2\n")]
    [DataRow("weapon\n")]
    [DataRow(" Weapon \n")]
    public void RunEncounter_WeaponChoice_BuysWeapon(string input)
    {
        Player player = NewFighter();
        Messages messages = GetMessages();

        string output = RunEncounter(player, input, messages);

        Assert.AreEqual("magical long sword", player.Weapon!.Type);
        StringAssert.Contains(output, string.Format(messages.GetMessage("merchant_bought_weapon"), "magical long sword", 16, 20));
    }

    [DataTestMethod]
    [DataRow("4\n")]
    [DataRow("leave\n")]
    [DataRow("  LEAVE\n")]
    [DataRow("")] // closed input
    public void RunEncounter_LeaveOrClosedInput_BuysNothing(string input)
    {
        Player player = NewFighter();
        Messages messages = GetMessages();

        string output = RunEncounter(player, input, messages);

        Assert.AreEqual(100, player.Gold);
        Assert.IsFalse(player.HasMagicalArmor);
        Assert.AreEqual("long sword", player.Weapon!.Type);
        Assert.IsTrue(player.HasMetMerchant);
        StringAssert.Contains(output, messages.GetMessage("merchant_leave"));
    }

    [TestMethod]
    public void RunEncounter_InvalidInput_RepromptsThenAcceptsValidChoice()
    {
        Player player = NewFighter();
        Messages messages = GetMessages();

        string output = RunEncounter(player, "x\n\n9\n1\n", messages);

        Assert.IsTrue(player.HasMagicalArmor);
        Assert.AreEqual(40, player.Gold);
        Assert.IsFalse(player.Weapon!.IsMagical);
        StringAssert.Contains(output, messages.GetMessage("invalid"));
    }

    [TestMethod]
    public void RunEncounter_NotEnoughGold_ShowsMessage_AndLetsPlayerChooseAgain()
    {
        Player player = NewFighter();
        player.Gold = 70; // enough for armor (60) but not the weapon (80)
        Messages messages = GetMessages();

        string output = RunEncounter(player, "2\n1\n", messages);

        StringAssert.Contains(output, messages.GetMessage("merchant_not_enough_gold"));
        Assert.IsTrue(player.HasMagicalArmor);
        Assert.AreEqual(10, player.Gold);
    }

    // --- South path ---

    [TestMethod]
    public void SouthPath_FirstTrip_MeetsMerchant_SecondTrip_MerchantIsGone()
    {
        Game game = new Game();
        Player player = NewFighter();
        Messages messages = GetMessages();

        Console.SetIn(new StringReader("1\n"));
        using StringWriter first = new StringWriter();
        Console.SetOut(first);
        game.HandleSouthPath(player, messages);

        StringAssert.Contains(first.ToString(), messages.GetMessage("merchant_intro"));
        Assert.IsTrue(player.HasMagicalArmor);

        Console.SetIn(new StringReader("2\n")); // would buy the weapon if the menu appeared
        using StringWriter second = new StringWriter();
        Console.SetOut(second);
        game.HandleSouthPath(player, messages);

        string output = second.ToString();
        StringAssert.Contains(output, messages.GetMessage("merchant_gone"));
        Assert.IsFalse(output.Contains(messages.GetMessage("merchant_intro")));
        Assert.IsFalse(player.Weapon!.IsMagical);
    }

    [TestMethod]
    public void SouthPath_AfterLeavingWithNothing_MerchantIsStillGone()
    {
        Game game = new Game();
        Player player = NewFighter();
        Messages messages = GetMessages();

        Console.SetIn(new StringReader("4\n"));
        Console.SetOut(new StringWriter());
        game.HandleSouthPath(player, messages);

        using StringWriter second = new StringWriter();
        Console.SetOut(second);
        game.HandleSouthPath(player, messages);

        StringAssert.Contains(second.ToString(), messages.GetMessage("merchant_gone"));
        Assert.AreEqual(100, player.Gold);
    }

    // --- Stats screen ---

    [TestMethod]
    public void DisplayStats_ShowsGold_AndArmorOnlyOnceBought()
    {
        Player player = NewFighter();
        Messages messages = GetMessages();

        using StringWriter before = new StringWriter();
        Console.SetOut(before);
        player.DisplayStats(messages);
        StringAssert.Contains(before.ToString(), messages.GetMessage("stats_gold") + 100);
        Assert.IsFalse(before.ToString().Contains(messages.GetMessage("stats_armor")));

        new Merchant().TryBuy(player, MerchantItem.MagicalArmor);

        using StringWriter after = new StringWriter();
        Console.SetOut(after);
        player.DisplayStats(messages);
        StringAssert.Contains(after.ToString(), messages.GetMessage("stats_gold") + 40);
        StringAssert.Contains(after.ToString(), messages.GetMessage("stats_armor") + messages.GetMessage("magical_armor"));
    }

    // --- Language data ---

    private static readonly string[] MerchantKeys =
    {
        "merchant_intro", "merchant_gold", "merchant_offer_armor", "merchant_offer_weapon",
        "merchant_offer_leave", "merchant_bought_armor", "merchant_bought_weapon",
        "merchant_not_enough_gold", "merchant_no_weapon", "merchant_leave", "merchant_gone",
        "stats_gold", "stats_armor", "magical_armor"
    };

    private static readonly string[] MagicalWeaponNames =
    {
        "magical long sword", "greater lightning bolt spell", "magical dagger", "magical long bow"
    };

    [DataTestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void MerchantText_ExistsInEveryLanguage(string language)
    {
        Messages messages = GetMessages(language);

        foreach (string key in MerchantKeys)
            Assert.AreNotEqual($"[{key}]", messages.GetMessage(key), $"Key '{key}' is missing from {language}.");
    }

    [DataTestMethod]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void MagicalWeaponNames_AreTranslated(string language)
    {
        Messages messages = GetMessages(language);

        foreach (string name in MagicalWeaponNames)
            Assert.AreNotEqual(name, messages.TranslateWeaponForDisplay(name), $"'{name}' is not translated in {language}.");
    }

    [DataTestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void FullEncounter_InEveryLanguage_ShowsNoMissingKeys(string language)
    {
        Messages messages = GetMessages(language);
        Game game = new Game();
        using StringWriter writer = new StringWriter();
        Console.SetOut(writer);

        // Invalid input, then too poor for the weapon, then buy armor; then the summary; then a second trip south.
        Player buyer = NewFighter();
        buyer.Gold = 70;
        Console.SetIn(new StringReader("x\n2\n1\n"));
        game.HandleSouthPath(buyer, messages);
        buyer.DisplayStats(messages);
        game.HandleSouthPath(buyer, messages);

        // A character with nothing to enchant, then leaving.
        Player unarmed = new Player { HealthPoints = 15, Weapon = null };
        Console.SetIn(new StringReader("2\n3\n"));
        game.HandleSouthPath(unarmed, messages);

        // A weapon purchase, so the magical weapon name is shown.
        Console.SetIn(new StringReader("2\n"));
        game.HandleSouthPath(NewFighter(), messages);

        string output = writer.ToString();
        Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(output, @"\[[a-z_]+\]"),
            $"Missing key shown in {language}:\n{output}");
    }
}
