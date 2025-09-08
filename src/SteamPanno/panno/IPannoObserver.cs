using System;

namespace SteamPanno.panno
{
	public interface IPannoObserver
	{
		void ProgressStart(bool indeterminate, string text = null);
		void ProgressUpdate(double value, string text = null);
		void ProgressStop();
		void Report(Exception e);
		void Report(string text);
	}
}
