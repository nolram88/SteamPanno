using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Shouldly;

namespace SteamPanno.panno.generation
{
	public class PannoGameLayoutGeneratorGradualDescentTest : PannoGameLayoutGeneratorTreeBasedTest
	{
		public PannoGameLayoutGeneratorGradualDescentTest()
			: base(new PannoGameLayoutGeneratorGradualDescent())
		{
		}
	}
}
