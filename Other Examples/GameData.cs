[System.Serializable]
public class GameData {    
    public GameState gameState;
    public SelectState selectState;
    public int currentCube;
    public int[,] levelHowToBoss = new int[3, 2];
    public int[] levelUnlocks = new int[3];
    public int[] levelSelects = new int[3];
    public bool[] cubeCompletes = new bool[3];
    public bool[] didCutscene = new bool[6];
}
