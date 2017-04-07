using System.Collections;
using System.Linq;
using UnityEngine;

namespace CompleteProject
{
    public class EnemyManager : MonoBehaviour
    {
        public GameObject enemy;                // The enemy prefab to be spawned.
        
        IEnumerator Start ()
        { 
            
            yield return new WaitForSeconds(2);
            // Call the Spawn function after a delay of the spawnTime and then continue to call after the same amount of time.
            Invoke ("Spawn",0);
        }


        void Spawn ()
        {
            //Get Points where enemy can guard and wander, points where the way splits in at least 3
            var possiblePoints = ApplicationSettings.neighbours.ToList().Where(x => x.Count > 2 ).ToList();
            // Find a random index between zero and one less than the number of spawn points.
            var difficulty = ApplicationSettings.difficulty;

            //get cell index in grid array and then world coordinates to spawn enemy.
            for (int i = 0; i < difficulty * 5; i++)
            {
                int spawnCell = Random.Range(0, possiblePoints.Count);
                int index = 0;
                int cell = 0;
                foreach (var n in ApplicationSettings.neighbours)
                {
                    if (n.Count > 2) {
                        if (index == spawnCell)
                            break;
                        index++;
                    }
                    cell++;
                }
                var gridX = cell % Mathf.Sqrt((float)Grid.grid.Length);
                var gridY = cell / Mathf.Sqrt((float)Grid.grid.Length);
                Debug.Log(cell);
                var worldPos = Grid.grid[(int)gridX, (int)gridY].worldPosition;
                // Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
                Instantiate(enemy,worldPos - new Vector3(0,0.5f,0) ,Quaternion.identity);
            }

            
        }
    }
}