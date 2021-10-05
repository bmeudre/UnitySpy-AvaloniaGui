namespace HackF5.UnitySpy.AvaloniaGui
{
    using Autofac;
    using HackF5.UnitySpy.AvaloniaGui.Mvvm;

    public class GuiAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.AutoRegisterAssemblyTypes(this.ThisAssembly);
            builder.RegisterAssemblyTypes(this.ThisAssembly).Where(t => t.Name.EndsWith("ViewModel"));
            ViewLocator.RegisterAssembly(this.ThisAssembly);
        }
    }
}