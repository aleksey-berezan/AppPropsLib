using System;
using System.IO;
using NUnit.Framework;
using System.Text;

namespace AppPropsLib.Tests {
	[TestFixture]
	public class AppPropsTest {
		private readonly String _path = "app.properties";

		private void SaveFile(String contents) {
			File.WriteAllText(_path, contents);
		}

		[TearDown]
		public void DeleteFile() {
			File.Delete(_path);
		}

		[Test]
		public void Can_read_simple_file() {
			SaveFile("key=value");
			var p = new AppProps(_path);
			Assert.AreEqual("value", p.Get("key"));
			Assert.AreEqual(1, p.GetInternalProperty("Count"));
		}

		[Test]
		public void Can_read_multiple_lines_file() {
			// prepare
			var sb = new StringBuilder();
			const int count = 10;
			for (Int32 i = 0; i < count; i++) {
				sb.AppendLine(String.Format("key{0}=value{0}#comment{0}", i));
			}
			SaveFile(sb.ToString());

			// do
			var p = new AppProps(_path);

			// verify
			Assert.AreEqual(count, p.GetInternalProperty("Count"));
			for (Int32 i = 0; i < count; i++) {
				Assert.AreEqual("value" + i, p.Get("key" + i));
			}
		}
		[Test]
		public void Can_save_multiple_lines_file() {// TODO: split it to multiple test cases
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
			SaveFile(sb.ToString());

			// do
			var p = new AppProps(_path);
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