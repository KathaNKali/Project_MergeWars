namespace MergeWars.Heroes
{
    /// <summary>
    /// A hero's core combat role — the "Class" layer of the character
    /// architecture (see /GameDocs/Systems/SYS_HeroDefinition.md). Replaces
    /// the old HeroClassId (Ground/Air/Vehicles), which was a
    /// movement/targeting tag, not a combat role. This is fixed for a
    /// hero's lifetime — it does not change on merge/star-up.
    /// </summary>
    public enum HeroRole
    {
        Tank,
        MeleeDps,
        RangedDps,
        Support,
        Controller
    }
}
