﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using Newtonsoft.Json.Converters;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using Windows.Data.Json;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
#endif

namespace Newtonsoft.Json.Tests.Converters
{
  [TestFixture]
  public class JsonValueConverterTests : TestFixtureBase
  {
    [Test]
    public void WriteJson()
    {
      JsonObject o = JsonObject.Parse(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}");

      StringWriter sw = new StringWriter();
      JsonTextWriter writer = new JsonTextWriter(sw);

      JsonValueConverter converter = new JsonValueConverter();
      converter.WriteJson(writer, o, null);

      string json = sw.ToString();

      Assert.AreEqual(@"{""Drives"":[""DVD read/writer"",""500 gigabyte hard drive""],""CPU"":""Intel""}", json);
    }

    [Test]
    public void ReadJson()
    {
      string json = @"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}";

      JsonTextReader writer = new JsonTextReader(new StringReader(json));

      JsonValueConverter converter = new JsonValueConverter();
      JsonObject o = (JsonObject)converter.ReadJson(writer, typeof(JsonObject), null, null);

      Assert.AreEqual(2, o.Count);
      Assert.AreEqual("Intel", o.GetNamedString("CPU"));
      Assert.AreEqual("DVD read/writer", o.GetNamedArray("Drives")[0].GetString());
      Assert.AreEqual("500 gigabyte hard drive", o.GetNamedArray("Drives")[1].GetString());
    }

    [Test]
    public void ReadJsonComments()
    {
      string json = @"{/*comment!*/
  ""CPU"": ""Intel"",/*comment!*/
  ""Drives"": [/*comment!*/
    ""DVD read/writer"",
    /*comment!*/""500 gigabyte hard drive""
  ]/*comment!*/
}";

      JsonTextReader writer = new JsonTextReader(new StringReader(json));

      JsonValueConverter converter = new JsonValueConverter();
      JsonObject o = (JsonObject)converter.ReadJson(writer, typeof(JsonObject), null, null);

      Assert.AreEqual(2, o.Count);
      Assert.AreEqual("Intel", o.GetNamedString("CPU"));
      Assert.AreEqual("DVD read/writer", o.GetNamedArray("Drives")[0].GetString());
      Assert.AreEqual("500 gigabyte hard drive", o.GetNamedArray("Drives")[1].GetString());
    }

    [Test]
    public void ReadJsonNullValue()
    {
      string json = "null";

      JsonTextReader writer = new JsonTextReader(new StringReader(json));

      JsonValueConverter converter = new JsonValueConverter();
      JsonValue v = (JsonValue)converter.ReadJson(writer, typeof(JsonValue), null, null);

      Assert.AreEqual(JsonValueType.Null, v.ValueType);
    }

    [Test]
    public void ReadJsonUnsupportedValue()
    {
      string json = "undefined";

      JsonTextReader writer = new JsonTextReader(new StringReader(json));

      JsonValueConverter converter = new JsonValueConverter();

      ExceptionAssert.Throws<JsonException>("Unexpected or unsupported token: Undefined. Path '', line 1, position 9.",
      () =>
      {
        converter.ReadJson(writer, typeof(JsonValue), null, null);
      });
    }

    [Test]
    public void ReadJsonUnexpectedEndInArray()
    {
      string json = "[";

      JsonTextReader writer = new JsonTextReader(new StringReader(json));

      JsonValueConverter converter = new JsonValueConverter();

      ExceptionAssert.Throws<JsonException>("Unexpected end. Path '', line 1, position 1.",
      () =>
      {
        converter.ReadJson(writer, typeof(JsonValue), null, null);
      });
    }

    [Test]
    public void ReadJsonUnexpectedEndAfterComment()
    {
      string json = "[/*comment!*/";

      JsonTextReader writer = new JsonTextReader(new StringReader(json));

      JsonValueConverter converter = new JsonValueConverter();

      ExceptionAssert.Throws<JsonException>("Unexpected end. Path '', line 1, position 13.",
      () =>
      {
        converter.ReadJson(writer, typeof(JsonValue), null, null);
      });
    }

