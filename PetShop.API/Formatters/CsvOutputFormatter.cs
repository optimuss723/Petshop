using System.Collections;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace PetShop.API.Formatters;

/// <summary>
/// Custom Media Formatter: Xuất dữ liệu định dạng CSV khi client gửi header Accept: text/csv
/// Đáp ứng yêu cầu bài toán: "Có Media Formatter, Content Negotiation"
/// </summary>
public class CsvOutputFormatter : TextOutputFormatter
{
    public CsvOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    protected override bool CanWriteType(Type? type)
    {
        return type != null;
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var response = context.HttpContext.Response;
        var buffer = new StringBuilder();

        var value = context.Object;
        if (value == null) return;

        // Xử lý unwrapping nếu đối tượng được bọc trong ApiResponse<T>
        var valType = value.GetType();
        if (valType.IsGenericType && valType.Name.StartsWith("ApiResponse"))
        {
            var dataProp = valType.GetProperty("Data");
            if (dataProp != null)
            {
                value = dataProp.GetValue(value);
            }
        }

        if (value == null) return;

        if (value is IEnumerable list && value is not string)
        {
            bool headerWritten = false;
            foreach (var item in list)
            {
                if (item == null) continue;
                var props = item.GetType().GetProperties()
                    .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                    .ToArray();

                if (!headerWritten)
                {
                    buffer.AppendLine(string.Join(",", props.Select(p => EscapeCsv(p.Name))));
                    headerWritten = true;
                }

                var values = props.Select(p => EscapeCsv(p.GetValue(item)?.ToString() ?? ""));
                buffer.AppendLine(string.Join(",", values));
            }
        }
        else
        {
            var props = value.GetType().GetProperties()
                .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                .ToArray();

            buffer.AppendLine(string.Join(",", props.Select(p => EscapeCsv(p.Name))));
            var values = props.Select(p => EscapeCsv(p.GetValue(value)?.ToString() ?? ""));
            buffer.AppendLine(string.Join(",", values));
        }

        await response.WriteAsync(buffer.ToString(), selectedEncoding);
    }

    private static bool IsSimpleType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsPrimitive 
            || underlying.IsEnum 
            || underlying == typeof(string) 
            || underlying == typeof(decimal) 
            || underlying == typeof(DateTime) 
            || underlying == typeof(Guid);
    }

    private static string EscapeCsv(string text)
    {
        if (string.IsNullOrEmpty(text)) return "\"\"";
        if (text.Contains(",") || text.Contains("\"") || text.Contains("\n") || text.Contains("\r"))
        {
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }
        return text;
    }
}

