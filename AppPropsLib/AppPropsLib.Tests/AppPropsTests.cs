using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace AppPropsLib.Tests {

	[TestFixture]
	public class AppPropsTests {
		private const String AppPropertiesPath = "app.properties";

		#region help-methods

		private static AppProps GetAppProps() {
			return AppProps.FromFile(AppPropertiesPath);
		}

		private static void CreateFile(String contents) {
			File.WriteAllText(AppPropertiesPath, contents);
		}

		[TearDown]
		public void DeleteFile() {
			File.Delete(AppPropertiesPath);
		}

		private static void AssertItemsCountInOutputFile(AppProps p, int count) {
			String path = Path.GetTempFileName();
			p.SaveToFile(path);
			try {
				Assert.AreEqual(count, File.ReadAllLines(path).Length);
			} finally {
				File.Delete(path);
			}
		}

		#endregion

		[Test]
		public void Can_read_simple_file() {
			CreateFile("key=value");
			var p = GetAppProps();
			Assert.AreEqual("value", p["key"]);
			Assert.AreEqual(1, p.Count);
		}

		[Test]
		public void Can_read_multiple_lines_file() {
			// prepare
			var sb = new StringBuilder();
			const int count = 10;
			for (Int32 i = 0; i < count; i++) {
				sb.AppendLine(String.Format("key{0}=value{0}#comment{0}", i));
			}
			CreateFile(sb.ToString());

			// do
			var p = GetAppProps();

			// verify
			Assert.AreEqual(count, p.Count);
			for (Int32 i = 0; i < count; i++) {
				Assert.AreEqual("value" + i, p["key" + i]);
			}
		}

		[Test]
		public void Can_return_0_items_from_empty_file() {
			CreateFile(String.Empty);
			var p = GetAppProps();
			Assert.NotNull(p);
			Assert.AreEqual(0, p.Count);
		}

		[Test]
		public void Can_return_1_emtpty_item_from_one_empty_line_file() {
			var sb = new StringBuilder();
			sb.AppendLine(String.Empty);
			CreateFile(sb.ToString());

			var p = GetAppProps();

			Assert.NotNull(p);
			int count = 1;

			AssertItemsCountInOutputFile(p, count);
		}

		[Test]
		public void Can_save_multiple_lines_file() {// TODO: break this epic test down into multiple test cases
			// prepare
			var sb = new StringBuilder();
			const int count = 10;
			for (Int32 i = 0; i < count; i++) {
				if (i % 5 == 0) {
					sb.AppendLine();
				} else {
					sb.AppendLine(i % 2 == 0 ? "#comment_only_" + i : String.Format("key{0}=value{0}#comment{0}", i));
				}
			}
			CreateFile(sb.ToString());

			// do

			var p = GetAppProps();
			for (Int32 i = 0; i < count; i++) {
				if (i % 3 == 0 && i % 2 != 0) {
					p[string.Format("key{0}", i)] = string.Format("{0}_modified", p["key" + i]);
				}
			}
			for (Int32 i = 0; i < count; i++) {
				p.Append(string.Format("key{0}{0}", i), string.Format("value{0}{0}", i));
			}
			p.SaveToFile(AppPropertiesPath);

			// verify

			String[] lines = File.ReadAllLines(AppPropertiesPath);
			for (Int32 i = 0; i < count; i++) {
				string actual = lines[i];
				if (i % 3 == 0 && i % 2 != 0) {
					Assert.AreEqual(string.Format("key{0}=value{0}_modified#comment{0}", i), actual);
				} else if (i % 5 == 0) {
					Assert.AreEqual("", actual);
				} else {
					Assert.AreEqual(i % 2 == 0
						? string.Format("#comment_only_{0}", i)
						: String.Format("key{0}=value{0}#comment{0}", i), actual);
				}
			}
			for (Int32 i = 0; i < count; i++) {
				Assert.AreEqual(string.Format("key{0}{0}=value{0}{0}", i), lines[i + count]);
			}
		}

		[Test]
		public void Can_remove_item() {
			var sb = new StringBuilder();
			sb.AppendLine("key1=value1");
			sb.AppendLine("key2=value2");
			CreateFile(sb.ToString());

			var p = GetAppProps();
			p.Remove("key1");
			p.SaveToFile(AppPropertiesPath);

			Assert.AreEqual(1, File.ReadAllLines(AppPropertiesPath).Length);
			Assert.AreEqual("key2=value2", File.ReadAllLines(AppPropertiesPath)[0]);
		}

		[Test]
		public void Can_merge_non_intersecting_props() {
			var p1 = new AppProps();
			p1.Append("key1", "value1");
			var p2 = new AppProps();
			p2.Append("key2", "value2");

			var merged = AppProps.Merge(p1, p2);

			Assert.AreEqual("value1", merged["key1"]);
			Assert.AreEqual("value2", merged["key2"]);

			AssertItemsCountInOutputFile(merged, 2);
		}

		[Test]
		public void Can_merge_intersecting_props() {
			var p1 = new AppProps();
			p1.Append("key1", "value1");
			var p2 = new AppProps();
			p2.Append("key1", "value2");

			var merged = AppProps.Merge(p1, p2);

			Assert.AreEqual("value2", merged["key1"]);

			AssertItemsCountInOutputFile(merged, 1);
		}

		[Test]
		public void Can_merge_props_with_empty_lines() {
			// prepare
			// create base props
			var sb = new StringBuilder();
			sb.AppendLine("key1=value1");
			sb.AppendLine("");// with empty line
			CreateFile(sb.ToString());
			var baseProps = GetAppProps();

			// create override props
			sb = new StringBuilder();
			sb.AppendLine("");
			sb.AppendLine("key2=value2");
			CreateFile(sb.ToString());
			var overrideProps = GetAppProps();

			// dp
			var mergedProps = AppProps.Merge(baseProps, overrideProps);

			// verify
			Assert.AreEqual(2, mergedProps.Count);
			Assert.AreEqual("value1", mergedProps["key1"]);
			Assert.AreEqual("value2", mergedProps["key2"]);
		}

		[Test]
		public void Can_return_all_non_empty_items() {
			// prepare
			var sb = new StringBuilder();
			sb.AppendLine("key1=value1");
			sb.AppendLine("key2=value2");
			sb.AppendLine("");
			sb.AppendLine("#some comment ...");
			sb.AppendLine("#");
			CreateFile(sb.ToString());

			// do
			var items = GetAppProps().Items;

			// verify
			Assert.NotNull(items);
			Assert.AreEqual(2, items.Count);
			Assert.AreEqual("key1=value1", items[0].ToString(true));
			Assert.AreEqual("key2=value2", items[1].ToString(true));
		}

		[Test]
		public void Can_return_false_when_dont_contains_requested_key() {
			var sb = new StringBuilder();
			sb.AppendLine("key1=value1");
			CreateFile(sb.ToString());

			Assert.IsFalse(GetAppProps().ContainsKey("not_existing_key"));
		}

		[Test]
		public void Can_return_true_when_contains_requested_key() {
			var sb = new StringBuilder();
			sb.AppendLine("key1=value1");
			CreateFile(sb.ToString());

			Assert.IsTrue(GetAppProps().ContainsKey("key1"));
		}

		[Test]
		public void Can_initialize_from_string_collection_ctor() {
			var appProps = new AppProps(new[] { "key1=value1", "key2=value2" });

			Assert.AreEqual(2, appProps.Count);
			Assert.AreEqual("value1", appProps["key1"]);
			Assert.AreEqual("value2", appProps["key2"]);
		}

		[Test]
		public void Can_initialize_from_items_collection_ctor() {
			var appProps = new AppProps(new[] { new AppPropsItem("key1=value1"), new AppPropsItem("key2=value2") });

			Assert.AreEqual(2, appProps.Count);
			Assert.AreEqual("value1", appProps["key1"]);
			Assert.AreEqual("value2", appProps["key2"]);
		}

		[Test]
		public void Can_return_existing_value_when_contains_key() {
			var appProps = new AppProps(new[] { new AppPropsItem("key1=value1"), new AppPropsItem("key2=value2") });
			Assert.AreEqual("value1", appProps.GetExistingOrDefault("key1", "defaultValue"));
		}

		[Test]
		public void Can_return_default_value_when_dont_contains_key() {
			var appProps = new AppProps(new[] { new AppPropsItem("key1=value1"), new AppPropsItem("key2=value2") });
			Assert.AreEqual("defaultValue", appProps.GetExistingOrDefault("not_existing", "defaultValue"));
		}
	}
}