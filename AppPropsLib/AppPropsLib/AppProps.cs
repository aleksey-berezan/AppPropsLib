using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace AppPropsLib {

	public class AppProps {

		private readonly Dictionary<String, AppPropsItem> _propertiesDic = new Dictionary<String, AppPropsItem>();
		private readonly List<AppPropsItem> _propertiesList = new List<AppPropsItem>();

		#region .ctors

		public AppProps() { }

		private AppProps(String path) {
			String[] lines = File.ReadAllLines(path);
			for (Int32 i = 0; i < lines.Length; i++) {
				var item = new AppPropsItem(lines[i], i);
				_propertiesDic.Add(item.HashKey, item);
				_propertiesList.Add(item);
			}
		}

		public AppProps(IEnumerable<String> lines) {
			foreach (var line in lines) {
				Append(new AppPropsItem(line));
			}
		}

		public AppProps(IEnumerable<AppPropsItem> items) {
			foreach (var item in items) {
				Append(item);
			}
		}

		// actually, not a .ctor, but sort of ...
		public static AppProps FromFile(String path) {
			return new AppProps(path);
		}

		#endregion

		public static AppProps Merge(AppProps baseProps, AppProps overrideProps) {
			var merged = new AppProps();

			// collecting items from base
			foreach (var appProps in baseProps._propertiesDic.Values) {
				merged.Append(appProps.Key, appProps.Value);
			}

			// merging and overriding it with items from overridee
			foreach (var appProps in overrideProps._propertiesDic.Values) {
				if (appProps.IsEmpty) continue;

				if (merged.ContainsKey(appProps.Key)) {
					merged[appProps.Key] = appProps.Value;
				} else {
					merged.Append(appProps.Key, appProps.Value);
				}
			}

			return merged;
		}

		public void SaveToFile(String path) {
			using (var sw = File.CreateText(path)) {
				foreach (var item in _propertiesList) {
					sw.WriteLine(item.ToString(true));
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

		public ReadOnlyCollection<AppPropsItem> Items {
			get { return _propertiesDic.Values.Where(p => !p.IsEmpty).ToList().AsReadOnly(); }
		}

		public void Append(String key, String value) {
			var item = new AppPropsItem(key, value);
			Append(item);
		}

		private void Append(AppPropsItem item) {
			_propertiesDic.Add(item.HashKey, item);
			_propertiesList.Add(item);
		}

		public String GetExistingOrDefault(String key, String defaultValue) {
			return ContainsKey(key) ? this[key] : defaultValue;
		}

		public void Remove(String key) {
			_propertiesDic.Remove(key);
			_propertiesList.Remove(_propertiesList.First(p => p.Key == key));
		}

		public Int32 Count {
			get { return _propertiesList.Count(p => !p.IsEmpty); }
		}

		public Boolean ContainsKey(String key) {
			return _propertiesDic.ContainsKey(key);
		}
	}
}