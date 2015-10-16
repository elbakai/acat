﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="RowWidget.cs" company="Intel Corporation">
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

using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using ACAT.Lib.Core.Utility;
using ACAT.Lib.Core.WidgetManagement;

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

namespace ACAT.Lib.Core.Widgets
{
    /// <summary>
    /// Represents a 'row' of widgets.  The Row is typically
    /// a TableLayoutPanel with one row and multiple columns.
    /// Each column will host a Button (or any other .NET control)
    /// </summary>
    public class RowWidget : Widget, IRowWidget
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="control">the inner .NET Control for the widget</param>
        public RowWidget(Control control)
            : base(control)
        {
        }

        /// <summary>
        /// Turns highlight off
        /// </summary>
        /// <returns>true</returns>
        protected override bool highlightOff()
        {
            base.highlightOff();

            Windows.SetRegion(UIControl, null);
            UIControl.Invalidate();
            return true;
        }

        /// <summary>
        /// Turns highlight on
        /// </summary>
        /// <returns>true</returns>
        protected override bool highlightOn()
        {
            base.highlightOn();

            UIControl.Region = null;
            UIControl.Invalidate();

            return true;
        }
    }
}