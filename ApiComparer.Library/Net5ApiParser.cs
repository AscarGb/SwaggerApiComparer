using System;
using System.Collections.Generic;
using System.Linq;
using ApiComparer.Lib.Models;
using Newtonsoft.Json.Linq;

namespace ApiComparer.Lib
{
    public class Net5ApiParser
    {
        private List<Definition> _definitions;
        private JObject _oldApiObject;
        private List<RequestPath> _paths;

        public static Net5ApiParser Create() => new();

        public RequestPathData Parse(string oldApi)
        {
            _oldApiObject = JObject.Parse(oldApi);
            _paths = new List<RequestPath>();
            _definitions = new List<Definition>();
            ParseDefinitions();

            foreach (var path in _oldApiObject["paths"].Children())
                ParsePath(path);

            var requestPathData = new RequestPathData
            {
                RequestPath = _paths,
                Definitions = _definitions
            };

            return requestPathData;
        }

        private void ParseDefinitions()
        {
            foreach (var definition in _oldApiObject["components"]["schemas"].Children())
            {
                var definitionName = ((JProperty)definition).Name;
                var definitionModel = ParseRefTypeProperties(definition.Children().First(), definitionName);
                _definitions.Add(definitionModel);
            }
        }

        private void ParsePath(JToken path)
        {
            var pathData = new RequestPath();
            _paths.Add(pathData);

            var pathName = ((JProperty)path).Name;
            pathData.Key = pathName;

            var methods = new List<RequestHttpMethod>();
            pathData.Methods = methods;

            foreach (var method in path.Children().SelectMany(a => a.Children()))
                ParseMethod(method, methods);
        }

        private void ParseMethod(JToken method, List<RequestHttpMethod> methods)
        {
            var methodName = ((JProperty)method).Name;

            var methodData = new RequestHttpMethod
            {
                Key = methodName
            };

            methods.Add(methodData);

            var parameters = new List<RequestHttpMethodParameter>();
            methodData.Parameters = parameters;

            var responses = new List<ResponseStatusCode>();
            methodData.Responses = responses;

            foreach (var parameter in method.Children()["parameters"].Children())
                ParseParameter(parameter, parameters);

            ParseRequestBody(method, parameters);

            foreach (var response in method.Children()["responses"].Children())
                ParseResponse(response, responses);
        }

        private void ParseRequestBody(JToken method, List<RequestHttpMethodParameter> parameters)
        {
            if (!method.Children()["requestBody"].Any())
                return;

            var requestBodyType = "";

            if (method.Children()["requestBody"]["content"]["application/json"]["schema"]["$ref"].Any())
            {
                var requestBodyRefTypeName = method.Children()["requestBody"]["content"]["application/json"]["schema"]
                    ["$ref"].First().ToString();

                requestBodyType = GetRefTypeProperties(ParseRefType(requestBodyRefTypeName)).Name;
            }
            else
            {
                requestBodyType = method.Children()["requestBody"]["content"]["application/json"]["schema"]
                    ["type"].First().ToString();
            }

            var parameterData = new RequestHttpMethodParameter
            {
                Key = "request",
                In = "body",
                Required = false,
                ParameterType = requestBodyType
            };

            parameters.Add(parameterData);
        }

        private void ParseResponse(JToken response, List<ResponseStatusCode> responses)
        {
            var key = string.Empty;
            var isArray = false;

            var schema = response.Children()["content"]["application/json"]["schema"];

            if (schema["type"].Any())
            {
                var shemaType = (string)schema["type"]
                    .FirstOrDefault();

                if (shemaType.Equals("array", StringComparison.OrdinalIgnoreCase))
                {
                    isArray = true;

                    key = (string)schema["items"]["$ref"]
                        .FirstOrDefault();
                }
                else
                    key = shemaType;
            }
            else if (schema["$ref"].Any())
                key = (string)schema["$ref"].FirstOrDefault();

            var modelType = ParseRefType(key);

            var responseSchemaModel = new ResponseShemaModel
            {
                IsArray = isArray
            };

            if (modelType != null)
            {
                var modelTypeObject = _oldApiObject["components"]["schemas"]?[modelType];

                if (modelTypeObject != null)
                    responseSchemaModel.Definition = GetRefTypeProperties(modelType);
            }

            var responseData = new ResponseStatusCode
            {
                Code = ((JProperty)response).Name,
                ResponseSchemaModel = responseSchemaModel
            };

            responses.Add(responseData);
        }

        private Definition GetRefTypeProperties(string modelType)
        {
            var definition =
                _definitions.First(a => a.Name.Equals(modelType, StringComparison.Ordinal));

            return definition;
        }

        private Definition ParseRefTypeProperties(JToken modelTypeObject, string modelType)
        {
            var definition =
                _definitions.FirstOrDefault(a => a.Name.Equals(modelType, StringComparison.Ordinal));

            if (definition != null)
                return definition;

            var modelTypeProperties = new Definition();
            var modelProperties = new List<DefinitionProperty>();
            modelTypeProperties.Name = modelType;
            modelTypeProperties.DefinitionType = (string)modelTypeObject["type"];
            modelTypeProperties.DefinitionProperties = modelProperties;

            if (modelTypeObject["properties"] == null)
                return modelTypeProperties;

            foreach (JProperty property in modelTypeObject["properties"])
            {
                var propertyName = property.Name;

                var propertyFormat = property.Children()["format"].FirstOrDefault()?.ToString() ??
                                     string.Empty;

                var propertyType = property.Children()["type"].FirstOrDefault()?.ToString() ??
                                   string.Empty;

                var propertyRefType = property.Children()["$ref"].FirstOrDefault()?.ToString() ??
                                      string.Empty;

                var arrayRefType = string.Empty;

                if (property.Children()["items"].Any())
                    arrayRefType = property.Children()["items"]["$ref"].FirstOrDefault()?.ToString() ??
                                   string.Empty;

                //5
                var nullable = bool.Parse(property.Children()["nullable"].FirstOrDefault()?.ToString() ?? "false");

                modelProperties.Add(new DefinitionProperty
                {
                    PropertyName = propertyName,
                    PropertyFormat = propertyFormat,
                    PropertyType = propertyType,
                    PropertyRefType = ParseRefType(propertyRefType),
                    Nullable = nullable,
                    ArrayRefType = ParseRefType(arrayRefType)
                });
            }

            return modelTypeProperties;
        }

        private string ParseRefType(string key)
        {
            var keyParts = key?.Split("/") ?? Array.Empty<string>();
            var modelType = keyParts.Any() ? keyParts.Last() : key;

            return modelType;
        }

        private void ParseParameter(JToken parameter, List<RequestHttpMethodParameter> parameters)
        {
            var parameterData = new RequestHttpMethodParameter
            {
                Key = (string)parameter["name"],
                In = (string)parameter["in"],
                Required = (bool)(parameter["required"] ?? false),
                ParameterType = (string)(parameter["type"] ?? parameter["schema"]["type"])
            };

            parameters.Add(parameterData);
        }
    }
}