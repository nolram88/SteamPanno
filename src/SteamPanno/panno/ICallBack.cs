using System;

namespace SteamPanno.panno
{
	public interface ICallBack
	{
		void ProgressSet(double value, string text = null);
		void ProgressStop();
		void Report(Exception e);
		void Report(string text);
	}
}
