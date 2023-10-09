using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using Starter.View;
using UnityEngine;
using UnityEngine.Events;

public class ViewConditionTypeForStringAttribute : Attribute { }

public class ViewConditionTypeForNumericAttribute : Attribute { }

public class ViewConditionTypeForObject : Attribute { }

public class IfView : View {
    public enum ConditionType {
        None,
        Equal,
        NotEqual,

        [ViewConditionTypeForNumeric]
        GreaterThan,

        [ViewConditionTypeForNumeric]
        GreaterOrEqualThan,

        [ViewConditionTypeForNumeric]
        LessThan,

        [ViewConditionTypeForNumeric]
        LessOrEqualThan,

        [ViewConditionTypeForString]
        EqualIgnoreCase,

        [ViewConditionTypeForString]
        NullOrEmpty,

        [ViewConditionTypeForString]
        NullOrWhiteSpace,

        [ViewConditionTypeForObject]
        IsNull,
    }

    public enum LogicalType {
        Or,
        And,
    }

    public List<Condition> conditions;
    public UnityEvent      onTrue;
    public UnityEvent      onFalse;

    [Serializable]
    public class Condition {
        public LogicalType logicalType = LogicalType.Or;

        [ViewModelPath]
        public string path;

        public ConditionType type = ConditionType.None;
        public string        value;
    }

    public async void Start() {
        await WaitViewModelInitialized();
        UpdateView();
    }

    protected override void OnPathRegistration() {
        foreach (var condition in conditions) {
            RegistePath(condition.path);
        }
    }

    public override void OnPropertyChanged(string propertyName) {
        UpdateView();
    }

    private void UpdateView() {
        bool? result = null;
        foreach (var condition in conditions) {
            var value = GetPropertyValue(condition.path);
            var conditionResult = value is null
                                      ? condition.type switch {
                                          ConditionType.Equal => string.IsNullOrWhiteSpace(condition.value) || condition.value.Equals("null", StringComparison.OrdinalIgnoreCase),
                                          ConditionType.NotEqual => !string.IsNullOrWhiteSpace(condition.value) && !condition.value.Equals("null", StringComparison.OrdinalIgnoreCase),
                                          ConditionType.IsNull => true,
                                          ConditionType.NullOrEmpty => true,
                                          ConditionType.NullOrWhiteSpace => true,
                                          _ => false,
                                      }
                                      : condition.type switch {
                                          ConditionType.Equal => ChangeType(condition, value).Equals(value),
                                          ConditionType.NotEqual => !ChangeType(condition, value).Equals(value),
                                          ConditionType.GreaterThan => ChangeType(condition, value) is IComparable comparable && comparable.CompareTo(value) < 0,
                                          ConditionType.GreaterOrEqualThan => ChangeType(condition, value) is IComparable comparable && comparable.CompareTo(value) <= 0,
                                          ConditionType.LessThan => ChangeType(condition, value) is IComparable comparable && comparable.CompareTo(value) > 0,
                                          ConditionType.LessOrEqualThan => ChangeType(condition, value) is IComparable comparable && comparable.CompareTo(value) >= 0,
                                          ConditionType.EqualIgnoreCase => string.Equals(value as string, condition.value, StringComparison.OrdinalIgnoreCase),
                                          ConditionType.NullOrEmpty => string.IsNullOrEmpty(value as string),
                                          ConditionType.NullOrWhiteSpace => string.IsNullOrWhiteSpace(value as string),
                                          ConditionType.IsNull => value == null,
                                          _ => false
                                      };

            if (result is null) {
                result = conditionResult;
                continue;
            }
            
            result = condition.logicalType switch {
                LogicalType.Or when conditionResult => true,
                LogicalType.And when !conditionResult => false,
                _ => result
            };
        }

        if (result ?? false) {
            onTrue?.Invoke();
        }
        else {
            onFalse?.Invoke();
        }
    }

    private static object ChangeType(Condition condition, object value) {
        var conversionType = value.GetType();
        if (conversionType.IsEnum)
            return Enum.TryParse(conversionType, condition.value, true, out var enumValue) ? enumValue : null;
        return Convert.ChangeType(condition.value, conversionType);
    }
}