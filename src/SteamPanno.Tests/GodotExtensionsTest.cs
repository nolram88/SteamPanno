using Godot;
using Shouldly;
using Xunit;

namespace SteamPanno
{
	public class GodotExtensionsTest
	{
		[Theory]
		[InlineData(100, 100, false)]
		[InlineData(100, 120, false)]
		[InlineData(120, 100, true)]
		public void ShouldReturnOrientation(int x, int y, bool horizontal)
		{
			var result = new Rect2I(0, 0, x, y).PreferHorizontal();
			result.ShouldBe(horizontal);
		}
	}
}
