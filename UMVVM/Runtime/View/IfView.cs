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
        var result = false;
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
                                          ConditionType.Equal => Convert.ChangeType(condition.value, value.GetType()).Equals(value),
                                          ConditionType.NotEqual => !Convert.ChangeType(condition.value, value.GetType()).Equals(value),
                                          ConditionType.GreaterThan => Convert.ChangeType(condition.value, value.GetType()) is IComparable comparable && comparable.CompareTo(value) < 0,
                                          ConditionType.GreaterOrEqualThan => Convert.ChangeType(condition.value, value.GetType()) is IComparable comparable && comparable.CompareTo(value) <= 0,
                                          ConditionType.LessThan => Convert.ChangeType(condition.value, value.GetType()) is IComparable comparable && comparable.CompareTo(value) > 0,
                                          ConditionType.LessOrEqualThan => Convert.ChangeType(condition.value, value.GetType()) is IComparable comparable && comparable.CompareTo(value) >= 0,
                                          ConditionType.EqualIgnoreCase => string.Equals(value as string, condition.value, StringComparison.OrdinalIgnoreCase),
                                          ConditionType.NullOrEmpty => string.IsNullOrEmpty(value as string),
                                          ConditionType.NullOrWhiteSpace => string.IsNullOrWhiteSpace(value as string),
                                          ConditionType.IsNull => value == null,
                                          _ => false
                                      };

            result = condition.logicalType switch {
                LogicalType.Or when conditionResult => true,
                LogicalType.And when !conditionResult => false,
                _ => result
            };
        }

        if (result) {
            onTrue?.Invoke();
        }
        else {
            onFalse?.Invoke();
        }
    }
}