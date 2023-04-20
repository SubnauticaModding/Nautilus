using BepInEx.Logging;
using Nautilus.Utility;

namespace Nautilus.Options.Utility;

internal static class Validator
{
    internal static bool ValidateChoiceOrDropdownOption<T>(string id, string label, T[] options, int index)
    {
        if (!ValidateID(id, out string result))
        {
            InternalLogger.Log($"There was an error while trying to add choice option with id: {id}. {result}", LogLevel.Error);
            return false;
        }
        if (!ValidateLabel(label, out result))
        {
            InternalLogger.Log($"There was an error while trying to add choice option with id: {id}. {result}", LogLevel.Error);
            return false;
        }
        if (!ValidateArray<T>(options, index, out result))
        {
            InternalLogger.Log($"There was an error while trying to add choice option with id: {id}. {result}", LogLevel.Error);
            return false;
        }
        return true;
    }

    internal static bool ValidateID(string id, out string result)
    {
        result = ValidateID(id);
        return result == null;
    }
    internal static bool ValidateLabel(string id, out string result)
    {
        result = ValidateLabel(id);
        return result == null;
    }
    internal static bool ValidateArray<T>(T[] array, int index, out string result)
    {
        result = ValidateArray<T>(array, index);
        return result == null;
    }

    private static string ValidateID(string id)
    {
        if (id == null)
        {
            return "The provided ID is null.";
        }

        if (string.IsNullOrEmpty(id.Trim()))
        {
            return "The provided ID is empty or whitespace.";
        }

        return null;
    }
    private static string ValidateLabel(string label)
    {
        if (label == null)
        {
            return "The provided label is null.";
        }

        if (string.IsNullOrEmpty(label.Trim()))
        {
            return "The provided label is empty or whitespace.";
        }

        return null;
    }
    private static string ValidateArray<T>(T[] array, int index)
    {
        if (array == null)
        {
            return "The options array is null!";
        }

        if (array.Length <= 0)
        {
            return "The options array is empty!";
        }

        if (index <= -1)
        {
            return $"The provided index ({index}) is negative.";
        }

        if (index >= array.Length)
        {
            return $"The provided index ({index}) is outside of the array.";
        }

        return null;
    }
}