using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonListener : MonoBehaviour {
	
	// Use this for initialization
	

	public void ResetMazeTask()
	{
		Destroy(GameObject.Find("Mazer"));
		GameObject mazeGenerator = GameObject.Find("MazeGenerator").gameObject;
		mazeGenerator.AddComponent<Maze>();
		
	}
	public void StartLevelWithDifficulty(int difficulty)
	{
		ApplicationSettings.difficulty = difficulty;
		SceneManager.LoadScene("MazeGenerator");
	}

	public void StartLevelNoMaze()
	{
		SceneManager.LoadScene("NoWalls");
	}
	public void ExitTask()
	{
#if UNITY_EDITOR
		// Application.Quit() does not work in the editor so
		// UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
		UnityEditor.EditorApplication.isPlaying = false;
#else
		 Application.Quit();
#endif
	}
	
}
