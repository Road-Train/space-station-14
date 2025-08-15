using System.Globalization;
using System.Text;

namespace Content.Scripts;

public static class ButtConverter
{
    public static Task Convert()
    {
        var yml = new StringBuilder();
        var loc = new StringBuilder();
        var sets = new[] { "pair" };
        var culture = new CultureInfo("en-US", false).TextInfo;
        foreach (var set in sets)
        {
            for (var i = 0; i < 8; i++)
            {
                var spriteNumber = i + 1;
                var setTitle = culture.ToTitleCase(set);
                var breastSprite = $"m_butt_{set}_{spriteNumber}_adj_primary";
                var id = $"ALButts{setTitle}{spriteNumber}";
                yml.Append($@"
- type: marking
  parent: ALButtsBase
  id: {id}
  sprites:
  - sprite: _Afterlight/Genitals/butts.rsi
    state: {breastSprite}
");

                loc.AppendLine($"marking-{id} = Butt {spriteNumber}");
            }

            Console.WriteLine(yml);
            Console.WriteLine(loc);
            yml.Clear();
            loc.Clear();
        }

        return Task.CompletedTask;
    }
}
