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
            var setTitle = culture.ToTitleCase(set);
            var breastSprite = $"m_breasts_{set}_0_front_primary";
            var breastBackSprite = $"m_breasts_{set}_0_behind_primary";
            var nippleSprite = $"m_breasts_{set}_0_front_secondary";
            var nippleBackSprite = $"m_breasts_{set}_0_behind_secondary";
            yml.Append($@"
- type: marking
  parent: ALBreastsBase
  id: ALBreasts{setTitle}Flat
  sprites:
  - sprite: _Afterlight/Genitals/breasts.rsi
    state: {breastSprite}
  - sprite: _Afterlight/Genitals/breasts.rsi
    state: {nippleSprite}
  backSprites:
  - rsi:
      sprite: _Afterlight/Genitals/breasts.rsi
      state: {breastBackSprite}
  - rsi:
      sprite: _Afterlight/Genitals/breasts.rsi
      state: {nippleBackSprite}
");

            for (var i = 0; i < Sizes.Length; i++)
            {
                var size = Sizes[i];
                var spriteNumber = i + 1;
                breastSprite = $"m_breasts_{set}_{spriteNumber}_front_primary";
                breastBackSprite = $"m_breasts_{set}_{spriteNumber}_behind_primary";
                nippleSprite = $"m_breasts_{set}_{spriteNumber}_front_secondary";
                nippleBackSprite = $"m_breasts_{set}_{spriteNumber}_behind_secondary";
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
  - rsi:
      sprite: _Afterlight/Genitals/breasts.rsi
      state: {breastBackSprite}
  - rsi:
      sprite: _Afterlight/Genitals/breasts.rsi
      state: {nippleBackSprite}
");
            }

            Console.WriteLine(yml);
            yml.Clear();

            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            Console.WriteLine("----------------------");
        }

        return Task.CompletedTask;
    }
}
