using EnvDTE;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using MultiSolutionBuild.Utilities;
using stdole;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSolutionBuild.Log
{
    public class OutputPaneLog
    {
        private readonly DTE _DTＥ;
        private readonly Window _outputWindow;
        private readonly OutputWindowPane _outputPane;

        private static OutputPaneLog _Instance = null;

        public static OutputPaneLog GetInstance(DTE dte)
        {
            if (_Instance == null)
            {
                _Instance = new OutputPaneLog(dte);
            }
            return _Instance;
        }


        private OutputPaneLog(DTE dte)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _DTＥ = dte;
            // create the output pane.
            _outputWindow = _DTＥ.Windows.Item(Constants.vsWindowKindOutput);
            _outputPane = ((OutputWindow)_outputWindow.Object).OutputWindowPanes.Add("MultiSolutionBuild");
        }

        public void WriteLog(string s)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindow.Visible = true;
            _outputPane.Activate();
            _outputPane.OutputString(s);
        }
    }
}
