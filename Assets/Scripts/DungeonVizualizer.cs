using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonVizualizer : MonoBehaviour
{
    /*Debug section*/
    Vector2Int prevCell;
    Player player;
    List<Vector2Int> cellsToPaint = new List<Vector2Int>(9);
    /*Debug section*/
    private Dungeon dungeon;
    private int H = Dungeon.HEIGHT;
    private int W = Dungeon.WIDTH;
    [SerializeField]
    GameObject pr_floor;
    [SerializeField]
    GameObject pr_wall;
    [SerializeField]
    GameObject pr_ceiling;

    /*[SerializeField]
    private float Radius = 5f;*/

    private List<DungeonElement> dungeonElements = new List<DungeonElement>();

    private List<RectInt> rooms = new List<RectInt>();
    private int activeRoom = Dungeon.NOROOM;

    private void Start()
    {
        dungeon = Dungeon.GetInstance();

        string str = "";

        //Print dungeon to stream
        for (int y = 0; y < H; y++)
        {
            string x_str = "";
            List<DungeonElement> dungeonElements_X = new List<DungeonElement>();
            for (int x = W - 1; x >= 0; x--)
            {
                char ch = dungeon._dungeon[y, x];
                x_str += ch;

                dungeonElements_X.Add(InstantiateDungeonElement(x, y, ch));
            }

            str += StringFuncs.ReverseString(x_str) + "\n";

            dungeonElements_X.Reverse();
            foreach (DungeonElement dungeonElement in dungeonElements_X)
                dungeonElements.Add(dungeonElement);
        }

        Debug.Log(str);

        rooms = dungeon.GetRooms();

        /*Debug section*/
        /*GameObject obj = GameObject.Find("Player");
        player = obj.GetComponent<Player>();
        prevCell = CoordsConverter.FromWorldPointToCell(player.transform.position - new Vector3(0, 0.25f, 0));*/
        /*Debug section*/
    }

    private void Update()
    {/*Debug section*/
        /*Vector2Int playerCell = CoordsConverter.FromWorldPointToCell(player.transform.position - new Vector3(0, 0.25f, 0));

        if (playerCell == prevCell && cellsToPaint.Count > 0)
            return;

        SetAreaBrightness(playerCell);

        prevCell = playerCell;*/
        /*Debug section*/
    }

    /*FOR DEBUG*/
    /*private void SetAreaBrightness(Vector2Int playerCell)
    {
        foreach(Vector2Int cell in cellsToPaint)
        {
            DungeonElement d = GetDungeonElement(cell.x, cell.y);
            d.SetBrightness(0f);
        }    

        cellsToPaint.Clear();

        float max = -1;
        float min = 9999;

        int R = (int)(Radius);//5;

        for (int x = -R; x <= R; x++)
        {
            for (int y = -R; y <= R; y++)
            {
                float dist = distance(playerCell, playerCell + new Vector2Int(x, y));

                if (dist > max)
                    max = dist;

                if (dist < min)
                    min = dist;
            }
        }


        string str = "";

        for (int x = -R; x <= R; x++)
        {
            for (int y = -R; y <= R; y++)
            {
                Vector2Int cell = playerCell + new Vector2Int(x, y);

                float dist = distance(playerCell, cell);
                float brightness = dist / max;

                str += "x: " + cell.x + " y: " + cell.y + " b:" + brightness + "\n";

                DungeonElement d = GetDungeonElement(cell.x, cell.y);
                d.SetBrightness(1 - brightness);

                cellsToPaint.Add(cell);
            }
        }

        float distance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
        };
    }*/
    /*FOR DEBUG*/

    private DungeonElement InstantiateDungeonElement(int x, int y, char ch)
    {
        GameObject obj = Instantiate(prefab(ch), vec3(x, y), Quaternion.identity, this.transform);
        DungeonElement dungeonElement = obj.GetComponent<DungeonElement>();
        return dungeonElement;
    }

    private Vector3 vec3(int x, int y)
    {
        return new Vector3(0f + 0.5f * (x + y), -0.25f + 0.25f * (x - y), x + (Dungeon.HEIGHT - y));
    }

    private GameObject prefab(char ch)
    {
        if (ch == '.')
            return pr_floor;

        if (ch == ' ')
            return pr_ceiling;

        if (ch == '#')
            return pr_wall;

        return null;
    }

    public DungeonElement GetDungeonElement(int X, int Y)
    {
        if (0 <= X && X < Dungeon.WIDTH &&
        0 <= Y && Y < Dungeon.HEIGHT)
            return dungeonElements[Y * W + X];

        return null;
    }

    public void MakeDungeonElementTransparent(int X, int Y)
    {
        DungeonElement dungeonElement = GetDungeonElement(X, Y);
        dungeonElement.MakeTransparent();  
    }

    private void SetDungeonElementBrightness(int X, int Y, float brightness)
    {
        DungeonElement dungeonElement = GetDungeonElement(X, Y);
        dungeonElement.SetBrightness(brightness);
    }

    private void MakeRoomDark(RectInt rect)
    {
        for (int x = rect.min.x; x <= rect.max.x; x++)
            for (int y = rect.min.y; y <= rect.max.y; y++)
                SetDungeonElementBrightness(x, y, 0.5f);
    }

    private void MakeRoomBright(RectInt rect)
    {
        for (int x = rect.min.x; x <= rect.max.x; x++)
            for (int y = rect.min.y; y <= rect.max.y; y++)
                SetDungeonElementBrightness(x, y, 1f);
    }

    public void SetActiveRoom(int roomToActivate)
    {
        if (activeRoom == roomToActivate)
            return;

        if(activeRoom != Dungeon.NOROOM)
            MakeRoomDark(rooms[activeRoom]);
        if(roomToActivate != Dungeon.NOROOM)
            MakeRoomBright(rooms[roomToActivate]);
        activeRoom = roomToActivate;
    }
}
