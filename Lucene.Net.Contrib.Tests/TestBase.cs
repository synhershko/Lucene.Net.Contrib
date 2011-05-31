using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucene.Net.Contrib.Tests
{
	public abstract class TestBase
	{
		protected static void assertEquals(int strOff, int correctedOff)
		{
			Assert.AreEqual(strOff, correctedOff);
		}

		protected static void assertTrue(string p0, bool p1)
		{
			Assert.IsTrue(p1, p0);
		}

		protected static FileStream GetTestFile(string fileName)
		{
			var fullPath = System.Reflection.Assembly.GetAssembly(typeof(TestBase)).Location;
			var testsDirectory = Path.GetDirectoryName(fullPath) ?? string.Empty;
			//return new FileStream(Path.Combine(testsDirectory, @"..\TestFiles\", fileName), FileMode.Open);
			return new FileStream(Path.Combine(testsDirectory, @"z:\Projects\Lucene.Net.Contrib\Lucene.Net.Contrib\TestFiles\", fileName), FileMode.Open);
		}
	}
}
