using EnvDTE;
using Microsoft.VisualStudio.CommandBars;
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
    class OutputPaneLog
    {
        readonly DTE _dte;
        readonly Window _outputWindow;
        readonly OutputWindowPane _outputPane;
        readonly StdPicture[] _statusImages =
{
            imageToPicture(ImageBuilder.createStatusImage(VSColors.Notification.Warning, false)),
            imageToPicture(ImageBuilder.createStatusImage(VSColors.Notification.Positive, false)),
            imageToPicture(ImageBuilder.createStatusImage(VSColors.Notification.Negative, false))
        };
        readonly StdPicture[] _statusImagesProcessing =
        {
            imageToPicture(ImageBuilder.createStatusImage(VSColors.Notification.Warning, true)),
            imageToPicture(ImageBuilder.createStatusImage(VSColors.Notification.Positive, true)),
            imageToPicture(ImageBuilder.createStatusImage(VSColors.Notification.Negative, true))
        };
        readonly string _buttonTag;
        BuildStatus _status = BuildStatus.Indeterminate;
        bool _processing;
        const string BarButtonControlCaption = "BuildOnSave Status";

        public OutputPaneLog(DTE dte)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _dte = dte;
            // create the output pane.
            _outputWindow = _dte.Windows.Item(Constants.vsWindowKindOutput);
            _outputPane = ((OutputWindow)_outputWindow.Object).OutputWindowPanes.Add("MultiSolutionBuild");
            _buttonTag = Guid.NewGuid().ToString();

            // http://stackoverflow.com/questions/12049362/programmatically-add-add-in-button-to-the-standard-toolbar
            // add a toolbar button to the standard toolbar
            var bar = ((CommandBars)_dte.CommandBars)["Standard"];
            if (bar != null)
            {
                var control = (CommandBarButton)bar.Controls.Add(MsoControlType.msoControlButton, Type.Missing, Type.Missing, Type.Missing, true);
                control.Style = MsoButtonStyle.msoButtonIcon;
                control.TooltipText = BarButtonControlCaption;
                control.Caption = BarButtonControlCaption;
                control.Tag = _buttonTag;
                control.BeginGroup = true;
                control.Click += (CommandBarButton ctrl, ref bool d) =>
                {
                    Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                    _outputWindow.Visible = true;
                    _outputPane.Activate();
                };
            }
            updateUI();
        }

        public void WriteLog(string s)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputPane.OutputString(s);
        }

        void updateUI()
        {
            withButton(button =>
            {
                using (DevTools.measureBlock("setting image"))
                    button.Picture = getImage(_status, _processing);
            });
        }

        public void Dispose()
        {
            withButton(button => button.Delete());
        }

        // Note that the button may be disposed at any time, so we have to look it up every time
        // we access it.
        void withButton(Action<CommandBarButton> action)
        {
            var bar = ((CommandBars)_dte.CommandBars)["Standard"];
            var button = bar?.FindControl(Tag: _buttonTag) as CommandBarButton;
            if (button != null)
                action(button);
        }

        public void setBuildStatus(BuildStatus status)
        {
            _status = status;
            _processing = false;
            updateUI();
        }

        public void notifyBeginBuild()
        {
            _processing = true;
            updateUI();
        }

        public void reportError(Exception e)
        {
            _outputPane.reportException(e);
            setBuildStatus(BuildStatus.Failed);
        }

        StdPicture getImage(BuildStatus status, bool processing)
        {
            var images = processing ? _statusImagesProcessing : _statusImages;
            var statusAsInt = (int)status;
            if (statusAsInt >= images.Length)
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
            return images[statusAsInt];
        }

        static StdPicture imageToPicture(Image image)
        {
            return PictureConverter.getIPicture(image);
        }

        // http://stackoverflow.com/questions/31324924/vs-2013-sdk-how-to-set-a-commandbarbutton-picture

        sealed class ImageToPictureDispConverter : AxHost
        {
            public ImageToPictureDispConverter() : base("{63109182-966B-4e3c-A8B2-8BC4A88D221C}")
            { }

            public StdPicture getIPicture(Image image)
            {
                return (StdPicture)GetIPictureFromPicture(image);
            }
        }

        static readonly ImageToPictureDispConverter PictureConverter = new ImageToPictureDispConverter();
    }

    enum BuildStatus
    {
        Indeterminate,
        Ok,
        Failed
    }

    static class Helpers
    {
        public static void reportException(this OutputWindowPane pane, Exception e)
        {
            pane.OutputString($"---------- BuildOnSave CRASHED, please report to https://www.github.com/pragmatrix/BuildOnSave/issues\n");
            pane.OutputString(e.ToString());
        }
    }

}
