using Godot;
using GodotUtilities.Ui;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Client.Ui;

public class UnitKeyboardMode : KeyboardInputMode
{
    public UnitKeyboardMode(HexGeneralClient client)
    {
        AddAction(Key.M, k =>
        {
            if (k.IsPressed() == false)
            {
                client.GetComponent<MapGraphics>()
                    .Units.CycleDomainPriority(client);
            }
        });
    }
    
}