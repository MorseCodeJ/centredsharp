using CentrED.Map;
using CentrED.UI;
using CentrED.UI.Windows;
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
    private float _grade;

    private int _mode;
    private bool _drawMode;
    private bool _smoothMode;
    private bool _classicSmooth;
    private bool _ignoreWater;

    private LandTile[] _landTiles = new LandTile[1];
    private LandTile _origin = new(ushort.MaxValue, 0, 0, 0);

    internal override void Draw()
    {
        base.Draw();
        ImGui.Checkbox("Ignore Water", ref _ignoreWater);
        ImGui.Checkbox("Random Tile Set", ref MapManager.UseRandomTileSet);
        ImGui.Checkbox("Draw Mode", ref _drawMode);
        ImGui.Checkbox("Smooth Mode", ref _smoothMode);
        if (_smoothMode)
        {
            ImGui.Checkbox("Classic Smoothing", ref _classicSmooth);
        }
        UIManager.DragInt("Radius", ref _radius, 1, 1, 15);
        ImGui.RadioButton("Add/Remove Z", ref _mode, 0);
        ImGui.RadioButton("Set Z", ref _mode, 1);
        UIManager.DragInt("Z Value", ref value, 1, -128, 127);
        ImGui.DragFloat("Grade", ref _grade, 0.01f, 0, 1);
        ImGui.Checkbox("Use Z floor.", ref useZFloor);
        if (useZFloor)
        {
            UIManager.DragInt("Z Floor", ref zFloor, 1, -128, 128);
        }
    }
    private double WeightedZ(LandTile tile)
    {
        double dist = Math.Sqrt(Math.Pow(_origin.X - tile.X, 2) + Math.Pow(_origin.Y - tile.Y, 2)) + 1;
        double weight = ((value / dist) * (1 - _grade)) + _grade;
        return weight;
    }

    private sbyte NewZ(LandTile tile)
    {
        sbyte z;
        if (_smoothMode)
        {
            LandObject neighborS = Application.CEDGame.MapManager.LandTiles[tile.X, tile.Y + 1];
            LandObject neighborN = Application.CEDGame.MapManager.LandTiles[tile.X, tile.Y - 1];
            LandObject neighborE = Application.CEDGame.MapManager.LandTiles[tile.X - 1, tile.Y];
            LandObject neighborW = Application.CEDGame.MapManager.LandTiles[tile.X + 1, tile.Y];

            int avg = (neighborS.LandTile.Z + neighborN.LandTile.Z + neighborE.LandTile.Z + neighborW.LandTile.Z) / 4;
            int diff = tile.Z - avg;
            z = (sbyte)(tile.Z - (diff * _grade / 2));
            if (!_classicSmooth)
            {
                z += (sbyte)(tile.Z > _origin.Z ? -1 : 1);
            }
            return z;
        }

        z =  (sbyte)(useZFloor ? Math.Max(tile.Z + WeightedZ(tile), zFloor) : tile.Z + WeightedZ(tile));
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

    protected override void GhostApply(TileObject? o)
    {        
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
                    if (newLand == null || (_ignoreWater && newLand.AlwaysFlat(newLand.Tile.Id)))
                    {
                        continue;
                    }
                    LandTile lt = (LandTile)newLand.Tile;
                    double dist = Math.Sqrt(Math.Pow(_origin.X - lt.X, 2) + Math.Pow(_origin.Y - lt.Y, 2));
                    if (dist > _radius)
                    {
                        continue;
                    }
                    sbyte z = NewZ(lt);
                    if (_drawMode)
                    {
                        lt.ReplaceLand(UIManager.GetWindow<TilesWindow>().ActiveId, NewZ(lt));
                    }
                    else
                    {
                        lt.Z = z;
                    }
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

    //protected override void InternalApply(TileObject? o)
    //{
    //    if (o is LandObject lo)
    //    {
    //        _origin = (LandTile)lo.Tile;
    //        for (int i = -_radius; i < _radius; i++)
    //        {
    //            for (int j = -_radius; j < _radius; j++)
    //            {
    //                LandObject newLand = Application.CEDGame.MapManager.LandTiles[_origin.X + i, _origin.Y + j];
    //                if (newLand == null)
    //                {
    //                    return;
    //                }
    //                if (MapManager.GhostLandTiles.TryGetValue(newLand, out var ghostTile))
    //                {
    //                    newLand.Tile.Z = ghostTile.Tile.Z;
    //                    if (_drawMode)
    //                    {
    //                        newLand.Tile.Id = ghostTile.Tile.Id;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
}