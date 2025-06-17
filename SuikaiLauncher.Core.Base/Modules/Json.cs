#pragma warning disable CS8604

using SuikaiLauncher.Core.Override;
using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
namespace SuikaiLauncher.Core
{

    public class Json
    {
        public static T Deserialize <T>(string? jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
                throw new ArgumentNullException(nameof(jsonText), "参数不能为 null 或空");

            try
            {
                var options = new JsonSerializerOptions
                {
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                };
                return JsonSerializer.Deserialize<T>(jsonText,options)
                       ?? throw new JsonException($"无法转换为 {typeof(T).Name}");
            }
            catch (Exception ex)
            {
                throw new InvalidCastException("反序列化 Json 失败",ex);
            }
        }
        public static string Serialize<T>(T obj)
        {
            if (obj is null) throw new ArgumentNullException("参数不能为 null 或空");
                try {
                    return JsonSerializer.Serialize(obj);
                }
                catch(Exception ex)
                {
                throw new InvalidCastException("序列化 Json 失败", ex);
                }
            }
    }
}
/*
public class Runtime
{
    /// <summary>
    /// 为某个类动态附加属性
    /// </summary>
    /// <typeparam name="T">指定附加类实例的类型</typeparam>
    /// <typeparam name="TValue">附加属性值的类型</typeparam>
    /// <param name="propertyName">属性名称</param>
    public static Action<T, TValue> CreatePropertySetter<T, TValue>(string PropertyName)
    {
        var ObjParam = Expression.Parameter(typeof(T));
        var ValueParam = Expression.Parameter(typeof(TValue));

        var Property = Expression.Property(ObjParam, PropertyName);
        var Assignment = Expression.Assign(Property, ValueParam);

        return Expression.Lambda<Action<T, TValue>>(Assignment, ObjParam, ValueParam).Compile();
    }
}
*/



// 参考 https://learn.microsoft.com/zh-cn/dotnet/standard/serialization/system-text-json/converters-how-to

public sealed class DynamicConverter:JsonConverter<ExpandoObject>
{
    public DynamicJsonObject() { }
    public override ExpandoObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        using (JsonDocument DocumentObj = JsonDocument.ParseValue(ref reader))
        {
            JsonElement RootElement = DocumentObj.RootElement;
            
        }
    }
    public ExpandoObject? Read(JsonElement RootElement,JsonSerializerOptions Options)
    {
        ExpandoObject Obj = new();
        IDictionary<string,object> Dict = Obj;
        foreach (JsonProperty Property in RootElement.EnumerateObject())
        {
            Dict[Property.Name] = Property.Value;
        }
    }
}


// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// 不知道 o4-mini 到底从哪找来的这段源码，据它所说是 .NET 8 标准库里面的
// 我找了半天没找到来源，但是这个 Converter 确实是能用的
// .NET 标准库的源码以 MIT 许可证开源，所以先把它的许可丢在这里，等后面找到真的来源了再说

// ReSharper disable All
#nullable disable

public sealed class ExpandoObjectConverter : JsonConverter<ExpandoObject>
{
    public ExpandoObjectConverter() { }

    public override ExpandoObject Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        using (JsonDocument document = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = document.RootElement;
            return Read(root, options);
        }
    }

    public static ExpandoObject Read(
        JsonElement element,
        JsonSerializerOptions options)
    {
        ExpandoObject expandoObject = new ExpandoObject();
        IDictionary<string, object> dict = expandoObject;
        foreach (JsonProperty property in element.EnumerateObject())
        {
            object value = JsonSerializer.Deserialize<object>(
                property.Value.GetRawText(), options);
            dict.Add(property.Name, value);
        }
        return expandoObject;
    }

    public override void Write(
        Utf8JsonWriter writer,
        ExpandoObject value,
        JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        foreach (KeyValuePair<string, object> kvp in value)
        {
            writer.WritePropertyName(kvp.Key);
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }

        writer.WriteEndObject();
    }
}


