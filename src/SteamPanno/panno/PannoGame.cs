namespace SteamPanno.panno
{
	public record PannoGame
	{
		public int Id { get; init; }
		public string Name { get; init; }
		public decimal HoursOnRecord { get; set; }
	}
}
