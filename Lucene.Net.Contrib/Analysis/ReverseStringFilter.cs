﻿/**
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
using Lucene.Net.Analysis.Tokenattributes;

namespace Lucene.Net.Analysis
{
	/**
	 * Reverse token string, for example "country" => "yrtnuoc".
	* <p>
	* If <code>marker</code> is supplied, then tokens will be also prepended by
	* that character. For example, with a marker of &#x5C;u0001, "country" =>
	* "&#x5C;u0001yrtnuoc". This is useful when implementing efficient leading
	* wildcards search.
	* </p>
	* 
	* @version $Id: ReverseStringFilter.java 805769 2009-08-19 12:07:15Z rmuir $
	*/
	public class ReverseStringFilter : TokenFilter
	{
		private readonly TermAttribute termAtt;
		private readonly char marker;
		private const char NOMARKER = '\uFFFF';

		/**
		 * Example marker character: U+0001 (START OF HEADING) 
		 */
		public static readonly char START_OF_HEADING_MARKER = '\u0001';

		/**
		 * Example marker character: U+001F (INFORMATION SEPARATOR ONE)
		 */
		public static readonly char INFORMATION_SEPARATOR_MARKER = '\u001F';

		/**
		 * Example marker character: U+EC00 (PRIVATE USE AREA: EC00) 
		 */
		public static readonly char PUA_EC00_MARKER = '\uEC00';

		/**
		 * Example marker character: U+200F (RIGHT-TO-LEFT MARK)
		 */
		public static readonly char RTL_DIRECTION_MARKER = '\u200F';

		/**
		 * Create a new ReverseStringFilter that reverses all tokens in the 
		 * supplied {@link TokenStream}.
		 * <p>
		 * The reversed tokens will not be marked. 
		 * </p>
		 * 
		 * @param in {@link TokenStream} to filter
		 */
		public ReverseStringFilter(TokenStream input)
			: base(input)
		{
			this.marker = NOMARKER;
			termAtt = (TermAttribute)AddAttribute(typeof(TermAttribute));
		}

		/**
		 * Create a new ReverseStringFilter that reverses and marks all tokens in the
		 * supplied {@link TokenStream}.
		 * <p>
		 * The reversed tokens will be prepended (marked) by the <code>marker</code>
		 * character.
		 * </p>
		 * 
		 * @param in {@link TokenStream} to filter
		 * @param marker A character used to mark reversed tokens
		 */
		public ReverseStringFilter(TokenStream input, char marker)
			: base(input)
		{
			this.marker = marker;
			termAtt = (TermAttribute)AddAttribute(typeof(TermAttribute));
		}

		public override bool IncrementToken()
		{
			if (input.IncrementToken())
			{
				int len = termAtt.TermLength();
				if (marker != NOMARKER)
				{
					len++;
					termAtt.ResizeTermBuffer(len);
					termAtt.TermBuffer()[len - 1] = marker;
				}
				Reverse(termAtt.TermBuffer(), len);
				termAtt.SetTermLength(len);
				return true;
			}
			else
			{
				return false;
			}

		}

		public static String Reverse(string input)
		{
			char[] charInput = input.ToCharArray();
			Reverse(charInput);
			return new String(charInput);
		}

		public static void Reverse(char[] buffer)
		{
			Reverse(buffer, buffer.Length);
		}

		public static void Reverse(char[] buffer, int len)
		{
			Reverse(buffer, 0, len);
		}

		public static void Reverse(char[] buffer, int start, int len)
		{
			if (len <= 1) return;
			int num = len >> 1;
			for (int i = start; i < (start + num); i++)
			{
				char c = buffer[i];
				buffer[i] = buffer[start * 2 + len - i - 1];
				buffer[start * 2 + len - i - 1] = c;
			}
		}
	}
}
