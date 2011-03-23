// DebugProbeConfigFile.cs
// Author: Adam Nathan
// Date: 5/11/2003
// For more information, see http://blogs.gotdotnet.com/anathan
//
// Defines classes that encapsulate XML configuration files.
using System;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Configuration;
using System.Runtime.InteropServices;

namespace ClrSpy
{
	public class DebugProbeConfigFile : ConfigFile
	{
		XmlNode settingsNode;

		public DebugProbeConfigFile(string filename, bool createIfNecessary) : base(filename, createIfNecessary)
		{
			// Ensure there's a <runtime> node inside <configuration>
			// If not, create it.
			XmlNode runtimeNode = config.SelectSingleNode("configuration/runtime");
			if (runtimeNode == null)
			{
				runtimeNode = config.CreateNode(XmlNodeType.Element, "runtime", "");
				config.SelectSingleNode("configuration").AppendChild(runtimeNode);
			}

			// Ensure there's a <developerSettings> node inside <runtime>
			// If not, create it.
			settingsNode = runtimeNode.SelectSingleNode("developerSettings");
			if (settingsNode == null)
			{
				settingsNode = config.CreateNode(XmlNodeType.Element, "developerSettings", "");
				runtimeNode.AppendChild(settingsNode);
			}
		}

		public void SetAttribute(string name, string value)
		{
			XmlAttribute a = config.CreateAttribute(name);
			a.Value = value;
			settingsNode.Attributes.SetNamedItem(a);
		}

		public static void EraseAllProbeSettings(string configFilename, bool appConfig)
		{
			using (DebugProbeConfigFile config = new DebugProbeConfigFile(configFilename, false))
			{
				if (!appConfig)
				{
					// Clear all global settings, which only applies to machine.config.
					foreach (Probe p in Probe.GlobalSettingsList)
					{
						config.SetAttribute(p.ConfigName, "");
					}
				}

				// Clear all probe settings.  If this is machine.config, it allows
				// application config settings to be honored.  If this is an application config
				// file, it's the same as setting the attribute to "false"
				foreach (Probe p in Probe.ProbeList)
				{
					config.SetAttribute(p.ConfigName, "");
				}
			}
		}
	}

	public class ConfigFile : IDisposable
	{
		protected ConfigXmlDocument config;
		private readonly string filename;

		public ConfigFile(string filename, bool createIfNecessary)
		{
			this.filename = filename;
			config = new ConfigXmlDocument();

			if (File.Exists(filename))
			{
				// Load the file if it already exists
				config.Load(filename);
			}
			else if (createIfNecessary)
			{
				// Create the file since it doesn't exist
				using (StreamWriter w = File.CreateText(filename))
				{
					w.Write("<configuration></configuration>");
				}
				config.Load(filename);
			}
			else
			{
				throw new FileNotFoundException("The configuration file \"" + filename + "\" was not found.", filename);
			}
		}

		public void SetAppSetting(string name, string value)
		{
			// Ensure there's an <appSettings> node inside <configuration>
			// If not, create it.
			XmlNode settingsNode = config.SelectSingleNode("configuration/appSettings");
			if (settingsNode == null)
			{
				settingsNode = config.CreateNode(XmlNodeType.Element, "appSettings", "");
				config.SelectSingleNode("configuration").AppendChild(settingsNode);
			}

			XmlAttribute key = config.CreateAttribute("key");
			key.Value = name;

			XmlAttribute val = config.CreateAttribute("value");
			val.Value = value;

			bool found = false;

			foreach (XmlNode n in settingsNode.ChildNodes)
			{
				if (n.Name == "add" && n.Attributes.GetNamedItem("key").Value == name)
				{
					n.Attributes.SetNamedItem(key);
					n.Attributes.SetNamedItem(val);
					found = true;
					break;
				}
			}

			if (!found)
			{
				XmlNode itemNode = config.CreateNode(XmlNodeType.Element, "add", "");
				settingsNode.AppendChild(itemNode);
				itemNode.Attributes.SetNamedItem(key);
				itemNode.Attributes.SetNamedItem(val);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Save the new .config file
				config.Save(filename);
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		~ConfigFile()
		{
			Debug.Assert(false, "You forgot to explicitly dispose this ConfigFile instance!");
			Dispose(false);
		}
	}
}