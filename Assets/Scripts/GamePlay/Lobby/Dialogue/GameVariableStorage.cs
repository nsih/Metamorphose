using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

// 런타임 휘발성 VariableStorage
public class GameVariableStorage : VariableStorageBehaviour
{
    private Dictionary<string, bool> _bools = new Dictionary<string, bool>();
    private Dictionary<string, float> _numbers = new Dictionary<string, float>();
    private Dictionary<string, string> _strings = new Dictionary<string, string>();

    private void Awake()
    {
        SetValue("$BossDefeated", false);
        SetValue("$ItemGiven", false);
    }

    public override bool TryGetValue<T>(string variableName, out T result)
    {
        if (typeof(T) == typeof(bool) && _bools.TryGetValue(variableName, out bool boolVal))
        {
            result = (T)(object)boolVal;
            return true;
        }
        if (typeof(T) == typeof(float) && _numbers.TryGetValue(variableName, out float floatVal))
        {
            result = (T)(object)floatVal;
            return true;
        }
        if (typeof(T) == typeof(string) && _strings.TryGetValue(variableName, out string stringVal))
        {
            result = (T)(object)stringVal;
            return true;
        }

        result = default;
        return false;
    }

    public override void SetValue(string variableName, string stringValue)
    {
        _strings[variableName] = stringValue;
    }

    public override void SetValue(string variableName, float floatValue)
    {
        _numbers[variableName] = floatValue;
    }

    public override void SetValue(string variableName, bool boolValue)
    {
        _bools[variableName] = boolValue;
    }

    // Unity 코드에서 플래그 읽기
    public bool GetBool(string variableName, bool defaultValue = false)
    {
        return _bools.TryGetValue(variableName, out bool val) ? val : defaultValue;
    }

    // Unity 코드에서 플래그 쓰기
    public void SetBool(string variableName, bool value)
    {
        _bools[variableName] = value;
        Debug.Log($"flag: {variableName} = {value}");
    }

    public override void Clear()
    {
        _bools.Clear();
        _numbers.Clear();
        _strings.Clear();

        SetValue("$BossDefeated", false);
        SetValue("$ItemGiven", false);

        Debug.Log("variable storage cleared");
    }

    public override bool Contains(string variableName)
    {
        return _bools.ContainsKey(variableName)
            || _numbers.ContainsKey(variableName)
            || _strings.ContainsKey(variableName);
    }

    public override (Dictionary<string, float>, Dictionary<string, string>, Dictionary<string, bool>) GetAllVariables()
    {
        return (_numbers, _strings, _bools);
    }

    public override void SetAllVariables(Dictionary<string, float> floats, Dictionary<string, string> strings, Dictionary<string, bool> bools, bool clear = true)
    {
        if (clear)
        {
            _numbers.Clear();
            _strings.Clear();
            _bools.Clear();
        }

        foreach (var item in floats) _numbers[item.Key] = item.Value;
        foreach (var item in strings) _strings[item.Key] = item.Value;
        foreach (var item in bools) _bools[item.Key] = item.Value;
    }
}