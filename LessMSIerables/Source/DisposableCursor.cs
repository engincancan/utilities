/*
Scott Willeke - 2004
http://scott.willeke.com 
Consider this code licensed under Common Public License Version 1.0 (http://www.opensource.org/licenses/cpl1.0.txt).
*/
using System;
using System.Windows.Forms;

namespace Willeke.Shared.Windows.Forms
{
	/// <summary>
	/// DisposableCursor allows you to use the C# using statement to return to a normal cursor.
	/// </summary>
	/// <example>Simple Example of using the DisposableCursor with the C# using statement
	/// <code lang="C#">
	/// using (new Willeke.Shared.Windows.Forms.DisposableCursor(this))
	/// {
	///		// Put the busy operation code here...
	///		System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
	/// }
	/// </code>
	/// </example>
	internal class DisposableCursor : IDisposable
	{
		// The cursor used before this DisposableCursor was initialized.
		private Cursor previousCursor;
		// The control this cursor instance is used with.
		private Control control;


		/// <summary>
		/// Initializes an instance of the DisposableCursor class with "Wait Cursor" displayed for the specified control.
		/// </summary>
		/// <param name="control">The control to display the cursor over.</param>
		public DisposableCursor(Control control)
			:this(control, Cursors.WaitCursor)
		{
		}

		/// <summary>
		/// Initializes an instance of the DisposableCursor class with the specified cursor displayed for the specified control.
		/// </summary>
		/// <param name="control">The control to display the cursor over.</param>
		/// <param name="newCursor">The cursor to display while the mouse pointer is over the control.</param>
		public DisposableCursor(Control control, Cursor newCursor)
		{
			if (control == null)
				throw new ArgumentNullException("control");
			if (newCursor == null)
				throw new ArgumentNullException("cursor");

			this.previousCursor = control.Cursor;
			this.control = control;
			control.Cursor = newCursor;
			control.Update();
		}

		#region Implementation of IDisposable
		public void Dispose()
		{
			// Dispose the existing cursor (the one created by this class)
			//this.control.Cursor.Dispose();// DON'T dispose this.  Aparently .NET doesn't like you disposing system cursors :D

			// Give the control back it's old cursor
			this.control.Cursor = this.previousCursor;
		}
		#endregion

	}
}
