using System;

public sealed class Level1States: LevelStates
{
    private static readonly Lazy<Level1States> lazySingleton =
        new Lazy<Level1States>(() => new Level1States());
    public static Level1States Instance { get { return lazySingleton.Value; } }
    private Level1States(){
        
    }
    public string level1SpecificVar = "sus";
}

// https://csharpindepth.com/Articles/Singleton
// public sealed class Level1States : LevelStates
// {
//     private static readonly Level1States singletonInstance = new Level1States();
//     static Level1States(){}
//     private Level1States(){}
//     public static Level1States Instance { get { return singletonInstance; } }
// }