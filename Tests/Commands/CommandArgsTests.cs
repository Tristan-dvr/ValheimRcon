using NUnit.Framework;
using ValheimRcon.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ValheimRcon.Tests.Commands
{
    [TestFixture]
    public class CommandArgsTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_WithValidArgs_ShouldInitializeCorrectly()
        {
            var args = new[] { "command", "arg1", "arg2" };
            var commandArgs = new CommandArgs(args);

            Assert.IsNotNull(commandArgs);
            Assert.AreEqual(3, commandArgs.Arguments.Count);
            Assert.AreEqual("command", commandArgs.Arguments[0]);
            Assert.AreEqual("arg1", commandArgs.Arguments[1]);
            Assert.AreEqual("arg2", commandArgs.Arguments[2]);
        }

        [Test]
        public void Constructor_WithEmptyArgs_ShouldInitializeCorrectly()
        {
            var args = new string[0];

            var commandArgs = new CommandArgs(args);

            Assert.IsNotNull(commandArgs);
            Assert.AreEqual(0, commandArgs.Arguments.Count);
        }

        #endregion

        #region GetString Tests

        [TestCase(0, "command")]
        [TestCase(1, "arg1")]
        [TestCase(2, "arg2")]
        public void GetString_WithValidIndex_ShouldReturnCorrectValue(int index, string expected)
        {
            var args = new[] { "command", "arg1", "arg2" };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.GetString(index);

            Assert.AreEqual(expected, result);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(-1)]
        public void GetString_WithInvalidIndex_ShouldThrowArgumentException(int index)
        {
            var args = new[] { "command" };
            var commandArgs = new CommandArgs(args);

            Assert.Throws<ArgumentException>(() => commandArgs.GetString(index));
        }

        #endregion

        #region TryGetString Tests

        [TestCase(1, "arg1", "default", "arg1")]
        [TestCase(2, "arg2", "default", "arg2")]
        [TestCase(5, "default", "default", "default")]
        [TestCase(-1, "default", "default", "default")]
        public void TryGetString_WithVariousIndexes_ShouldReturnCorrectValue(int index, string argValue, string defaultValue, string expected)
        {
            var args = new[] { "command", "arg1", "arg2" };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.TryGetString(index, defaultValue);

            Assert.AreEqual(expected, result);
        }

        #endregion

        #region GetInt Tests

        [TestCase("123", 123)]
        [TestCase("-456", -456)]
        [TestCase("0", 0)]
        [TestCase("2147483647", int.MaxValue)]
        [TestCase("-2147483648", int.MinValue)]
        public void GetInt_WithValidIntegers_ShouldReturnCorrectValue(string input, int expected)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.GetInt(1);

            Assert.AreEqual(expected, result);
        }

        [TestCase("notanumber")]
        [TestCase("12.34")]
        [TestCase("")]
        [TestCase("abc123")]
        [TestCase("123abc")]
        public void GetInt_WithInvalidStrings_ShouldThrowArgumentException(string input)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            Assert.Throws<ArgumentException>(() => commandArgs.GetInt(1));
        }

        #endregion

        #region TryGetInt Tests

        [TestCase("789", 0, 789)]
        [TestCase("-123", 42, -123)]
        [TestCase("0", 999, 0)]
        public void TryGetInt_WithValidIntegers_ShouldReturnCorrectValue(string input, int defaultValue, int expected)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.TryGetInt(1, defaultValue);

            Assert.AreEqual(expected, result);
        }

        [TestCase(5, 42, 42)]
        [TestCase(-1, 999, 999)]
        public void TryGetInt_WithInvalidIndex_ShouldReturnDefaultValue(int index, int defaultValue, int expected)
        {
            var args = new[] { "command" };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.TryGetInt(index, defaultValue);

            Assert.AreEqual(expected, result);
        }

        #endregion

        #region GetLong Tests

        [TestCase("9223372036854775807", 9223372036854775807L)]
        [TestCase("-9223372036854775808", -9223372036854775808L)]
        [TestCase("0", 0L)]
        [TestCase("123456789", 123456789L)]
        public void GetLong_WithValidLongs_ShouldReturnCorrectValue(string input, long expected)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.GetLong(1);

            Assert.AreEqual(expected, result);
        }

        [TestCase("notanumber")]
        [TestCase("12.34")]
        [TestCase("")]
        [TestCase("abc123")]
        public void GetLong_WithInvalidStrings_ShouldThrowArgumentException(string input)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            Assert.Throws<ArgumentException>(() => commandArgs.GetLong(1));
        }

        #endregion

        #region TryGetLong Tests

        [TestCase("123456789", 0L, 123456789L)]
        [TestCase("-987654321", 999L, -987654321L)]
        [TestCase("0", 42L, 0L)]
        public void TryGetLong_WithValidLongs_ShouldReturnCorrectValue(string input, long defaultValue, long expected)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.TryGetLong(1, defaultValue);

            Assert.AreEqual(expected, result);
        }

        #endregion

        #region GetFloat Tests

        [TestCase("3.14", 3.14f)]
        [TestCase("-2.5", -2.5f)]
        [TestCase("0.0", 0.0f)]
        [TestCase("1.0", 1.0f)]
        [TestCase("123.456", 123.456f)]
        [TestCase("-0.001", -0.001f)]
        public void GetFloat_WithValidFloats_ShouldReturnCorrectValue(string input, float expected)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.GetFloat(1);

            Assert.AreEqual(expected, result, 0.001f);
        }

        [TestCase("notanumber")]
        [TestCase("")]
        [TestCase("abc123")]
        [TestCase("12.34.56")]
        public void GetFloat_WithInvalidStrings_ShouldThrowArgumentException(string input)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            Assert.Throws<ArgumentException>(() => commandArgs.GetFloat(1));
        }

        #endregion

        #region TryGetFloat Tests

        [TestCase("1.23", 0.0f, 1.23f)]
        [TestCase("-5.5", 10.0f, -5.5f)]
        [TestCase("0.0", 42.0f, 0.0f)]
        public void TryGetFloat_WithValidFloats_ShouldReturnCorrectValue(string input, float defaultValue, float expected)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.TryGetFloat(1, defaultValue);

            Assert.AreEqual(expected, result, 0.001f);
        }

        #endregion

        #region GetUInt Tests

        [TestCase("4294967295", 4294967295U)]
        [TestCase("0", 0U)]
        [TestCase("12345", 12345U)]
        [TestCase("1", 1U)]
        public void GetUInt_WithValidUInts_ShouldReturnCorrectValue(string input, uint expected)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.GetUInt(1);

            Assert.AreEqual(expected, result);
        }

        [TestCase("-1")]
        [TestCase("notanumber")]
        [TestCase("")]
        [TestCase("12.34")]
        [TestCase("abc123")]
        public void GetUInt_WithInvalidStrings_ShouldThrowArgumentException(string input)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            Assert.Throws<ArgumentException>(() => commandArgs.GetUInt(1));
        }

        #endregion

        #region TryGetUInt Tests

        [TestCase("12345", 0U, 12345U)]
        [TestCase("999", 42U, 999U)]
        [TestCase("0", 100U, 0U)]
        public void TryGetUInt_WithValidUInts_ShouldReturnCorrectValue(string input, uint defaultValue, uint expected)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.TryGetUInt(1, defaultValue);

            Assert.AreEqual(expected, result);
        }

        #endregion

        #region GetOptionalArguments Tests

        [TestCase(new[] { "command", "arg1", "arg2" }, new int[0])]
        [TestCase(new[] { "command", "-v", "arg1" }, new[] { 1 })]
        [TestCase(new[] { "command", "-v", "arg1", "-f", "arg2", "-x" }, new[] { 1, 3, 5 })]
        [TestCase(new[] { "command", "-v", "arg1", "-F", "arg2", "-xYz" }, new[] { 1, 3, 5 })]
        [TestCase(new[] { "command", "--invalid", "-", "arg1", "-v" }, new[] { 4 })]
        [TestCase(new[] { "command", "-a", "-b", "-c", "arg1" }, new[] { 1, 2, 3 })]
        [TestCase(new[] { "command", "-single", "arg1", "-multiWord" }, new[] { 1, 3 })]
        public void GetOptionalArguments_WithVariousArgs_ShouldReturnCorrectIndices(string[] args, int[] expectedIndices)
        {
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.GetOptionalArguments();

            var indices = result.Select(value => value.Index).ToList();
            Assert.AreEqual(expectedIndices, indices);
        }

        #endregion

        #region ToString Tests

        [TestCase(new[] { "command", "arg1", "arg2", "arg3" }, "command arg1 arg2 arg3")]
        [TestCase(new[] { "command" }, "command")]
        [TestCase(new string[0], "")]
        [TestCase(new[] { "single" }, "single")]
        [TestCase(new[] { "cmd", "-v", "arg1" }, "cmd -v arg1")]
        public void ToString_WithVariousArgs_ShouldReturnSpaceSeparatedString(string[] args, string expected)
        {
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.ToString();

            Assert.AreEqual(expected, result);
        }

        #endregion

        #region GetObjectId Tests

        [TestCase("123:456", 123U, 456L)]
        [TestCase("0:0", 0U, 0L)]
        [TestCase("4294967295:9223372036854775807", 4294967295U, 9223372036854775807L)]
        [TestCase("1:1", 1U, 1L)]
        [TestCase("999:12345678901234", 999U, 12345678901234L)]
        [TestCase("123:-456", 123U, -456L)]
        [TestCase("999:-9223372036854775808", 999U, -9223372036854775808L)]
        public void GetObjectId_WithValidFormat_ShouldReturnCorrectValue(string input, uint expectedId, long expectedUserId)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.GetObjectId(1);

            Assert.AreEqual(expectedId, result.Id);
            Assert.AreEqual(expectedUserId, result.UserId);
        }

        [TestCase("123")]
        [TestCase("123:")]
        [TestCase(":456")]
        [TestCase("")]
        [TestCase("abc:123")]
        [TestCase("123:abc")]
        [TestCase("123:456:789")]
        [TestCase("-1:456")]
        [TestCase("123.5:456")]
        [TestCase("123:456.7")]
        public void GetObjectId_WithInvalidFormat_ShouldThrowArgumentException(string input)
        {
            var args = new[] { "command", input };
            var commandArgs = new CommandArgs(args);

            Assert.Throws<ArgumentException>(() => commandArgs.GetObjectId(1));
        }

        #endregion

        #region GetVector3 Tests

        [TestCase("1.5", "2.5", "3.5", 1.5f, 2.5f, 3.5f)]
        [TestCase("0.0", "0.0", "0.0", 0.0f, 0.0f, 0.0f)]
        [TestCase("-1.5", "-2.5", "-3.5", -1.5f, -2.5f, -3.5f)]
        [TestCase("123.456", "789.012", "345.678", 123.456f, 789.012f, 345.678f)]
        [TestCase("1.0", "2.0", "3.0", 1.0f, 2.0f, 3.0f)]
        public void GetVector3_WithValidFloats_ShouldReturnCorrectValue(string x, string y, string z, float expectedX, float expectedY, float expectedZ)
        {
            var args = new[] { "command", x, y, z };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.GetVector3(1);

            Assert.AreEqual(expectedX, result.x, 0.001f);
            Assert.AreEqual(expectedY, result.y, 0.001f);
            Assert.AreEqual(expectedZ, result.z, 0.001f);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(-1)]
        public void GetVector3_WithInvalidIndex_ShouldThrowArgumentException(int index)
        {
            var args = new[] { "command", "1.0", "2.0" };
            var commandArgs = new CommandArgs(args);

            Assert.Throws<ArgumentException>(() => commandArgs.GetVector3(index));
        }

        [TestCase("notanumber", "2.0", "3.0")]
        [TestCase("1.0", "notanumber", "3.0")]
        [TestCase("1.0", "2.0", "notanumber")]
        public void GetVector3_WithInvalidFloats_ShouldThrowArgumentException(string x, string y, string z)
        {
            var args = new[] { "command", x, y, z };
            var commandArgs = new CommandArgs(args);

            Assert.Throws<ArgumentException>(() => commandArgs.GetVector3(1));
        }

        #endregion

        #region GetVector2i Tests

        [TestCase("10", "20", 10, 20)]
        [TestCase("0", "0", 0, 0)]
        [TestCase("-10", "-20", -10, -20)]
        [TestCase("2147483647", "-2147483648", int.MaxValue, int.MinValue)]
        [TestCase("123", "456", 123, 456)]
        public void GetVector2i_WithValidIntegers_ShouldReturnCorrectValue(string x, string y, int expectedX, int expectedY)
        {
            var args = new[] { "command", x, y };
            var commandArgs = new CommandArgs(args);

            var result = commandArgs.GetVector2i(1);

            Assert.AreEqual(expectedX, result.x);
            Assert.AreEqual(expectedY, result.y);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(-1)]
        public void GetVector2i_WithInvalidIndex_ShouldThrowArgumentException(int index)
        {
            var args = new[] { "command", "10" };
            var commandArgs = new CommandArgs(args);

            Assert.Throws<ArgumentException>(() => commandArgs.GetVector2i(index));
        }

        [TestCase("notanumber", "20")]
        [TestCase("10", "notanumber")]
        [TestCase("12.34", "20")]
        [TestCase("10", "12.34")]
        public void GetVector2i_WithInvalidIntegers_ShouldThrowArgumentException(string x, string y)
        {
            var args = new[] { "command", x, y };
            var commandArgs = new CommandArgs(args);

            Assert.Throws<ArgumentException>(() => commandArgs.GetVector2i(1));
        }

        #endregion
    }
}
