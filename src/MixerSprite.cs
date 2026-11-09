// Copyright (C) 2026 XuFly
// SPDX-License-Identifier: GPL-3.0-only
// See LICENSE and NOTICE for license terms and warranty disclaimer.

namespace ProbablyStolenMixer
{
    internal static class MixerSprite
    {
        public static string Key(bool hasTarget, bool hasSource)
        {
            if (hasTarget && hasSource) return "precision_mixer_both";
            if (hasTarget) return "precision_mixer_target";
            if (hasSource) return "precision_mixer_source";
            return "precision_mixer";
        }
    }
}
