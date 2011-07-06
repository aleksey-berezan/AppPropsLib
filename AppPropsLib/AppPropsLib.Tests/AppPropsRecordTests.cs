using NUnit.Framework;

namespace AppPropsLib.Tests {

	[TestFixture]
	public class AppPropsRecordTests {

		[Test]
		public void Can_parse_simple_line() {
			var r = new AppPropsRecord("key=value");
			Assert.AreEqual("key", r.Key);
			Assert.AreEqual("value", r.Value);
		}

		[Test]
		public void Can_parse_line_with_comments() {
			var r = new AppPropsRecord("key=value#bla-bla");
			Assert.AreEqual("key", r.Key);
			Assert.AreEqual("value", r.Value);
			Assert.AreEqual("key=value#bla-bla", r.ToString(true));
		}

		[Test]
		public void Can_parse_line_with_comments_with_double_delimiter() {
			var r = new AppPropsRecord("key=value####bla-bla");
			Assert.AreEqual("key", r.Key);
			Assert.AreEqual("value", r.Value);
			Assert.AreEqual("key=value####bla-bla", r.ToString(true));
		}

		[Test]
		public void Can_parse_simple_line_with_white_spaces() {
			var r = new AppPropsRecord("    key    =    value");
			Assert.AreEqual("key", r.Key);
			Assert.AreEqual("value", r.Value);
		}

		[Test]
		public void Can_parse_empty_line() {
			var r = new AppPropsRecord("");
			Assert.AreEqual("", r.Key);
			Assert.AreEqual("", r.Value);
		}

	}
}
