using System;
using System.IO;
using NUnit.Framework;
using System.Text;

namespace AppPropsLib.Tests {

	[TestFixture]
	public class AppPropsTest {
		private readonly String _path = "app.properties";

		#region help-methods

		private AppProps GetAppProps() {
			return AppProps.FromFile(_path);
		}

		private void CreateFile(String contents) {
			File.WriteAllText(_path, contents);
		}

		[TearDown]
		public void DeleteFile() {
			File.Delete(_path);
		}

		#endregion

		[Test]
		public void Can_read_simple_file() {
			CreateFile("key=value");
			var p = GetAppProps();
			Assert.AreEqual("value", p.Get("key"));
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
				Assert.AreEqual("value" + i, p.Get("key" + i));
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
			p.SaveToFile(_path);
			Assert.AreEqual(1, File.ReadAllLines(_path).Length);
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
					p.Set(string.Format("key{0}", i), string.Format("{0}_modified", p.Get("key" + i)));
				}
			}
			for (Int32 i = 0; i < count; i++) {
				p.Append(string.Format("key{0}{0}", i), string.Format("value{0}{0}", i));
			}
			p.SaveToFile(_path);

			// verify

			String[] lines = File.ReadAllLines(_path);
			for (Int32 i = 0; i < count; i++) {
				string actual = lines[i];
				if (i % 3 == 0 && i % 2 != 0) {
					Assert.AreEqual(string.Format("key{0}=value{0}_modified#comment{0}", i), actual);
				} else if (i % 5 == 0) {
					Assert.AreEqual("", actual);
				} else {
					Assert.AreEqual(i % 2 == 0 ? string.Format("#comment_only_{0}", i) : String.Format("key{0}=value{0}#comment{0}", i), actual);
				}
			}
			for (Int32 i = 0; i < count; i++) {
				Assert.AreEqual(string.Format("key{0}{0}=value{0}{0}", i), lines[i + count]);
			}
		}
	}
}