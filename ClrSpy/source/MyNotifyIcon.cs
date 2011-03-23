// MyNotifyIcon.cs
// Author: Adam Nathan
// Date: 5/11/2003
// For more information, see http://blogs.gotdotnet.com/anathan
//
// Defines a class similar to System.Windows.Forms.NotifyIcon, but one
// that supports balloon tooltips
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;    
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ClrSpy
{
	// Custom "notify icon" that supports showing balloon tooltips
	public class MyNotifyIcon : Component 
	{
		private Icon icon = null;
		private bool iconShowing = false;
		private bool visible = false;
		private MyNativeWindow window = null;
		private NOTIFYICONDATA iconData;

		internal MyNotifyIcon()
		{
			window = new MyNativeWindow(this);
			iconData = new NOTIFYICONDATA();
			iconData.cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA));
			iconData.hWnd = window.Handle;
			iconData.uID = 1;
			iconData.uCallbackMessage = WM_NOTIFYICONCALLBACK;
		}

		internal MyNotifyIcon(IContainer container) : this()
		{
			container.Add(this);
		}

		// Show a balloon tooltip with the passed-in message, title, and icon
		public void ShowBalloon(string message, string title, BalloonIcon balloonIcon)
		{
			iconData.uFlags = NIF_INFO;
			iconData.szInfo = message;
			iconData.szInfoTitle = title;
			iconData.dwInfoFlags = (int)balloonIcon;

			Shell_NotifyIcon(NIM_MODIFY, ref iconData);
		}

		#region Browsable properties and events
		[Browsable(true)]
		public event EventHandler DoubleClick;

		protected void OnDoubleClick(EventArgs e)
		{
			if (DoubleClick != null) DoubleClick(this, e);
		}

		[Browsable(true)]
		public MyContextMenu ContextMenu
		{
			get { return window.ContextMenu; }
			set { window.ContextMenu = value; }
		}

		[Browsable(true)]
		public Icon Icon
		{
			get { return icon; }
			set
			{
				if (value == null) throw new ArgumentNullException("value", "Cannot set the Icon property to null.");
				this.icon = value;
				iconData.hIcon = icon.Handle;
				if (!DesignMode) UpdateIcon();
			}
		}

		[Browsable(true)]
		public string Text
		{
			get { return iconData.szTip; }
			set
			{
				iconData.szTip = value;
				if (!DesignMode && iconShowing) UpdateIcon();
			}
		}

		[Browsable(true)]
		public bool Visible
		{
			get { return visible; }
			set
			{
				visible = value;
				if (!DesignMode) UpdateIcon();
			}
		}
		#endregion

		protected override void Dispose(bool disposing) 
		{
			if (disposing && window != null)
			{
				visible = false;
				UpdateIcon();
				icon = null;
				window.Dispose();
				window = null;
			}

			base.Dispose(disposing);
		}

		// Handles showing, hiding, or modifying the notify icon
		private void UpdateIcon() 
		{
			if (icon == null) throw new InvalidOperationException("The icon is currently not set.");

			iconData.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;

			if (visible)
			{
				if (iconShowing)
				{
					Shell_NotifyIcon(NIM_MODIFY, ref iconData);
				}
				else
				{
					iconShowing = true;
					Shell_NotifyIcon(NIM_ADD, ref iconData);
				}
			}
			else if (iconShowing)
			{
				iconShowing = false;
				Shell_NotifyIcon(NIM_DELETE, ref iconData);
			}
		}

		// Native window for the notify icon
		private class MyNativeWindow : NativeWindow, IDisposable
		{
			private MyNotifyIcon notifyIcon;
			public MyContextMenu ContextMenu;

			internal MyNativeWindow(MyNotifyIcon notifyIcon)
			{
				CreateHandle(new CreateParams());
				this.notifyIcon = notifyIcon;
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing) 
			{
				PostMessage(new HandleRef(this, Handle), WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
				DestroyHandle();
			}

			~MyNativeWindow()
			{
				Dispose(false);
			}

			protected override void WndProc(ref Message m)
			{
				switch (m.Msg)
				{
					case WM_NOTIFYICONCALLBACK:
					switch ((int)m.LParam)
					{
						case WM_LBUTTONDBLCLK:
							notifyIcon.OnDoubleClick(new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0));
							break;
						case WM_RBUTTONUP:
							if (ContextMenu != null)
							{
								POINT pt = new POINT();
								GetCursorPos(out pt);

								// See Q135788 in the Microsoft Knowledge Base for more info.
								SetForegroundWindow(new HandleRef(this, this.Handle));

								ContextMenu.Show();

								TrackPopupMenuEx(new HandleRef(ContextMenu, ContextMenu.Handle), 0, pt.x, pt.y, new HandleRef(this, this.Handle), IntPtr.Zero);
								PostMessage(new HandleRef(this, this.Handle), WM_NULL, IntPtr.Zero, IntPtr.Zero);
							}
							break;
					}
						break;
					case WM_COMMAND:
						if (IntPtr.Zero == m.LParam)
						{
							int item = LOWORD(m.WParam);
							int flags = HIWORD(m.WParam);

							if ((flags & MF_POPUP) == 0)
							{
								foreach (MenuItem menuItem in ContextMenu.MenuItems)
								{
									if (menuItem as MyMenuItem == null)
									{
										Debug.Assert(false, "The context menu items must all be of type MyMenuItem, but \"" + menuItem.Text + "\" is not.");
										throw new ApplicationException("The context menu items must all be of type MyMenuItem, but \"" + menuItem.Text + "\" is not.");
									}

									if (item == (menuItem as MyMenuItem).GetMenuID())
									{
										menuItem.PerformClick();
										break;
									}
								}
							}
						}
						base.WndProc(ref m);
						break;
					default:
						if (m.Msg == WM_TASKBARCREATED)
						{
							notifyIcon.iconShowing = false;
							notifyIcon.UpdateIcon();
						}
						base.WndProc(ref m);
						break;
				}
			}

			private static int HIWORD(IntPtr x)
			{
				return (unchecked((int)(long)x) >> 16) & 0xffff;
			}
    
			private static int LOWORD(IntPtr x)
			{
				return unchecked((int)(long)x) & 0xffff;
			}
		}

		#region Interop definitions
		private const int MF_POPUP = 0x10;
		private const int NIF_ICON = 2;
		private const int NIF_INFO = 0x10;
		private const int NIF_MESSAGE = 1;
		private const int NIF_TIP = 4;
		private const int NIM_ADD = 0;
		private const int NIM_DELETE = 2;
		private const int NIM_MODIFY = 1;
		private const int WM_CLOSE = 0x10;
		private const int WM_COMMAND = 0x111;
		private const int WM_LBUTTONDBLCLK = 0x203;
		private const int WM_NOTIFYICONCALLBACK = WM_USER + 1024;
		private const int WM_NULL = 0;
		private const int WM_RBUTTONUP = 0x205;
		private const int WM_USER = 0x400;

		private static uint WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");

		[DllImport("user32.dll")]
		private static extern bool GetCursorPos(out POINT lpPoint);

		[DllImport("user32.dll")]
		private static extern bool PostMessage(HandleRef hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		private static extern uint RegisterWindowMessage(string lpString);

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(HandleRef hWnd);

		[DllImport("shell32.dll", CharSet=CharSet.Auto)]
		private static extern bool Shell_NotifyIcon(int dwMessage, [In] ref NOTIFYICONDATA lpdata);

		[DllImport("user32.dll")]
		private static extern bool TrackPopupMenuEx(HandleRef hmenu, uint fuFlags, int x, int y, HandleRef hwnd, IntPtr lptpm);

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		internal struct NOTIFYICONDATA
		{
			internal int cbSize;
			internal IntPtr hWnd;
			internal uint uID;
			internal uint uFlags;
			internal uint uCallbackMessage;
			internal IntPtr hIcon;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
			internal string szTip;
			internal int dwState;
			internal int dwStateMask;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
			internal string szInfo;
			internal uint uTimeout;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=64)]
			internal string szInfoTitle;
			internal int dwInfoFlags;
			internal Guid guidItem;
		}

		internal struct POINT
		{
			internal int x;
			internal int y;
		}
		#endregion
	}
    
	// Custom menu item that exposes its ID
	public class MyMenuItem : MenuItem
	{
		internal int GetMenuID()
		{
			return MenuID;
		}
	}

	// Custom context menu that exposes a method for showing it
	public class MyContextMenu : ContextMenu
	{
		internal void Show()
		{
			this.OnPopup(null);
		}
	}

	public enum BalloonIcon
	{
		None = 0,
		Info = 1,
		Warning = 2,
		Error = 3,
	}
}