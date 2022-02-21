using System;
using System.Windows.Forms;

namespace RoadCaptain.Host.Console
{
    public static class WinFormsExtensionMethods
    {
        public static void Invoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}