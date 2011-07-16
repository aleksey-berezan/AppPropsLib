using System;
using System.Text.RegularExpressions;

namespace AppPropsLib {

	public sealed class AppPropsRecord {
		private const String KeyValuePattern = @"[^#]+=[^#]+";
		private const String CommentPattern = "#+.*";
		private const string EmptyPrefix = "Empty_";

		private readonly String _hashKey;
		private readonly String _key;
		private String _value;

		private readonly String _originalLine;
		private readonly Int32 _lineNumber;

		public String Value {
			get { return _value; }
			set { _value = value; }
		}

		internal String HashKey {
			get { return _hashKey; }
		}

		public String Key {
			get { return _key; }
		}

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
				_hashKey = _key = pair[0].Trim();
				_value = pair[1].Trim();
			} else {
				_hashKey = EmptyPrefix + _lineNumber;
				_key = String.Empty;
				_value = String.Empty;
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