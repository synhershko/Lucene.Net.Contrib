/**
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lucene.Net.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucene.Net.Contrib.Tests.Analysis
{
	[TestClass]
	public class HTMLStripCharFilterTest : TestBase
	{
		[TestMethod]
		public void Test()
		{
			const string html = "<div class=\"foo\">this is some text</div> here is a <a href=\"#bar\">link</a> and " +
								"another <a href=\"http://lucene.apache.org/\">link</a>. " +
								"This is an entity: &amp; plus a &lt;.  Here is an &. <!-- is a comment -->";
			const string gold = " this is some text  here is a  link  and " +
								"another  link . " +
								"This is an entity: & plus a <.  Here is an &.  ";
			var reader = new HTMLStripCharFilter(CharReader.Get(new StringReader(html)));
			var builder = new StringBuilder();
			var ch = -1;
			var goldArray = gold.ToCharArray();
			var position = 0;
			while ((ch = reader.Read()) != -1)
			{
				var theChar = (char)ch;
				builder.Append(theChar);
				Assert.IsTrue(theChar == goldArray[position], "\"" + theChar + "\"" + " at position: " + position + " does not equal: \"" + goldArray[position]
						   + "\". Buffer so far: " + builder + "<EOB>");
				position++;
			}
			Assert.AreEqual(gold, builder.ToString());
		}

		//Some sanity checks, but not a full-fledged check
		[TestMethod]
		public void TestHTML()
		{
			var reader = new HTMLStripCharFilter(CharReader.Get(new StreamReader(GetTestFile("htmlStripReaderTest.html"))));
			var builder = new StringBuilder();
			var ch = -1;
			while ((ch = reader.Read()) != -1)
			{
				builder.Append((char)ch);
			}
			var str = builder.ToString();
			Assert.IsTrue(str.IndexOf("&lt;") == -1, "Entity not properly escaped");//there is one > in the text
			Assert.IsTrue(str.IndexOf("forrest") == -1 && str.IndexOf("Forrest") == -1, "Forrest should have been stripped out");
			Assert.IsTrue(str.Trim().StartsWith("Welcome to Solr"), "File should start with 'Welcome to Solr' after trimming");

			Assert.IsTrue(str.Trim().EndsWith("Foundation."), "File should start with 'Foundation.' after trimming");

		}

		[TestMethod]
		public void TestGamma()
		{
			const string test = "&Gamma;";
			const string gold = "\u0393";
			var set = new HashSet<String> { "reserved" };
			var reader = new HTMLStripCharFilter(CharReader.Get(new StringReader(test)), set);
			var builder = new StringBuilder();
			int ch = 0;
			while ((ch = reader.Read()) != -1)
			{
				builder.Append((char)ch);
			}
			var result = builder.ToString();
			// System.out.println("Resu: " + result + "<EOL>");
			// System.out.println("Gold: " + gold + "<EOL>");
			Assert.IsTrue(result.Equals(gold), result + " is not equal to " + gold + "<EOS>");
		}

		[TestMethod]
		public void TestEntities()
		{
			const string test = "&nbsp; &lt;foo&gt; &Uuml;bermensch &#61; &Gamma; bar &#x393;";
			const string gold = "  <foo> \u00DCbermensch = \u0393 bar \u0393";
			var set = new HashSet<String> { "reserved" };
			var reader = new HTMLStripCharFilter(CharReader.Get(new StringReader(test)), set);
			var builder = new StringBuilder();
			int ch = 0;
			while ((ch = reader.Read()) != -1)
			{
				builder.Append((char)ch);
			}
			var result = builder.ToString();
			// System.out.println("Resu: " + result + "<EOL>");
			// System.out.println("Gold: " + gold + "<EOL>");
			Assert.IsTrue(result.Equals(gold), result + " is not equal to " + gold + "<EOS>");
		}

		[TestMethod]
		public void TestMoreEntities()
		{
			const string test = "&nbsp; &lt;junk/&gt; &nbsp; &#33; &#64; and &#8217;";
			const string gold = "  <junk/>   ! @ and ’";
			var set = new HashSet<String> {"reserved"};
			var reader = new HTMLStripCharFilter(CharReader.Get(new StringReader(test)), set);
			var builder = new StringBuilder();
			int ch = 0;
			while ((ch = reader.Read()) != -1)
			{
				builder.Append((char)ch);
			}
			var result = builder.ToString();
			// System.out.println("Resu: " + result + "<EOL>");
			// System.out.println("Gold: " + gold + "<EOL>");
			Assert.IsTrue(result.Equals(gold), result + " is not equal to " + gold);
		}

		[TestMethod]
		public void TestReserved()
		{
			const string test = "aaa bbb <reserved ccc=\"ddddd\"> eeee </reserved> ffff <reserved ggg=\"hhhh\"/> <other/>";
			var set = new HashSet<String> {"reserved"};
			var reader = new HTMLStripCharFilter(CharReader.Get(new StringReader(test)), set);
			var builder = new StringBuilder();
			int ch = 0;
			while ((ch = reader.Read()) != -1)
			{
				builder.Append((char)ch);
			}
			var result = builder.ToString();
			// System.out.println("Result: " + result);
			assertTrue("Escaped tag not preserved: " + result.IndexOf("reserved"), result.IndexOf("reserved") == 9);
			assertTrue("Escaped tag not preserved: " + result.IndexOf("reserved", 15), result.IndexOf("reserved", 15) == 38);
			assertTrue("Escaped tag not preserved: " + result.IndexOf("reserved", 41), result.IndexOf("reserved", 41) == 54);
			assertTrue("Other tag should be removed", result.IndexOf("other") == -1);
		}

		[TestMethod]
		public void TestMalformedHTML()
		{
			const string test = "a <a hr<ef=aa<a>> </close</a>";
			const string gold = "a <a hr<ef=aa > </close ";
			//					   <aa hhr<<eef=aa > </close<
			var reader = new HTMLStripCharFilter(CharReader.Get(new StringReader(test)));
			var builder = new StringBuilder();
			var ch = 0;
			while ((ch = reader.Read()) != -1)
			{
				builder.Append((char)ch);
			}
			var result = builder.ToString();
			// System.out.println("Resu: " + result + "<EOL>");
			// System.out.println("Gold: " + gold + "<EOL>");
			assertTrue(result + " is not equal to " + gold + "<EOS>", result.Equals(gold));
		}

		[TestMethod]
		public void TestBufferOverflow()
		{
			var testBuilder = new StringBuilder(HTMLStripCharFilter.DEFAULT_READ_AHEAD + 50);
			testBuilder.Append("ah<?> ??????");
			appendChars(testBuilder, HTMLStripCharFilter.DEFAULT_READ_AHEAD + 500);
			processBuffer(testBuilder.ToString(), "Failed on pseudo proc. instr."); //processing instructions

			testBuilder.Length = 0;
			testBuilder.Append("<!--"); //comments
			appendChars(testBuilder, 3 * HTMLStripCharFilter.DEFAULT_READ_AHEAD + 500); //comments have two lookaheads

			testBuilder.Append("-->foo");
			processBuffer(testBuilder.ToString(), "Failed w/ comment");

			testBuilder.Length = 0;
			testBuilder.Append("<?");
			appendChars(testBuilder, HTMLStripCharFilter.DEFAULT_READ_AHEAD + 500);
			testBuilder.Append("?>");
			processBuffer(testBuilder.ToString(), "Failed with proc. instr.");

			testBuilder.Length = 0;
			testBuilder.Append("<b ");
			appendChars(testBuilder, HTMLStripCharFilter.DEFAULT_READ_AHEAD + 500);
			testBuilder.Append("/>");
			processBuffer(testBuilder.ToString(), "Failed on tag");
		}

		private static void appendChars(StringBuilder testBuilder, int numChars)
		{
			var i1 = numChars / 2;
			for (int i = 0; i < i1; i++)
			{
				testBuilder.Append('a').Append(' ');
				//tack on enough to go beyond the mark readahead limit, since <?> makes HTMLStripCharFilter think it is a processing instruction
			}
		}


		private static void processBuffer(String test, String assertMsg)
		{
			// System.out.println("-------------------processBuffer----------");
			var reader = new HTMLStripCharFilter(CharReader.Get(new StringReader(test))); //force the use of BufferedReader
			var builder = new StringBuilder();
			try
			{
				var ch = 0;
				while ((ch = reader.Read()) != -1)
				{
					builder.Append((char)ch);
				}
			}
			finally
			{
				// System.out.println("String (trimmed): " + builder.toString().trim() + "<EOS>");
			}
			Assert.AreEqual(test, builder.ToString(), assertMsg);
		}

		[TestMethod]
		public void TestComment()
		{

			const string test = "<!--- three dashes, still a valid comment ---> ";
			const string gold = "  ";
			var reader = new HTMLStripCharFilter(CharReader.Get(new StringReader(test))); //force the use of BufferedReader
			int ch = 0;
			var builder = new StringBuilder();
			try
			{
				while ((ch = reader.Read()) != -1)
				{
					builder.Append((char)ch);
				}
			}
			finally
			{
				// System.out.println("String: " + builder.toString());
			}
			assertTrue(builder.ToString() + " is not equal to " + gold + "<EOS>", builder.ToString().Equals(gold) == true);
		}


		public void doTestOffsets(String input)
		{
			var reader = new HTMLStripCharFilter(CharReader.Get(new StringReader(input)));
			int ch = 0;
			int off = 0; // offset in the reader
			int strOff = -1; // offset in the original string
			while ((ch = reader.Read()) != -1)
			{
				var correctedOff = reader.CorrectOffset(off);

				if (ch == 'X')
				{
					strOff = input.IndexOf('X', strOff + 1);
					assertEquals(strOff, correctedOff);
				}

				off++;
			}
		}

		[TestMethod]
		public void TestOffsets()
		{
			doTestOffsets("hello X how X are you");
			doTestOffsets("hello <p> X<p> how <p>X are you");
			doTestOffsets("X &amp; X &#40; X &lt; &gt; X");

			// test backtracking
			doTestOffsets("X < &zz >X &# < X > < &l > &g < X");
		}

		[TestMethod]
		public void TestHebrewScenarios()
		{
			const string html = "<div class=\"foo\">בדיקה ראשונה</div> וכאן נוסיף גם <a href=\"#bar\">לינק</a> ועכשיו " +
					"גם <a alt=\"לינק מסובך עם תיאור\" href=\"http://lucene.apache.org/\">לינק מסובך יותר</a>. " +
					" <!-- הערה אחת ויחידה -->";
			const string gold = " בדיקה ראשונה  וכאן נוסיף גם  לינק  ועכשיו " +
			                    "גם  לינק מסובך יותר .   ";
			var reader = new HTMLStripCharFilter(CharReader.Get(new StringReader(html)));
			var builder = new StringBuilder();
			var ch = -1;
			var goldArray = gold.ToCharArray();
			var position = 0;
			while ((ch = reader.Read()) != -1)
			{
				var theChar = (char)ch;
				builder.Append(theChar);
				Assert.IsTrue(theChar == goldArray[position], "\"" + theChar + "\"" + " at position: " + position + " does not equal: \"" + goldArray[position] + "\". Buffer so far: " + builder + "<EOB>");
				position++;
			}
			Assert.AreEqual(gold, builder.ToString());

			doTestOffsets("שלום X מה X שלומך חבר");
		}
	}
}
