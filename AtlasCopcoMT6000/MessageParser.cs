using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasCopcoMT6000
{
    public class MessageParser
    {
        public class Parameter
        {
            public string ParameterId { get; set; }
            public int Length { get; set; }
            public string DataType { get; set; }
            public string Unit { get; set; }
            public string StepNo { get; set; }
            public string Value { get; set; }
        }
        public static List<Parameter> ParseMessage(string message)
        {
            List<Parameter> parameters = new List<Parameter>();
            int index = 43;


            try
            {
                while (index < message.Length - 1)
                {

                    // ParameterId (5 karakter)
                    string parameterId = message.Substring(index, 5);
                    index += 5;

                    if (index == 1443)
                    {
                    }

                    var a = message.Substring(index, 3);
                    // Length (3 karakter)
                    int length = int.Parse(message.Substring(index, 3));
                    index += 3;

                    // DataType (2 karakter)
                    string dataType = message.Substring(index, 2);
                    index += 2;

                    // Unit (3 karakter)
                    string unit = message.Substring(index, 3);
                    index += 3;

                    // StepNo (4 karakter)
                    string stepNo = message.Substring(index, 4);
                    index += 4;

                    // Value (Length kadar karakter)
                    string value = message.Substring(index, length);
                    index += length;

                    parameters.Add(new Parameter
                    {
                        ParameterId = parameterId,
                        Length = length,
                        DataType = dataType,
                        Unit = unit,
                        StepNo = stepNo,
                        Value = value
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nHata : " + ex.Message);
            }

            return parameters;
        }
        private static (string, int) ExtractValue(string message, int startIndex, int length)
        {
            return (message.Substring(startIndex, length), startIndex + length);
        }

        public static string ConvertToJson(List<Parameter> parameters)
        {
            return JsonConvert.SerializeObject(parameters, Newtonsoft.Json.Formatting.Indented);
        }
    }
}
