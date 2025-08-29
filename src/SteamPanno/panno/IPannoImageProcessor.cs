namespace SteamPanno.panno
{
	public interface IPannoImageProcessor
	{
		PannoImage Create(int x, int y);
		PannoImage Blur(PannoImage src);
	}
}
