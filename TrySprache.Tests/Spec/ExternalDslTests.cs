using System;
using Sprache;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace TrySprache.Tests {
	public class Question {
		public Question(string id, string prompt) {
			Id = id;
			Prompt = prompt;
		}
		public string Id { set; get; }
		public string Prompt { set; get; }
	}

	public class Section {

		public Section(string id, string title, IEnumerable<Question> questions) {
			Id = id;
			Title = title;
			Questions = questions;
		}

		public string Id { set; get; }
		public string Title { set; get; }
		public IEnumerable<Question> Questions { set; get; } = new List<Question>();
	}


	public class ExternalDslTests {

		static readonly Parser<string> Identifier = Parse.Letter.AtLeastOnce().Text().Token();

		static readonly Parser<string> QuotedText =
			 (from open in Parse.Char('"')
			 from content in Parse.CharExcept('"').Many().Text()
			 from close in Parse.Char('"')
			 select content).Token();


		static readonly Parser<Question> Question =
			from id in Identifier
			from prompt in QuotedText
			select new Question(id, prompt);

		static readonly Parser<Section> Section =
			from id in Identifier
			from title in QuotedText
			from lbracket in Parse.Char('[').Token()
			from questions in Question.Many()
			from rbracket in Parse.Char(']').Token()
			select new Section(id, title, questions);

		public void ShouldParseIdentifier() {
			var input = "name";
			Identifier.Parse(input).Should().Be("name");
		}

		public void ShouldParseQuoteText() {
			var input = "\"this is text\"";
			QuotedText.Parse(input).Should().Be("this is text");
		}

		public void ShouldParseQuestion() {
			var input = "name \"Full Name\"";
			var rs = Question.Parse(input);
			rs.Id.Should().Be("name");
			rs.Prompt.Should().Be("Full Name");
		}

		public void ShouldParseSection() {
			var input = @"
identification ""Personal Details""
[
	name		""Full Name""
	department	""Department""
]
			";

			var rs = Section.Parse(input);
			rs.Id.Should().Be("identification");
			rs.Title.Should().Be("Personal Details");

			var questions = rs.Questions.ToList();
			questions.Count.Should().Be(2);
			questions[0].Id.Should().Be("name");
			questions[0].Prompt.Should().Be("Full Name");
		}
	}
}

