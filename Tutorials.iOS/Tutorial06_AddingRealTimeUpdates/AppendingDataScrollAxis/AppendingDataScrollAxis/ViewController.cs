﻿using System;
using UIKit;
using SciChart.iOS.Charting;
using Foundation;


namespace AppendingDataScrollAxis
{
    public partial class ViewController : UIViewController
    {
        private SCIChartSurface _surface;
        private XyDataSeries<Double, Double> _lineDataSeries;
        private XyDataSeries<Double, Double> _scatterDataSeries;


        private SCIFastLineRenderableSeries _lineRenderableSeries;
        private SCIXyScatterRenderableSeries _scatterRenderableSeries;

        // timer, used for updating data
        private NSTimer _timer;
        // phase variable used for data slipping
        private double _phase = 0.0;
        // instance variable for counting the data points in data series
        private int _i = 0;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            _surface = new SCIChartSurface();
            _surface.TranslatesAutoresizingMaskIntoConstraints = true;
            _surface.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            _surface.Frame = this.View.Bounds;

            this.View.AddSubview(_surface);

            _surface.XAxes.Add(new SCINumericAxis() { GrowBy = new SCIDoubleRange(0.1, 0.1) });
            _surface.YAxes.Add(new SCINumericAxis() { GrowBy = new SCIDoubleRange(0.1, 0.1) });

            CreateDataSeries();
            CreateRenderableSeries();
            AddModifiers();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (_timer == null)
            {
                _timer = NSTimer.CreateRepeatingScheduledTimer(0.01, (timer) =>
                {
                    _i++;

                    _lineDataSeries.Append(_i, Math.Sin(_i * 0.1 + _phase));
                    _scatterDataSeries.Append(_i, Math.Cos(_i * 0.1 + _phase));

                    _phase += 0.01;

                    var minIndex = _lineDataSeries.Count - 500;
                    var maxIndex = _lineDataSeries.Count - 1;


                    var max = 10;//_lineDataSeries.XColumn[maxIndex];
                    var min = 0;//_lineDataSeries.XColumn[minIndex];


                    var visibleRange = _surface.XAxes[0].VisibleRange as SCIDoubleRange;
                    var vMin = visibleRange.Min + 1.0;
                    var vMax = visibleRange.Max + 500 * 0.1 + 1.0;

                    // calculating new visible range to simulate the auto scrolling functionality
                    if (vMin < min && vMax > max)
                    {
                        visibleRange.Min = visibleRange.Min + 1.0;
                        visibleRange.Max = visibleRange.Max + 1.0;
                    }
                    _surface.InvalidateElement();
                });
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            _timer.Invalidate();
            _timer = null;
        }

        void CreateDataSeries()
        {
            // Init line data series
            _lineDataSeries = new XyDataSeries<Double, Double>();
            // Naming data series, so its name will be shown in LegendModifier
            _lineDataSeries.SeriesName = "LineSeries";
            for (var i = 0; i < 500; i++)
            {
                _lineDataSeries.Append(i, Math.Sin(i * 0.1));
            }

            // Init scatter data series
            _scatterDataSeries = new XyDataSeries<Double, Double>();
            // Naming data series, so its name will be shown in LegendModifier
            _scatterDataSeries.SeriesName = "ScatterSeries";
            for (var i = 0; i < 500; i++)
            {
                _scatterDataSeries.Append(i, Math.Cos(i * 0.1));
            }

            _i = _lineDataSeries.Count;
        }

        void CreateRenderableSeries()
        {
            _lineRenderableSeries = new SCIFastLineRenderableSeries();
            _lineRenderableSeries.DataSeries = _lineDataSeries;


            _scatterRenderableSeries = new SCIXyScatterRenderableSeries();
            _scatterRenderableSeries.DataSeries = _scatterDataSeries;


            _surface.RenderableSeries.Add(_lineRenderableSeries);
            _surface.RenderableSeries.Add(_scatterRenderableSeries);
        }

        void AddModifiers()
        {
            var xAxisDragmodifier = new SCIXAxisDragModifier();
            xAxisDragmodifier.DragMode = SCIAxisDragMode.Pan;
            xAxisDragmodifier.ClipModeX = SCIClipMode.None;

            var yAxisDragmodifier = new SCIYAxisDragModifier();
            yAxisDragmodifier.DragMode = SCIAxisDragMode.Pan;

            var extendZoomModifier = new SCIZoomExtentsModifier();
            var pinchZoomModifier = new SCIPinchZoomModifier();

            // Adding Rollover and Legend modifiers
            var rolloverModifier = new SCIRolloverModifier();
            var legendCollectionModifier = new SCILegendModifier();

            var groupModifier = new SCIChartModifierCollection();
            groupModifier.Add(xAxisDragmodifier);
            groupModifier.Add(yAxisDragmodifier);
            groupModifier.Add(pinchZoomModifier);
            groupModifier.Add(extendZoomModifier);
            groupModifier.Add(rolloverModifier);
            groupModifier.Add(legendCollectionModifier);

            _surface.ChartModifiers = groupModifier;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.

        }
    }
}