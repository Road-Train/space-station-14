using System.Globalization;
using System.Text;

namespace Content.Scripts;

public static class BreastsConverter
{
    public static Task Convert()
    {
        var yml = new StringBuilder();
        var sets = new[] { "pair", "quad", "sextuple" };
        const string Sizes = "ABCDEFGHIJKLMNOPQRS";
        var culture = new CultureInfo("en-US", false).TextInfo;
        foreach (var set in sets)
        {
            for (var i = 0; i < Sizes.Length; i++)
            {
                var size = Sizes[i];
                var spriteNumber = i + 1;
                var setTitle = culture.ToTitleCase(set);
                var breastSprite = $"m_breasts_{set}_{spriteNumber}_front_primary";
                var breastBackSprite = $"m_breasts_{set}_{spriteNumber}_behind_primary";
                var nippleSprite = $"m_breasts_{set}_{spriteNumber}_front_secondary";
                var nippleBackSprite = $"m_breasts_{set}_{spriteNumber}_behind_secondary";
                yml.Append($@"
- type: marking
  parent: ALBreastsBase
  id: ALBreasts{setTitle}{size}
  sprites:
  - sprite: _Afterlight/Genitals/breasts.rsi
    state: {breastSprite}
  - sprite: _Afterlight/Genitals/breasts.rsi
    state: {nippleSprite}
  backSprites:
  - sprite: _Afterlight/Genitals/breasts.rsi
    state: {breastBackSprite}
  - sprite: _Afterlight/Genitals/breasts.rsi
    state: {nippleBackSprite}
");
            }

            Console.WriteLine(yml);
            yml.Clear();
        }

        return Task.CompletedTask;
    }
}
