﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="SwitchWindowsAgent.cs" company="Intel Corporation">
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
using System.Windows.Automation;
using System.Windows.Forms;
using ACAT.Lib.Core.AgentManagement;
using ACAT.Lib.Core.AgentManagement.TextInterface;
using ACAT.Lib.Core.PanelManagement;
using ACAT.Lib.Core.Utility;
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

namespace ACAT.Extensions.Default.FunctionalAgents.SwitchWindowsAgent
{
    /// <summary>
    /// Functional agent that the user to switch between active windows
    /// on the desktop. This is the alt-tab equivalent.
    /// Displays a scanner with the names of the windows. User
    /// selects a window and the agent activates the window and exits.
    /// </summary>
    [DescriptorAttribute("16478B95-A328-4575-A3F1-D8289781CC20",
                        "Switch Windows Agent",
                        "Alt-Tab equivalent.  Allows the user to switch between active windows")]
    internal class SwitchWindowsAgent : FunctionalAgentBase
    {
        /// <summary>
        /// The form that displays a list of windows
        /// </summary>
        private static SwitchWindowsScanner _switchWindowsScanner;

        /// <summary>
        /// Meta data for window welected
        /// </summary>
        private EnumWindows.WindowInfo _windowInfo;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public SwitchWindowsAgent()
        {
            Name = DescriptorAttribute.GetDescriptor(GetType()).Name;
        }

        /// <summary>
        /// Gets or sets only processes that match the specified name
        /// </summary>
        public String FilterByProcessName { get; set; }

        /// <summary>
        /// Invoked when the Functional agent is activated.  This is
        /// the entry point.
        /// </summary>
        /// <returns>true on success</returns>
        public override bool Activate()
        {
            _windowInfo = null;
            ExitCode = CompletionCode.ContextSwitch;
            _switchWindowsScanner = Context.AppPanelManager.CreatePanel("SwitchWindowsScanner") as SwitchWindowsScanner;

            if (_switchWindowsScanner != null)
            {
                subscribeToEvents();

                _switchWindowsScanner.FilterByProcessName = FilterByProcessName;
                Context.AppPanelManager.ShowDialog(_switchWindowsScanner);
            }

            return true;
        }

