using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLayer
{
    public class Graph
    {
        public List<List<int>> AccessMatrix { get; set; }
        public List<List<double>> CostMatrix { get; set; }
        public List<List<int>> Ways { get; set; }
        public int Vertices { get; set; }
        public List<double> Sums { get; set; }
        public int IndexCrit { get; set; }
        public List<double> Budgets { get; set; }
        public double Crit { get; set; }
        public List<WorkTimeRow> WorkTimeRows { get; set; }

        public class WorkTimeRow
        {
            public int State1 { get; set; }
            public int State2 { get; set; }
            public double Cost { get; set; }
            public double StartEarlier { get; set; }
            public double StartLatest { get; set; }
            public double FinishEarlier { get; set; }
            public double FinishLatest { get; set; }
            public double Budget { get; set; }

            public WorkTimeRow()
            {
            }

            public WorkTimeRow(int state1, int state2, double cost, double startEarlier, double startLatest,
                double finishEarlier, double finishLatest, double budget)
            {
                State1 = state1;
                State2 = state2;
                Cost = cost;
                StartEarlier = startEarlier;
                StartLatest = startLatest;
                FinishEarlier = finishEarlier;
                FinishLatest = finishLatest;
                Budget = budget;
            }
        }

        public Graph(int[][] accessMatrix, double[][] costMatrix)
        {
            AccessMatrix = accessMatrix.Select(m => m.ToList()).ToList();
            CostMatrix = costMatrix.Select(m => m.ToList()).ToList();
            Vertices = AccessMatrix.Count;

            Ways = GetWays();
            Sums = GetSums(Ways);
            IndexCrit = Sums.FindIndex(s => s == Sums.Max());
            Crit = Sums.Max();
            Budgets = Sums.ToList();
            var max = Sums.Max();
            Budgets = Budgets.Select(b => max - b).ToList();
        }

        public List<List<int>> GetWays(int iFrom = 0, int iTo = -1)
        {
            if (iTo == -1)
            {
                iTo = Vertices - 1;
            }

            var foundWays = new List<List<int>>();
            var ways = new List<List<int>> {new List<int> {iFrom}};
            var addingWays = new List<List<int>>();
            var removingWays = new List<List<int>>();
            while (true)
            {
                ways.ForEach(way =>
                {
                    var copyWay = way.ToList();
                    var currentPosition = way.LastOrDefault();
                    var check = true;

                    for (var i = 0; i < AccessMatrix[currentPosition].Count; i++)
                    {
                        if (AccessMatrix[currentPosition][i] == 0) continue;
                        if (check)
                        {
                            way.Add(i);
                            check = false;
                            if (i != iTo) continue;
                            foundWays.Add(way);
                            removingWays.Add(way);
                        }
                        else
                        {
                            var addingWay = copyWay.ToList();
                            addingWay.Add(i);
                            if (i != iTo)
                            {
                                addingWays.Add(addingWay);
                            }
                            else
                            {
                                foundWays.Add(addingWay);
                            }
                        }
                    }

                    if (check)
                    {
                        removingWays.Add(way);
                    }
                });

                addingWays.ForEach(ways.Add);
                removingWays.ForEach(way => ways.Remove(way));

                addingWays.Clear();
                removingWays.Clear();

                if (ways.Count == 0) break;
            }

            return foundWays;
        }

        public List<double> GetSums(List<List<int>> ways)
        {
            var sums = new List<double>();
            ways.ForEach(way =>
            {
                var sum = 0.0;
                for (var i = 0; i < way.Count - 1; i++)
                {
                    sum += CostMatrix[way[i]][way[i + 1]];
                }

                sums.Add(sum);
            });

            return sums;
        }

        public void PrintTable()
        {
            var maxWayLength = Ways.Max(way => WayToString(way).Length) + 2;
            Console.WriteLine();
            PrintBr();
            Console.WriteLine(string.Format("| {0, -5} | {1, " + -maxWayLength + "} | {2, -6} | {3, -6} |", "Index",
                "Way", "Cost", "Budget"));
            PrintBr();
            for (var i = 0; i < Ways.Count; i++)
            {
                if (i == IndexCrit)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(GetRowString(i));
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.WriteLine(GetRowString(i));
                }

                PrintBr();
            }
        }

        public string GetRowString(int i = 0)
        {
            var maxWayLength = Ways.Max(way => WayToString(way).Length) + 2;
            return string.Format("| {0, -5} | {1, " + -maxWayLength + "} | {2, -6} | {3, -6} |", i + 1,
                WayToString(Ways[i]), Sums[i], Budgets[i]);
        }

        public void PrintBr(int length = -1)
        {
            Console.WriteLine(length == -1 ? "".PadLeft(GetRowString().Length, '-') : "".PadLeft(length, '-'));
        }

        public string WayToString(List<int> way)
        {
            return way.Select(v => (++v).ToString()).Aggregate((p, n) => p + " -> " + n);
        }

        public List<WorkTimeRow> GetWorkTimes()
        {
            WorkTimeRows = new List<WorkTimeRow>();
            for (var i = 0; i < AccessMatrix.Count; i++)
            {
                var row = AccessMatrix[i];
                for (var j = 0; j < row.Count; j++)
                {
                    if (row[j] == 0 || CostMatrix[i][j] == 0) continue;
                    var workTime = new WorkTimeRow
                    {
                        State1 = i,
                        State2 = j,
                        Cost = CostMatrix[i][j]
                    };
                    if (j == Vertices - 1)
                    {
                        workTime.FinishEarlier = Crit;
                        workTime.FinishLatest = Crit;
                    }
                    else
                    {
                        var ways = GetWays(0, j);
                        var sums = GetSums(ways);
                        workTime.FinishEarlier = sums.Max();
                        ways = GetBackWays(Vertices - 1, j);
                        var values = GetSumValues(ways);
                        workTime.FinishLatest = values.Min();
                    }

                    workTime.Budget = workTime.FinishLatest - workTime.FinishEarlier;
                    workTime.StartEarlier = workTime.FinishEarlier - workTime.Cost;
                    workTime.StartLatest = workTime.FinishLatest - workTime.Cost;

                    WorkTimeRows.Add(workTime);
                }
            }
            return WorkTimeRows;
        }

        private List<double> GetSumValues(List<List<int>> ways)
        {
            var result = new List<double>();
            ways.ForEach(way =>
            {
                var value = Crit;
                for (var i = 0; i < way.Count - 1; i++)
                {
                    value -= CostMatrix[way[i + 1]][way[i]];
                }

                result.Add(value);
            });
            return result;
        }

        private List<List<int>> GetBackWays(int iFrom, int iTo)
        {
            var foundWays = new List<List<int>>();
            var ways = new List<List<int>> {new List<int> {iFrom}};
            var addingWays = new List<List<int>>();
            var removingWays = new List<List<int>>();
            while (true)
            {
                ways.ForEach(way =>
                {
                    var copyWay = way.ToList();
                    var currentPosition = way.LastOrDefault();
                    var check = true;

                    for (var i = 0; i < Vertices; i++)
                    {
                        if (AccessMatrix[i][currentPosition] == 0) continue;
                        if (check)
                        {
                            way.Add(i);
                            check = false;
                            if (i != iTo) continue;
                            foundWays.Add(way);
                            removingWays.Add(way);
                        }
                        else
                        {
                            var addingWay = copyWay.ToList();
                            addingWay.Add(i);
                            if (i != iTo)
                            {
                                addingWays.Add(addingWay);
                            }
                            else
                            {
                                foundWays.Add(addingWay);
                            }
                        }
                    }

                    if (check)
                    {
                        removingWays.Add(way);
                    }
                });

                addingWays.ForEach(ways.Add);
                removingWays.ForEach(way => ways.Remove(way));

                addingWays.Clear();
                removingWays.Clear();

                if (ways.Count == 0) break;
            }

            return foundWays;
        }

        public void PrintWorkTimeRows()
        {
            var formatHead =
                $"| {"Index",-5} | {"Work",-7} | {"Cost",-4} | {"StartEarlier",-12} | {"StartLatest",-11} | {"FinishEarlier",-13} | {"FinishLatest",-12} | {"Budget",-6} |";
            var lengthRow = formatHead.Length;
            PrintBr(lengthRow);
            Console.WriteLine(formatHead);
            PrintBr(lengthRow);
            for (var i = 0; i < WorkTimeRows.Count; i++)
            {
                var row = WorkTimeRows[i];
                Console.WriteLine(
                    $"| {i + 1,-5} | {(row.State1 + 1) + " - " + (row.State2 + 1),-7} | {row.Cost,-4} | {row.StartEarlier,-12} | {row.StartLatest,-11} | {row.FinishEarlier,-13} | {row.FinishLatest,-12} | {row.Budget, -6} |");
                PrintBr(lengthRow);
            }
        }
    }
}