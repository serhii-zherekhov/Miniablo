using System.Collections.Generic;
using UnityEngine;

public sealed class Dungeon
{
    public const int HEIGHT = 100;
    public const int WIDTH = 100;
    public const int NOROOM = -1;
    private int roomCounter = 0;
    private AStar_Pathfinding aStar = null;

    public AStar_Pathfinding GetAStar()
    {
        return aStar;
    }

    private static Dungeon _instance = null;
    private Dungeon() 
    { 
        GenerateDungeon();
        GenerateEnemies();
        aStar = new AStar_Pathfinding(this);
    }

    public static Dungeon GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Dungeon();
        }
        return _instance;
    }

    public /*private*/ List<Room> room_vec_ = new List<Room>();
    private int r_min_height_, r_min_width_;
    private int r_max_height_, r_max_width_;
    private char wall_, floor_, nothing_;
    private int x_pos_, y_pos_;
    private int counter_;
    private int min_room_num_;
    private char[,] dungeon_;
    public char[,] _dungeon
    {
        get
        {
            return dungeon_;
        }
    }
    private bool is_executed_;

    private List<Vector3Int> enemies = new List<Vector3Int>();
    public List<Vector3Int> _enemies
    {
        get
        {
            return enemies;
        }
    }

    public void GenerateDungeon()
    {
        //Make the "canvas"
        dungeon_ = new char[HEIGHT, WIDTH];

        //Set the default parameters
        setMin(4, 4);
        setMax(HEIGHT / 5, WIDTH / 5);
        setChars('#', '.', ' ');
        setMinRoomNum(60);

        //Starting point of the first room
        y_pos_ = Random.Range(0, HEIGHT + 1);
        x_pos_ = Random.Range(0, WIDTH + 1);

        //This is needed for genRoom() (recursive calls)
        counter_ = 1;
        is_executed_ = false;

        //Draw the "dungeon" on a "canvas"
        while (!genRoom()) ;
        genPassages();

        /*DEBUG*/
        for (int i = 0; i < 25; i++)
        {
            bool success = false;

            while (success == false)
            {
                int X = Random.Range(0, Dungeon.WIDTH);
                int Y = Random.Range(0, Dungeon.HEIGHT);

                if (dungeon_[Y, X] == floor_)
                {
                    dungeon_[Y, X] = wall_;
                    success = true;
                }
            }
        }
        /*DEBUG*/
    }

    public void setMin(int height, int width)
    {
        if (height < 3 || height > HEIGHT
            || width < 3 || width > WIDTH)
            Debug.LogError("Wrong setMin() parameters. It has to be more than 2 and less than or equal to D_HEIGHT_/D_WIDTH_");
        r_min_height_ = height;
        r_min_width_ = width;
    }
    public void setMax(int height, int width)
    {
        if (height < r_min_height_ || height > HEIGHT
            || width < r_min_width_ || width > WIDTH)
            Debug.LogError("Wrong setMax() parameters. It should be more than r_min_height_/r_min_width_ and less than or equal to D_HEIGHT_/D_WIDTH_");
        r_max_height_ = height;
        r_max_width_ = width;
    }
    public void setChars(char wall, char floor, char nothing)
    {
        wall_ = wall;
        floor_ = floor;
        nothing_ = nothing;

        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                dungeon_[y, x] = nothing_;
            }
        }
    }

    public void setMinRoomNum(int num)
    {
        if (num < 0)
            Debug.LogError("Wrong setMinRoomNum() parameter. It has to be more than or equal to 0");
        min_room_num_ = num;
    }

    public /*private*/ enum dir_t
    {
        s_e,
        s_w,
        n_e,
        n_w
    };

    public /*private*/ struct Room
    {
        public int number;
        public int start_x, start_y;
        public int end_x, end_y;
        public dir_t dir;

        public Room(int x, int y, int xx, int yy, dir_t d, int n)
        {
            start_x = x;
            start_y = y;
            end_x = xx;
            end_y = yy;
            dir = d;
            number = n;
        }
    };

    private bool genRoom()
    {
        //Room width and height
        int width = Random.Range(r_min_width_, r_max_width_ + 1);
        int height = Random.Range(r_min_height_, r_max_height_ + 1);

        //s_e - south east; s_w - south west; n_e - north east; n_w - north west;
        dir_t s_e = dir_t.s_e; dir_t n_e = dir_t.n_e;
        dir_t s_w = dir_t.s_w; dir_t n_w = dir_t.n_w;

        //Store possible directions in %dir_vec vector
        List<dir_t> dir_vec = new List<dir_t>();
        if (check(s_e, width, height))
        {
            dir_vec.Add(s_e);
        }
        if (check(s_w, width, height))
        {
            dir_vec.Add(s_w);
        }
        if (check(n_e, width, height))
        {
            dir_vec.Add(n_e);
        }
        if (check(n_w, width, height))
        {
            dir_vec.Add(n_w);
        }

        //Do a little trick if there is no possible directions and less than %min_room_num rooms
        //!!! It is not guaranteed that the number of rooms will be equal to %min_room_num
        if (dir_vec.Count == 0 && room_vec_.Count < min_room_num_)
        {
            if (room_vec_.Count - counter_ > 0)
            {
                x_pos_ = room_vec_[room_vec_.Count - counter_].end_x;
                y_pos_ = room_vec_[room_vec_.Count - counter_].end_y;
                counter_++;
                while (!genRoom()) ;
                while (!genRoom()) ;
            }
            else if (!is_executed_ && room_vec_.Count - counter_ == 0)
            {
                x_pos_ = room_vec_[0].start_x;
                y_pos_ = room_vec_[0].start_y;
                is_executed_ = true; //This condition should be executed only ONCE
                genRoom();
            }
        }

        //Break if no possible directions
        if (dir_vec.Count == 0) return true;

        //Make room in randomly selected direction
        dir_t rnd_dir = dir_vec[Random.Range(0, dir_vec.Count)];

        switch (rnd_dir)
        {
            case dir_t.s_e:
                {
                    for (int y = y_pos_; y < y_pos_ + height; y++)
                    {
                        for (int x = x_pos_; x < x_pos_ + width; x++)
                        {
                            if (y == y_pos_ || y == y_pos_ + (height - 1)
                                    || x == x_pos_ || x == x_pos_ + (width - 1))
                            {
                                dungeon_[y, x] = wall_;
                            }
                            else
                            {
                                dungeon_[y, x] = floor_;
                            }
                        }
                    }
                    //Keep track of all rooms
                    room_vec_.Add(new Room(x_pos_, y_pos_, x_pos_ + (width - 1), y_pos_ + (height - 1), s_e, roomCounter++));
                    //Set y&&x position to the opposite corner
                    y_pos_ += (height - 1);
                    x_pos_ += (width - 1);
                }
                break;
            case dir_t.s_w:
                {
                    for (int y = y_pos_; y < y_pos_ + height; y++)
                    {
                        for (int x = x_pos_; x > x_pos_ - width; x--)
                        {
                            if (y == y_pos_ || y == y_pos_ + (height - 1)
                                    || x == x_pos_ || x == x_pos_ - (width - 1))
                            {
                                dungeon_[y, x] = wall_;
                            }
                            else
                            {
                                dungeon_[y, x] = floor_;
                            }
                        }
                    }
                    room_vec_.Add(new Room(x_pos_, y_pos_, x_pos_ - (width - 1), y_pos_ + (height - 1), s_w, roomCounter++));
                    y_pos_ += (height - 1);
                    x_pos_ -= (width - 1);
                }
                break;
            case dir_t.n_e:
                {
                    for (int y = y_pos_; y > y_pos_ - height; y--)
                    {
                        for (int x = x_pos_; x < x_pos_ + width; x++)
                        {
                            if (y == y_pos_ || y == y_pos_ - (height - 1)
                                    || x == x_pos_ || x == x_pos_ + (width - 1))
                            {
                                dungeon_[y, x] = wall_;
                            }
                            else
                            {
                                dungeon_[y, x] = floor_;
                            }
                        }
                    }
                    room_vec_.Add(new Room(x_pos_, y_pos_, x_pos_ + (width - 1), y_pos_ - (height - 1), n_e, roomCounter++));
                    y_pos_ -= (height - 1);
                    x_pos_ += (width - 1);
                }
                break;
            case dir_t.n_w:
                {
                    for (int y = y_pos_; y > y_pos_ - height; y--)
                    {
                        for (int x = x_pos_; x > x_pos_ - width; x--)
                        {
                            if (y == y_pos_ || y == y_pos_ - (height - 1)
                                    || x == x_pos_ || x == x_pos_ - (width - 1))
                            {
                                dungeon_[y, x] = wall_;
                            }
                            else
                            {
                                dungeon_[y, x] = floor_;
                            }
                        }
                    }
                    room_vec_.Add(new Room(x_pos_, y_pos_, x_pos_ - (width - 1), y_pos_ - (height - 1), n_w, roomCounter++));
                    y_pos_ -= (height - 1);
                    x_pos_ -= (width - 1);
                }
                break;
        }

        //Signal that there is still possible directions left
        return false;
    }
    private bool check(dir_t dir, int width, int height)
    {
        //Check if it's possible to make room in the direction(%dir) that was passed
        switch (dir)
        {
            case dir_t.s_e:
                if (y_pos_ + height <= HEIGHT && x_pos_ + width <= WIDTH)
                {
                    for (int y = y_pos_; y < y_pos_ + height; y++)
                    {
                        for (int x = x_pos_; x < x_pos_ + width; x++)
                        {
                            if (y == y_pos_ || y == y_pos_ + (height - 1)
                                    || x == x_pos_ || x == x_pos_ + (width - 1)) continue; //Ignore wall_ collision
                            if (dungeon_[y, x] != nothing_) return false;
                        }
                    }
                }
                else return false;
                return true;

            case dir_t.s_w:
                if (y_pos_ + height <= HEIGHT && x_pos_ - width >= 0)
                {
                    for (int y = y_pos_; y < y_pos_ + height; y++)
                    {
                        for (int x = x_pos_; x > x_pos_ - width; x--)
                        {
                            if (y == y_pos_ || y == y_pos_ + (height - 1)
                                    || x == x_pos_ || x == x_pos_ - (width - 1)) continue;
                            if (dungeon_[y, x] != nothing_) return false;
                        }
                    }
                }
                else return false;
                return true;

            case dir_t.n_e:
                if (y_pos_ - height >= 0 && x_pos_ + width <= WIDTH)
                {
                    for (int y = y_pos_; y > y_pos_ - height; y--)
                    {
                        for (int x = x_pos_; x < x_pos_ + width; x++)
                        {
                            if (y == y_pos_ || y == y_pos_ - (height - 1)
                                    || x == x_pos_ || x == x_pos_ + (width - 1)) continue;
                            if (dungeon_[y, x] != nothing_) return false;
                        }
                    }
                }
                else return false;
                return true;

            case dir_t.n_w:
                if (y_pos_ - height >= 0 && x_pos_ - width >= 0)
                {
                    for (int y = y_pos_; y > y_pos_ - height; y--)
                    {
                        for (int x = x_pos_; x > x_pos_ - width; x--)
                        {
                            if (y == y_pos_ || y == y_pos_ - (height - 1)
                                    || x == x_pos_ || x == x_pos_ - (width - 1)) continue;
                            if (dungeon_[y, x] != nothing_) return false;
                        }
                    }
                }
                else return false;
                return true;
        }

        //Something went wrong if program reached this
        Debug.LogError("Something wrong in check() function");
        return false;
    }

    private void genPassages()
    {
        //Make passage between rooms
        for (int i = 1; i < room_vec_.Count; ++i)
        {
            for (int n = 1; n <= i; ++n)
            {
                if (room_vec_[i - n].end_y == room_vec_[i].start_y
                        && room_vec_[i - n].end_x == room_vec_[i].start_x)
                {
                    switch (room_vec_[i - n].dir)
                    {
                        case dir_t.s_e:
                            if (room_vec_[i].dir == dir_t.s_e)
                            {  //Because nested switches look ugly
                                genVestibule(dir_t.s_e, i);
                            }
                            else if (room_vec_[i].dir == dir_t.s_w)
                            {
                                dungeon_[room_vec_[i].start_y , room_vec_[i].start_x - 1] = floor_;
                            }
                            else if (room_vec_[i].dir == dir_t.n_e)
                            {
                                dungeon_[room_vec_[i].start_y - 1 , room_vec_[i].start_x] = floor_;
                            }
                            break;

                        case dir_t.s_w:
                            if (room_vec_[i].dir == dir_t.s_e)
                            {
                                dungeon_[room_vec_[i].start_y , room_vec_[i].start_x + 1] = floor_;
                            }
                            else if (room_vec_[i].dir == dir_t.s_w)
                            {
                                genVestibule(dir_t.s_w, i);
                            }
                            else if (room_vec_[i].dir == dir_t.n_w)
                            {
                                dungeon_[room_vec_[i].start_y - 1 , room_vec_[i].start_x] = floor_;
                            }
                            break;

                        case dir_t.n_e:
                            if (room_vec_[i].dir == dir_t.s_e)
                            {
                                dungeon_[room_vec_[i].start_y + 1 , room_vec_[i].start_x] = floor_;
                            }
                            else if (room_vec_[i].dir == dir_t.n_e)
                            {
                                genVestibule(dir_t.n_e, i);
                            }
                            else if (room_vec_[i].dir == dir_t.n_w)
                            {
                                dungeon_[room_vec_[i].start_y , room_vec_[i].start_x - 1] = floor_;
                            }
                            break;

                        case dir_t.n_w:
                            if (room_vec_[i].dir == dir_t.s_w)
                            {
                                dungeon_[room_vec_[i].start_y + 1 , room_vec_[i].start_x] = floor_;
                            }
                            else if (room_vec_[i].dir == dir_t.n_e)
                            {
                                dungeon_[room_vec_[i].start_y , room_vec_[i].start_x + 1] = floor_;
                            }
                            else if (room_vec_[i].dir == dir_t.n_w)
                            {
                                genVestibule(dir_t.n_w, i);
                            }
                            break;
                    }
                }
            }
        }
    }
    private void genVestibule(dir_t dir, int i)
    {
        //This belongs to genPassages()
        //Have put this in separate method for the sake of clarity
        switch (dir)
        {
            case dir_t.s_w:
            case dir_t.n_e:
                //Draw the wall_s if this vestibule is not collapsing with other rooms
                if (dungeon_[room_vec_[i].start_y + 1 , room_vec_[i].start_x + 1] == nothing_)
                {
                    dungeon_[room_vec_[i].start_y + 2 , room_vec_[i].start_x + 1] = wall_;
                    dungeon_[room_vec_[i].start_y + 2 , room_vec_[i].start_x + 2] = wall_;
                    dungeon_[room_vec_[i].start_y + 1 , room_vec_[i].start_x + 2] = wall_;
                }
                if (dungeon_[room_vec_[i].start_y - 1 , room_vec_[i].start_x - 1] == nothing_)
                {
                    dungeon_[room_vec_[i].start_y - 2 , room_vec_[i].start_x - 2] = wall_;
                    dungeon_[room_vec_[i].start_y - 2 , room_vec_[i].start_x - 1] = wall_;
                    dungeon_[room_vec_[i].start_y - 1 , room_vec_[i].start_x - 2] = wall_;
                }

                dungeon_[room_vec_[i].start_y - 1 , room_vec_[i].start_x] = floor_;
                dungeon_[room_vec_[i].start_y - 1 , room_vec_[i].start_x + 1] = floor_;
                dungeon_[room_vec_[i].start_y - 1 , room_vec_[i].start_x - 1] = floor_;
                dungeon_[room_vec_[i].start_y + 1 , room_vec_[i].start_x - 1] = floor_;
                dungeon_[room_vec_[i].start_y + 1 , room_vec_[i].start_x] = floor_;
                dungeon_[room_vec_[i].start_y + 1 , room_vec_[i].start_x + 1] = floor_;
                dungeon_[room_vec_[i].start_y , room_vec_[i].start_x - 1] = floor_;
                dungeon_[room_vec_[i].start_y , room_vec_[i].start_x + 1] = floor_;
                dungeon_[room_vec_[i].start_y , room_vec_[i].start_x] = floor_;
                break;
            case dir_t.s_e:
            case dir_t.n_w:
                if (dungeon_[room_vec_[i].start_y + 1 , room_vec_[i].start_x - 1] == nothing_)
                {
                    dungeon_[room_vec_[i].start_y + 2 , room_vec_[i].start_x - 1] = wall_;
                    dungeon_[room_vec_[i].start_y + 2 , room_vec_[i].start_x - 2] = wall_;
                    dungeon_[room_vec_[i].start_y + 1 , room_vec_[i].start_x - 2] = wall_;
                    dungeon_[room_vec_[i].start_y + 1 , room_vec_[i].start_x - 1] = floor_;
                    dungeon_[room_vec_[i].start_y + 1 , room_vec_[i].start_x] = floor_;
                    dungeon_[room_vec_[i].start_y + 1 , room_vec_[i].start_x + 1] = floor_;
                    dungeon_[room_vec_[i].start_y , room_vec_[i].start_x - 1] = floor_;
                    dungeon_[room_vec_[i].start_y , room_vec_[i].start_x + 1] = floor_;
                    dungeon_[room_vec_[i].start_y , room_vec_[i].start_x] = floor_;
                }
                if (dungeon_[room_vec_[i].start_y - 1 , room_vec_[i].start_x + 1] == nothing_)
                {
                    dungeon_[room_vec_[i].start_y - 2 , room_vec_[i].start_x + 2] = wall_;
                    dungeon_[room_vec_[i].start_y - 2 , room_vec_[i].start_x + 1] = wall_;
                    dungeon_[room_vec_[i].start_y - 1 , room_vec_[i].start_x + 2] = wall_;
                    dungeon_[room_vec_[i].start_y - 1 , room_vec_[i].start_x] = floor_;
                    dungeon_[room_vec_[i].start_y - 1 , room_vec_[i].start_x + 1] = floor_;
                    dungeon_[room_vec_[i].start_y - 1 , room_vec_[i].start_x - 1] = floor_;
                    dungeon_[room_vec_[i].start_y , room_vec_[i].start_x - 1] = floor_;
                    dungeon_[room_vec_[i].start_y , room_vec_[i].start_x + 1] = floor_;
                    dungeon_[room_vec_[i].start_y , room_vec_[i].start_x] = floor_;
                }
                break;
        }
    }

    private void GenerateEnemies()
    {
        foreach (Room room in room_vec_)
        {
            int room_start_x = Mathf.Min(room.start_x, room.end_x);
            int room_start_y = Mathf.Min(room.start_y, room.end_y);
            int room_end_x = Mathf.Max(room.start_x, room.end_x);
            int room_end_y = Mathf.Max(room.start_y, room.end_y);

            float S = (room_end_x - room_start_x) * (room_end_y - room_start_y);
            int E = (int)(S / 20);

            if (E >= 10)
            {
                E += Random.Range(0, 10 + 1) - 5;
            }

            for (int i = 0; i < E; i++)
            {
                int x = 0;
                int y = 0;

                while (dungeon_[y, x] != '.')
                {
                    x = Random.Range(room_start_x + 2, room_end_x - 1);
                    y = Random.Range(room_start_y + 2, room_end_y - 1);

                    if (dungeon_[y, x] == '.')
                    {
                        enemies.Add(new Vector3Int(x, y, room.number));
                    }
                }
            }
        }
    }

    public char GetChar(Vector2Int cell)
    {
        if (0 <= cell.x && cell.x < Dungeon.WIDTH &&
                0 <= cell.y && cell.y < Dungeon.HEIGHT)
            return dungeon_[cell.y, cell.x];

        return '\0';
    }

    public int GetRoomFromCell(Vector2Int cell)
    {
        foreach (Room room in room_vec_)
        {
            int room_start_x = Mathf.Min(room.start_x, room.end_x);
            int room_start_y = Mathf.Min(room.start_y, room.end_y);
            int room_end_x = Mathf.Max(room.start_x, room.end_x);
            int room_end_y = Mathf.Max(room.start_y, room.end_y);

            if (room_start_x <= cell.x && cell.x <= room_end_x &&
                room_start_y <= cell.y && cell.y <= room_end_y)
                return room.number;
        }

        return Dungeon.NOROOM;
    }
    public List<RectInt> GetRooms()
    {
        List<RectInt> rooms = new List<RectInt>();

        foreach (Room room in room_vec_)
        {
            int room_start_x = Mathf.Min(room.start_x, room.end_x);
            int room_start_y = Mathf.Min(room.start_y, room.end_y);
            int room_end_x = Mathf.Max(room.start_x, room.end_x);
            int room_end_y = Mathf.Max(room.start_y, room.end_y);

            Vector2Int position = new Vector2Int(room_start_x, room_start_y);
            Vector2Int size = new Vector2Int(room_end_x - room_start_x, room_end_y - room_start_y);

            RectInt rect = new RectInt(position, size);

            rooms.Add(rect);
        }

        return rooms;
    }
}














