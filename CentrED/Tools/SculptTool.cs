using CentrED.Map;
using CentrED.UI;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using System;

namespace CentrED.Tools;

public class SculptTool : BaseTool
{
    public override string Name => "Sculpt";
    public override Keys Shortcut => Keys.F12;

    private bool useZFloor;
    private int zFloor;
    private int value;
    private int _radius = 1;

    private LandTile[] _landTiles = new LandTile[1];
    private LandTile _origin = new(ushort.MaxValue, 0, 0, 0);

    private double WeightedZ(LandTile tile)
    {
        double dist = Math.Sqrt(Math.Pow(_origin.X - tile.X, 2) + Math.Pow(_origin.Y - tile.Y, 2));
        double weight = dist / _radius;
        return weight;
    }

    private sbyte NewZ(LandTile tile)
    {
        sbyte z =  (sbyte)(useZFloor ? Math.Max(tile.Z + WeightedZ(tile), zFloor) : tile.Z + value);
        if (z > 127)
        {
            z = 127;
        }
        else if (z < -127)
        {
            z = -127;
        }

        return z;
    }

    internal override void Draw()
    {
        base.Draw();
        UIManager.DragInt("Z Offset", ref value, 1, -128, 127);
        UIManager.DragInt("Radius", ref _radius, 1, 1, 15);
        ImGui.Checkbox("Use Z floor.", ref useZFloor);
        if (useZFloor)
        {
            UIManager.DragInt("Z Floor", ref zFloor, 1, -128, 128);
        }
    }
    protected override void GhostApply(TileObject? o)
    {
        if (o is LandObject lo)
        {
            _origin = (LandTile)lo.Tile;
            for (int i = -_radius; i < _radius; i++)
            {
                for (int j = -_radius; j < _radius; j++)
                {
                    LandObject newLand = Application.CEDGame.MapManager.LandTiles[_origin.X + i, _origin.Y + j];
                    if (newLand == null)
                    {
                        return;
                    }
                    LandTile lt = (LandTile)newLand.Tile;
                    lo.Visible = false;
                    var newTile = new LandTile(lt.Id, (ushort)(lt.X + i), (ushort)(lt.Y + j), NewZ(lt));
                    MapManager.GhostLandTiles[newLand] = new LandObject(newTile);
                }
            }
        }
    }
    protected override void GhostClear(TileObject? o)
    {
        o?.Reset();
        if (o is LandObject lo)
        {
            _origin = (LandTile)lo.Tile;
            for (int i = -_radius; i < _radius; i++)
            {
                for (int j = -_radius; j < _radius; j++)
                {
                    MapManager.GhostLandTiles.Remove(Application.CEDGame.MapManager.LandTiles[_origin.X + i, _origin.Y + j]);
                }
            }        
        }
    }

    protected override void InternalApply(TileObject? o)
    {
        if (o is LandObject lo)
        {
            _origin = (LandTile)lo.Tile;
            for (int i = -_radius; i < _radius; i++)
            {
                for (int j = -_radius; j < _radius; j++)
                {
                    LandObject newLand = Application.CEDGame.MapManager.LandTiles[_origin.X + i, _origin.Y + j];
                    if (newLand == null)
                    {
                        return;
                    }
                    if (MapManager.GhostLandTiles.TryGetValue(newLand, out var ghostTile))
                    {
                        newLand.Tile.Z = ghostTile.Tile.Z;
                    }
                }
            }
        }
    }
}