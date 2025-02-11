public abstract class LevelStates
{
    private bool lockedLevel = true;
    private int starCount = 0;

    public bool getLockedLevel(){ return lockedLevel; }
    public void unlockLevel(){ lockedLevel = false; }
    public int getStarCount(){ return starCount; }
    public void setStartCount(int stars){ starCount = stars; }
}