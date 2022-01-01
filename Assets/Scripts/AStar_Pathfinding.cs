using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar_Pathfinding
{
	private Dungeon dungeon;

	public AStar_Pathfinding(Dungeon D)
    {
		dungeon = D;
		Load();
	}

	/*change to private when OnDrawGizmos in Player deleted*/public class sNode
	{
		public int x;                          // Nodes position in 2D space
		public int y;
		public bool bObstacle;         // Is the node an obstruction?
		public bool bVisited;          // Have we searched this node before?
		public float fGlobalGoal;              // Distance to goal so far
		public float fLocalGoal;               // Distance to goal if we took the alternative route
		public List<sNode> vecNeighbours;   // Connections to neighbours
		public sNode parent;                  // Node connecting to this node that offers shortest parent

		public sNode(int X, int Y, bool bObs, bool bVis, sNode Par)
		{
			x = X;
			y = Y;
			bObstacle = bObs;
			bVisited = bVis;
			fGlobalGoal = 0;
			fLocalGoal = 0;
			vecNeighbours = new List<sNode>();
			parent = Par;
		}
	};

	private int nMapWidth;
	private int nMapHeight;
	private List<sNode> nodes = new List<sNode>();

	public List<sNode> _nodes
    {
        get
        {
			return nodes;
        }
    }

	/*change to private when OnDrawGizmos in Player deleted*/public sNode nodeStart = null;
	/*change to private when OnDrawGizmos in Player deleted*/public sNode nodeEnd = null;

	public void setNodeStart(Vector2Int vec)
    {
		nodeStart = nodes[vec.y * nMapWidth + vec.x];
	}

	public void setNodeEnd(Vector2Int vec)
	{
		nodeEnd = nodes[vec.y * nMapWidth + vec.x];
	}

	public void setNodeObstacle(Vector2Int vec, bool isObstacle)
    {
		if (0 <= vec.x && vec.x < Dungeon.WIDTH &&
		0 <= vec.y && vec.y <= Dungeon.HEIGHT)
			nodes[vec.y * Dungeon.WIDTH + vec.x].bObstacle = isObstacle;
	}

	public bool getNodeObstacle(Vector2Int vec)
	{
		if (0 <= vec.x && vec.x < Dungeon.WIDTH &&
		0 <= vec.y && vec.y < Dungeon.HEIGHT)
			return nodes[vec.y * Dungeon.WIDTH + vec.x].bObstacle;

		return true;
	}
	public void inverseNodeObstacle(Vector2Int vec)
	{
		if (0 <= vec.x && vec.x < Dungeon.WIDTH &&
		0 <= vec.y && vec.y <= Dungeon.HEIGHT)
			nodes[vec.y * Dungeon.WIDTH + vec.x].bObstacle = !nodes[vec.y * Dungeon.WIDTH + vec.x].bObstacle;
	}

	private void Load()
	{
		nMapHeight = Dungeon.HEIGHT;
		nMapWidth = Dungeon.WIDTH;

		// Create a 2D array of nodes - this is for convenience of rendering and construction
		// and is not required for the algorithm to work - the nodes could be placed anywhere
		// in any space, in multiple dimensions...
		for (int y = 0; y < nMapHeight; y++)
			for (int x = 0; x < nMapWidth; x++)	
			{
				char ch = dungeon._dungeon[y, x];
				bool isObstacle = (ch == '#' || ch == ' ');
				nodes.Add(new sNode(x, y, isObstacle, false, null));
			}

		// Create connections - in this case nodes are on a regular grid
		for (int y = 0; y < nMapHeight; y++)
			for (int x = 0; x < nMapWidth; x++)
			{
				if (y > 0)
					nodes[y * nMapWidth + x].vecNeighbours.Add(nodes[(y - 1) * nMapWidth + (x + 0)]);
				if (y < nMapHeight - 1)
					nodes[y * nMapWidth + x].vecNeighbours.Add(nodes[(y + 1) * nMapWidth + (x + 0)]);
				if (x > 0)
					nodes[y * nMapWidth + x].vecNeighbours.Add(nodes[(y + 0) * nMapWidth + (x - 1)]);
				if (x < nMapWidth - 1)
					nodes[y * nMapWidth + x].vecNeighbours.Add(nodes[(y + 0) * nMapWidth + (x + 1)]);

				// We can also connect diagonally
				if (y>0 && x>0)
					nodes[y*nMapWidth + x].vecNeighbours.Add(nodes[(y - 1) * nMapWidth + (x - 1)]);
				if (y<nMapHeight-1 && x>0)
					nodes[y*nMapWidth + x].vecNeighbours.Add(nodes[(y + 1) * nMapWidth + (x - 1)]);
				if (y>0 && x<nMapWidth-1)
					nodes[y*nMapWidth + x].vecNeighbours.Add(nodes[(y - 1) * nMapWidth + (x + 1)]);
				if (y<nMapHeight - 1 && x<nMapWidth-1)
					nodes[y*nMapWidth + x].vecNeighbours.Add(nodes[(y + 1) * nMapWidth + (x + 1)]);
				
			}
	}

	public List<Vector2Int> Solve_AStar()
	{
		const float INFINITY = 9999f;

		// Reset Navigation Graph - default all node states
		for (int x = 0; x < nMapWidth; x++)
			for (int y = 0; y < nMapHeight; y++)
			{
				nodes[y * nMapWidth + x].bVisited = false;
				nodes[y * nMapWidth + x].fGlobalGoal = INFINITY;
				nodes[y * nMapWidth + x].fLocalGoal = INFINITY;
				nodes[y * nMapWidth + x].parent = null;  // No parents
			}

		float distance(sNode a, sNode b) // For convenience
		{
			return Mathf.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
		};

		float heuristic(sNode a, sNode b) // So we can experiment with heuristic
		{
			return distance(a, b);
		};

		// Setup starting conditions
		sNode nodeCurrent = nodeStart;
		nodeStart.fLocalGoal = 0.0f;
		nodeStart.fGlobalGoal = heuristic(nodeStart, nodeEnd);

		// Add start node to not tested list - this will ensure it gets tested.
		// As the algorithm progresses, newly discovered nodes get added to this
		// list, and will themselves be tested later
		List<sNode> listNotTestedNodes = new List<sNode>();
		listNotTestedNodes.Add(nodeStart);

		// if the not tested list contains nodes, there may be better paths
		// which have not yet been explored. However, we will also stop 
		// searching when we reach the target - there may well be better
		// paths but this one will do - it wont be the longest.
		while (listNotTestedNodes.Count != 0 && nodeCurrent != nodeEnd)// Find absolutely shortest path // && nodeCurrent != nodeEnd)
		{
			// Sort Untested nodes by global goal, so lowest is first
			listNotTestedNodes.Sort((sNode lhs, sNode rhs) => lhs.fGlobalGoal.CompareTo(rhs.fGlobalGoal));

			// Front of listNotTestedNodes is potentially the lowest distance node. Our
			// list may also contain nodes that have been visited, so ditch these...
			while (listNotTestedNodes.Count != 0 && listNotTestedNodes[0].bVisited)
				listNotTestedNodes.RemoveAt(0);

			// ...or abort because there are no valid nodes left to test
			if (listNotTestedNodes.Count == 0)
				break;

			nodeCurrent = listNotTestedNodes[0];
			nodeCurrent.bVisited = true; // We only explore a node once


			// Check each of this node's neighbours...
			foreach(sNode nodeNeighbour in nodeCurrent.vecNeighbours)
			{
				// ... and only if the neighbour is not visited and is 
				// not an obstacle, add it to NotTested List
				if (nodeNeighbour.bVisited == false && nodeNeighbour.bObstacle == false)
					listNotTestedNodes.Add(nodeNeighbour);

				// Calculate the neighbours potential lowest parent distance
				float fPossiblyLowerGoal = nodeCurrent.fLocalGoal + distance(nodeCurrent, nodeNeighbour);

				// If choosing to path through this node is a lower distance than what 
				// the neighbour currently has set, update the neighbour to use this node
				// as the path source, and set its distance scores as necessary
				if (fPossiblyLowerGoal < nodeNeighbour.fLocalGoal)
				{
					nodeNeighbour.parent = nodeCurrent;
					nodeNeighbour.fLocalGoal = fPossiblyLowerGoal;

					// The best path length to the neighbour being tested has changed, so
					// update the neighbour's score. The heuristic is used to globally bias
					// the path algorithm, so it knows if its getting better or worse. At some
					// point the algo will realise this path is worse and abandon it, and then go
					// and search along the next best path.
					nodeNeighbour.fGlobalGoal = nodeNeighbour.fLocalGoal + heuristic(nodeNeighbour, nodeEnd);
				}
			}
		}

		return Result();
	}

	private List<Vector2Int> Result()
    {
		if (nodeEnd == null)
			return null;

		List<Vector2Int> result = new List<Vector2Int>();

		sNode p = nodeEnd;
		result.Add(new Vector2Int(p.x, p.y));

		while (p.parent != null)
		{
			result.Add(new Vector2Int(p.parent.x, p.parent.y));
			p = p.parent;
		}
		
		return result;
	}
}
