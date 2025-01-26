using System.Collections.Generic;
using UnityEngine;

public class AgentNavigator : MonoBehaviour
{
    public static AgentNavigator instance = null;

    private void Awake()
    {
        instance = this;
    }
    public GameObject targetLocation;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.C))
        {
            return;
        }

        foreach (Agent agent in GameManager.instance.agents)
        {
            if (agent.isNavigating)
            {
                continue;
            }

            List<Vector2Int> path = new List<Vector2Int>();

            Vector2Int targetPosition = new Vector2Int((int)targetLocation.transform.position.x, (int)targetLocation.transform.position.y);

            path = AStarSearch(agent.tilePosition, targetPosition);

            //while (path.Count == 0)
            //{
            //    Vector2Int targetPosition = Vector2Int.zero;
            //    while (targetPosition == Vector2Int.zero)
            //    {
            //        Vector3Int position = new Vector3Int(
            //            Random.Range(GameManager.instance.walkableTileMap.cellBounds.xMin, GameManager.instance.walkableTileMap.cellBounds.xMax),
            //            Random.Range(GameManager.instance.walkableTileMap.cellBounds.yMin, GameManager.instance.walkableTileMap.cellBounds.yMax),
            //            0
            //            );

            //        if (GameManager.instance.walkableTileMap.GetTile(position))
            //        {
            //            targetPosition = new Vector2Int(position.x, position.y);
            //        }
            //    }

            //    path = AStarSearch(agent.tilePosition, targetPosition);
            //}

            StartCoroutine(agent.MoveAlongPath(path));

            return;
        }
    }

    public void MoveToPosition(Agent agent, Vector2Int position)
    {
        if (!GameManager.instance.walkableTileMap.GetTile(new Vector3Int(position.x, position.y, 0)))
        {
            return;
        }

        var path = AStarSearch(agent.tilePosition, position);
        StartCoroutine(agent.MoveAlongPath(path));
    }

    private List<Vector2Int> GetWalkableNeighbours(Vector2Int position)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        Vector2Int up = position + Vector2Int.up;
        Vector2Int down = position + Vector2Int.down;
        Vector2Int left = position + Vector2Int.left;
        Vector2Int right = position + Vector2Int.right;

        if (TileWalkable(up))
        {
            result.Add(up);
        }
        if (TileWalkable(down))
        {
            result.Add(down);
        }
        if (TileWalkable(left))
        {
            result.Add(left);
        }
        if (TileWalkable(right))
        {
            result.Add(right);
        }

        return result;
    }

    private bool TileWalkable(Vector2Int position)
    {
        if (GameManager.instance.collisionTileMap.GetTile((Vector3Int)position) != null)
        {
            return false;
        }

        if (GameManager.instance.walkableTileMap.GetTile((Vector3Int)position) == null)
        {
            return false;
        }

        return true;
    }

    public class AStarNode
    {
        public Vector2Int position;
        public float g;
        public float h;
        public float f;
        public AStarNode parent;

        public override bool Equals(object obj)
        {
            AStarNode otherNode = (AStarNode)obj;

            return position == otherNode.position;
        }
    }

    // Based on https://medium.com/@nicholas.w.swift/easy-a-star-pathfinding-7e6689c7f7b2
    List<Vector2Int> AStarSearch(Vector2Int from, Vector2Int to)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        if (from == to)
        {
            return result;
        }

        List<AStarNode> openList = new List<AStarNode>();
        List<AStarNode> closedList = new List<AStarNode>();

        AStarNode startNode = new AStarNode()
        {
            position = from,
            g = 0.0f,
            h = 0.0f,
            f = 0.0f
        };

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            openList.Sort(delegate (AStarNode x, AStarNode y)
            {
                return x.f.CompareTo(y.f);
            });

            AStarNode currentNode = openList[0];
            openList.RemoveAt(0);

            closedList.Add(currentNode);

            if (currentNode.position == to)
            {
                AStarNode tracebackNode = currentNode;
                while (tracebackNode.parent != null)
                {
                    result.Add(tracebackNode.position);
                    tracebackNode = tracebackNode.parent;
                }

                result.Reverse();

                return result;
            }

            foreach (Vector2Int neighbourPosition in GetWalkableNeighbours(currentNode.position))
            {
                AStarNode neighbourNode = new AStarNode()
                {
                    position = neighbourPosition,
                    g = 0.0f,
                    h = 0.0f,
                    f = 0.0f,
                    parent = currentNode
                };

                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }

                neighbourNode.g = currentNode.g + 1;
                neighbourNode.h = Vector2Int.Distance(neighbourPosition, to);
                neighbourNode.f = neighbourNode.g + neighbourNode.h;

                int openListIndex = openList.IndexOf(neighbourNode);
                if (openListIndex != -1 /* && neighbourNode.g > openList[openListIndex].g*/)
                {
                    continue;
                }

                openList.Add(neighbourNode);
            }
        }

        return result;
    }
}
