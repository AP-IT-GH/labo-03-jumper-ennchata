using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour {
    public GameObject obstacleTemplate;
    public Transform obstacleParent;
    public float MaxObstaclePosition = 16f;
    public float ObstacleSpawnDistanceModifier = 125f;

    private readonly List<GameObject> obstacles = new List<GameObject>();
    private readonly List<GameObject> passsedObstacles = new List<GameObject>();

    public void CreateObstacle() {
        GameObject item = Instantiate(obstacleTemplate, obstacleParent);
        item.SetActive(true);
        obstacles.Add(item);
    }

    public void TryCreateObstacle(float velocity) {
        if (obstacles.Count == 0) {
            CreateObstacle();
            return;
        }

        GameObject newest = obstacles.Last();
        float distance = Vector3.Distance(obstacleTemplate.transform.position, newest.transform.position);

        if (distance >= velocity * ObstacleSpawnDistanceModifier) CreateObstacle();
    }

    public void ClearObstacles() {
        foreach (GameObject item in obstacles) Destroy(item);
        obstacles.Clear();
    }

    public void MoveObstacles(float velocity) {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject item in obstacles) {
            item.transform.localPosition += new Vector3(velocity, 0, 0);
            if (item.transform.localPosition.x > MaxObstaclePosition && passsedObstacles.Contains(item)) 
                toRemove.Add(item);
        }

        foreach (GameObject item in toRemove) {
            if (obstacles.Contains(item)) obstacles.Remove(item);
            if (passsedObstacles.Contains(item)) passsedObstacles.Remove(item);
            Destroy(item);
        }
    }

    public int CountObstaclesPassed(Vector3 position) {
        int count = 0;
        foreach (GameObject item in obstacles) {
            if (passsedObstacles.Contains(item)) continue;

            if (item.transform.position.x > position.x) {
                count++;
                passsedObstacles.Add(item);
            }
        }
        return count;
    }
}
