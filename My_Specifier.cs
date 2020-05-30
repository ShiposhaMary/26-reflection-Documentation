using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;

namespace Documentation
{
    public class Specifier<T> : ISpecifier
    {
        public string GetApiDescription()
        {
          return typeof(T).GetCustomAttributes<ApiDescriptionAttribute>()
                .FirstOrDefault()?.Description;
        }

        public string[] GetApiMethodNames()
        { 
          return typeof(T).GetMethods().Where(x => x.GetCustomAttributes<ApiMethodAttribute>().Any())
                .Select(x => x.Name)
                .ToArray();
        }
        MethodInfo GetMethodInfo(Type type, string methodName)
        {
            
            return type.GetMethods()
                .Where(x => x.Name == methodName)
                .FirstOrDefault();
        }

        ParameterInfo GetParameterInfo(MethodInfo method, string paramName)
        {
            return method.GetParameters()
                .Where(x => x.Name == paramName)
                .FirstOrDefault();
        }
        public string GetApiMethodDescription(string methodName)
        {
            return GetMethodInfo(typeof(T), methodName)?

                .GetCustomAttributes(false)
                .OfType<ApiDescriptionAttribute>()
                .FirstOrDefault()?.Description;
        }

        public string[] GetApiMethodParamNames(string methodName)
        {
            return GetMethodInfo(typeof(T), methodName).GetParameters()
                .Select(x => x.Name)
                .ToArray();
        }

        public string GetApiMethodParamDescription(string methodName, string paramName)
        {
            return GetParameterInfo(GetMethodInfo(typeof(T), methodName), paramName)
                .GetCustomAttributes(false)
                .OfType<ApiDescriptionAttribute>()
                .FirstOrDefault()?.Description;
        }

        public ApiParamDescription GetApiMethodParamFullDescription(string methodName, string paramName)
        {  
            if (GetParameterInfo(GetMethodInfo(typeof(T), methodName), paramName) == null) return null;
            else {
                var fullDiscription = GetParameterInfo(GetMethodInfo(typeof(T), methodName), paramName)
                    .GetCustomAttributes(false);
                return new ApiParamDescription
                {
                    MinValue = fullDiscription?.OfType<ApiIntValidationAttribute>()
                        .FirstOrDefault()?.MinValue,
                    MaxValue = fullDiscription?.OfType<ApiIntValidationAttribute>()
                        .FirstOrDefault()?.MaxValue,
                    ParamDescription = new CommonDescription(paramName, fullDiscription?.OfType<ApiDescriptionAttribute>()
                        .FirstOrDefault()?.Description),
                    Required = fullDiscription?.OfType<ApiRequiredAttribute>().
                        FirstOrDefault() is null ? false
                        : fullDiscription.OfType<ApiRequiredAttribute>().FirstOrDefault().Required
                };
            }
        }

        public ApiMethodDescription GetApiMethodFullDescription(string methodName)
        {
            var methodDiscription = GetApiMethodDescription(methodName);
            var paramNames = GetApiMethodParamNames(methodName);
            var paramFullDiscription = paramNames
                .Select(x => GetApiMethodParamFullDescription(methodName, x))
                .ToArray();

            var apiMethodDiscription = new ApiMethodDescription
            {
                MethodDescription = new CommonDescription(methodName, methodDiscription),
                ParamDescriptions = paramFullDiscription,
                ReturnDescription = GetReturnTypeDescription(GetMethodInfo(typeof(T), methodName))
            };

            return GetMethodInfo(typeof(T), methodName)
                .GetCustomAttributes<ApiMethodAttribute>().Any() ? apiMethodDiscription : null;
        }
        ApiParamDescription GetReturnTypeDescription(MethodInfo mI)
        {
            var requiredAttribute = mI.ReturnTypeCustomAttributes.GetCustomAttributes(false)?
                .OfType<ApiRequiredAttribute>()
                .FirstOrDefault();
            var intValidationAttribut = mI.ReturnTypeCustomAttributes.GetCustomAttributes(false)?.OfType<ApiIntValidationAttribute>().FirstOrDefault();

            return mI.ReturnTypeCustomAttributes.GetCustomAttributes(false).Length == 0 ? null : new ApiParamDescription
            {
                Required = requiredAttribute is null ? false : requiredAttribute.Required,
                MaxValue = intValidationAttribut?.MaxValue,
                MinValue = intValidationAttribut?.MinValue,
                ParamDescription = new CommonDescription()
            };
        }
    }
}