namespace SteamPanno
{
	public class Settings
	{
		public string AccountId { get; set; }
		public (int, int) Resolution { get; set; }
		public decimal MinimalHours { get; set; }
		public string GenerationMethod { get; set; }
		public string TileExpansionMethod { get; set; }
	}
}
