using UnityEngine;


public class LevelStatesTest : MonoBehaviour
{
    void Start() 
    {
        Level1States.Instance.setStartCount(1);
        Level2States.Instance.unlockLevel();
        Level2States.Instance.setStartCount(2);
        Debug.Log("1 - lockedLevel: " + Level1States.Instance.getLockedLevel() + ", starsCount: " + Level1States.Instance.getStarCount());
        Debug.Log("2 - lockedLevel: " + Level2States.Instance.getLockedLevel() + ", starsCount: " + Level2States.Instance.getStarCount());
        Debug.Log(Level1States.Instance.level1SpecificVar);
    }
}
