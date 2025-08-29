using System;

namespace SteamPanno.panno
{
	public interface IPannoObserver
	{
		void ProgressSet(double value, string text = null);
		void ProgressStop();
		void Report(Exception e);
		void Report(string text);
	}
}
