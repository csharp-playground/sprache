using System;
using Sprache;
namespace TrySprache.Tests {
	public class SimpleKeyValueTests {


		public void ShouldParseKeyValue() {
			var input = "virtualHost=Copa;username=Copa;host=192.168.1.1;password=abc_xyz;port=12345;requestedHeartbeat=3";
		}

		public void AnIdentifierIsASequenceOfCharacters() {
			var input = "name";
		}
	}
}

