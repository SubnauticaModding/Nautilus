using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Nautilus.Extensions;

namespace Nautilus.Commands;

internal struct Parameter
{
    [Flags]
    public enum ValidationError
    {
        Valid = 0,
        UnsupportedType = 1,
        ArrayNotParams = 2,
    }
    private static Dictionary<Type, Func<string, object>> _typeConverters = new()
    {
        [typeof(string)] = (s) => s,
        [typeof(bool)] = (s) => bool.Parse(s),
        [typeof(int)] = (s) => int.Parse(s, CultureInfo.InvariantCulture.NumberFormat),
        [typeof(float)] = (s) => float.Parse(s, CultureInfo.InvariantCulture.NumberFormat),
        [typeof(double)] = (s) => double.Parse(s, CultureInfo.InvariantCulture.NumberFormat)
    };

    public static IEnumerable<Type> SupportedTypes => _typeConverters.Keys;

    public Type ParameterType { get; }
    public Type UnderlyingValueType { get; }
    public bool IsOptional { get; }
    public string Name { get; }
    public ValidationError ValidState { get; }

    public Parameter(ParameterInfo parameter)
    {
        ParameterType = parameter.ParameterType;
        UnderlyingValueType = ParameterType.GetUnderlyingType();
        IsOptional = parameter.IsOptional || ParameterType.IsArray;
        Name = parameter.Name;
        ValidState = ValidateParameter(parameter);
    }

    private readonly ValidationError ValidateParameter(ParameterInfo paramInfo)
    {
        ValidationError valid = ValidationError.Valid;
        // arrays MUST be a "params T[]" parameter
        // this enforces them being last *and* only having one
        if (ParameterType.IsArray && !paramInfo.IsDefined(typeof(ParamArrayAttribute), false))
            valid |= ValidationError.ArrayNotParams;
        if (!_typeConverters.ContainsKey(UnderlyingValueType))
            valid |= ValidationError.UnsupportedType;

        return valid;
    }

    public object Parse(string input)
    {
        Type paramType = ParameterType;
        if (paramType.TryUnwrapArrayType(out Type elementType))
            paramType = elementType;
        if (paramType.TryUnwrapNullableType(out _))
        {
            if (string.Equals(input, "null", StringComparison.OrdinalIgnoreCase))
                return null;
        }

        return _typeConverters[UnderlyingValueType](input);
    }
}