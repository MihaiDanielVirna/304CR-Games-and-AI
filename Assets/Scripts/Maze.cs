using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Maze : MonoBehaviour {
	[System.Serializable]
	public class Cell
	{
		public bool visited;
		public GameObject north;
		public GameObject east;
		public GameObject west;
		public GameObject south;
	}

	public GameObject wallPrefab;
	public float wallLength = 1.0f;
	public int xSize = 40;
	public int ySize = 20;
	public Cell[] cells;
	private Vector3 initialPos;
	private GameObject wallHolder;
	private int visitedCells = 0;
	private bool startedBuilding = false;
	private int currentNeighbour = 0;
	private List<int> lastCells = new List<int>();
	private int backingUp = 0;
	private int currentCell = 0;
	private int wallToBreak = 0;
	private Vector3 initialPosition;
	// Use this for initialization
	public void Start ()
	{
        
		wallHolder = new GameObject();
		wallHolder.name = "Mazer";
		ApplicationSettings.neighbours = new List<Vector2>[xSize * ySize];
		CreateWalls();

	}
	
	void CreateWalls()
	{
		initialPos = new Vector3(-xSize / 2 + wallLength / 2, 0.0f, (-ySize / 2) + wallLength / 2);
		Vector3 myPos = initialPos;
		initialPosition = initialPos;
		Debug.Log(initialPos);
		GameObject tempWall;
		for (int i = 0; i < ySize; i++)
		{
			for (int j = 0; j <= xSize; j++)
			{
				myPos = new Vector3(initialPos.x + (j * wallLength) - wallLength / 2, 0.0f, initialPos.z + (i * wallLength) - wallLength / 2);
				tempWall = Instantiate(Resources.Load("Wall"), myPos, Quaternion.identity) as GameObject;
				tempWall.transform.parent = wallHolder.transform;
				tempWall.transform.localScale = new Vector3(0.1f, 1, wallLength);
			}
		}
		for (int i = 0; i <= ySize; i++)
		{
			for (int j = 0; j < xSize; j++)
			{
				myPos = new Vector3(initialPos.x + (j * wallLength), 0.0f, initialPos.z + (i * wallLength) - wallLength);
				tempWall = Instantiate(Resources.Load("Wall"), myPos, Quaternion.Euler(0,90.0f,0)) as GameObject;
				tempWall.transform.parent = wallHolder.transform;
				tempWall.transform.localScale = new Vector3(0.1f, 1, wallLength);
			}
		}

		CreateCells();
		if (SceneManager.GetActiveScene().name.Equals("MainMenu")){
			wallHolder.transform.Rotate(new Vector3(1, 0, 0), 90f);
			wallHolder.transform.localPosition = new Vector3(0f, 5f, 100f);
			wallHolder.transform.localScale = new Vector3(7, 7, 7);

			var currentPosition = new Vector3(initialPosition.x, 0, initialPosition.z - wallLength / 2);
			var floor = Instantiate(Resources.Load("Floor"), currentPosition, Quaternion.Euler(0, 0, 90)) as GameObject;
			floor.transform.localScale = new Vector3(xSize * 7, ySize * 7, floor.transform.localScale.z);
			floor.transform.Rotate(new Vector3(0, 0, 1), 90f);
			floor.transform.localPosition = new Vector3(0f, 5f, 102f);
		}
		else
		{
			var currentPosition = new Vector3(initialPosition.x, 0, initialPosition.z - wallLength / 2);
			var floor = Instantiate(Resources.Load("Floor"), currentPosition, Quaternion.Euler(0, 0, 90)) as GameObject;
			floor.transform.localScale = new Vector3(floor.transform.localScale.z, ySize * wallLength, xSize * wallLength);
			floor.transform.localPosition = new Vector3(wallLength/2, -1, 0/*- wallLength/2*/);
		}
}

	void CreateCells()
	{
		int children = wallHolder.transform.childCount;
		GameObject[] allWalls = new GameObject[children];
		cells = new Cell[xSize * ySize];
		int eastWestProcess = 0;
		int childProcess = 0;
		int termCount = 0;

		for (int i = 0; i < children; i++)
		{
			allWalls[i] = wallHolder.transform.GetChild(i).gameObject;
		}

		for (int progress = 0; progress < cells.Length; progress++)
		{
			cells[progress] = new Cell()
			{
				east = allWalls[eastWestProcess],
				south = allWalls[childProcess + (xSize + 1) * ySize]
			};
			if (termCount == xSize)
			{
				eastWestProcess += 2;
				termCount = 0;
			}
			else eastWestProcess++;
			termCount++;
			childProcess++;
			cells[progress].west = allWalls[eastWestProcess];
			cells[progress].north = allWalls[(childProcess + (xSize + 1) * ySize) + xSize - 1];

		}
		StartCoroutine("CreateMaze");
	}

	IEnumerator CreateMaze()
	{
		while(visitedCells < ySize * xSize)
		{
			if (startedBuilding)
			{
				GiveNeighbours();
				if(!cells[currentNeighbour].visited && cells[currentCell].visited)
				{
					BreakWall();
					cells[currentNeighbour].visited = true;
					visitedCells++;
					lastCells.Add(currentCell);
					currentCell = currentNeighbour;
					if (lastCells.Count > 0)
					{
						backingUp = lastCells.Count - 1;
					}
				}
			}
			else
			{
				currentCell = UnityEngine.Random.Range(0, ySize * xSize);
				cells[currentCell].visited = true;
				visitedCells++;
				startedBuilding = true;
			}
			//Invoke("CreateMaze", 0f);
			//CreateMaze();
			if (SceneManager.GetActiveScene().name.Equals("MainMenu"))
			{
				yield return null;
			}

		}
		//CreateFloor();
		//yield return null;

	}
	

	void BreakWall()
	{

		var gridX = currentCell % xSize;
		var gridY = currentCell / xSize;
		if(ApplicationSettings.neighbours[currentCell] == null)
		{
			ApplicationSettings.neighbours[currentCell] = new List<Vector2>();
		}
		switch (wallToBreak)
		{
			case 1:
				Destroy(cells[currentCell].north);
				ApplicationSettings.neighbours[currentCell].Add(new Vector2(gridX,gridY+1));
				if (ApplicationSettings.neighbours[(gridY + 1) * xSize + gridX] == null)
				{
					ApplicationSettings.neighbours[(gridY + 1) * xSize + gridX] = new List<Vector2>();
				}
				ApplicationSettings.neighbours[(gridY + 1) * xSize + gridX].Add(new Vector2(gridX, gridY));
				break;
			case 2:
				Destroy(cells[currentCell].east);
				ApplicationSettings.neighbours[currentCell].Add(new Vector2(gridX - 1, gridY));
				if (ApplicationSettings.neighbours[gridY * xSize + gridX - 1] == null)
				{
					ApplicationSettings.neighbours[gridY * xSize + gridX - 1] = new List<Vector2>();
				}
				ApplicationSettings.neighbours[gridY * xSize + gridX - 1].Add(new Vector2(gridX, gridY));
				break;
			case 3:
				Destroy(cells[currentCell].west);
				ApplicationSettings.neighbours[currentCell].Add(new Vector2(gridX + 1, gridY));
				if (ApplicationSettings.neighbours[gridY * xSize + gridX + 1] == null)
				{
					ApplicationSettings.neighbours[gridY * xSize + gridX + 1] = new List<Vector2>();
				}
				ApplicationSettings.neighbours[gridY  * xSize + gridX + 1].Add(new Vector2(gridX, gridY));
				break;
			case 4:
				Destroy(cells[currentCell].south);
				ApplicationSettings.neighbours[currentCell].Add(new Vector2(gridX, gridY - 1));
				if (ApplicationSettings.neighbours[(gridY - 1) * xSize + gridX] == null)
				{
					ApplicationSettings.neighbours[(gridY - 1) * xSize + gridX] = new List<Vector2>();
				}
				ApplicationSettings.neighbours[(gridY - 1) * xSize + gridX].Add(new Vector2(gridX, gridY));
				break;
		}
	}

	void GiveNeighbours()
	{
		int length = 0;
		int[] neighbours = new int[4];
		int[] connectingWall = new int[4];
		int check = 0;

		check = (currentCell + 1) / xSize;
		check -= 1;
		check *= xSize;
		check += xSize;
		if (currentCell + 1 < xSize * ySize && (currentCell + 1) != check)
		{
			if (!cells[currentCell + 1].visited)
			{
				neighbours[length] = currentCell + 1;
				connectingWall[length] = 3;
				length++;
			}
		}
		if (currentCell - 1 >= 0 && currentCell != check)
		{
			if (!cells[currentCell - 1].visited)
			{
				neighbours[length] = currentCell - 1;
				connectingWall[length] = 2;
				length++;
			}
		}
		if (currentCell + xSize < xSize * ySize)
		{
			if (!cells[currentCell + xSize].visited)
			{
				neighbours[length] = currentCell + xSize;
				connectingWall[length] = 1;
				length++;
			}
		}
		if (currentCell - xSize >= 0)
		{
			if (!cells[currentCell - xSize].visited)
			{
				neighbours[length] = currentCell - xSize;
				connectingWall[length] = 4;
				length++;
			}
		}

		if (length != 0)
		{
			int nextNeighbour = UnityEngine.Random.Range(0, length);
			currentNeighbour = neighbours[nextNeighbour];
			wallToBreak = connectingWall[nextNeighbour];
		}
		else
		{
			if (backingUp > 0)
			{
				currentCell = lastCells[backingUp];
				backingUp--;
			}

		}

		
	}
	// Update is called once per frame
	void Reset () {
		
	}
}
