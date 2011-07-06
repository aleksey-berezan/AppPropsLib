using System;
using System.Collections.Generic;
using System.IO;

namespace AppPropsLib {

	public class AppProps {

		// useful for unit testing
		// kinda encapsulation hack :)
		private readonly Dictionary<String, Func<Object>> _internalProperties = new Dictionary<String, Func<Object>>();

		private readonly Dictionary<String, AppPropsRecord> _propertiesDic = new Dictionary<String, AppPropsRecord>();
		private readonly List<AppPropsRecord> _propertiesList = new List<AppPropsRecord>();

		public AppProps() { }

		public AppProps(String path) {
			String[] lines = File.ReadAllLines(path);
			for (Int32 i = 0; i < lines.Length; i++) {
				var record = new AppPropsRecord(lines[i], i);
				_propertiesDic.Add(record.HashKey, record);
				_propertiesList.Add(record);
			}

			_internalProperties.Add("Count", () => _propertiesDic.Count);
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

		public Object GetInternalProperty(String key) {
			return _internalProperties[key]();
		}

		public Boolean Exists(String key) {
			return _propertiesDic.ContainsKey(key);
		}
	}
}