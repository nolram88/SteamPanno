using Shouldly;
using Xunit;

namespace SteamPanno
{
	public class StringExtensionsTest
	{
		[Theory]
		[InlineData(null, false)]
		[InlineData("", false)]
		[InlineData("abc", false)]
		[InlineData("abc (76561190123456789)", true, "76561190123456789")]
		public void ShouldParseSteamId(string input, bool success, string output = null)
		{
			var result = input.TryParseSteamId(out var value);
			result.ShouldBe(success);
			value.ShouldBe(output);
		}

		[Theory]
		[InlineData(null, false)]
		[InlineData("", false)]
		[InlineData("1600x800", true, 1600, 800)]
		[InlineData("1920 by 1080", true, 1920, 1080)]
		public void ShouldParseResolution(
			string input, bool success, int x = 0, int y = 0)
		{
			var result = input.TryParseResolution(out var value);
			result.ShouldBe(success);
			value.X.ShouldBe(x);
			value.Y.ShouldBe(y);
		}
	}
}
