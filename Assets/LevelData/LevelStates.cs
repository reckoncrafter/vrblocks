public class LevelState
{
    private string name;
    private bool isLockedLevel = true;
    private int starCount = 0;

    public LevelState(string name)
    {
        this.name = name;
    }

    public string getName(){ return name; }
    public bool getIsLockedLevel(){ return isLockedLevel; }
    public void setIsLockedLevel(bool isLocked){ isLockedLevel = isLocked; }
    public int getStarCount(){ return starCount; }
    public void setStartCount(int stars){ starCount = stars; }
}