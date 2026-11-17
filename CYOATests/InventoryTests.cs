// CYOATests/InventoryTests.cs
[TestClass]
[DoNotParallelize]
public class InventoryTests
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

    private static string RunWithInput(string typedInput, Action action)
    {
        Console.SetIn(new StringReader(typedInput));
        using StringWriter writer = new StringWriter();
        Console.SetOut(writer);
        action();
        return writer.ToString();
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

    private static Player NewFighter() => new Player
    {
        Name = "Test",
        Occupation = "Fighter",
        HealthPoints = 15,
        MaxHealthPoints = 15,
        Weapon = new Weapon("long sword", 12, "-)=====>")
    };

    // --- The Inventory view ---

    [TestMethod]
    public void Inventory_NewFighter_ShowsEveryLine_WithNoneForArmor()
    {
        Player player = NewFighter();
        Messages messages = GetMessages();

        string inventory = player.GetInventoryDisplay(messages);

        string expected = string.Join(Environment.NewLine,
            "----- Inventory -----",
            "Weapon: long sword (Maximum Damage: 12)",
            "Armor: none",
            "Healing Potions: 0 of 3",
            "Gold: 100");
        Assert.AreEqual(expected, inventory);
    }

    [TestMethod]
    public void Inventory_AfterBuyingMagicalWeaponAndPotion_ShowsThem()
    {
        Player player = NewFighter();
        Merchant merchant = new Merchant();
        merchant.TryBuy(player, MerchantItem.MagicalWeapon);
        merchant.TryBuy(player, MerchantItem.HealingPotion);

        string inventory = player.GetInventoryDisplay(GetMessages());

        StringAssert.Contains(inventory, "Weapon: magical long sword (Maximum Damage: 16)");
        StringAssert.Contains(inventory, "Healing Potions: 1 of 3");
        StringAssert.Contains(inventory, "Gold: 0");
    }

    [TestMethod]
    public void Inventory_WearingMagicalArmor_ShowsIt()
    {
        Player player = NewFighter();
        new Merchant().TryBuy(player, MerchantItem.MagicalArmor);

        string inventory = player.GetInventoryDisplay(GetMessages());

        StringAssert.Contains(inventory, "Armor: Magical Armor");
    }

    [TestMethod]
    public void Inventory_WithNoWeapon_ShowsNone()
    {
        Player player = new Player();

        string inventory = player.GetInventoryDisplay(GetMessages());

        StringAssert.Contains(inventory, "Weapon: none");
    }

    // --- At the crossroads ---

    [DataTestMethod]
    [DataRow("i\nn\n")]
    [DataRow(" INVENTORY \nn\n")]
    public void Crossroads_Inventory_ShowsIt_ThenAsksAgain(string input)
    {
        Player player = NewFighter();
        Messages messages = GetMessages();
        string path = "";

        string output = RunWithInput(input, () => path = new Game().PromptForPath(player, messages));

        Assert.AreEqual("north", path);
        StringAssert.Contains(output, player.GetInventoryDisplay(messages));
        Assert.AreEqual(2, CountOf(output, messages.GetMessage("path_prompt_full")));
        Assert.IsFalse(output.Contains(messages.GetMessage("path_invalid")));
    }

    // --- In combat ---
    // A weaponless player can't hurt the dragon, and a Strength-0 dragon can't hit the player,
    // so every round reaches the prompt whatever the dice roll.

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(2)]
    public void Combat_Inventory_ShowsIt_WithoutUsingTheTurn(int potions)
    {
        Player player = new Player { Name = "Test", HealthPoints = 10, MaxHealthPoints = 20, HealingPotions = potions };
        Dragon dragon = new Dragon(new Die()) { Strength = 0 };
        Messages messages = GetMessages();
        string dragonAttack = string.Format(messages.GetMessage("dragon_attack_intro"), dragon.Name, player.Name,
            messages.TranslateWeaponForDisplay(dragon.Weapon.Type));

        string output = RunWithInput("i\nr\n", () => new Combat(player, dragon, messages).StartCombat());

        StringAssert.Contains(output, player.GetInventoryDisplay(messages));
        Assert.AreEqual(1, CountOf(output, dragonAttack), "Checking the Inventory must not give the dragon another attack.");
        Assert.IsFalse(output.Contains(messages.GetMessage("invalid")));
        Assert.AreEqual(potions, player.HealingPotions);
    }

    // --- Language data ---

    private static readonly string[] InventoryKeys =
    {
        "inventory_header", "inventory_weapon", "inventory_none", "inventory_potions"
    };

    [DataTestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Inventory_InEveryLanguage_HasNoMissingText_AndPromptsMentionI(string language)
    {
        Messages messages = GetMessages(language);
        foreach (string key in InventoryKeys)
            Assert.AreNotEqual($"[{key}]", messages.GetMessage(key), $"{key} missing in {language}");

        Player player = NewFighter();
        new Merchant().TryBuy(player, MerchantItem.MagicalArmor);
        string inventory = player.GetInventoryDisplay(messages);
        Assert.IsFalse(inventory.Contains('['), $"Missing text in {language}: {inventory}");

        foreach (string key in new[] { "path_prompt_full", "path_invalid", "attack_prompt", "attack_prompt_with_potion" })
            StringAssert.Contains(messages.GetMessage(key), "'i'", $"{key} in {language} should mention 'i'");
    }
}
