using System.Collections.Generic;
using System.Linq;
using SaritasaShortestPath.ShortestPathSolver;

namespace SaritasaShortestPath.ShortestPathSolver {
    // For every SPSolver. Could be moved to a separate file.
    public interface INode
    {
        IEnumerable<ILink> GetLinks();
    }

    public interface ILink
    {
        INode Neighbour { get; }
        int Weight { get; }
    }

}

namespace SaritasaShortestPath.Dijkstra
{

    interface IPriorityQueue
    {
        int Count { get; }
        bool IsEmpty();
        INode PullLowest();
        void DecreasePriority(INode node, int lowerPriority);
    }

    struct PriorityQueue : IPriorityQueue
    {
        public int Count {
            get {
                return Table.Count;
            }
        }
        readonly List<INode> Table;
        readonly Dictionary<INode, int> Distances;
        public PriorityQueue(IEnumerable<INode> notVisitedNodes, Dictionary<INode, int> distances)
        {
            Table = notVisitedNodes.ToList();
            this.Distances = distances;
        }
        public bool IsEmpty()
        {
            return Table.Count == 0;
        }
        public INode PullLowest()
        {
            var last = Table.Last();
            Table.RemoveAt(Table.Count - 1);
            return last;
        }
        public void DecreasePriority(INode node, int lowerPriority)
        {
            Distances[node] = lowerPriority;
            var nodeIndex = Table.IndexOf(node); // TODO: inefficient.
            if (nodeIndex == Table.Count - 1)
                return; // Already of the lowest priority.
                        // Find new position with Binary Search.
            if (nodeIndex < 0)
                throw new System.Exception("FOOBAR");
            var self = this;
            int newIndex = Table.BinarySearch(
                nodeIndex + 1,
                Table.Count - 1 - nodeIndex,
                null,
                Comparer<INode>.Create((left, _) =>
            {
                // Sic: the comparator is reversed, we sort the table from a high priority to a low.
                return lowerPriority.CompareTo(self.Distances[left]);
            })
            );
            /*
                On `newIndex` value:
                    When BinarySearch finds no object with priority equal to lowerPriority
                    it returns a binary complement of the next index with lower priority or `table.Count`.
                    For `newIndex < 0`  -> ~(newIndex) gives table.Count or next index.
                        `--newIndex` in conjunction with nodeElement removal again gives next index or Count.
                        That is what we want.
                    For `newIndex > 0`  -> fine anyway. Decrement, shift, insert.
                Feel like I have to write my on BinarySearch to keep my code less obfuscated.
            */
            if (newIndex < 0)
                newIndex = ~(newIndex);
            --newIndex;
            Table.RemoveAt(nodeIndex);
            Table.Insert(newIndex, node);
        }
    }

    public static class DijkstraAlgorithm
    {
        public static IEnumerable<INode> Run(INode start, INode finish, IEnumerable<INode> nodes)
        {
            if (!nodes.Contains(start) || !nodes.Contains(finish))
                throw new System.ArgumentException("Both `start` and `finish` must be in `nodes`.");

            var distances = nodes.ToDictionary(node => node, _ => int.MaxValue);

            var notVisitedQueue = new PriorityQueue(nodes, distances);

            notVisitedQueue.DecreasePriority(start, 0);

            var previous = new Dictionary<INode, INode>(notVisitedQueue.Count);

            bool ifPathFound = false;
            while (!notVisitedQueue.IsEmpty())
            {
                var nearest = notVisitedQueue.PullLowest();
                if (distances[nearest] == int.MaxValue)
                    break;
                if (nearest.Equals(finish))
                {
                    ifPathFound = true;
                    break;
                }
                var links = nearest.GetLinks();
                foreach (var link in links)
                {
                    var alt = distances[nearest] + link.Weight;
                    if (alt < distances[link.Neighbour])
                    {
                        notVisitedQueue.DecreasePriority(link.Neighbour, alt);
                        previous[link.Neighbour] = nearest;
                    }
                }
            }

            List<INode> result = new List<INode>();
            if (ifPathFound)
            {
                INode current = finish;
                while (current != start)
                {
                    result.Add(current);
                    current = previous[current];
                }
                result.Add(current);
                result.Reverse();
            }
            
            return result.AsEnumerable();
        }
    }
}
