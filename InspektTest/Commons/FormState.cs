using System;
using System.Windows.Forms;

namespace Visutronik.Commons
{
	/// <summary>
	/// Class used to preserve / restore state of the form
	/// http://www.vesic.org/english/blog/winforms/full-screen-maximize/
	/// Hinweis von Lulu:
	/// mit "targetForm.TopMost = true"
	/// werden Dialoge im Hintergrund geöffnet und die GUI wird unbedienbar!!!
	/// Damit zu Programmstart im Window-Modus alles normal angezeigt wird,
	/// wurden winState und brdStyle entsprechend initialisiert.
	/// </summary>
	public class FormState
	{
		private FormWindowState winState = FormWindowState.Normal;
		private FormBorderStyle brdStyle = FormBorderStyle.Sizable;
		//private bool topMost = false;
		private System.Drawing.Rectangle bounds;

		private bool IsMaximized = false;

		public void Maximize(Form targetForm)
		{
			if (!IsMaximized)
			{
				IsMaximized = true;
				Save(targetForm);
				targetForm.WindowState = FormWindowState.Maximized;
				targetForm.FormBorderStyle = FormBorderStyle.None;
				//targetForm.TopMost = true;
				WinApi.SetWinFullScreen(targetForm.Handle);
			}
		}
		
		public void Save(Form targetForm)
		{
			winState = targetForm.WindowState;
			brdStyle = targetForm.FormBorderStyle;
			//topMost = targetForm.TopMost;
			bounds = targetForm.Bounds;
		}

		public void Restore(Form targetForm)
		{
			if (IsMaximized)
			{
				targetForm.WindowState = winState;
				targetForm.FormBorderStyle = brdStyle;
				//targetForm.TopMost = topMost;
				targetForm.Bounds = bounds;
				IsMaximized = false;
			}
		}
	}
	
}
