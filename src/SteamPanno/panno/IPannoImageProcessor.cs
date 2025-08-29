using System.Threading.Tasks;

namespace SteamPanno.panno
{
	public interface IPannoImageProcessor
	{
		PannoImage Create(int x, int y);
		Task<PannoImage> Blur(PannoImage src);
	}
}
