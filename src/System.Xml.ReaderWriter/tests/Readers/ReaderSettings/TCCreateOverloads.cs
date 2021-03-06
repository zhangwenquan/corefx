﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Xml;
using OLEDB.Test.ModuleCore;
using XmlReaderTest.Common;

namespace XmlReaderTest.ReaderSettingsTest
{
    public partial class TCCreateOverloads : TCXMLReaderBaseGeneral
    {
        // Type is XmlReaderTest.ReaderSettingsTest.TCCreateOverloads
        // Test Case
        public override void AddChildren()
        {
            // for function v1
            {
                this.AddChild(new CVariation(v1) { Attribute = new Variation("Null Input") });
            }


            // for function v2
            {
                this.AddChild(new CVariation(v2) { Attribute = new Variation("Valid Input") });
            }


            // for function v3
            {
                this.AddChild(new CVariation(v3) { Attribute = new Variation("Null Settings") });
            }


            // for function v5
            {
                this.AddChild(new CVariation(v5) { Attribute = new Variation("Null ParserContext") });
            }


            // for function v6
            {
                this.AddChild(new CVariation(v6) { Attribute = new Variation("Valid Settings") });
            }


            // for function v7
            {
                this.AddChild(new CVariation(v7) { Attribute = new Variation("Valid ParserContext") });
            }
        }
    }
}
