// SPDX-FileCopyrightText: 2025 Afterlight RnD
// SPDX-License-Identifier: ASL-1.0

using Content.Client.UserInterface.Systems.Character;
using Content.Shared._Starlight.Character.Info;
using Robust.Client.UserInterface;

// ReSharper disable CheckNamespace

namespace Content.Client.CharacterInfo;

public sealed partial class CharacterInfoSystem
{
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    private CharacterUIController _controller => _ui.GetUIController<CharacterUIController>();

    private void SL_Initialize()
    {
        // TODO AFTERLIGHT move to the UI controller when subscriptions there don't get wiped by disconnecting without restarting
        SubscribeLocalEvent<OpenInspectCharacterInfoEvent>(OnOpenCharacterInspect);
    }

    private void OnOpenCharacterInspect(OpenInspectCharacterInfoEvent ev)
    {
        // TODO AFTERLIGHT move to the UI controller when subscriptions there don't get wiped by disconnecting without restarting
        _controller.OpenInspectCharacterWindow(ev);
    }
}