namespace HexGeneral.Game;

public class MoveTypePredefs(HexGeneralData data) : IPredefHolder<MoveType>
{
    public MoveType SupplyMoveType
        => (MoveType)data.Models.ModelsByName[nameof(SupplyMoveType)];
}