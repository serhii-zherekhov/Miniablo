using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Dungeon dungeon;

    private AStar_Pathfinding aStar;
    private List<Vector2Int> path = null;

    [SerializeField]
    public DungeonKeeper dungeonKeeper = null;
    private Enemy target;

    [SerializeField]
    private Frame frame;

    [SerializeField]
    private HealthBar healthBar;
    private int health;
    private const int maxHealth = 9999;

    private Animator animator;
    private int numberOfKicks = -1;
    private bool isAttacking = false;
    [SerializeField]
    TextSystem textSystem;

    [SerializeField]
    private const float speed = 1.0f;
    private Vector2Int playerCell;
    private Vector2Int nextCell;
    private Vector2Int previousCell;
    public Vector2Int PlayerCell => nextCell;
    private Vector2 direction;

    /*DEBUG*/
    [SerializeField]
    private AudioSource punch;
    /*DEBUG*/

    private void Start()
    {
        dungeon = Dungeon.GetInstance();
        aStar = dungeon.GetAStar();

        animator = GetComponent<Animator>();
        direction = new Vector2(-1, 1);

        char ch = ' ';
        while (ch != '.')
        {
            int X = Random.Range(0, Dungeon.WIDTH);
            int Y = Random.Range(0, Dungeon.HEIGHT);
            if (dungeon.GetChar(new Vector2Int(X, Y)) == '.')
            {
                playerCell = new Vector2Int(X, Y);
                previousCell = nextCell = playerCell;
                transform.position = CoordsConverter.FromCellToWorldPoint(X, Y);
                ch = '.';
            }
        }

        health = maxHealth;
    }


    private void Update()
    {
        UnblockObstacles();
        MouseControl();
        Move();
        BlockObstacles();
        Attack();
        SetDirection();
        SetState();
        Animate();
        dungeonKeeper.SetActiveRoom(dungeon.GetRoomFromCell(playerCell));
        MakeNearestDungeonElementTransparent();
    }

    private void MouseControl()
    {
        Vector2Int mouseCell = GetMouseCell();
        frame.transform.position = CoordsConverter.FromCellToWorldPoint(mouseCell);

        frame.SetFrameColor(aStar.getNodeObstacle(mouseCell));

        if (Input.GetButtonDown("Fire1") && dungeon.GetChar(mouseCell) == '.')
        {
            target = dungeonKeeper.getEnemyFromCell(mouseCell);
            path = GetPath(playerCell, mouseCell, target != null);
            Enemy.MakeBlink(target);
        }
    }

    private void Move()
    {
        if (path == null)
            return;

        nextCell = path[0];
        Vector3 nextCellPos = CoordsConverter.FromCellToWorldPoint(nextCell.x, nextCell.y);
        transform.position = Vector3.MoveTowards(transform.position, nextCellPos, speed * Time.deltaTime);

        if (transform.position == nextCellPos)
        {
            playerCell = nextCell;

            if(target != null)
                path = GetPath(playerCell, target.EnemyCell, target != null);
            else
                path.RemoveAt(0);

            if (path != null && path.Count <= 0)
                path = null;
        }
    }

    private void UnblockObstacles()
    {
        if (aStar.getNodeObstacle(previousCell) == true)
            aStar.setNodeObstacle(previousCell, false);

        if (aStar.getNodeObstacle(nextCell) == true)
            aStar.setNodeObstacle(nextCell, false);

        if (aStar.getNodeObstacle(playerCell) == true)
            aStar.setNodeObstacle(playerCell, false);
    }

    private void BlockObstacles()
    {
        if (previousCell == nextCell)
        {
            aStar.setNodeObstacle(nextCell, true);
        }

        if (previousCell != nextCell)
        {
            aStar.setNodeObstacle(previousCell, false);
            aStar.setNodeObstacle(nextCell, true);
        }
        previousCell = nextCell;       
    }

    private void Attack()
    {
        if (isAttacking == false || target == null)
            return;

        float time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        int numberOfRepeats = (int)time;
        float kickProgress = time - numberOfRepeats;

        if (numberOfKicks != numberOfRepeats && kickProgress > 0.42f)
        {
            numberOfKicks = numberOfRepeats;
            int damage = Random.Range(20, 80 + 1); //weapon
            target.ApplyDamage(damage);

            textSystem.AddMessage("-" + /*DEBUG*/(int)damage/ 10/*DEBUG*/);

            /*DEBUG*/
            punch.Play();
            /*DEBUG*/
        }
    }

    private void SetDirection()
    {
        if (path != null)
            direction = transform.position - CoordsConverter.FromCellToWorldPoint(path[0].x, path[0].y);
        else if (target != null)
            direction = transform.position - target.transform.position;
    }

    private void SetState()
    {
        if (path == null && target != null)
        {
            isAttacking = true;
        }
        else
        {
            isAttacking = false;

            if (numberOfKicks > -1) numberOfKicks = -1;
        }
    }

    private void Animate()
    {
        string state = "";

        if (isAttacking == true)
            state = "attack";
        else
            state = "walk";

        if (direction.y >= 0 && direction.x > 0)
            animator.Play(state + "Left");

        if (direction.y < 0 && direction.x >= 0)
            animator.Play(state + "Up");

        if (direction.y <= 0 && direction.x < 0)
            animator.Play(state + "Right");

        if (direction.y > 0 && direction.x <= 0)
            animator.Play(state + "Down");      
    }

    private Vector2Int GetMouseCell()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        return CoordsConverter.FromWorldPointToCell(worldPos);
    }

    private List<Vector2Int> GetPath(Vector2Int start, Vector2Int end, bool toEnemy)
    {
        aStar.setNodeStart(start);
        aStar.setNodeEnd(end);
        List<Vector2Int> path = aStar.Solve_AStar();

        if (path != null)
        {
            path.Reverse();
            path.RemoveAt(0); //optional (если убрать, то персонаж будет идти немного иначе - сначала в точку своего изначального пребывания)
            if (toEnemy == true && path.Count > 0)
                path.RemoveAt(path.Count - 1);
            if (path.Count <= 0)
                path = null;
            return path;
        }
        else
        { 
            return null;
        }
    }

    private void MakeNearestDungeonElementTransparent()
    {
        Vector2Int playerPos = CoordsConverter.FromWorldPointToCell(new Vector3(this.transform.position.x, this.transform.position.y - 0.25f, transform.position.z));

        for (int x = -1; x <= 0; x++)
        {
            for (int y = 0; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                Vector2Int neighbour = playerPos + new Vector2Int(x, y);

                if (dungeon.GetChar(neighbour) == '#')
                    dungeonKeeper.dungeonVizualizer.MakeDungeonElementTransparent(neighbour.x, neighbour.y);
            }
        }
    }

    public void ApplyDamage(int damage)
    {
        health -= damage;
        healthBar.SetHeight((float)health / (float)maxHealth);
    }

    private void OnDrawGizmosSelected()
    {
        if (aStar == null) return;

        float nNodeSize = 0.1f;

        float offsetY = 0.25f;

        // Draw Connections First - lines from this nodes position to its
        // connected neighbour node positions

        Gizmos.color = Color.blue;
        for (int x = 0; x < Dungeon.WIDTH; x++)
            for (int y = 0; y < Dungeon.HEIGHT; y++)
            {
                foreach (AStar_Pathfinding.sNode n in aStar._nodes[y * Dungeon.WIDTH + x].vecNeighbours)
                {
                    Vector3 vec3 = CoordsConverter.FromCellToWorldPoint(x, y);
                    Vector3 vec3_n = CoordsConverter.FromCellToWorldPoint(n.x, n.y);
                    Gizmos.DrawLine(new Vector3(vec3.x, vec3.y - offsetY, 0), new Vector3(vec3_n.x, vec3_n.y - offsetY, 0));

                    //Gizmos.DrawLine(new Vector3(x * nNodeSize + x * nNodeBorder, -y * nNodeSize - y * nNodeBorder, 0), new Vector3(n.x * nNodeSize + n.x * nNodeBorder, -n.y * nNodeSize - n.y * nNodeBorder, 0));
                }
            }
        Gizmos.color = Color.white;

        // Draw Nodes on top
        for (int y = 0; y < Dungeon.HEIGHT; y++)
            for (int x = 0; x < Dungeon.WIDTH; x++)
            {
                if (aStar._nodes[y * Dungeon.WIDTH + x].bObstacle == true)
                    Gizmos.color = Color.white;
                else
                    Gizmos.color = Color.blue;

                /*if (aStar._nodes[y * Dungeon.WIDTH + x].bVisited == true)
                    Gizmos.color = Color.cyan;

                if (aStar._nodes[y * Dungeon.WIDTH + x] == aStar.nodeStart)
                    Gizmos.color = Color.green;

                if (aStar._nodes[y * Dungeon.WIDTH + x] == aStar.nodeEnd)
                    Gizmos.color = Color.red;*/

                Vector3 vec3 = CoordsConverter.FromCellToWorldPoint(x, y);
                Gizmos.DrawCube(new Vector3(vec3.x, vec3.y - offsetY, 0), new Vector3(nNodeSize, nNodeSize, 0));
                //Gizmos.DrawCube(new Vector3(x * nNodeSize + x * nNodeBorder, -y * nNodeSize - y * nNodeBorder, 0), new Vector3(nNodeSize, nNodeSize, 0));

                Gizmos.color = Color.white;
            }


        /*Gizmos.color = Color.yellow;
        // Draw Path by starting ath the end, and following the parent node trail
        // back to the start - the start node will not have a parent path to follow
        if (aStar.nodeEnd != null)
        {
            AStar_Pathfinding.sNode p = aStar.nodeEnd;
            while (p.parent != null)
            {
                Vector3 vec3 = CoordsConverter.FromCellToWorldPoint(p.x, p.y);
                Vector3 vec3_p = CoordsConverter.FromCellToWorldPoint(p.parent.x, p.parent.y);
                Gizmos.DrawLine(new Vector3(vec3.x, vec3.y - offsetY, 0), new Vector3(vec3_p.x, vec3_p.y - offsetY, 0));

                //Gizmos.DrawLine(new Vector3(p.x * nNodeSize + p.x * nNodeBorder, -p.y * nNodeSize - p.y * nNodeBorder, 0), new Vector3(p.parent.x * nNodeSize + p.parent.x * nNodeBorder, -p.parent.y * nNodeSize - p.parent.y * nNodeBorder, 0));

                // Set next node to this node's parent
                p = p.parent;
            }
        }
        Gizmos.color = Color.white;*/

    }
}
