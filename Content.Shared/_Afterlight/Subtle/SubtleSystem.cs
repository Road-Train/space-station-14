using Content.Shared._Afterlight.CCVar;
using Content.Shared._Afterlight.Chat;
using Content.Shared.Administration.Logs;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Ghost;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Shared._Afterlight.Subtle;

public sealed class SubtleSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly SharedALChatSystem _alChat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly INetConfigurationManager _netConfiguration = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<GhostComponent> _ghostQuery;

    private readonly SoundSpecifier _sound =
        new SoundPathSpecifier("/Audio/_Afterlight/Effects/Achievement/glockenspiel_ping.ogg");
    private float _range;

    public override void Initialize()
    {
        _ghostQuery = GetEntityQuery<GhostComponent>();

        SubscribeNetworkEvent<SubtleClientEvent>(OnSubtleClient);

        Subs.CVar(_config, ALCVars.ALSubtleRange, v => _range = v, true);
    }

    private void OnSubtleClient(SubtleClientEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        if (!CanSubtle(user))
            return;

        _adminLog.Add(LogType.ALSubtle, $"{ToPrettyString(user)} sent subtle emote:\n{msg.Emote}");

        var wrappedMessage = Loc.GetString("chat-manager-entity-me-wrap-message",
            ("entityName", Identity.Name(user, EntityManager)),
            ("entity", user),
            ("message", FormattedMessage.RemoveMarkupOrThrow(msg.Emote)));
        var coords = _transform.GetMapCoordinates(user);
        var userId = args.SenderSession.UserId;
        var chatFilter = Filter.Empty()
            .AddInRange(coords, _range, _player, EntityManager)
            .RemoveWhereAttachedEntity(e => _ghostQuery.HasComp(e));

        if (_net.IsServer)
        {
            foreach (var recipient in chatFilter.Recipients)
            {
                if (recipient == args.SenderSession)
                    continue;

                if (recipient.AttachedEntity is not { } recipientEnt)
                    continue;

                _popup.PopupEntity(Loc.GetString("al-subtle-received", ("target", recipientEnt)), recipientEnt, user);
            }
        }

        var audioFilter = chatFilter
            .Clone()
            .RemoveWhere(s =>
                s == args.SenderSession ||
                !_netConfiguration.GetClientCVar(s.Channel, ALCVars.ALSubtlePlaySound)
            );
        _audio.PlayGlobal(_sound, audioFilter, false);

        if (!msg.AntiGhost)
        {
            chatFilter.AddWhere(s =>
                _ghostQuery.HasComp(s.AttachedEntity) &&
                _netConfiguration.GetClientCVar(s.Channel, ALCVars.ALGhostSeeAllEmotes)
            );
        }

        _alChat.ChatMessageToMany(msg.Emote, wrappedMessage, chatFilter, ChatChannel.Emotes, user, recordReplay: true, author: userId);
    }

    public bool CanSubtle(EntityUid ent)
    {
        if (_mobState.IsIncapacitated(ent))
            return false;

        if (HasComp<GhostComponent>(ent) && !HasComp<BypassInteractionChecksComponent>(ent))
            return false;

        return true;
    }
}