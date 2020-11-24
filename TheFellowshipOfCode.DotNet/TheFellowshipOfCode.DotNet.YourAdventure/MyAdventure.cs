using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HTF2020.Contracts;
using HTF2020.Contracts.Enums;
using HTF2020.Contracts.Models;
using HTF2020.Contracts.Models.Adventurers;
using HTF2020.Contracts.Models.Party;
using HTF2020.Contracts.Requests;
using Dijkstra.NET.Graph;
using Dijkstra.NET.Graph.Simple;
using Dijkstra.NET.ShortestPath;

namespace TheFellowshipOfCode.DotNet.YourAdventure
{
    public class MyAdventure : IAdventure
    {
        private const int WEIGHT_ENEMY = 10;
        private const int WEIGHT_TREASURE = 50;
        private const int WEIGHT_NOTHING = 200;
        
        private TurnAction[] turnActions = null;
        private const int START = 0;
        private const int END = 35;
        private int currentStepInTurnActions = 0;
        

        public Task<Party> CreateParty(CreatePartyRequest request)
        {
            var party = new Party
            {
                Name = "My Party",
                Members = new List<PartyMember>()
            };

            for (var i = 0; i < request.MembersCount; i++)
            {
                party.Members.Add(new Wizard()
                {
                    Id = i,
                    Name = $"Member {i + 1}",
                    Constitution = 11,
                    Strength = 12,
                    Intelligence = 11
                });
            }

            return Task.FromResult(party);
        }

        public Task<Turn> PlayTurn(PlayTurnRequest request)
        {
            if (turnActions == null)
            {
                turnActions = getTurnActionsFromPathLocations(getPathLocationsFromMap(request.Map));
            }
            return PlayToEnd();

            Task<Turn> PlayToEnd()
            {
                return Strategic();
            }

            Task<Turn> Strategic()
            {
                TurnAction turnAction;
                
                if (request.PossibleActions.Contains(TurnAction.Attack))
                {
                    turnAction = TurnAction.Attack;
                }
                else if(request.PossibleActions.Contains(TurnAction.Open))
                {
                    turnAction = TurnAction.Open;
                }
                else if(request.PossibleActions.Contains(TurnAction.Loot))
                {
                    turnAction = TurnAction.Loot;
                }
                else
                {
                    currentStepInTurnActions++;
                    turnAction =  turnActions[currentStepInTurnActions - 1];
                }
                return Task.FromResult(new Turn(turnAction));


            }
        }

        private TurnAction[] getTurnActionsFromPathLocations(Point[] pathLocations)
        {
            TurnAction[] result = new TurnAction[pathLocations.Length];
            int i = 0;
            while (i < pathLocations.Length-1)
            {
                Point nextLocation = pathLocations[i + 1];
                Point currentLocation = pathLocations[i];
                if (currentLocation.X > nextLocation.X)
                {
                    result[i] = TurnAction.WalkWest;
                } 
                else if (currentLocation.X < nextLocation.X)
                {
                    result[i] = TurnAction.WalkEast;
                } 
                else if (currentLocation.Y > nextLocation.Y)
                {
                    result[i] = TurnAction.WalkSouth;
                } 
                else if (currentLocation.Y < nextLocation.Y)
                {
                    result[i] = TurnAction.WalkSouth;
                }
                i++;
            }

            
            return result;
        }

        private Point[] getPathLocationsFromMap(Map map)
        {
            var dijkstras = dijkstrasAlgorithm(map);
            return dijkstras;
            return new[] 
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(2, 0),
                new Point(3, 0),
                new Point(4, 0),
                new Point(5, 0),
                new Point(5, 1),
                new Point(5, 2),
                new Point(5, 3),
                new Point(5, 4),
                new Point(5, 5)
            };
        }

        private Point[] dijkstrasAlgorithm(Map map)
        {
            var graph = new Graph<Point, string>();
            addTilesToGraph(graph, map); 
            connectTilesFromGraph(graph, map);
            //oneseign: 41 67
            //Split: 42 48
            //splitting: 12 44
            ShortestPathResult result = graph.Dijkstra(12, 44);
            var path = result.GetPath();
            var enumer = path.GetEnumerator();
            var points = getPointsFromPath(enumer, map);
            return points;
        }

