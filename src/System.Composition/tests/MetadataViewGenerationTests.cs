﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition.Hosting;
using System.Composition.UnitTests.Util;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Composition.Lightweight.UnitTests
{
    public class MetadataViewGenerationTests
    {
        [Export, ExportMetadata("Name", "A")]
        public class HasNameA { }

        public class Named { public string Name { get; set; } }

        [Fact]
        public void AConcreteTypeWithWritablePropertiesIsAMetadataView()
        {
            var cc = new ContainerConfiguration()
                        .WithPart<HasNameA>()
                        .CreateContainer();

            var hn = cc.GetExport<Lazy<HasNameA, Named>>();

            Assert.Equal("A", hn.Metadata.Name);
        }

        [Export]
        public class HasNoName { }

        public class OptionallyNamed {[DefaultValue("B")] public string Name { get; set; } }

        [Fact]
        public void MetadataViewsCanCarryDefaultValues()
        {
            var cc = new ContainerConfiguration()
                        .WithPart<HasNoName>()
                        .CreateContainer();

            var hn = cc.GetExport<Lazy<HasNoName, OptionallyNamed>>();

            Assert.Equal("B", hn.Metadata.Name);
        }

        public class DictionaryName
        {
            public DictionaryName(IDictionary<string, object> metadata)
            {
                RetrievedName = (string)metadata["Name"];
            }

            public string RetrievedName { get; set; }
        }

        [Fact]
        public void AConcreteTypeWithDictionaryConstructorIsAMetadataView()
        {
            var cc = new ContainerConfiguration()
                        .WithPart<HasNameA>()
                        .CreateContainer();

            var hn = cc.GetExport<Lazy<HasNameA, DictionaryName>>();

            Assert.Equal("A", hn.Metadata.RetrievedName);
        }

        public class InvalidConcreteView
        {
            public InvalidConcreteView(string unsupported) { }
        }

        [Fact]
        public void AConcreteTypeWithUnsupportedConstructorsCannotBeUsedAsAMetadataView()
        {
            var cc = new ContainerConfiguration()
                        .WithPart<HasNameA>()
                        .CreateContainer();

            var x = AssertX.Throws<CompositionFailedException>(() => cc.GetExport<Lazy<HasNoName, InvalidConcreteView>>());

            Assert.Equal("The type 'InvalidConcreteView' cannot be used as a metadata view. A metadata view must be a concrete class with a parameterless or dictionary constructor.", x.Message);
        }

        [Export, ExportMetadata("Name", "A")]
        public class ExportsWithMetadata { }

        public interface INamed { string Name { get; } }

        [Export]
        public class ImportsWithMetadataInterface
        {
            [Import]
            public Lazy<ExportsWithMetadata, INamed> Imported { get; set; }
        }

        [Fact]
        public void UnsupportedMetadataViewMessageIsInformative()
        {
            var cc = new ContainerConfiguration().WithParts(typeof(ImportsWithMetadataInterface), typeof(ExportsWithMetadata)).CreateContainer();
            var x = AssertX.Throws<CompositionFailedException>(() => cc.GetExport<ImportsWithMetadataInterface>());
            Assert.Equal("The type 'INamed' cannot be used as a metadata view. A metadata view must be a concrete class with a parameterless or dictionary constructor.", x.Message);
        }

        public class ReadonlyNameOrderMetadata
        {
            public int Order { get; set; }
            public string Name { get { return "Name"; } }
        }

        [Export, ExportMetadata("Order", 1)]
        public class HasOrder { }

        [Fact]
        public void ReadOnlyPropertiesOnMetadataViewsAreIgnored()
        {
            var c = new ContainerConfiguration()
                .WithPart<HasOrder>()
                .CreateContainer();

            var l = c.GetExport<Lazy<HasOrder, ReadonlyNameOrderMetadata>>();
            Assert.Equal(1, l.Metadata.Order);
        }
    }
}
