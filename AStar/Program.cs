using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace AStar
{
    class Program
    {
        static void Main(string[] args)
        {
            Problem problem = new Problem();
            problem.Solve();
        }
    }

    public class Action
    {
        public string ResultState
        {
            get;
            private set;
        }

        public int Cost
        {
            get;
            private set;
        }

        public Action(string resultState, int cost, int zeroPos, int otherPos)
        {
            this.ResultState = resultState;
            this.Cost = cost;
            this.Swaps = Tuple.Create(zeroPos, otherPos);
        }

        public Tuple<int, int> Swaps
        {
            get;
            private set;
        }
    }

    public class Board
    {

        private string stateAsString;
        private int size;

        public string StateAsString
        {
            get { return this.stateAsString; }
        }

        public Board(int[][] initialStateAsArray, int size)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    sb.Append(initialStateAsArray[i][j]);
                }
            }
            this.stateAsString = sb.ToString();
            this.size = size;
        }

        public Board(string state, int size)
        {
            this.stateAsString = state;
            this.size = size;
        }

        private int GetZeroPosition()
        {
            return stateAsString.IndexOf('0');
        }

        private int GetManhattenDistanceChangeFromSwap(Point char1GoalPosition, char character2, Point oldChar1Position, int newchar1Index)
        {
            Point char2GoalPosition = this.GetPositionInGoal(character2);
            Point newChar1Position = this.GetPositionFromIndex(newchar1Index);
            int oldchar1Distance = this.GetManhattenDistanceFromGoal(char1GoalPosition, oldChar1Position);
            int newchar1Distance = this.GetManhattenDistanceFromGoal(char1GoalPosition, newChar1Position);
            int oldchar2Distance = this.GetManhattenDistanceFromGoal(char2GoalPosition, newChar1Position);
            int newchar2Distance = this.GetManhattenDistanceFromGoal(char2GoalPosition, oldChar1Position);
            return (newchar1Distance - oldchar1Distance) + (newchar2Distance - oldchar2Distance);
        }

        private int GetManhattenDistanceFromGoal(Point goalPosition, Point currentPosition)
        {
            int manhattenDistance = (goalPosition.X - currentPosition.X) + (goalPosition.Y - currentPosition.Y);
            return Math.Abs(manhattenDistance);
        }

        private Point GetPositionInGoal(char c)
        {
            int indexInGoal = Int32.Parse(c.ToString());
            return this.GetPositionFromIndex(indexInGoal);
        }

        private Point GetPositionFromIndex(int i)
        {
            int row = i / this.size;
            int column = i < this.size ? i : i % this.size;
            return new Point(column, row);
        }

        public List<Action> GetActionResults()
        {
            int zeroIndex = this.GetZeroPosition();
            Point currentZeroPosition = this.GetPositionFromIndex(zeroIndex);
            Point zeroGoalPosition = this.GetPositionInGoal('0');

            int positionAbove = zeroIndex - this.size;
            int positionBelow = zeroIndex + this.size;
            int positionLeft = zeroIndex - 1;
            int positionRight = zeroIndex + 1;

            Func<int, bool> hasPositionAbove = p => p >= 0;
            Func<int, bool> hasPositionBelow = p => p < (this.size * this.size);
            Func<int, bool> hasPositionLeft = p => p >= 0 && (p / this.size == currentZeroPosition.Y);
            Func<int, bool> hasPositionRight = p => p / this.size == currentZeroPosition.Y;

            Func<int, Action> getActionFromPosition = p =>
            {
                string result = SwapCharacters(
                    this.stateAsString, zeroIndex, p);
                int cost =
                    this.GetManhattenDistanceChangeFromSwap(
                        zeroGoalPosition,
                        stateAsString[p],
                        currentZeroPosition,
                        p);
                return new Action(result, cost, zeroIndex, p);
            };

            var list = new List<Tuple<int, Func<int, bool>>>
                           {
                               Tuple.Create(positionAbove, hasPositionAbove),
                               Tuple.Create(positionBelow, hasPositionBelow),
                               Tuple.Create(positionLeft, hasPositionLeft),
                               Tuple.Create(positionRight, hasPositionRight)
                           };

            return list.Where(t => t.Item2(t.Item1)).
                Select(t => getActionFromPosition(t.Item1)).
                ToList();
        }

        public static string SwapCharacters(string value, int position1, int position2)
        {
            char[] array = value.ToCharArray();
            char temp = array[position1];
            array[position1] = array[position2];
            array[position2] = temp;
            return new string(array);
        }

        public int GetNumberMisplacedBlocksDistanceSum()
        {
            int distanceSum = 0;
            for (int k = 0; k < this.StateAsString.Length; k++)
            {
                Point goalPosition = this.GetPositionInGoal(this.StateAsString[k]);
                Point currentPosition = this.GetPositionFromIndex(k);
                distanceSum += this.GetManhattenDistanceFromGoal(goalPosition, currentPosition);
            }
            return distanceSum;
        }

        public bool IsGoal()
        {
            return this.StateAsString.Equals("123456780");
        }

        public void Print()
        {
            for (int i = 0; i < this.size; i++)
            {
                Console.WriteLine(this.stateAsString.Substring((i * this.size), this.size));
            }
            Console.WriteLine();
        }
    }

    public class Problem
    {
        //int[][] initialstate = new int[3][];
        private string initialstate;
        public IEnumerable<Tuple<int, int>> Swaps
        {
            get;
            private set;
        }


        public Problem()
        {
            //this.initialstate[0] = new int[3] { 3, 8, 6 };
            //this.initialstate[1] = new int[3] { 5, 7, 1 };
            //this.initialstate[2] = new int[3] { 2, 4, 0 };

        }

        public Problem(string initialstate)
        {
            this.initialstate = initialstate;
        }

        public void Solve()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            const int size = 3;
            var nodestoExplore = new PriorityQueueDemo.PriorityQueue<int, Node>();
            var exploredStates = new Dictionary<string, Board>();
            var initialBoard = new Board(initialstate, size);
            var initialCost = initialBoard.GetNumberMisplacedBlocksDistanceSum();
            var intialPath = new Node(
                                                    initialBoard,
                                                    null,
                                                    initialCost,
                                                    null
                                                    );

            nodestoExplore.Enqueue(initialCost, intialPath, intialPath.EndState.StateAsString);

            var currentPath = new Node(null);

            while (true)
            {
                currentPath = nodestoExplore.DequeueValue();
                nodestoExplore.DeleteFromStateDictionary(currentPath.EndState.StateAsString);
                var currentState = currentPath.EndState;

                exploredStates.Add(currentState.StateAsString, currentState);
                if (currentState.IsGoal())
                {
                    break;
                }
                foreach (Action action in currentState.GetActionResults())
                {
                    if (
                            !exploredStates.ContainsKey(action.ResultState)
                            &&
                            !nodestoExplore.ContainsState(action.ResultState)

                        )
                    {
                        int cost = currentPath.Cost + action.Cost + 1;
                        Node pathToAdd = new Node(
                                                    new Board(action.ResultState, size),
                                                    currentPath,
                                                    cost,
                                                    action
                                                    );

                        nodestoExplore.Enqueue(cost, pathToAdd, pathToAdd.EndState.StateAsString);

                    }

                }
            }
            stopWatch.Stop();
            //currentPath.Print();
            this.Swaps = currentPath.GetSwapsList();
            //var hmm = "386571240";
            /*foreach (var t in test.Reverse())
            {

                hmm = Board.SwapCharacters(hmm, t.Item1, t.Item2);
                Console.WriteLine(hmm);
            }


            Console.ReadKey();*/
        }
    }

    class Node
    {
        private Node parent;
        private int cost = 0;

        public int Cost
        {
            get { return this.cost; }
        }
        public Board EndState
        {
            get;
            private set;

        }
        public Action Action
        {
            get;
            private set;
        }

        public Node(Board endState)
        {
            this.EndState = endState;
        }

        public Node(Board endState, Node parent, int heuristicCost, Action action)
            : this(endState)
        {

            this.parent = parent;
            this.cost = heuristicCost;
            this.Action = action;
        }

        public void Print()
        {
            this.EndState.Print();
            if (parent != null)
            {
                parent.Print();
            }
        }

        public IEnumerable<Tuple<int, int>> GetSwapsList()
        {
            return this.parent.GetSwapsList(Enumerable.Empty<Tuple<int, int>>());
        }

        public IEnumerable<Tuple<int, int>> GetSwapsList(IEnumerable<Tuple<int, int>> swaps)
        {
            if (this.Action != null)
            {
                var toConcat = Enumerable.Repeat(this.Action.Swaps, 1);
                swaps = swaps.Concat(toConcat);
            }
            return parent == null ? swaps : this.parent.GetSwapsList(swaps);
        }

    }

    class Point
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
