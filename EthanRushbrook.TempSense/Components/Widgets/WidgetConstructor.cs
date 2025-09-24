using System;
using Avalonia.Controls;
using EthanRushbrook.TempSense.Contracts;

namespace EthanRushbrook.TempSense.Components.Widgets;

public static class WidgetConstructor
{
    public static WidgetTemplate ConstructWidget(WidgetDefinition definition)
    {
        var template = new WidgetTemplate();
        
        Control widget = definition.DisplayType switch
        {
            WidgetDisplayType.Fluid => new FluidWidget(definition.Id)
            {
                Caption = definition.Caption,
                Value = double.Parse(definition.InitialValue)
            },
            WidgetDisplayType.Readout => new ReadoutWidget(definition.Id)
            {
                Caption = definition.Caption,
                Value = definition.InitialValue,
                Header = definition.Header
            },
            WidgetDisplayType.Round => new CircleWidget(definition.Id)
            {
                Caption = definition.Caption,
                Value = double.Parse(definition.InitialValue),
                Header = definition.Header
            },
            _ => throw new Exception("Unsupported widget type")
        };

        template.Widget = widget;

        return template;
    }
}
