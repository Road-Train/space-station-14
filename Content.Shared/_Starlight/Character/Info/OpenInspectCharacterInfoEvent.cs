// SPDX-FileCopyrightText: 2025 Starlight
// SPDX-License-Identifier: Starlight-MIT

namespace Content.Shared._Starlight.Character.Info;

public readonly record struct OpenInspectCharacterInfoEvent(EntityUid Target, EntityUid Viewer);