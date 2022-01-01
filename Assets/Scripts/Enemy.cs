using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Player player;
    private Dungeon dungeon;
    private AStar_Pathfinding aStar;

    private DungeonVizualizer dungeonVizualizer = null;

    private Vector2Int enemyCell;
    public Vector2Int EnemyCell => nextCell;
    private Vector2Int nextCell;
    private Vector2 direction;

    private const float speed = 0.8f;

    private SpriteRenderer s_Renderer;
    private Animator animator;
    private int numberOfKicks = -1;
    private bool isAttacking = false;
    private float blinkTimer = 0;
    private float coeff = 0;
    private int health;

    public bool isDead => (health < 0);

    private void Start()
    {
        s_Renderer = GetComponent<SpriteRenderer>();
        dungeon = Dungeon.GetInstance();
        aStar = dungeon.GetAStar();

        GameObject playerObj = GameObject.Find("Player");
        player = playerObj.GetComponent<Player>();

        GameObject gridObj = GameObject.Find("Grid");
        dungeonVizualizer = gridObj.GetComponent<DungeonVizualizer>();

        animator = GetComponent<Animator>();
        animator.Play("walkDown");
        animator.speed = animator.speed + Random.Range(0.0f, 0.2f) - 0.1f;

        health = 200/*100 + Random.Range(-50, 50 + 1)*/;

        nextCell = GetPath(enemyCell, player.PlayerCell);
        aStar.setNodeObstacle(enemyCell, true);
    }

    private void Update()
    {
        UnblockObstacles();
        Move();
        BlockObstacles();
        Attack();
        SetDirection();
        SetState();
        Animate();
        Blink();
        MakeNearestDungeonElementTransparent();
    }

    private void Move()
    {
        /*if (nextCell == enemyCell)
            return;*/

        Vector3 nextCellPos = CoordsConverter.FromCellToWorldPoint(nextCell.x, nextCell.y);
        transform.position = Vector3.MoveTowards(transform.position, nextCellPos, speed * Time.deltaTime);

        if (transform.position == nextCellPos)
        {
            enemyCell = nextCell;
            nextCell = GetPath(enemyCell, player.PlayerCell);
        }
    }

    private Vector2Int GetPath(Vector2Int start, Vector2Int end)
    {
        Vector2Int path = start;
        float minDistance = 9999f;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                Vector2Int neighbour = start + new Vector2Int(x, y);

                if (neighbour == end)
                {
                    isAttacking = true;
                    return start;
                }

                if (aStar.getNodeObstacle(neighbour) == true)
                    continue;

                float d = distance(neighbour, end);

                if (d < minDistance)
                {
                    minDistance = d;
                    path = neighbour;
                }
            }
        }

        isAttacking = false;
        return path;

        float distance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
        };
    }

    private void Attack()
    {
        if (isAttacking == false)
            return;

        float time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        int numberOfRepeats = (int)time;
        float kickProgress = time - numberOfRepeats;

        if (numberOfKicks != numberOfRepeats && kickProgress > 0.42f)
        {
            numberOfKicks = numberOfRepeats;
            int damage = 1;
            player.ApplyDamage(damage);
        }
    }

    private void SetDirection()
    {
        direction = transform.position - player.transform.position;
    }

    private void SetState()
    {
        if (isAttacking == false)
        {
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
    private void Blink()
    {
        Color clr = s_Renderer.color;

        if (blinkTimer > 0)
        {                    
            coeff = blinkTimer % 1f;
            if (coeff < 0.5f)
                coeff = 1f - coeff;

            clr.a = coeff;
            s_Renderer.color = clr;
            blinkTimer -= Time.deltaTime * 2;
        }
        else
        {
            clr.a = 1f;
            s_Renderer.color = clr;
        }
    }

    public static void MakeBlink(Enemy enemy)
    {
        if(enemy != null)
            enemy.blinkTimer = 2;     
    }

    public void ApplyDamage(int damage)
    {
        health -= damage;
    }

    public void SetCell(int X, int Y)
    {
        enemyCell = new Vector2Int(X, Y);
    }

    private void UnblockObstacles()
    {
        if (aStar.getNodeObstacle(nextCell) == true)
            aStar.setNodeObstacle(nextCell, false);

        if (aStar.getNodeObstacle(enemyCell) == true)
            aStar.setNodeObstacle(enemyCell, false);
    }

    private void BlockObstacles()
    {
        aStar.setNodeObstacle(nextCell, true);
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
                    dungeonVizualizer.MakeDungeonElementTransparent(neighbour.x, neighbour.y);
            }
        }
    }
}
