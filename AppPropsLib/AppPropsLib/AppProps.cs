using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		public static AppProps Merge(AppProps baseProps, AppProps overrideProps) {
			var merged = new AppProps();

			// collecting items from base
			foreach (var appProps in baseProps._propertiesDic.Values) {
				merged.Append(appProps.Key, appProps.Value);
			}

			// merging and overriding it with items from overridee
			foreach (var appProps in overrideProps._propertiesDic.Values) {
				if (appProps.IsEmpty) continue;

				if (merged.Exists(appProps.Key)) {
					merged[appProps.Key] = appProps.Value;
				} else {
					merged.Append(appProps.Key, appProps.Value);
				}
			}

			return merged;
		}

		public void SaveToFile(String path) {
			using (var sw = File.CreateText(path)) {
				foreach (var record in _propertiesList) {
					sw.WriteLine(record.ToString(true));
				}
			}
		}

		public String this[String key] {
			get {
				return _propertiesDic[key].Value;
			}
			set {
				_propertiesDic[key].Value = value;
			}
		}

		public ReadOnlyCollection<AppPropsRecord> Items {
			get { return _propertiesDic.Values.Where(p => !p.IsEmpty).ToList().AsReadOnly(); }
		}

		public void Append(String key, String value) {
			var record = new AppPropsRecord(key, value);
			_propertiesDic.Add(record.HashKey, record);
			_propertiesList.Add(record);
		}

		public void Remove(String key) {
			_propertiesDic.Remove(key);
			_propertiesList.Remove(_propertiesList.First(p => p.Key == key));
		}

		public Int32 Count {
			get { return _propertiesList.Count(p => !p.IsEmpty); }
		}

		public Boolean Exists(String key) {
			return _propertiesDic.ContainsKey(key);
		}
	}
}