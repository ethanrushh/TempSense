using System;
using Avalonia.Controls;
using EthanRushbrook.TempSense.Contracts;

namespace EthanRushbrook.TempSense.Components.Widgets;

public static class WidgetConstructor
{
    public static WidgetTemplate ConstructWidget(WidgetDefinition definition)
    {
        Control widget = definition.DisplayType switch
        {
            WidgetDisplayType.Fluid => new FluidWidget
            {
                [FluidWidget.CaptionProperty] = definition.Caption,
                [WidgetBase.ValueProperty] = double.Parse(definition.InitialValue)
            },
            WidgetDisplayType.Readout => new ReadoutWidget
            {
                [ReadoutWidget.CaptionProperty] = definition.Caption,
                [WidgetBase.ValueProperty] = definition.InitialValue,
                [WidgetBase.HeaderProperty] = definition.Header
            },
            WidgetDisplayType.Round => new CircleWidget
            {
                [CircleWidget.CaptionProperty] = definition.Caption,
                [WidgetBase.ValueProperty] = double.Parse(definition.InitialValue),
                [WidgetBase.HeaderProperty] = definition.Header
            },
            _ => throw new Exception("Unsupported widget type")
        };

        return new WidgetTemplate
        {
            [WidgetTemplate.WidgetProperty] = widget
        };
    }
}
