using System;

public sealed class Level2States: LevelStates
{
    private static readonly Lazy<Level2States> lazySingleton =
        new Lazy<Level2States>(() => new Level2States());
    public static Level2States Instance { get { return lazySingleton.Value; } }
    private Level2States(){}
}