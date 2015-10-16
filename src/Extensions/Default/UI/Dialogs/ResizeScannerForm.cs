﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="ResizeScannerForm.cs" company="Intel Corporation">
//
// Copyright (c) 2013-2015 Intel Corporation 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;
using System.Windows.Forms;
using ACAT.Lib.Core.PanelManagement;
using ACAT.Lib.Core.Utility;
using ACAT.Lib.Core.WidgetManagement;
using ACAT.Lib.Extension;

#region SupressStyleCopWarnings

[module: SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1126:PrefixCallsCorrectly",
        Scope = "namespace",
        Justification = "Not needed. ACAT naming conventions takes care of this")]
[module: SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1101:PrefixLocalCallsWithThis",
        Scope = "namespace",
        Justification = "Not needed. ACAT naming conventions takes care of this")]
[module: SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1121:UseBuiltInTypeAlias",
        Scope = "namespace",
        Justification = "Since they are just aliases, it doesn't really matter")]
[module: SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1200:UsingDirectivesMustBePlacedWithinNamespace",
        Scope = "namespace",
        Justification = "ACAT guidelines")]
[module: SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1309:FieldNamesMustNotBeginWithUnderscore",
        Scope = "namespace",
        Justification = "ACAT guidelines. Private fields begin with an underscore")]
[module: SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1300:ElementMustBeginWithUpperCaseLetter",
        Scope = "namespace",
        Justification = "ACAT guidelines. Private/Protected methods begin with lowercase")]

#endregion SupressStyleCopWarnings

namespace ACAT.Extensions.Default.UI.Dialogs
{
    /// <summary>
    /// Dialog that lets the user resize and reposition the
    /// scanner. Has buttons to zoom in, zoom out The user can
    /// make the scanner bigger or smaller.  The fonts on the
    /// scanner scale proportionately. The user can then save
    /// the size of the scanner as default.
    /// User can also set the default position of the scanner
    /// in the display.
    /// This dialog displays in the center of the monitor
    /// and the alphabet scanner is displayed so the user can
    /// resize/reposition it.
    /// </summary>
    [DescriptorAttribute("7310C2B3-94DD-4356-824E-8D475832CE34",
                        "ResizeScannerForm",
                        "Resize Scanner Dialog")]
    public partial class ResizeScannerForm : Form, IDialogPanel
    {
        /// <summary>
        /// The DialogCommon object
        /// </summary>
        private readonly DialogCommon _dialogCommon;

        /// <summary>
        /// Scanner position before it is moved, so it can
        /// be restored later if reqd.
        /// </summary>
        private readonly Windows.WindowPosition _initialWindowPosition;

        /// <summary>
        /// Did the user change anything?
        /// </summary>
        private bool _isDirty;

        /// <summary>
        /// The scanner form that will be used to visually show
        /// the size as the user resizes it.
        /// </summary>
        private Form _previewScanner;

        /// <summary>
        /// The interface of the scanner form that will be used to visually show
        /// the size as the user resizes it.
        /// </summary>
        private IScannerPreview _previewScreenInterface;

        /// <summary>
        /// Watchdog object that makes sure that the scanner
        /// is not overlapped by another window
        /// </summary>
        private WindowOverlapWatchdog _windowOverlapWatchdog;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ResizeScannerForm()
        {
            InitializeComponent();

            _dialogCommon = new DialogCommon(this);

            _initialWindowPosition = Context.AppWindowPosition;

            _dialogCommon.AutoDockScanner = false;

            if (!_dialogCommon.Initialize())
            {
                Log.Debug("Initialization error");
            }

            Load += ResizeScannerScreen_Load;
            FormClosing += ResizeScannerScreen_FormClosing;

            subscribeToButtonEvents();
        }

        /// <summary>
        /// Gets the descriptor for this class
        /// </summary>
        public IDescriptor Descriptor
        {
            get { return DescriptorAttribute.GetDescriptor(GetType()); }
        }

        /// <summary>
        /// Gets the synchronization object for this scanner
        /// </summary>
        public SyncLock SyncObj
        {
            get { return _dialogCommon.SyncObj; }
        }

