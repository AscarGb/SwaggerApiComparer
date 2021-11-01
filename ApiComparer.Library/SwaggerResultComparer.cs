using System;
using System.Collections.Generic;
using System.Linq;
using ApiComparer.Lib.Models;

namespace ApiComparer.Lib
{
    public class SwaggerResultComparer
    {
        private SwaggerResultComparer(bool ignore204Response) => _ignore204Response = ignore204Response;
        private readonly bool _ignore204Response;
        private List<string> _compareDefinitionsResult;
        private List<string> _compareRequestPathResult;
        private RequestPathData _paths48;
        private RequestPathData _paths5;
        public static SwaggerResultComparer Create(bool ignore204Response = false) => new(ignore204Response);

        public CompareResult Compare(RequestPathData paths48, RequestPathData paths5)
        {
            _paths5 = paths5;
            _paths48 = paths48;
            _compareDefinitionsResult = new List<string>();
            _compareRequestPathResult = new List<string>();

            var compareResult = new CompareResult
            {
                CompareDefinitionsResult = _compareDefinitionsResult,
                CompareRequestPathsResult = _compareRequestPathResult
            };

            CompareDefinitions();

            CompareRequestPaths();

            return compareResult;
        }

        private void CompareRequestPaths()
        {
            foreach (var requestPath in _paths48.RequestPath)
            {
                var path5 = _paths5.RequestPath.FirstOrDefault(a =>
                    a.Key.Equals(requestPath.Key, StringComparison.OrdinalIgnoreCase));

                if (path5 == null)
                {
                    _compareRequestPathResult.Add($"Cant find path {requestPath.Key}");

                    continue;
                }

                CompareMethods(requestPath, path5);
            }
        }

        private void CompareMethods(RequestPath requestPath, RequestPath? path5)
        {
            foreach (var method in requestPath.Methods)
            {
                var method5 = path5.Methods
                    .FirstOrDefault(a => a.Key.Equals(method.Key, StringComparison.OrdinalIgnoreCase));

                if (method5 == null)
                {
                    _compareRequestPathResult.Add($"Cant find method for path {requestPath.Key} {method.Key}");

                    continue;
                }

                CompareParameters(requestPath, method, method5);

                CompareResponses(requestPath, method, method5);
            }
        }

        private void CompareResponses(RequestPath requestPath, RequestHttpMethod method, RequestHttpMethod method5)
        {
            foreach (var response in method.Responses)
            {
                if (_ignore204Response && response.Code.Equals("204", StringComparison.Ordinal))
                    continue;

                var response5 =
                    method5.Responses.FirstOrDefault(a =>
                        a.Code.Equals(response.Code, StringComparison.Ordinal));

                if (response5 == null)
                {
                    _compareRequestPathResult.Add(
                        $"Cant find response for path {requestPath.Key} {method.Key} {response.Code}");

                    continue;
                }

                if (response.ResponseSchemaModel.IsArray != response5.ResponseSchemaModel.IsArray)
                    _compareRequestPathResult.Add(
                        $"Response array type different {requestPath.Key} {method.Key} {response.Code} {response.ResponseSchemaModel.IsArray} {response5.ResponseSchemaModel.IsArray}");

                if (response.ResponseSchemaModel.Definition != null && response5.ResponseSchemaModel.Definition == null)
                {
                    _compareRequestPathResult.Add(
                        $"Response empty definition {requestPath.Key} {method.Key} {response.Code}");

                    continue;
                }

                if (response.ResponseSchemaModel.Definition != null)
                    if (!response.ResponseSchemaModel.Definition.Name.Equals(
                        response5.ResponseSchemaModel.Definition.Name,
                        StringComparison.Ordinal))
                        _compareRequestPathResult.Add(
                            $"Response type different {requestPath.Key} {method.Key} {response.Code} Type1: {response.ResponseSchemaModel.Definition.Name} Type2 {response5.ResponseSchemaModel.Definition.Name}");
            }
        }

