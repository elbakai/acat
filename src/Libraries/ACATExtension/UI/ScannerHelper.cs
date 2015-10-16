﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="ScannerHelper.cs" company="Intel Corporation">
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
using System.Reflection;
using System.Windows.Forms;
using ACAT.Lib.Core.AgentManagement;
using ACAT.Lib.Core.PanelManagement;
using ACAT.Lib.Core.Utility;

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

namespace ACAT.Lib.Extension
{
    /// <summary>
    /// Helper functions for scanners
    /// </summary>
    public class ScannerHelper
    {
        /// <summary>
        /// Initializes an instances of the class
        /// </summary>
        /// <param name="panel">the scanner object</param>
        /// <param name="startupArg">initialization arguments</param>
        public ScannerHelper(IScannerPanel panel, StartupArg startupArg)
        {
            DialogMode = startupArg.DialogMode;

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += currentDomain_AssemblyResolve;
        }

        /// <summary>
        /// Gets the DialogMode.  If this is true, then
        /// the scanner is being used as a companion scanner
        /// for an ACAT dialog.
        /// </summary>
        public bool DialogMode { get; private set; }

        public bool CheckWidgetEnabled(CheckEnabledArgs arg)
        {
            arg.Handled = false;
            if (DialogMode)
            {
                switch (arg.Widget.SubClass)
                {
                    case "ToggleTalkWindow":
                    case "ShowMainMenu":
                    case "MouseScanner":
                    case "ContextualMenu":
                    case "ToolsMenu":
                    case "ShowWindowPosSizeMenu":
                        arg.Enabled = false;
                        arg.Handled = true;
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Call this function in the OnFormClosing function
        /// of the scanner form
        /// </summary>
        /// <param name="e"></param>
        public void OnFormClosing(FormClosingEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve -= currentDomain_AssemblyResolve;
        }

        /// <summary>
        /// Resolve assembly handler
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="args">event arg</param>
        /// <returns></returns>
        private Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Log.Debug("ScannerHelper.  Assembly resolve raised");
            return FileUtils.AssemblyResolve(Assembly.GetExecutingAssembly(), args);
        }
    }
}