using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipboardWatcherClass
{
    public sealed class ClipboardWatcher : IDisposable
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);


        private class Window : NativeWindow, IDisposable
        {
            private static int WM_DRAWCLIPBOARD = 0x308;

            public Window()
            {
                // create the handle for the window.
                this.CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == WM_DRAWCLIPBOARD)
                {
                    IDataObject Data = Clipboard.GetDataObject();
                    if (ClipboardChanged != null)
                        ClipboardChanged(this, new ClipboardChangedEventArgs(Data));
                }
            }

            public event EventHandler<ClipboardChangedEventArgs> ClipboardChanged;

            #region IDisposable Members

            public void Dispose()
            {
                this.DestroyHandle();
            }

            #endregion

        }

        private static Window _window = new Window();
        private static IntPtr _ClipboardViewer = SetClipboardViewer(_window.Handle);

        public ClipboardWatcher()
        {
            _window.ClipboardChanged += delegate (object sender, ClipboardChangedEventArgs args)
            {
                if (ClipboardChanged != null)
                    ClipboardChanged(this, args);
            };

        }

        public event EventHandler<ClipboardChangedEventArgs> ClipboardChanged;

        #region IDisposable Members

        public void Dispose()
        {
            _window.Dispose();
        }

        #endregion
    }

    public class ClipboardChangedEventArgs : EventArgs
    {
        public readonly IDataObject DataObject;

        public ClipboardChangedEventArgs(IDataObject dataObject)
        {
            DataObject = dataObject;
        }
    }

}