using CentrED.Map;
using CentrED.UI;
using CentrED.UI.Windows;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;

namespace CentrED.Tools;

public class ElevateTool : BaseTool
{
    
    public override string Name => "Elevate";
    public override Keys Shortcut => Keys.F4;
    
    enum ZMode
    {
        ADD = 0,
        SET = 1,
        RANDOM = 2
    }

    private int _zMode;
    private int _value;
    private int _randomZ = 0;

    private bool _useZFloor;
    private int _zFloor;
    private bool _drawMode;

    internal override void Draw()
    {
        base.Draw();
        ImGui.RadioButton("Add", ref _zMode, (int)ZMode.ADD);
        ImGui.RadioButton("Set", ref _zMode, (int)ZMode.SET);
        ImGui.RadioButton("Random +/-", ref _zMode, (int)ZMode.RANDOM);
        ImGui.Checkbox("Draw Mode", ref _drawMode);
        if (ImGui.Button("Inverse"))
        {
            _value = -_value;
        }
        UIManager.DragInt("Z", ref _value, 1, -128, 127);
        if (_zMode == (int)ZMode.ADD || _zMode == (int)ZMode.SET)
        {
            UIManager.DragInt("Add Random Z", ref _randomZ, 1, 0, 127);
        }
        ImGui.Checkbox("Use Z floor.", ref _useZFloor);
        if (_useZFloor)
        {
            UIManager.DragInt("Z Floor", ref _zFloor, 1, -128, 128);
        }
    }

    private sbyte NewZ(BaseTile tile) => (sbyte)((ZMode)_zMode switch
    {
        ZMode.ADD => _useZFloor ? Math.Max(tile.Z + _value + Random.Next(0, _randomZ), _zFloor) : tile.Z + _value + Random.Next(0, _randomZ),
        ZMode.SET => _value + Random.Next(0, _randomZ),
        ZMode.RANDOM => tile.Z + Random.Next(-Math.Abs(_value), Math.Abs(_value) + 1),
        _ => throw new ArgumentOutOfRangeException()
    });

    protected override void GhostApply(TileObject? o)
    {
        if (o is StaticObject so)
        {
            var tile = so.StaticTile;
            so.Alpha = 0.3f;
            var newTile = new StaticTile(tile.Id, tile.X, tile.Y, NewZ(tile), tile.Hue);
            MapManager.GhostStaticTiles[so] = new StaticObject(newTile);
        }
        else if (o is LandObject lo)
        {
            var tile = lo.LandTile;
            lo.Visible = false;
            var newTile = new LandTile(tile.Id, tile.X, tile.Y, NewZ(tile));
            MapManager.GhostLandTiles[lo] = new LandObject(newTile);
        }
    }

    protected override void GhostClear(TileObject? o)
    {
        o?.Reset();
        if (o is StaticObject)
        {
            MapManager.GhostStaticTiles.Remove(o);
        }
        else if (o is LandObject lo)
        {
            MapManager.GhostLandTiles.Remove(lo);
        }
    }

    protected override void InternalApply(TileObject? o)
    {
        if (o is StaticObject)
        {
            if (MapManager.GhostStaticTiles.TryGetValue(o, out var ghostTile))
            {
                o.Tile.Z = ghostTile.Tile.Z;
            }
        }
        else if (o is LandObject lo && lo.Tile is LandTile lt)
        {
            if (MapManager.GhostLandTiles.TryGetValue(lo, out var ghostTile))
            {
                if (_drawMode)
                {
                    lt.ReplaceLand(UIManager.GetWindow<TilesWindow>().ActiveId, ghostTile.Tile.Z);
                }
                else
                {
                    lt.Z = ghostTile.Tile.Z;
                }
            }
        }
    }
}