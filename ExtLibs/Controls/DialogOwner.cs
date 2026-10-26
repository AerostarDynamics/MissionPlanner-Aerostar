using System.Windows.Forms;

namespace MissionPlanner.Controls
{
    /// <summary>
    /// Keeps modal dialogs in front of Mission Planner.
    /// </summary>
    /// <remarks>
    /// A dialog shown with ShowDialog() and no owner can open behind the main window (e.g. when the user clicked another
    /// window while connecting, or after Alt+Tab). The main window is then disabled while it waits for a dialog the user
    /// cannot see, and the program looks frozen. An owned window always stays above its owner, so giving every modal
    /// dialog an owner prevents that.
    /// </remarks>
    public static class DialogOwner
    {
        /// <summary>
        /// The window a new modal dialog should belong to: the application's active window, otherwise its most
        /// recently opened visible window (normally the main window, or the dialog already on screen). Null if none.
        /// </summary>
        public static Form Find(Form dialog)
        {
            var active = Form.ActiveForm;
            if (IsCandidate(active, dialog))
                return active;

            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
            {
                var form = Application.OpenForms[i];
                if (IsCandidate(form, dialog))
                    return form;
            }

            return null;
        }

        /// <summary>
        /// Shows <paramref name="dialog"/> modally, owned by <see cref="Find"/>, and brings it to the front when it
        /// appears.
        /// </summary>
        public static DialogResult ShowDialog(Form dialog)
        {
            dialog.ShowInTaskbar = true;
            dialog.Shown += (sender, e) =>
            {
                dialog.Activate();
                dialog.BringToFront();
            };

            var owner = Find(dialog);
            return owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        }

        static bool IsCandidate(Form form, Form dialog)
        {
            return form != null && form != dialog && !form.IsDisposed && form.Visible && !form.InvokeRequired;
        }
    }
}
