namespace HexGeneral.Game;

public class LandformPredefs : IPredefHolder<Landform>
{
    public Landform Plain { get; private set; }
        = new Landform(nameof(Plain));
    public Landform Sea { get; private set; }
        = new Landform(nameof(Sea));
}