/*
Scott Willeke - 2004
http://scott.willeke.com 
Consider this code licensed under Common Public License Version 1.0 (http://www.opensource.org/licenses/cpl1.0.txt).
*/
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

using Microsoft.Tools.WindowsInstallerXml.Msi;

namespace Willeke.Scott.LessMSIerables
{
	class ViewWrapper : IDisposable
	{
		public ViewWrapper(View underlyingView)
		{
			this._underlyingView = underlyingView;
			CreateColumnInfos();
		}

		private View _underlyingView;
		private ColumnInfo[]_columns;

		public ColumnInfo[] Columns
		{
			get { return _columns; }
		}


		private void CreateColumnInfos()
		{
			const int MSICOLINFONAMES = 0;
				const int MSICOLINFOTYPES = 1;
			
			ArrayList colList = new ArrayList();/*<ColumnInfo>*/

			Record namesRecord; Record typesRecord;
			_underlyingView.GetColumnInfo(MSICOLINFONAMES, out namesRecord);
			_underlyingView.GetColumnInfo(MSICOLINFOTYPES, out typesRecord);

			int fieldCount = namesRecord.GetFieldCount();
			Debug.Assert(typesRecord.GetFieldCount() == fieldCount);

			for (int colIndex = 1; colIndex <= fieldCount; colIndex++)
			{
				colList.Add(new ColumnInfo(namesRecord.GetString(colIndex), typesRecord.GetString(colIndex)));
			}
			_columns = (ColumnInfo[])colList.ToArray(typeof(ColumnInfo));
		}


		private ArrayList/*<object[]>*/ _records;
		public IList/*<object[]>*/ Records
		{
			get
			{
				if (_records == null)
				{
					_records = new ArrayList/*<object[]>*/();
					Record sourceRecord;

					while (_underlyingView.Fetch(out sourceRecord))
					{
						object[] values = new object[this._columns.Length];

						for (int i = 0; i < this._columns.Length; i++)
						{
							if (this._columns[i].IsString)
								values[i] = sourceRecord.GetString(i + 1);
							else if (this._columns[i].IsInteger)
								values[i] = sourceRecord.GetInteger(i + 1);
							else
							{
								byte[] buffer = new byte[this._columns[i].Size];
								int actualLen = sourceRecord.GetStream(i + 1, buffer, buffer.Length);
								if (actualLen < buffer.Length)
								{
									byte[] trim = new byte[actualLen];
									Buffer.BlockCopy(buffer, 0, trim, 0, actualLen);
									buffer = trim;
								}
								values[i] = buffer;
							}
						}
						_records.Add(values);
					}
				}
				return _records;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (_underlyingView == null)
				return;
			_underlyingView.Close();
			_underlyingView = null;
		}

		#endregion
	}


	
}
