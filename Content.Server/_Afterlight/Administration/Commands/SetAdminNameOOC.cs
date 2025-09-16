using System.Linq;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Afterlight.Administration.Commands
{
    [AdminCommand(AdminFlags.NameColor)]
    internal sealed class SetAdminNameOOC : IConsoleCommand
    {
        public string Command => "setadminnameooc";
        public string Description => Loc.GetString("set-admin-ooc-name-command-description", ("command", Command));
        public string Help => Loc.GetString("set-admin-ooc-name-command-help-text", ("command", Command));

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (shell.Player == null)
            {
                shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
                return;
            }

            if (args.Length < 1)
                return;
            
            var colorArg = string.Join(" ", args).Trim();
            if (string.IsNullOrEmpty(colorArg))
                return;

            var color = Color.TryFromHex(colorArg);
            if (!color.HasValue)
            {
                shell.WriteError(Loc.GetString("shell-invalid-color-hex"));
                return;
            }
            
            // Disabled - AFTERLIGHT
            // var luminance = (0.2126f * color.Value.R) + (0.7152f * color.Value.G) + (0.0722f * color.Value.B);

            // if (luminance is < 0.2f or > 0.8f)
            // {
            //     shell.WriteError("The color is too close to black or white — pick a more contrasting shade.");
            //     return;
            // }

            var userId = shell.Player.UserId;
            // Save the DB
            var dbMan = IoCManager.Resolve<IServerDbManager>();
            dbMan.SaveAdminOOCNameColorAsync(userId, color.Value);
            // Update the cached preference
            var prefManager = IoCManager.Resolve<IServerPreferencesManager>();
            var prefs = prefManager.GetPreferences(userId);
            prefs.AdminOOCNameColor = color.Value;
        }
    }
}