        /// <summary>
        /// Invoked to check if a scanner button should be enabled.  Uses context
        /// to determine the 'enabled' state.
        /// </summary>
        /// <param name="arg">info about the scanner button</param>
        public override void CheckWidgetEnabled(CheckEnabledArgs arg)
        {
            arg.Handled = true;

            switch (arg.Widget.SubClass)
            {
                case "PunctuationScanner":
                case "NumberScanner":
                    arg.Enabled = true;
                    break;

                default:
                    if (_switchWindowsScanner != null)
                    {
                        _switchWindowsScanner.CheckWidgetEnabled(arg);
                    }
                    if (!arg.Handled)
                    {
                        arg.Enabled = false;
                        arg.Handled = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// Invoked when the focus changes either in the active window or when the
        /// active window itself changes.
        /// </summary>
        /// <param name="monitorInfo">Info about focused element</param>
        /// <param name="handled">was this handled</param>
        public override void OnFocusChanged(WindowActivityMonitorInfo monitorInfo, ref bool handled)
        {
            Log.Debug("OnFocus: " + monitorInfo);

            base.OnFocusChanged(monitorInfo, ref handled);

            handled = true;
        }

        /// <summary>
        /// A request came in to close the agent. We MUST
        /// quit if this call is ever made
        /// </summary>
        /// <returns>true on success</returns>
        public override bool OnRequestClose()
        {
            quit();
            return true;
        }

        /// <summary>
        /// Invoked when there is a request to run a command. This
        /// could as a result of the user activating a button on the
        /// scanner and there is a command associated with the button
        /// </summary>
        /// <param name="command">command to run</param>
        /// <param name="commandArg">any optional arguments</param>
        /// <param name="handled">was this handled?</param>
        public override void OnRunCommand(String command, object commandArg, ref bool handled)
        {
            _switchWindowsScanner.OnRunCommand(command, ref handled);
        }

        /// <summary>
        /// Creates and returns the text control interface for the agent
        /// </summary>
        /// <param name="handle">handle to the edit control</param>
        /// <param name="focusedElement">automation element</param>
        /// <param name="handled">was this handled?</param>
        /// <returns>text control agent interface</returns>
        protected override TextControlAgentBase createEditControlTextInterface(
                                                                        IntPtr handle,
                                                                        AutomationElement focusedElement,
                                                                        ref bool handled)
        {
            return new SwitchWindowsTextControlAgent(handle, focusedElement, ref handled);
        }

        /// <summary>
        /// We are done. Quit after confirmation
        /// </summary>
        private void _form_EvtDone()
        {
            if (confirm("Close?"))
            {
                quit();
            }
        }

        /// <summary>
        /// Release resources, reinitialize for next activation
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void _form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_switchWindowsScanner != null)
            {
                unsubscribeFromEvents();
            }

            _switchWindowsScanner = null;
        }

        /// <summary>
        /// Invoked when the selects a window in the switch windows scanner.
        /// Set focus to the selected window and quit the agent.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="windowInfo"></param>
        private void _switchWindowsScanner_EvtActivateWindow(object sender, EnumWindows.WindowInfo windowInfo)
        {
            _windowInfo = windowInfo;
            Windows.ActivateWindow(_windowInfo.Handle);
            closeScanner();
            Close();
        }

        /// <summary>
        /// Close the switch windows scanner
        /// </summary>
        private void closeScanner()
        {
            if (_switchWindowsScanner != null)
            {
                Windows.CloseForm(_switchWindowsScanner);
                _switchWindowsScanner = null;
            }
        }

        /// <summary>
        /// Get confirmation from the user
        /// </summary>
        /// <param name="prompt">prompt to display</param>
        /// <returns>true if yes</returns>
        private bool confirm(String prompt)
        {
            return DialogUtils.ConfirmScanner(PanelManager.Instance.GetCurrentForm(), prompt);
        }

        /// <summary>
        /// Close all scanners and quit
        /// </summary>
        private void quit()
        {
            ExitCode = CompletionCode.None;
            closeScanner();
            Close();
        }

        /// <summary>
        /// Subscribes to events
        /// </summary>
        private void subscribeToEvents()
        {
            if (_switchWindowsScanner != null)
            {
                _switchWindowsScanner.FormClosing += _form_FormClosing;
                _switchWindowsScanner.EvtDone += _form_EvtDone;
                _switchWindowsScanner.EvtActivateWindow += _switchWindowsScanner_EvtActivateWindow;
                _switchWindowsScanner.EvtShowScanner += switchWindows_EvtShowScanner;
            }
        }

        /// <summary>
        /// Event handler to display alphabet scanner
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="eventArgs">event args</param>
        private void switchWindows_EvtShowScanner(object sender, EventArgs eventArgs)
        {
            if (_switchWindowsScanner != null)
            {
                var arg = new PanelRequestEventArgs(PanelClasses.AlphabetMinimal, WindowActivityMonitor.GetForegroundWindowInfo())
                {
                    TargetPanel = _switchWindowsScanner,
                    RequestArg = _switchWindowsScanner,
                    UseCurrentScreenAsParent = true
                };

                showPanel(this, arg);
            }
        }

        /// <summary>
        /// Unsubscribes events
        /// </summary>
        private void unsubscribeFromEvents()
        {
            if (_switchWindowsScanner != null)
            {
                _switchWindowsScanner.FormClosing -= _form_FormClosing;
                _switchWindowsScanner.EvtDone -= _form_EvtDone;
                _switchWindowsScanner.EvtActivateWindow -= _switchWindowsScanner_EvtActivateWindow;
                _switchWindowsScanner.EvtShowScanner -= switchWindows_EvtShowScanner;
            }
        }
    }
}