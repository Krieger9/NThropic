﻿using Newtonsoft.Json;
using NJsonSchema.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClaudeApi.Agents.Orchestrations
{
    [JsonSchema("WorkItem")]
    [Description("Represents a work item with a goal and steps to achieve it.")]
    public class WorkItem : INotifyPropertyChanged
    {
        public static readonly string EmptyAssignment = "No assignment yet. Provide any additional input on the topic so far, if any or be social.";
        private static readonly string _defaultGoal = "Complete the assignment.";

        private string _goal = _defaultGoal;
        private string _assignment = EmptyAssignment;

        private readonly BehaviorSubject<List<string>> _stepsSubject = new(new List<string>());

        [Description("The goal of the work item. It can be used to measure success or trigger completion.")]
        public string Goal
        {
            get => _goal;
            set
            {
                if (_goal != value)
                {
                    _goal = value;
                    OnPropertyChanged();
                }
            }
        }

        [Description("An observable list of steps required to achieve the goal.")]
        public IObservable<List<string>> Steps => _stepsSubject.AsObservable();

        public event PropertyChangedEventHandler? PropertyChanged;

        public void AddStep(string step)
        {
            var steps = _stepsSubject.Value;
            steps.Add(step);
            _stepsSubject.OnNext(steps);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
