using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Ui;
using HexGeneral.Game.Client;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Components;

public class MobilizerComponent
    (ModelIdRef<Mobilizer> mobilizer, bool active, ERef<Unit> unit) 
    : IComponent
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public ModelIdRef<Mobilizer> Mobilizer { get; private set; } = mobilizer;
    public bool Active { get; private set; } = active;

    public void MarkActive(ProcedureKey key)
    {
        Active = true;
    }

    public void MarkInactive(ProcedureKey key)
    {
        Active = false;
    }

    public Control GetDisplay(HexGeneralClient client)
    {
        var vbox = new VBoxContainer();
        var unit = Unit.Get(client.Data);
        var model = mobilizer.Get(client.Data);
        var texture = new TextureRect();
        texture.Size = Vector2.One * 50f;
        texture.Texture = model.GetTexture();
        texture.StretchMode = TextureRect.StretchModeEnum.Keep;
        vbox.AddChild(texture);
        vbox.CreateLabelAsChild("Mobilizer: " + model.Name);
        Button b = null;
        b = vbox.AddButton(Active ? "Deactivate" : "Activate",
            () =>
            {
                var newValue = Active == false;
                var proc = new SetUnitMobilizationProcedure(Unit, 
                    newValue);

                var inner = new DoProcedureCommand(proc);

                var com = CallbackCommand.Construct(inner, () =>
                {
                    if (GodotObject.IsInstanceValid(b))
                    {
                        b.Text = newValue ? "Deactivate" : "Activate";
                    }
                }, client);
                client.SubmitCommand(com);
            });
        b.Disabled = unit.Moved;
        return vbox;
    }

    public void Added(ProcedureKey key)
    {
        key.Data.Data().Notices.UnitAltered?.Invoke(Unit.Get(key.Data));
    }
}