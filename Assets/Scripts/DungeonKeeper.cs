using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonKeeper : MonoBehaviour
{
    private AStar_Pathfinding aStar = null;

    [SerializeField]
    public DungeonVizualizer dungeonVizualizer;

    [SerializeField]
    private GameObject s_imp;
    [SerializeField]
    private GameObject s_skeleton;
    [SerializeField]
    private GameObject s_wizard;

    private Dungeon dungeon;
    private Dictionary<int, List<Enemy>> rooms = new Dictionary<int, List<Enemy>>();
    private int activeRoom = Dungeon.NOROOM;

    private void Start()
    {
        dungeon = Dungeon.GetInstance();
        aStar = dungeon.GetAStar();

        foreach (Vector3Int enemyPos in dungeon._enemies)
        {
            GameObject objToInstantiate = null;
            int r = Random.Range(0, 3);

            if (r == 0) objToInstantiate = s_imp;
            if (r == 1) objToInstantiate = s_skeleton;
            if (r == 2) objToInstantiate = s_wizard;

            Vector3 vec3 = CoordsConverter.FromCellToWorldPoint(enemyPos.x, enemyPos.y);
            GameObject enemyObj = Instantiate(objToInstantiate, new Vector3(vec3.x, vec3.y, 0), Quaternion.identity, this.transform);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            enemy.SetCell(enemyPos.x, enemyPos.y);

            int enemyRoom = enemyPos.z;
            if (rooms.ContainsKey(enemyRoom) == false)
                rooms.Add(enemyRoom, new List<Enemy>());

            rooms[enemyRoom].Add(enemy);

            enemyObj.SetActive(false);
            aStar.setNodeObstacle(enemy.EnemyCell, false);
        }
    }


    private void Update()
    {
        destroyDeadEnemies();
    }

    public Enemy getEnemyFromCell(Vector2Int cell)
    {
        if (rooms.ContainsKey(activeRoom) == false)
            return null;

        Enemy enemyToGet = null;

        foreach(List<Enemy> enemies in rooms.Values)
        {
            foreach (Enemy enemy in enemies)
            {
                if (cell == enemy.EnemyCell && enemy.gameObject.activeSelf)
                {
                    enemyToGet = enemy;
                }
            }
        }
   
        return enemyToGet;
    }

    private void destroyDeadEnemies()
    {
        if (rooms.ContainsKey(activeRoom) == false)
            return;

        foreach (Enemy enemy in rooms[activeRoom])
        {
            if (enemy.isDead == true)
            {
                aStar.setNodeObstacle(enemy.EnemyCell, false);
                rooms[activeRoom].Remove(enemy);
                Destroy(enemy.gameObject);
                break;
            }
        }
        
    }

    public void SetActiveRoom(int roomToActivate)
    {
        if (activeRoom == roomToActivate)
            return; 

        if (rooms.ContainsKey(activeRoom))
            foreach (Enemy enemy in rooms[activeRoom])
            {
                enemy.gameObject.SetActive(false);
                aStar.setNodeObstacle(enemy.EnemyCell, false);
            }

        if (rooms.ContainsKey(roomToActivate))
            foreach (Enemy enemy in rooms[roomToActivate])
            {
                enemy.gameObject.SetActive(true);
                aStar.setNodeObstacle(enemy.EnemyCell, true);
            }

        activeRoom = roomToActivate;

        dungeonVizualizer.SetActiveRoom(activeRoom);
    }
}
