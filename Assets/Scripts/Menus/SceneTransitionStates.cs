/*
 Variables for loading scenes e.g., loading directly to the level selector
*/
public static class SceneTransitionStates
{
    private static bool gameStart = true;
    private static int selectedLevel = 0;

    public static bool IsGameStart(){ return gameStart; }
    public static void SetGameStart(bool b){ gameStart = b; }
    public static int GetSelectedLevel(){ return selectedLevel; }
    public static void SetSelectedLevel(int i){ selectedLevel = i; }

}

