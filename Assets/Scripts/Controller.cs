using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriuAdjaciencia = new int[Constants.NumTiles, Constants.NumTiles];

        //TODO-DONE: Inicializar matriz a 0's
        FillingEmptyMatriu(matriuAdjaciencia);
        // Test_fillingEmptyMatriu(matriuAdjaciencia);

        //TODO-DONE: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)
        FillingAdjacentMatriu(matriuAdjaciencia);
        // Test_fillingAdjacentMatriu(matriuAdjaciencia);

        //TODO-DONE: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes
        FillingAdjacenyLists(tiles, matriuAdjaciencia);
        // Test_fillingAdjacenyLists(tiles);
    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);
        /*TODO: Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        - Movemos al caco a esa casilla
        - Actualizamos la variable currentTile del caco a la nueva casilla
        */
        int newPosition = FindNewRamdomPosition();
        //Test_findNewRamdomPosition(newPosition);

        robber.GetComponent<RobberMove>().MoveToTile(tiles[newPosition]);
        robber.GetComponent<RobberMove>().currentTile = tiles[newPosition].numTile;
        tiles[newPosition].current = true;

    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
                 
        int indexcurrentTile;        

        if (cop==true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO-DONE: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendrás que cambiar este código por el BFS
        /*
        for(int i = 0; i < Constants.NumTiles; i++)
        {
            tiles[i].selectable = true;
        }
        */
        FindSelectables(indexcurrentTile, nodes);
        Test_findSelectables(indexcurrentTile, nodes);
    }


    /********************************************************************************************
    Internal Function
    ********************************************************************************************/
    private void FillingEmptyMatriu(int[,] matriuAdjaciencia)
    {
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                matriuAdjaciencia[i, j] = 0;
            }
        }
    }

    private void FillingAdjacentMatriu(int[,] matriuAdjaciencia)
    {
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                if (i == j)
                {
                    matriuAdjaciencia[i, j] = 0;
                }
                else
                {
                    if ((((j == i - 1) || (j == i + 1)) || ((j == i - 8) || (j == i + 8))))
                    {
                        if ((j % 8 == 0) && (i == j - 1)) matriuAdjaciencia[i, j] = 0;
                        else if (((j + 1) % 8 == 0) && (i == j + 1)) matriuAdjaciencia[i, j] = 0;
                        else matriuAdjaciencia[i, j] = 1;
                    }
                    else
                    {
                        matriuAdjaciencia[i, j] = 0;
                    }
                } 
            }
        }
    }

    private void FillingAdjacenyLists(Tile[] tiles, int[,] matriuAdjaciencia)
    {
            for (int i = 0; i < Constants.NumTiles; i++)
            {
                for (int j = 0; j < Constants.NumTiles; j++)
                {
                    if (matriuAdjaciencia[i, j] == 1)
                    {
                        tiles[i].adjacency.Add(j);
                    }
                }
            }
    }

    private void FindSelectables(int indexcurrentTile, Queue<Tile> nodes)
    {
        foreach (int i in tiles[indexcurrentTile].adjacency)
        {
            nodes.Enqueue(tiles[i]);
            foreach (int j in tiles[i].adjacency)
            {
                if (!(nodes.Contains(tiles[j])))
                    if (!(tiles[j] == tiles[indexcurrentTile]))
                        nodes.Enqueue(tiles[j]);
            }
        }
        foreach (Tile tile in nodes)
            tile.selectable = true;
    }

    private int FindNewRamdomPosition()
    {
        List<int> alternatives = new List<int>();

        for (int i = 0; i < Constants.NumTiles; i++)
        {
            int ForbidenTile_0 = cops[0].GetComponent<CopMove>().currentTile;
            int ForbidenTile_1 = cops[1].GetComponent<CopMove>().currentTile;

            Debug.Log("Forbiden tiles = " + " " + ForbidenTile_1.ToString() + " , " + ForbidenTile_0.ToString());

            if (tiles[i].selectable == true)
                if ((i != ForbidenTile_0) && (i != ForbidenTile_1))
                    alternatives.Add(i);
        }
        int alternativa = Random.Range(0, alternatives.Count - 1);
        int[] alternativesArray = alternatives.ToArray();
        return alternativesArray[alternativa];
    }


    /********************************************************************************************
    Functional Tests
    ********************************************************************************************/
    private void Test_fillingEmptyMatriu(int[,] matriuAdjaciencia)
    {
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                if (matriuAdjaciencia[i, j] != 0)
                {
                    Debug.Log("TESTING:    Error al initzializar la matriuAdjaciencia d'acjacencia a la posició" + i.ToString() + " " + j.ToString());
                }
                else
                {
                    Debug.Log ("TESTING:    Creada la matriuAdjaciencia d'acjacencia neta inicial correctament");
                }
            }
        }
    }

    private void Test_fillingAdjacentMatriu(int[,] matriuAdjaciencia)
    {
        string adjacienciaString = "";

        for (int i = 0; i < Constants.NumTiles; i++)
        {
            adjacienciaString = "\nV" + i.ToString() + " adjacent a ";
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                if (matriuAdjaciencia[i, j] == 1)
                {
                    adjacienciaString = adjacienciaString + "V" + j.ToString() + " ";
                }
            }
            Debug.Log(adjacienciaString);
        }

    }

    private void Test_fillingAdjacenyLists(Tile[] tiles)
    {
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            string adjacienciaString = "V" + i.ToString() + ": ";
            foreach (int j in tiles[i].adjacency)
                adjacienciaString = adjacienciaString + "V" + j.ToString() + " ";
            Debug.Log(adjacienciaString);
        }
    }

    private void Test_findSelectables(int indexcurrentTile, Queue<Tile> nodes)
    {
        string infoString = "";
            infoString = infoString + "From " + indexcurrentTile.ToString() + " can be selected: ";
            foreach (Tile tile in nodes)
            {
                    infoString = infoString + tile.numTile.ToString() + " ";
            }
        Debug.Log(infoString);
    }

    private void Test_findNewRamdomPosition(int position)
    {
        int indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        Debug.Log("posición inicial del caco: " + indexcurrentTile.ToString());

        Debug.Log ("Nueva posición del caco: " + tiles[position].numTile);
    }
}
