using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace QueryExpress
{
	/// <summary>
	/// A mediator for managing Edit menu commands (copy, cut, paste, etc)
	/// </summary>
	public class EditManager : System.ComponentModel.Component
	{
		private System.ComponentModel.Container components = null;

		// MenuItems to which to connect
		MenuItem miEdit, miUndo, miCopy, miCut, miPaste, miSelectAll;

		public EditManager(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();
		}

		public EditManager()
		{
			InitializeComponent();
		}

		// Menu item implementing Edit submenu.  Attach/detach event handler
		// to popup event so we can enable/disable sub-items when menu is activated.
		public MenuItem MenuItemEdit
		{
			get {return miEdit;}
			set
			{
				if (miEdit != null)
					miEdit.Popup -= new System.EventHandler (miEdit_Popup);
				miEdit = value;
				if (miEdit != null)
					miEdit.Popup += new System.EventHandler (miEdit_Popup);
			}
		}

		public MenuItem MenuItemUndo
		{
			get {return miUndo;}
			set
			{
				if (miUndo != null)
					miUndo.Click -= new System.EventHandler (miUndo_Click);
				miUndo = value;
				if (miUndo != null)
					miUndo.Click += new System.EventHandler (miUndo_Click);
			}
		}

		public MenuItem MenuItemCopy
		{
			get {return miCopy;}
			set
			{
				if (miCopy != null)
					miCopy.Click -= new System.EventHandler (miCopy_Click);
				miCopy = value;
				if (miCopy != null)
					miCopy.Click += new System.EventHandler (miCopy_Click);
			}
		}

		public MenuItem MenuItemCut
		{
			get {return miCut;}
			set
			{
				if (miCut != null)
					miCut.Click -= new System.EventHandler (miCut_Click);
				miCut = value;
				if (miCut != null)
					miCut.Click += new System.EventHandler (miCut_Click);
			}
		}

		public MenuItem MenuItemPaste
		{
			get {return miPaste;}
			set
			{
				if (miPaste != null)
					miPaste.Click -= new System.EventHandler (miPaste_Click);
				miPaste = value;
				if (miPaste != null)
					miPaste.Click += new System.EventHandler (miPaste_Click);
			}
		}

		public MenuItem MenuItemSelectAll
		{
			get {return miSelectAll;}
			set
			{
				if (miSelectAll != null)
					miSelectAll.Click -= new System.EventHandler (miSelectAll_Click);
				miSelectAll = value;
				if (miSelectAll != null)
					miSelectAll.Click += new System.EventHandler (miSelectAll_Click);
			}
		}

		public void Undo()
		{
			Control c = GetActiveControl();
			if (c is TextBoxBase) ((TextBoxBase) c).Undo();
		}
		
		public void Copy()
		{
			Control c = GetActiveControl();
			if (c is TextBoxBase) ((TextBoxBase) c).Copy();
		}
		
		public void Cut()
		{
			Control c = GetActiveControl();
			if (c is TextBoxBase) ((TextBoxBase) c).Cut();
		}
		
		public void Paste()
		{
			Control c = GetActiveControl();
			if (c is TextBoxBase) ((TextBoxBase) c).Paste();
		}

		public void SelectAll()
		{
			Control c = GetActiveControl();
			if (c is TextBoxBase) ((TextBoxBase) c).SelectAll();
		}

		protected Control GetActiveControl()
		{
			Form form = Form.ActiveForm;
			if (form.IsMdiContainer)
				form = form.ActiveMdiChild;
			return (form == null) ? null : form.ActiveControl;
		}

		protected void miEdit_Popup (object sender, System.EventArgs e)
		{
			bool canUndo, canCopy, canCut, canPaste;
			canUndo = canCopy = canCut = canPaste = false;
			Control c = GetActiveControl();
			if (c != null) CanEdit (c, ref canUndo, ref canCopy, ref canCut, ref canPaste);
			if (miUndo != null) miUndo.Enabled = canUndo;
			if (miCopy != null) miCopy.Enabled = canCopy;
			if (miCut != null) miCut.Enabled = canCut;
			if (miPaste != null) miPaste.Enabled = canPaste; 
		}

		protected void CanEdit (Control c, ref bool canUndo, ref bool canCopy, ref bool canCut, ref bool canPaste)
		{
			if (c is TextBoxBase)
			{
				TextBoxBase t = (TextBoxBase) c;
				canUndo = t.CanUndo;
				canCopy = t.SelectionLength > 0;
				canCut = t.SelectionLength > 0 && !t.ReadOnly;
				IDataObject iData = Clipboard.GetDataObject();
				canPaste = !t.ReadOnly && iData.GetDataPresent (DataFormats.Text);;
			}
		}

		protected void miUndo_Click (object sender, System.EventArgs e)
		{
			Undo();
		}

		protected void miCopy_Click (object sender, System.EventArgs e)
		{
			Copy();
		}

		protected void miCut_Click (object sender, System.EventArgs e)
		{
			Cut();
		}

		protected void miPaste_Click (object sender, System.EventArgs e)
		{
			Paste();
		}

		protected void miSelectAll_Click (object sender, System.EventArgs e)
		{
			SelectAll();
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
