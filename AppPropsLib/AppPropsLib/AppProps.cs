using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AppPropsLib {

	public class AppProps {

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

		public Int32 Count {
			get { return _propertiesList.Count(p => !p.IsEmpty); }
		}

		public Boolean Exists(String key) {
			return _propertiesDic.ContainsKey(key);
		}
	}
}