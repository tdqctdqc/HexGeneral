namespace HexGeneral.Game;

public class DomainPredefs(HexGeneralData data)
    : IPredefHolder<Domain>
{
    public Domain LandDomain => _data.Models.GetModel<Domain>(nameof(LandDomain));
    public Domain AirDomain => _data.Models.GetModel<Domain>(nameof(AirDomain));
    public Domain SeaDomain => _data.Models.GetModel<Domain>(nameof(SeaDomain));
    private HexGeneralData _data = data;
}