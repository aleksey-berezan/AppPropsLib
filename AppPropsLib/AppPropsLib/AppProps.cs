using System;
using System.Collections.Generic;
using System.IO;

namespace AppPropsLib {

	public class AppProps {

		// useful for unit testing
		// kinda encapsulation hack :)
		private readonly Dictionary<String, Func<Object, Object>> _internalProperties = new Dictionary<String, Func<Object, Object>>();

		private readonly Dictionary<String, AppPropsRecord> _propertiesDic = new Dictionary<String, AppPropsRecord>();
		private readonly List<AppPropsRecord> _propertiesList = new List<AppPropsRecord>();

		public AppProps() { }

		private AppProps(String path) {
			String[] lines = File.ReadAllLines(path);
			for (Int32 i = 0; i < lines.Length; i++) {
				var record = new AppPropsRecord(lines[i], i);
				_propertiesDic.Add(record.HashKey, record);
				_propertiesList.Add(record);
			}

			_internalProperties.Add("Count", arg => _propertiesDic.Count);
			_internalProperties.Add("GetRecordAt", arg => _propertiesList[(Int32)arg]);
		}

		public static AppProps FromFile(String path) {
			return new AppProps(path);
		}

		public void SaveToFile(String path) {
			using (var sw = File.CreateText(path)) {
				foreach (var record in _propertiesList) {
					sw.WriteLine(record.ToString(true));
				}
			}
		}

		public String Get(String key) {
			return _propertiesDic[key].Value;
		}

		public void Set(String key, String value) {
			_propertiesDic[key].Value = value;
		}

		public void Append(String key, String value) {
			var record = new AppPropsRecord(key, value);
			_propertiesDic.Add(record.HashKey, record);
			_propertiesList.Add(record);
		}

		/// <summary>Created for unit-testing. 
		/// Allows to access private methods/properties. 
		/// 
		/// See source code for details. </summary>
		public Object InternalInvoke(String key, Object arg) {
			return _internalProperties[key](arg);
		}

		public Boolean Exists(String key) {
			return _propertiesDic.ContainsKey(key);
		}
	}
}