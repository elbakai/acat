﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="OutlookAgentTextInterface.cs" company="Intel Corporation">
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
using ACAT.Lib.Core.AgentManagement.TextInterface;
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

namespace ACAT.Lib.Extension.AppAgents.Outlook
{
    public class OutlookAgentTextInterface : EditTextControlAgent
    {
        /// <summary>
        /// Instantiates a new instance of the class. Disable
        /// abbreviation expansion and spell check.  This is used
        /// for fields such as the "TO" or "CC" fields where we 
        /// don't want spellcheck or abbreviations to expand.
        /// </summary>
        /// <param name="handle">handle to the eurdoa window</param>
        /// <param name="editControlElement">element in focus</param>
        /// <param name="handled">true if this was handled</param>
        public OutlookAgentTextInterface(IntPtr handle, 
                                        AutomationElement editControlElement, 
                                        ref bool handled)
            : base(handle, editControlElement, ref handled)
        {
            Log.Debug();
        }

        /// <summary>
        /// Disables abbreviations
        /// </summary>
        /// <returns>false always</returns>
        public override bool ExpandAbbreviations()
        {
            return false;
        }

        /// <summary>
        /// Disables spellchecking
        /// </summary>
        /// <returns>true always</returns>
        public override bool SupportsSpellCheck()
        {
            return true;
        }

        /// <summary>
        /// Disables smart punctuations
        /// </summary>
        /// <returns>false</returns>
        public override bool EnableSmartPunctuations()
        {
            return false;
        }
    }
}