using NUnit.Framework;

namespace AppPropsLib.Tests {

	[TestFixture]
	public class AppPropsItemTests {

		[Test]
		public void Can_parse_simple_line() {
			var r = new AppPropsItem("key=value");
			Assert.AreEqual("key", r.Key);
			Assert.AreEqual("value", r.Value);
		}

		[Test]
		public void Can_parse_line_with_comments() {
			var r = new AppPropsItem("key=value#blah-blah");
			Assert.AreEqual("key", r.Key);
			Assert.AreEqual("value", r.Value);
			Assert.AreEqual("key=value#blah-blah", r.ToString(true));
		}

		[Test]
		public void Can_parse_line_with_comments_with_double_delimiter() {
			var r = new AppPropsItem("key=value####blah-blah");
			Assert.AreEqual("key", r.Key);
			Assert.AreEqual("value", r.Value);
			Assert.AreEqual("key=value####blah-blah", r.ToString(true));
		}

		[Test]
		public void Can_parse_simple_line_with_white_spaces() {
			var r = new AppPropsItem("    key    =    value");
			Assert.AreEqual("key", r.Key);
			Assert.AreEqual("value", r.Value);
		}

		[Test]
		public void Can_parse_empty_line() {
			var r = new AppPropsItem("");
			Assert.AreEqual("", r.Key);
			Assert.AreEqual("", r.Value);
		}

		[Test]
		public void Can_parse_connection_string() {
			const string key = "connection.connection_string";
			const string value = @"Server=.\SQLEXPRESS;Initial catalog=master;Integrated Security=SSPI;";

			var r = new AppPropsItem(key + "=" + value);
			Assert.AreEqual(value, r.Value);
		}
	}
}