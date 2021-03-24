using System;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using AutomaticUpdates;


namespace SQLiteTurbo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            // Remove any stale table change files
            TableChanges.RemoveStaleChangeFiles();

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                MainForm mf =  new MainForm();
                if(args.Length > 1)
                    mf.setFiles(args[0], args[1]);
                _mainForm = mf;
                Application.Run(_mainForm);
                _mainForm = null;
            }
            catch (Exception ex)
            {
                _mainForm = null;
                ShowUnexpectedErrorDialog(ex);
            }
            finally
            {
                // Remove all active change files
                TableChanges.RemoveActiveChangeFiles();
            } // finally

            // If there are pending software updates - apply them now
            UpdateEngine.ApplyPendingUpdates();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            // Show error dialog
            ShowUnexpectedErrorDialog(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowUnexpectedErrorDialog((Exception)e.ExceptionObject);
        }

        private static void ShowUnexpectedErrorDialog(Exception error)
        {
            // Prevent multiple unexpected-error-dialogs
            lock (typeof(Program))
            {
                if (_mainForm != null)
                {
                    if (_mainForm.InvokeRequired)
                    {
                        _mainForm.Invoke(new MethodInvoker(delegate
                        {
                            UnexpectedErrorDialog dlg = new UnexpectedErrorDialog();
                            dlg.Error = error;
                            dlg.ShowDialog(_mainForm);
                        }));
                    }
                    else
                    {
                        UnexpectedErrorDialog dlg = new UnexpectedErrorDialog();
                        dlg.Error = error;
                        dlg.ShowDialog(_mainForm);
                    } // else
                }
                else
                {
                    UnexpectedErrorDialog dlg = new UnexpectedErrorDialog();
                    dlg.Error = error;
                    Application.Run(dlg);
                } // else

                Environment.Exit(1);
            } // lock
        }

        private static Mutex _mutex = null;
        private static Form _mainForm = null;
    }
}