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

namespace TheFellowshipOfCode.DotNet.YourAdventure
{
    public class MyAdventure : IAdventure
    {
        private TurnAction[] turnActions = null;
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
            int i = 1;
            while (i < pathLocations.Length-1)
            {
                Point previousLocation = pathLocations[i - 1];
                Point currentLocation = pathLocations[i];
                if (previousLocation.X > currentLocation.X)
                {
                    result[i-1] = TurnAction.WalkNorth;
                } 
                else if (previousLocation.X < currentLocation.X)
                {
                    result[i-1] = TurnAction.WalkSouth;
                } 
                else if (previousLocation.Y > currentLocation.Y)
                {
                    result[i-1] = TurnAction.WalkWest;
                } 
                else if (previousLocation.Y < currentLocation.Y)
                {
                    result[i-1] = TurnAction.WalkEast;
                }
                i++;
            }

            
            return result;
        }

        private Point[] getPathLocationsFromMap(Map map)
        {
            return new[] 
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(2, 0),
                new Point(3, 0),
                new Point(4, 0),
                new Point(5, 0),
                new Point(6, 0),
                new Point(6, 1),
                new Point(6, 2),
                new Point(6, 3),
                new Point(6, 4),
                new Point(6, 5),
                new Point(6, 6)
            };
        }
    }
}