        /// <summary>
        /// Sets the form style
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                var createParams = base.CreateParams;
                createParams.ExStyle |= Windows.WindowStyleFlags.WS_EX_NOACTIVATE;
                return createParams;
            }
        }

        /// <summary>
        /// Triggered when a widget is actuated.
        /// </summary>
        /// <param name="widget">Which one triggered?</param>
        public void OnButtonActuated(Widget widget)
        {
            Log.Debug("**Actuate** " + widget.UIControl.Name + " Value: " + widget.Value);

            var value = widget.Value;

            if (String.IsNullOrEmpty(value))
            {
                Log.Debug("OnButtonActuated() -- received actuation from empty widget!");
                return;
            }

            Invoke(new MethodInvoker(delegate()
            {
                switch (value)
                {
                    case "goBack":
                        onBack();
                        break;

                    case "ScannerZoomOut":
                        onZoomOut();
                        break;

                    case "ScannerZoomIn":
                        onZoomIn();
                        break;

                    case "ScannerRestoreDefaults":
                        onRestoreDefaults();
                        break;

                    case "ScannerMove":
                        onRepositionScanner();
                        break;

                    default:
                        Log.Debug("OnButtonActuated() -- unhandled widget actuation!");
                        break;
                }
            }));
        }

        /// <summary>
        /// Pauses the scanner
        /// </summary>
        public void OnPause()
        {
            _windowOverlapWatchdog.Pause();
            _dialogCommon.OnPause();
        }

        /// <summary>
        /// Resumes paused scanner
        /// </summary>
        public void OnResume()
        {
            _windowOverlapWatchdog.Resume();

            _dialogCommon.OnResume();
        }

        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="command"></param>
        /// <param name="handled"></param>
        public void OnRunCommand(string command, ref bool handled)
        {
            switch (command)
            {
                default:
                    handled = false;
                    break;
            }
        }

        /// <summary>
        /// Releases resources
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _dialogCommon.OnFormClosing(e);
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Window proc
        /// </summary>
        /// <param name="m"></param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        protected override void WndProc(ref Message m)
        {
            _dialogCommon.HandleWndProc(m);
            base.WndProc(ref m);
        }

        /// <summary>
        /// User wants to quit. Confirms and exits
        /// </summary>
        private void onBack()
        {
            Log.Debug("chose back button.");

            if (_isDirty)
            {
                if (DialogUtils.Confirm(this, "Save settings?"))
                {
                    _previewScreenInterface.SaveSettings();
                }
                else
                {
                    Context.AppWindowPosition = _initialWindowPosition;
                }
            }

            Windows.CloseForm(this);
        }

        /// <summary>
        /// Auto positions the scanner at the pre-defined
        /// locations on the display and lets
        /// the user select the default position
        /// </summary>
        private void onRepositionScanner()
        {
            _isDirty = true;

            _dialogCommon.GetAnimationManager().Interrupt();
            Windows.SetOpacity(this, 0.0f);

            var scanner = (_previewScanner as IScannerPanel);
            if (scanner != null)
            {
                scanner.ScannerCommon.PositionSizeController.EvtAutoRepositionScannerStop +=
                                            PositionSizeControllerOnEvtAutoRepositionScannerStop;
                scanner.ScannerCommon.PositionSizeController.AutoRepositionScannerStart();
            }
        }

        /// <summary>
        /// Restores default position and size of the scanner
        /// </summary>
        private void onRestoreDefaults()
        {
            if (DialogUtils.Confirm("Restore default settings?"))
            {
                _previewScreenInterface.RestoreDefaults();
                _isDirty = true;
            }
        }

        /// <summary>
        /// Increases the size of the scanner (zoom in)
        /// </summary>
        private void onZoomIn()
        {
            _previewScreenInterface.ScaleUp();
            _isDirty = true;
        }

        /// <summary>
        /// Decreases the size of the scanner (zoom out)
        /// </summary>
        private void onZoomOut()
        {
            _previewScreenInterface.ScaleDown();
            _isDirty = true;
        }

        /// <summary>
        /// Event handler for when the scanner stops moving
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="eventArgs">event args</param>
        private void PositionSizeControllerOnEvtAutoRepositionScannerStop(object sender, EventArgs eventArgs)
        {
            _dialogCommon.GetAnimationManager().Resume();
            var scanner = (_previewScanner as IScannerPanel);
            if (scanner != null)
            {
                scanner.ScannerCommon.PositionSizeController.ManualPosition = Context.AppWindowPosition;

                scanner.ScannerCommon.PositionSizeController.EvtAutoRepositionScannerStop -=
                    PositionSizeControllerOnEvtAutoRepositionScannerStop;
            }

            Windows.SetOpacity(this, 1.0f);
        }

        /// <summary>
        /// Form is closing. Releases resources
        /// </summary>
        private void ResizeScannerScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            unsubscribeToButtonEvents();

            if (_windowOverlapWatchdog != null)
            {
                _windowOverlapWatchdog.Dispose();
            }

            _previewScanner.Close();
            _dialogCommon.OnClosing();
        }

        /// <summary>
        /// Form has been loaded. Initializes resources
        /// </summary>
        private void ResizeScannerScreen_Load(object sender, EventArgs e)
        {
            var panel = Context.AppPanelManager.CreatePanel(PanelClasses.Alphabet) as IPanel;
            _previewScanner = panel as Form;

            _previewScreenInterface = panel as IScannerPreview;
            _previewScreenInterface.PreviewMode = true;

            var scannerPanel = panel as IScannerPanel;

            // we are going to manually set the position of the scanner below
            scannerPanel.ScannerCommon.PositionSizeController.AutoPosition = false;

            _previewScanner.Show();

            var position = Context.AppWindowPosition;
            if (position == Windows.WindowPosition.CenterScreen)
            {
                position = CoreGlobals.AppPreferences.ScannerPosition;
            }

            if (position == Windows.WindowPosition.CenterScreen)
            {
                position = Windows.WindowPosition.MiddleRight;
            }

            var scanner = _previewScanner as IScannerPanel;

            scanner.ScannerCommon.PositionSizeController.ManualPosition = position;

            _windowOverlapWatchdog = new WindowOverlapWatchdog(this, true);

            _dialogCommon.OnLoad();
            _dialogCommon.GetAnimationManager().Start(_dialogCommon.GetRootWidget());
        }

        /// <summary>
        /// Subscribtes to events for each of the buttons so we
        /// can display contexual help as each button is highlighted
        /// </summary>
        private void subscribeToButtonEvents()
        {
            foreach (var widget in _dialogCommon.GetRootWidget().Children)
            {
                widget.EvtHighlightOn += widget_EvtHighlightOn;
                widget.EvtHighlightOff += widget_EvtHighlightOff;
            }
        }

        /// <summary>
        /// Unsubscribe from button events
        /// </summary>
        private void unsubscribeToButtonEvents()
        {
            foreach (var widget in _dialogCommon.GetRootWidget().Children)
            {
                widget.EvtHighlightOn -= widget_EvtHighlightOn;
                widget.EvtHighlightOff -= widget_EvtHighlightOff;
            }
        }

        /// <summary>
        /// Handler for highlight off. Update the tooltip
        /// </summary>
        /// <param name="widget">Widget</param>
        /// <param name="handled">was it handled</param>
        private void widget_EvtHighlightOff(Widget widget, out bool handled)
        {
            handled = false;
            Windows.SetText(labelToolTip, String.Empty);
        }

        /// <summary>
        /// Event handler for when button is highlighted.
        /// Update tooltip
        /// </summary>
        /// <param name="widget">widget that was highlighted</param>
        /// <param name="handled">was it handled?</param>
        private void widget_EvtHighlightOn(Widget widget, out bool handled)
        {
            handled = false;
            var help = String.Empty;
            var buttonWidget = widget as IButtonWidget;
            if (buttonWidget != null)
            {
                help = buttonWidget.GetWidgetAttribute().ToolTip;
            }

            Windows.SetText(labelToolTip, help);
        }
    }
}