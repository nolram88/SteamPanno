using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamPanno.panno
{
	public interface IPannoImageProcessor
	{
		PannoImage Create(int x, int y);
		Task<PannoImage> Effect(PannoImage src, string path, Dictionary<string, Variant> parameters = null);
	}
}