        private Point[] getPointsFromPath(IEnumerator<uint> enumerator, Map map)
        {
            var list = new List<Point>();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                list.Add(getPointFromPath(current, map));
            }

            return list.ToArray();
        }

        private Point getPointFromPath(uint path, Map map)
        {
            var width = map.Tiles.GetLength(0);
            int x = (int) (path % width);
            int y = (int) (path / width);
            return new Point(x, y);
        }

        private void addTilesToGraph(Graph<Point, string> graph, Map map)
        {
            var matrix = map.Tiles;

            int x = 0;
            int y = 0;
            var width = matrix.GetLength(0);
            var length = matrix.GetLength(1);
            while (y < width)
            {
                x = 0;
                while (x < length)
                {
                    graph.AddNode(new Point(x, y));
                    x++;
                }

                y++;
            }
        }
        
        private void connectTilesFromGraph(Graph<Point, string> graph, Map map)
        {
            var matrix = map.Tiles;

            int x = 0;
            int y = 0;
            uint index = 0;
            var width = matrix.GetLength(0);
            var length = matrix.GetLength(1);
            while (y < length)
            {
                x = 0;
                while (x < width)
                {
                    var current = map.Tiles[x, y];
                    if (x < width-1)
                    {
                        var right = map.Tiles[x + 1, y];
                        var rightIndex = index + 1;
                        if (!(right.TileType == TileType.Wall || right.TerrainType == TerrainType.Water))
                        {
                            var weight = WEIGHT_NOTHING;
                            if (right.TileType == TileType.TreasureChest) weight = WEIGHT_TREASURE;
                            if (right.EnemyGroup != null) weight = WEIGHT_ENEMY;
                            graph.Connect(index, rightIndex, weight, "bla");
                        }
                    }
                    if (x > 0)
                    {
                        var left = map.Tiles[x - 1, y];
                        var leftIndex = index - 1;
                        if (!(left.TileType == TileType.Wall || left.TerrainType == TerrainType.Water))
                        {
                            var weight = WEIGHT_NOTHING;
                            if (left.TileType == TileType.TreasureChest) weight = WEIGHT_TREASURE;
                            if (left.EnemyGroup != null) weight = WEIGHT_ENEMY;
                            graph.Connect(index, leftIndex, weight, "bla");
                        }
                    }

                    if (y < length-1)
                    {
                        var under = map.Tiles[x, y + 1];
                        var underIndex = (uint) (index + width);
                        if (!(under.TileType == TileType.Wall || under.TerrainType == TerrainType.Water))
                        {
                            var weight = WEIGHT_NOTHING;
                            if (under.TileType == TileType.TreasureChest) weight = WEIGHT_TREASURE;
                            if (under.EnemyGroup != null) weight = WEIGHT_ENEMY;
                            graph.Connect(index, underIndex, weight, "bla");
                        }
                    }
                    
                    if (y > 0)
                    {
                        var above = map.Tiles[x, y - 1];
                        var aboveIndex = (uint) (index - width);
                        if (!(above.TileType == TileType.Wall || above.TerrainType == TerrainType.Water))
                        {
                            var weight = WEIGHT_NOTHING;
                            if (above.TileType == TileType.TreasureChest) weight = WEIGHT_TREASURE;
                            if (above.EnemyGroup != null) weight = WEIGHT_ENEMY;
                            graph.Connect(index, aboveIndex, weight, "bla");
                        }
                    }

                    index++;
                    x++;
                }

                y++;
            }
        }

        private int getNumberFromMatrix(Map map, TileType type)
        {
            var matrix = map.Tiles;
            int x = 0;
            int index = 0;
            int y = 0;
            var width = matrix.GetLength(0);
            var length = matrix.GetLength(1);
            while (y < width)
            {
                x = 0;
                while (x < length)
                {
                    if (matrix[x, y].TileType == type)
                    {
                        return index;
                    }

                    x++;
                }

                index++;
                y++;
            }

            return 0;
        }
    }
}