namespace HackF5.UnitySpy.AvaloniaGui.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.AvaloniaGui.Mvvm;
    using JetBrains.Annotations;

    public class ManagedObjectInstanceContentViewModel : PropertyChangedBase
    {
        private readonly IManagedObjectInstance instance;

        public ManagedObjectInstanceContentViewModel(
            [NotNull] IManagedObjectInstance instance)
        {
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));
            this.InstanceFields = this.instance.TypeDefinition.Fields
                .Where(f => !f.TypeInfo.IsStatic && !f.TypeInfo.IsConstant)
                .Select(f => new InstanceFieldViewModel(f, instance))
                .ToArray();
        }

        public delegate ManagedObjectInstanceContentViewModel Factory(IManagedObjectInstance instance);

        public event EventHandler<AppendToTrailEventArgs> AppendToTrail;

        public IEnumerable<InstanceFieldViewModel> InstanceFields { get; }

        public virtual void OnAppendToTrail(string value)
        {
            this.AppendToTrail?.Invoke(this, new AppendToTrailEventArgs(value));
        }
    }
}