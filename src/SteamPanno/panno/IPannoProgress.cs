namespace SteamPanno.panno
{
	public interface IPannoProgress
	{
		public void ProgressSet(double value, string text = null);
		void ProgressStop();
	}
}
