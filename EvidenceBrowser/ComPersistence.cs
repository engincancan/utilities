using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ComplexIT.COM
{
	/// <summary>
	/// Imports the 'mother of all COM persistence'
	/// </summary>
	[ComImport]
	[Guid("0000010c-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPersist
	{
		void GetClassID(out Guid pClassID);
	}
 
	/// <summary>
	/// Imports IPersistStreamInit, which happens to be what IE supports
	/// </summary>
	[ComImport]
	[Guid("7FD52380-4E07-101B-AE2D-08002B2EC713")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPersistStreamInit : IPersist
	{
		new void GetClassID(out Guid pClassID);
 
		[PreserveSig]
		int IsDirty();
		void Load([In] UCOMIStream pStm);
		void Save([In] UCOMIStream pStm, [In,
			MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
		void GetSizeMax(out long pcbSize);
		void InitNew();
	}
 
	/// <summary>
	/// A limited (non-transactional, non-locking, non-stat returning) in-memory
	/// implementation of IStream. Enough for IE though.
	/// </summary>
	/// <phew>
	/// Thank you, Microsoft, for already providing an interop def for IStream.
	/// </phew>
	public class AxMemoryStream : MemoryStream, UCOMIStream
	{
		void UCOMIStream.Clone(out UCOMIStream clone)
		{
			clone = this.MemberwiseClone() as UCOMIStream;
		}
 
		void UCOMIStream.Commit(int flags)
		{
			throw new NotImplementedException("AxMemoryStream is not transactional");
		}
 
		void UCOMIStream.CopyTo(UCOMIStream destination, long count, IntPtr pcbRead, IntPtr pcbWritten)
		{
			//////////
			// Copy the lot using 4k chunks
			//////////
			byte [] _buffer = new byte[4096];
			int _cbRead = 0;
			int _cbWritten = 0;
			while (count > 0)
			{
				int _chunk = (int)Math.Min(count, _buffer.Length);
				int _chunkRead = this.Read(_buffer, _cbRead, _chunk);
				destination.Write(_buffer, _chunk, IntPtr.Zero);
 
				_cbRead += _chunkRead;
				_cbWritten += _chunkRead;
			}
 
			//////////
			// Update the counts, if they were provided
			//////////
			if (pcbRead != IntPtr.Zero)
			{
				Marshal.WriteInt64(pcbRead, _cbRead);
			}
 
			if (pcbWritten != IntPtr.Zero)
			{
				Marshal.WriteInt64(pcbWritten, _cbWritten);
			}
		}
 
		void UCOMIStream.LockRegion(long offset, long count, int lockType)
		{
			throw new NotImplementedException("AxMemoryStream does not support locking");
		}
 
		void UCOMIStream.Read(byte [] buffer, int count, IntPtr pcbRead)
		{
			int _cbRead = this.Read(buffer, 0 ,count);
 
			if (pcbRead != IntPtr.Zero)
			{
				Marshal.WriteInt32(pcbRead, _cbRead);
			}
		}
 
		void UCOMIStream.Revert()
		{
			throw new NotImplementedException("AxMemoryStream is not transactional");
		}
 
		void UCOMIStream.Seek(long offset, int origin, IntPtr pcbPos)
		{
			long _position = this.Seek(offset, (SeekOrigin)origin);
 
			if (pcbPos != IntPtr.Zero)
			{
				Marshal.WriteInt64(pcbPos, _position);
			}
		}
 
		void UCOMIStream.SetSize(long newSize)
		{
			this.SetLength(newSize);
		}
 
		void UCOMIStream.Stat(out STATSTG stat, int flags)
		{
			stat = new STATSTG();
			stat.cbSize = Marshal.SizeOf(stat);
			stat.grfLocksSupported = 0;
		}
 
		void UCOMIStream.UnlockRegion(long offset, long count, int lockType)
		{
			throw new NotImplementedException("AxMemoryStream does not support locking");
		}
 
		void UCOMIStream.Write(byte [] buffer, int count, IntPtr pcbWritten)
		{
			this.Write(buffer, 0, count);
 
			if (pcbWritten != IntPtr.Zero)
			{
				Marshal.WriteInt32(pcbWritten, count);
			}
		}
	}
}