    [Test]
    public void ReadJsonUnexpectedEndInObject()
    {
      string json = "{'hi':";

      JsonTextReader writer = new JsonTextReader(new StringReader(json));

      JsonValueConverter converter = new JsonValueConverter();

      ExceptionAssert.Throws<JsonException>("Unexpected end. Path 'hi', line 1, position 6.",
      () =>
      {
        converter.ReadJson(writer, typeof(JsonValue), null, null);
      });
    }

    [Test]
    public void ReadJsonBadJsonType()
    {
      string json = "null";

      JsonTextReader writer = new JsonTextReader(new StringReader(json));

      JsonValueConverter converter = new JsonValueConverter();

      ExceptionAssert.Throws<JsonException>("Could not convert 'Windows.Data.Json.JsonValue' to 'Windows.Data.Json.JsonObject'. Path '', line 1, position 4.",
      () =>
      {
        converter.ReadJson(writer, typeof(JsonObject), null, null);
      });
    }

    [Test]
    public void JsonConvertDeserialize()
    {
      string json = @"[
  ""DVD read/writer"",
  ""500 gigabyte hard drive""
]";

      JsonArray a = JsonConvert.DeserializeObject<JsonArray>(json);

      Assert.AreEqual(2, a.Count);
      Assert.AreEqual("DVD read/writer", a[0].GetString());
      Assert.AreEqual("500 gigabyte hard drive", a[1].GetString());
    }

    [Test]
    public void JsonConvertSerialize()
    {
      JsonArray a = JsonArray.Parse(@"[
  ""DVD read/writer"",
  ""500 gigabyte hard drive""
]");

      string json = JsonConvert.SerializeObject(a, Formatting.Indented);

      Assert.AreEqual(@"[
  ""DVD read/writer"",
  ""500 gigabyte hard drive""
]", json);
    }

    //[Test]
    public void DeserializePerformance()
    {
      Stopwatch timer = new Stopwatch();
      timer.Start();
      for (int i = 0; i < 100000; i++)
      {
        JsonObject o = JsonObject.Parse(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}");
      }
      timer.Stop();

      string winrt = timer.Elapsed.TotalSeconds.ToString();

      timer = new Stopwatch();
      timer.Start();
      for (int i = 0; i < 100000; i++)
      {
        JObject o = JObject.Parse(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}");
      }
      timer.Stop();

      string linq = timer.Elapsed.TotalSeconds.ToString();

      timer = new Stopwatch();
      timer.Start();
      for (int i = 0; i < 100000; i++)
      {
        JsonObject o = JsonConvert.DeserializeObject<JsonObject>(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}");
      }
      timer.Stop();

      string jsonnet = timer.Elapsed.TotalSeconds.ToString();

      throw new Exception(string.Format("winrt: {0}, jsonnet: {1}, jsonnet linq: {2}", winrt, jsonnet, linq));
      Console.WriteLine(winrt);
      Console.WriteLine(jsonnet);
    }

    //[Test]
    public void SerializePerformance()
    {
      JsonObject o = JsonObject.Parse(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}");

      JObject o1 = JObject.Parse(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}");

      Stopwatch timer = new Stopwatch();
      timer.Start();
      for (int i = 0; i < 100000; i++)
      {
        o.Stringify();
      }
      timer.Stop();

      string winrt = timer.Elapsed.TotalSeconds.ToString();

      timer.Start();
      for (int i = 0; i < 100000; i++)
      {
        o1.ToString(Formatting.None);
      }
      timer.Stop();

      string linq = timer.Elapsed.TotalSeconds.ToString();

      timer = new Stopwatch();
      timer.Start();
      for (int i = 0; i < 100000; i++)
      {
        JsonConvert.SerializeObject(o);
      }
      timer.Stop();

      string jsonnet = timer.Elapsed.TotalSeconds.ToString();

      throw new Exception(string.Format("winrt: {0}, jsonnet: {1}, jsonnet linq: {2}", winrt, jsonnet, linq));
      Console.WriteLine(winrt);
      Console.WriteLine(jsonnet);
    }
  }
}