        private void CompareParameters(RequestPath requestPath, RequestHttpMethod method, RequestHttpMethod method5)
        {
            foreach (var parameter in method.Parameters)
            {
                if (parameter.In.Equals("body", StringComparison.Ordinal))
                {
                    var bodyParameter5 =
                        method5.Parameters
                            .FirstOrDefault(a => a.In.Equals("body", StringComparison.Ordinal));

                    if (bodyParameter5 == null)
                    {
                        _compareRequestPathResult.Add(
                            $"Body parameter missing {requestPath.Key} {method.Key} {parameter.Key}");

                        continue;
                    }

                    if (!string.IsNullOrEmpty(parameter.ParameterType))
                        if (!parameter.ParameterType.Equals(bodyParameter5.ParameterType,
                            StringComparison.OrdinalIgnoreCase))
                            _compareRequestPathResult.Add(
                                $"Types of body parameter are different {requestPath.Key} {method.Key} Name: {parameter.Key} Type1: {parameter.ParameterType} Type2: {bodyParameter5.ParameterType}");

                    continue;
                }

                var parameter5 =
                    method5.Parameters
                        .FirstOrDefault(a => a.Key.Equals(parameter.Key, StringComparison.Ordinal));

                if (parameter5 == null)
                {
                    _compareRequestPathResult.Add(
                        $"Parameter not found {requestPath.Key} {method.Key} {parameter.Key}");

                    continue;
                }

                if (!parameter.Key.Equals(parameter5.Key, StringComparison.OrdinalIgnoreCase))
                    _compareRequestPathResult.Add(
                        $"Names of parameter are different {requestPath.Key} {method.Key} {parameter.Key} {parameter5.Key}");

                if (!string.IsNullOrEmpty(parameter.In))
                    if (!parameter.In.Equals(parameter5.In, StringComparison.OrdinalIgnoreCase))
                        _compareRequestPathResult.Add(
                            $"In of parameter are different {requestPath.Key} {method.Key} {parameter.Key} {parameter.In} {parameter5.In}");

                if (!string.IsNullOrEmpty(parameter.ParameterType))
                    if (!parameter.ParameterType.Equals(parameter5.ParameterType,
                        StringComparison.OrdinalIgnoreCase))
                        _compareRequestPathResult.Add(
                            $"ParameterType different {requestPath.Key} {method.Key} {parameter.Key} {parameter.ParameterType} {parameter5.ParameterType}");
            }
        }

        private void CompareDefinitions()
        {
            foreach (var definition in _paths48.Definitions)
            {
                var def5 = _paths5.Definitions.FirstOrDefault(a =>
                    a.Name.Equals(definition.Name, StringComparison.Ordinal));

                if (def5 == null)
                {
                    _compareDefinitionsResult.Add($"Cant find definition {definition.Name}");

                    continue;
                }

                CompareDefinitionProperties(definition, def5);
            }
        }

        private void CompareDefinitionProperties(Definition definition, Definition def5)
        {
            foreach (var definitionProperty in definition.DefinitionProperties)
            {
                var definitionProperties5 = def5.DefinitionProperties;

                var definitionProperty5 = definitionProperties5.FirstOrDefault(a =>
                    a.PropertyName.Equals(definitionProperty.PropertyName, StringComparison.Ordinal));

                if (definitionProperty5 == null)
                {
                    _compareDefinitionsResult.Add(
                        $"Cant find definition property {definition.Name} {definitionProperty.PropertyName}");

                    continue;
                }

                if (!string.IsNullOrEmpty(definitionProperty.ArrayRefType))
                    if (!definitionProperty.ArrayRefType.Equals(definitionProperty5.ArrayRefType))
                        _compareDefinitionsResult.Add(
                            $"ArrayRefType different of property {definition.Name} {definitionProperty.PropertyName} {definitionProperty.ArrayRefType} {definitionProperty5.ArrayRefType}");

                if (!string.IsNullOrEmpty(definitionProperty.PropertyType))
                    if (!definitionProperty.PropertyType.Equals(definitionProperty5.PropertyType))
                        _compareDefinitionsResult.Add(
                            $"PropertyType different of property {definition.Name} {definitionProperty.PropertyName} {definitionProperty.PropertyType} {definitionProperty5.PropertyType}");

                if (!string.IsNullOrEmpty(definitionProperty.PropertyRefType))
                    if (!definitionProperty.PropertyRefType.Equals(definitionProperty5.PropertyRefType))
                        _compareDefinitionsResult.Add(
                            $"PropertyRefType different of property {definition.Name} {definitionProperty.PropertyName} {definitionProperty.PropertyRefType} {definitionProperty5.PropertyRefType}");

                if (!string.IsNullOrEmpty(definitionProperty.PropertyFormat))
                    if (!definitionProperty.PropertyFormat.Equals(definitionProperty5.PropertyFormat))
                        _compareDefinitionsResult.Add(
                            $"PropertyFormat different of property {definition.Name} {definitionProperty.PropertyName} {definitionProperty.PropertyFormat} {definitionProperty5.PropertyFormat}");
            }
        }
    }
}