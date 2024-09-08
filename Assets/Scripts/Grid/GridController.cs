using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEnum;

public class GridController : MonoBehaviour
{
    public int Distance(HexagonModel hexagon1, HexagonModel hexagon2)
    {
        Vector3 vec = Subtract(hexagon1.Qrs, hexagon2.Qrs);
        return (int)((Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z)) / 2);
    }

    private Vector3 Subtract(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    }

    private List<HexagonModel> GetTilesInRangeOfTile(List<HexagonModel> tiles, HexagonModel tile, int range)
    {
        List<HexagonModel> tilesInRange = new List<HexagonModel>();

        //Go over all the tiles
        foreach (var hexa in tiles)
        {
            //If the hexagon is the same as the one we calculate for we skip it
            if(hexa == tile) continue;

            //Get the distance betweent the tiles
            int dist = Distance(tile, hexa);

            //Check if the distance is in our range
            if(dist <= range)
            {
                tilesInRange.Add(hexa);
            }
        }

        return tilesInRange;
    }

    private List<HexagonModel> GetTilesInDiagonalRange(List<HexagonModel> tiles, HexagonModel tile, bool stopAfterBlock, int range, TroopScriptableObject troopData)
    {
        //Get tiles in all 3 (QRS) directions
        List<HexagonModel> tilesInDiagonal = AllTiles1DirectionRange(tiles, tile, stopAfterBlock, new Vector3(0, 1, 0), range, troopData);
        tilesInDiagonal.AddRange(AllTiles1DirectionRange(tiles, tile, stopAfterBlock, new Vector3(1, 0, 0), range, troopData));
        tilesInDiagonal.AddRange(AllTiles1DirectionRange(tiles, tile, stopAfterBlock, new Vector3(0, 0, 1), range, troopData));

        return tilesInDiagonal;
    }

    private List<HexagonModel> AllTiles1DirectionRange(List<HexagonModel> tiles, HexagonModel tile, bool stopAfterBlock, Vector3 dir, int range, TroopScriptableObject troopData)
    {
        List<HexagonModel> directionTiles = new List<HexagonModel>();

        //Store the tile where you start from 
        HexagonModel startTile = tile;

        bool switchedDir = false; //To go in the other direction(starts in the middle to 1 side, then needs to switch)

        int counter = 0; //To check the range

        //The qrs direction
        Vector3 addDir = new Vector3(); 

        if (dir.y == 1)
        {
            addDir = new Vector3(1, 0, -1);
        }

        else if (dir.x == 1)
        {
            addDir = new Vector3(0, 1, -1);
        }

        else if (dir.z == 1)
        {
            addDir = new Vector3(1, -1, 0);
        }

        //As long as we have a valid tile
        while (tile != null)
        {
            //Get neigbouring coordinates
            Vector3 qrs = tile.Qrs + addDir;

            //To check if we found a valid neigbour
            bool foundValid = false;

            bool switchDir = false;

            //Go over all the tiles
            foreach (var hex in tiles)
            {
                //Check if we found the neigbouring coordinates
                if (hex.GetComponent<HexagonModel>().Qrs == qrs)
                {
                    //If we need to stop after a unit and there is a unit on that tile or the tile is mud and the troop is a charging unit
                    if (stopAfterBlock && (hex.GetComponent<HexagonModel>().Troop != null || (hex.GetComponent<HexagonModel>().IsMud) && troopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.Charge)))
                    {
                        //Add the last tile
                        directionTiles.Add(hex);
                        switchDir = true;
                        break;
                    }

                    //Add the tile
                    directionTiles.Add(hex);

                    //Update the current tile to the neighbouring tile
                    tile = hex;

                    //Update foundvalid
                    foundValid = true;
                    break;
                }
            }

            ++counter;
            
            //Check if we reached the range
            if (counter >= range || switchDir)
            {
                //Check if we already swapped directions
                if (!switchedDir)
                {
                    //Change direction
                    addDir *= -1;
                    switchedDir = true;
                    switchDir = false;

                    //Reset back to the middle
                    tile = startTile;
                    counter = 0;
                }
                else break;
            }

            //If we found a valid neigbour we continue
            if (foundValid)
                continue;
        }

        //Remove the tile the diagonals are calculated from
        directionTiles.Remove(startTile); 

        return directionTiles;
    }

    //All the movementtiles 
    public List<HexagonModel> GetMovementTiles(List<HexagonModel> hexagons, HexagonModel tile, TroopScriptableObject cardData)
    {
        List<HexagonModel> movementTiles = new List<HexagonModel>();

        //Get the movement tiles
        movementTiles.AddRange(GetTilesInDiagonalRange(hexagons, tile, true, cardData.MovementRange, cardData));

        //Get the tiles to remove
        List<HexagonModel> removeHexagons = new List<HexagonModel>();

        //Check if there is an enemy on a tile
        foreach(var hexagon in movementTiles)
        {
            if (hexagon.Troop && movementTiles.Contains(hexagon))
                removeHexagons.Add(hexagon);
        }

        //Remove hexagons
        movementTiles.RemoveAll(tile => removeHexagons.Contains(tile));

        return movementTiles;
    }

    internal List<HexagonModel> GetAllAttackTiles(List<HexagonModel> hexagons, HexagonModel tile, TroopScriptableObject cardData)
    {
        List<HexagonModel> attackTiles = new List<HexagonModel>();

        //The characteristic that uses a radius attack
        if(cardData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.UseRadius))
        {
            attackTiles.AddRange(GetTilesInRangeOfTile(hexagons, tile, cardData.AttackRange));
            return attackTiles;
        }

        //The characteristic that uses the charge
        if(cardData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.Charge))
        {
            attackTiles.AddRange(GetTilesInDiagonalRange(hexagons, tile, true, cardData.MovementRange + cardData.AttackRange, cardData)); //Charge gives you 1 extra attackrange
            return attackTiles;
        }

        //Get the tiles that are in the attack range
        attackTiles.AddRange(GetTilesInDiagonalRange(hexagons, tile, true, cardData.AttackRange, cardData));

        //Can't shoot the first radius
        if(!cardData.CanAttackNeigbours)
        {
            var deadzone = GetTilesInRangeOfTile(hexagons, tile, 1);
            foreach (var hex in deadzone)
            {
                attackTiles.Remove(hex);
            }
        }
        return attackTiles;
    }

    public List<HexagonModel> GetAttackTiles(List<HexagonModel> hexagons, HexagonModel tile, TroopScriptableObject cardData)
    {
        List<HexagonModel> attackTiles = new List<HexagonModel>();

        // Check if the troop is reloading or stuck in mud
        if (!tile) return attackTiles;

        // Return empty list if the tile doesn't have a troop
        TroopModel troop = tile.Troop;
        if (!troop) return attackTiles;

        // Return empty list if the troop has to reload or is a charging unit stuck in mud
        if (troop.ShouldReload || troop.ReloadThisTurn || (!troop.CanMove && troop.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.Charge)))
            return attackTiles;

        // All the attacktiles
        List<HexagonModel> allAttackTiles = GetAllAttackTiles(hexagons, tile, cardData);

        // Check if the attack tiles has an enemy
        foreach (var hexa in allAttackTiles)
        {
            if (hexa.Troop && hexa.Troop.TroopData.Side != cardData.Side)
                attackTiles.Add(hexa);
        }

        return attackTiles;
    }
}
