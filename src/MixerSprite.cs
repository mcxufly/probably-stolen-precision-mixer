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
