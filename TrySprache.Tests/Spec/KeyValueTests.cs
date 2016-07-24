
using System;
namespace TrySprache.Tests {
	using System.Linq.Expressions;
	using Sprache;
	using System.Linq;
	using System.Collections.Generic;
	using UpdateConfig = Func<ConnectionConfiguration, ConnectionConfiguration>;
	using FluentAssertions;

	public class ConnectionConfiguration {
		public string Host { set; get; }
		public ushort Port { set; get; }
		public string VirtualHost { set;get;}
		public string UserName { set; get; }
		public string Password { set; get; }
		public ushort RequestedHeartbeat { set; get; }
	}

	public class KeyValueTests {

		static Action<TTarget, TProperty> CreateSetter<TTarget, TProperty>(Expression<Func<TTarget, TProperty>> getExpr) {
			var member = getExpr.Body;
			var param = Expression.Parameter(typeof(TProperty));
			var setExpr = Expression.Lambda<Action<TTarget, TProperty>>(
				Expression.Assign(member, param), getExpr.Parameters[0], param);

			return setExpr.Compile();
		}

		static Parser<string> Text = Parse.CharExcept(';').Many().Text();
		static Parser<ushort> Number = Parse.Number.Select(ushort.Parse);

		static Parser<UpdateConfig> Part = new List<Parser<UpdateConfig>> {
			BuildKeyValueParser("host", Text, c => c.Host),
			BuildKeyValueParser("port", Number, c => c.Port),
		    BuildKeyValueParser("virtualHost", Text, c => c.VirtualHost),
			BuildKeyValueParser("requestedHeartbeat", Number, c => c.RequestedHeartbeat),
			BuildKeyValueParser("username", Text, c => c.UserName),
			BuildKeyValueParser("password", Text, c => c.Password),
		}.Aggregate((a, b) => a.Or(b));

		static Parser<UpdateConfig> BuildKeyValueParser<T>(
			string keyName,
			Parser<T> valueParser,
			Expression<Func<ConnectionConfiguration, T>> getter) {

			return from key in Parse.String(keyName).Token()
				from separater in Parse.Char('=')
				from value in valueParser
				select (Func<ConnectionConfiguration, ConnectionConfiguration>)(c => {
					CreateSetter(getter)(c, value);
					return c;
			});
		}

		static IEnumerable<UpdateConfig> Cons(UpdateConfig head, IEnumerable<UpdateConfig> rest) {
			yield return head;
			foreach (var item in rest)
				yield return item;
		}

		static Parser<IEnumerable<UpdateConfig>> ConnectionStringBuilder() {
			var rs = from first in Part
					 from rest in Parse.Char(';').Then(_ => Part).Many()
					 select Cons(first, rest);
			return rs;
		}

		public void ShouldParseKeyValue() {
			var input = "virtualHost=Copa;username=Copa;host=192.168.1.1;password=abc_xyz;port=12345;requestedHeartbeat=3";
            var updater = ConnectionStringBuilder().Parse(input);
            var rs = updater.Aggregate(new ConnectionConfiguration(), (current, updateFunction) => updateFunction(current));

			rs.Host.Should().Be("192.168.1.1");
			rs.Port.Should().Be(12345);
        }
	}
}

