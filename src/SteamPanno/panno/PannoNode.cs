using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SteamPanno.panno
{
	public abstract class PannoNode
	{
		public abstract void Draw(Image image);
		public abstract int Count();
	}
}
