public class Weapon
{
    public string Type { get; set; }
    public int MaxDamage { get; set; }
    public string AsciiArt { get; set; }
    public bool IsMagical { get; }

    public Weapon(string type, int maxDamage, string asciiArt, bool isMagical = false)
    {
        Type = type;
        MaxDamage = maxDamage;
        AsciiArt = asciiArt;
        IsMagical = isMagical;
    }
}
