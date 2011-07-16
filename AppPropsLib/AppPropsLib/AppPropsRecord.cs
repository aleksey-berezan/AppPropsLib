using System;
using System.Text.RegularExpressions;

namespace AppPropsLib {

	public sealed class AppPropsRecord {
		private const String KeyValuePattern = @"[^#]+=[^#]+";
		private const String CommentPattern = "#+.*";
		private const string EmptyPrefix = "Empty_";

		private readonly String _originalLine;
		private readonly Int32 _lineNumber;

		public string Value { get; set; }
		public string Key { get; private set; }
		internal string HashKey { get; private set; }

		public bool IsEmpty {
			get { return HashKey.StartsWith(EmptyPrefix); }
		}

		public AppPropsRecord(String originalLine)
			: this(originalLine, -1) { }

		public AppPropsRecord(String key, String value)
			: this(String.Format("{0}={1}", key, value), -1) { }

		public AppPropsRecord(String originalLine, Int32 lineNumber) {
			_originalLine = originalLine;
			_lineNumber = lineNumber;

			var match = Regex.Match(_originalLine, KeyValuePattern);
			String[] pair;
			if (match.Groups.Count == 1 && (pair = match.Value.Trim().Split('=')).Length == 2) {
				HashKey = Key = pair[0].Trim();
				Value = pair[1].Trim();
			} else {
				HashKey = EmptyPrefix + _lineNumber;
				Key = String.Empty;
				Value = String.Empty;
			}
		}

		public String ToString(Boolean includeComments) {
			if (IsEmpty) {
				return _originalLine;
			}

			Match match;
			return includeComments // comments should be included
					&& (match = Regex.Match(_originalLine, CommentPattern)).Success // and comments exist
						? String.Format("{0}={1}{2}", Key, Value, match.Value)//key=value#comments
						: String.Format("{0}={1}", Key, Value); //key=value
		}

		public override String ToString() {
			return ToString(false);
		}
	}
}