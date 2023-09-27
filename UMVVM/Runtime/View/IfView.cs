using System;
using System.Collections;
using System.Collections.Generic;
using Attributes;
using Starter.View;
using UnityEngine;
using UnityEngine.Events;

public class ViewConditionTypeForStringAttribute : Attribute { }

public class ViewConditionTypeForNumericAttribute : Attribute { }

public class IfView : View {
    public enum ConditionType {
        None,
        Equal,
        NotEqual,

        [ViewConditionTypeForNumeric]
        Greater,

        [ViewConditionTypeForNumeric]
        GreaterOrEqual,

        [ViewConditionTypeForNumeric]
        Less,

        [ViewConditionTypeForNumeric]
        LessOrEqual,

        [ViewConditionTypeForString]
        EqualIgnoreCase,

        [ViewConditionTypeForString]
        NullOrEmpty,

        [ViewConditionTypeForString]
        NullOrWhiteSpace,
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
        public LogicalType   logicalType = LogicalType.Or;
        [ViewModelPath(1)]
        public string        path;
        public ConditionType type = ConditionType.None;
        public string        value;
    }

    public async void Start() {
        await viewmodel.InitializeAwaiter();
        UpdateView();
    }

    private void UpdateView() {
        var result = false;
        foreach (var condition in conditions) {
            var value = GetPropertyValue(condition.path);
            var conditionResult = condition.type switch {
                ConditionType.Equal => Convert.ChangeType(condition.value, value.GetType()).Equals(value),
                ConditionType.NotEqual => !Convert.ChangeType(condition.value, value.GetType()).Equals(value),
                ConditionType.Greater => Convert.ChangeType(condition.value, value.GetType()) is IComparable comparable && comparable.CompareTo(value) > 0,
                ConditionType.GreaterOrEqual => Convert.ChangeType(condition.value, value.GetType()) is IComparable comparable && comparable.CompareTo(value) >= 0,
                ConditionType.Less => Convert.ChangeType(condition.value, value.GetType()) is IComparable comparable && comparable.CompareTo(value) < 0,
                ConditionType.LessOrEqual => Convert.ChangeType(condition.value, value.GetType()) is IComparable comparable && comparable.CompareTo(value) <= 0,
                ConditionType.EqualIgnoreCase => string.Equals(value as string, condition.value, StringComparison.OrdinalIgnoreCase),
                ConditionType.NullOrEmpty => string.IsNullOrEmpty(value as string),
                ConditionType.NullOrWhiteSpace => string.IsNullOrWhiteSpace(value as string),
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