using CentrED.IO.Models;
using CentrED.Map;
using CentrED.UI.Windows;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CentrED.Tools
{
    public class ShorelineTool : BaseTool
    {
        public override string Name => "Shoreline";
        public override Keys Shortcut => Keys.F12;

        private bool _VirtualLayer;
        private int _VirtualLayerZ;

        private static readonly ushort _OuterSouthCorner = 0x001C;
        private static readonly ushort[] _InnerSouthCorners = [0x001D, 0x0048];
        private static readonly ushort[] _InnerWestCorners = [0x001E, 0x0049];
        private static readonly ushort[] _InnerEastCorners = [0x001F, 0x0050];
        private static readonly ushort[] _InnerNorthCorners = [0x0020, 0x0051];
        private static readonly ushort[] _SouthWalls = [0x0021, 0x0025, 0x0044];
        private static readonly ushort[] _WestWalls = [0x0022, 0x0026, 0x0045];
        private static readonly ushort[] _EastWalls = [0x0023, 0x0027, 0x0046];
        private static readonly ushort[] _NorthWalls = [0x0024, 0x0028, 0x0047];

        private static readonly ushort[] _DeepTiles = [0x1A, 0x1B, 0x50];

        private void AddStatic(LandObject lo, ushort[] ids, int[] loc)
        {
            //MapManager.UseVirtualLayer = true;
            //MapManager.VirtualLayerZ = -5;
            var newTile = new StaticTile
            (
                ids[Random.Next(ids.Length)],
                (ushort)(lo.Tile.X + loc[0]),
                (ushort)(lo.Tile.Y + loc[1]),
                -5,
                0
            );
            StaticObject so = new(newTile);
            Client.Add(so.StaticTile);
            //MapManager.VirtualLayerZ = -15;
        }

        private void BuildShore(LandObject lo)
        {
            LandTile tile = (LandTile)lo.Tile;
            switch (_Neighbors)
            {
                case (Direction.East):
                case (Direction.East | Direction.Right):
                case (Direction.East | Direction.Down):
                case (Direction.East | Direction.Down | Direction.Right):
                    tile.ReplaceLand(0x50, -15);
                    AddStatic(lo, [0x179D, 0x179E], [1, 0]);
                    return;
                case (Direction.South):
                case (Direction.South | Direction.Left):
                case (Direction.South | Direction.Down):
                case (Direction.South | Direction.Down | Direction.Left):
                    tile.ReplaceLand(0x50, -15);
                    AddStatic(lo, [0x17A1, 0x17A2], [0, 1]);
                    return;
                case (Direction.North):
                case (Direction.North | Direction.Up):
                case (Direction.North | Direction.Right):
                case (Direction.North | Direction.Right | Direction.Up):
                    tile.ReplaceLand(0x50, -15);
                    AddStatic(lo, [0x179F, 0x17A0], [0, 0]);
                    return;
                case (Direction.West):
                case (Direction.West | Direction.Left):
                case (Direction.West | Direction.Up):
                case (Direction.West | Direction.Up | Direction.Left):
                    tile.ReplaceLand(0x50, -15);
                    AddStatic(lo, [0x17A3, 0x17A4], [0, 0]);
                    return;
                case (Direction.Right):
                    tile.ReplaceLand(0x50, -15);
                    AddStatic(lo, [0x17A9], [0, 0]);
                    return;
                case (Direction.Right | Direction.North | Direction.East):
                    tile.ReplaceLand(0x50, -15);
                    AddStatic(lo, [0x17A5], [1, 0]);
                    return;
                case (Direction.Down):
                case (Direction.Down | Direction.South | Direction.East):
                    tile.ReplaceLand(0x50, -15);
                    AddStatic(lo, [0x17A7], [0, 0]);
                    return;
                case (Direction.Left):
                    tile.ReplaceLand(0x50, -15);
                    AddStatic(lo, [0x17AB], [0, 1]);
                    return;
                case (Direction.Left | Direction.South | Direction.West):
                    tile.ReplaceLand(0x50, -15);
                    AddStatic(lo, [0x17A8], [0, 0]);
                    return;
                case (Direction.Up):
                case (Direction.Up | Direction.North | Direction.West):
                case (Direction.North | Direction.Up | Direction.West | Direction.Left):
                    tile.ReplaceLand(0x50, -15);
                    AddStatic(lo, [0x17A6], [0, 0]);
                    return;
                default:
                    return;
            }
        }

        private Direction _Neighbors = Direction.None;

        private static LandObject GetNeighbor(LandObject lo, Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    return Application.CEDGame.MapManager.LandTiles[lo.Tile.X, lo.Tile.Y - 1];
                case Direction.Right:
                    return Application.CEDGame.MapManager.LandTiles[lo.Tile.X + 1, lo.Tile.Y - 1];
                case Direction.East:
                    return Application.CEDGame.MapManager.LandTiles[lo.Tile.X + 1, lo.Tile.Y];
                case Direction.Down:
                    return Application.CEDGame.MapManager.LandTiles[lo.Tile.X + 1, lo.Tile.Y + 1];
                case Direction.South:
                    return Application.CEDGame.MapManager.LandTiles[lo.Tile.X, lo.Tile.Y + 1];
                case Direction.Left:
                    return Application.CEDGame.MapManager.LandTiles[lo.Tile.X - 1, lo.Tile.Y + 1];
                case Direction.West:
                    return Application.CEDGame.MapManager.LandTiles[lo.Tile.X - 1, lo.Tile.Y];
                case Direction.Up:
                    return Application.CEDGame.MapManager.LandTiles[lo.Tile.X - 1, lo.Tile.Y - 1];
                default:
                    return null;
            }
        }
        private bool CheckLandNeighbors(LandObject origin)
        {
            if (origin.Tile.Id == 0x50)
            {
                return false;
            }
            Direction dir;
            for (int i = 0; i < 8; i++)
            {
                dir = (Direction)(1 << i);
                LandObject neighbor = GetNeighbor(origin, dir);
                if (!neighbor.AlwaysFlat(neighbor.Tile.Id) && !_DeepTiles.Contains<ushort>(neighbor.Tile.Id))
                {
                    _Neighbors |= dir;
                }
            }
            return !(_Neighbors == Direction.None);
        }

        public override void OnActivated(TileObject? o)
        {
            _VirtualLayer = MapManager.UseVirtualLayer;
            _VirtualLayerZ = MapManager.VirtualLayerZ;
            MapManager.UseVirtualLayer = false;
        }

        public override void OnDeactivated(TileObject? o)
        {
            MapManager.UseVirtualLayer = _VirtualLayer;
            MapManager.VirtualLayerZ = _VirtualLayerZ;
        }

        protected override void InternalApply(TileObject? o)
        {
            if (o is LandObject lo)
            {
                _Neighbors = Direction.None;
                if (lo.AlwaysFlat(lo.Tile.Id) && CheckLandNeighbors(lo))
                {
                    BuildShore(lo);
                }
            }
        }

        protected override void GhostApply(TileObject? o)
        {
        }
        protected override void GhostClear(TileObject? o)
        {
        }
    }
}
