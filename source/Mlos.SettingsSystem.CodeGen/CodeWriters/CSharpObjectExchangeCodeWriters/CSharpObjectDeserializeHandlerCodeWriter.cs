// -----------------------------------------------------------------------
// <copyright file="CSharpObjectDeserializeHandlerCodeWriter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root
// for license information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;

using Mlos.SettingsSystem.Attributes;

namespace Mlos.SettingsSystem.CodeGen.CodeWriters.CSharpObjectExchangeCodeWriters
{
    /// <summary>
    /// Code writer class which generates a dispatch table with object deserialize handlers.
    /// </summary>
    /// <remarks>
    /// // Generates a static table containing type information.
    /// </remarks>
    internal class CSharpObjectDeserializeHandlerCodeWriter : CSharpTypeTableCodeWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpObjectDeserializeHandlerCodeWriter"/> class.
        /// </summary>
        /// <param name="sourceTypesAssembly"></param>
        public CSharpObjectDeserializeHandlerCodeWriter(Assembly sourceTypesAssembly)
            : base(sourceTypesAssembly)
        {
        }

        /// <inheritdoc />
        public override bool Accept(Type sourceType) => sourceType.IsCodegenType();

        /// <summary>
        /// Write beginning of the file.
        /// </summary>
        /// <remarks>
        /// Proxy structures are defined in namespace Proxy.
        /// </remarks>
        public override void WriteBeginFile()
        {
            // Tell stylecop to ignore this file.
            //
            WriteLine("// <auto-generated />");

            WriteGlobalBeginNamespace();

            // Define a global dispatch table.
            //
            WriteBlock(@"
                /// <summary>
                /// Static callback class.
                /// </summary>
                [System.CodeDom.Compiler.GeneratedCodeAttribute(""Mlos.SettingsSystem.CodeGen"", """")]
                public static partial class ObjectDeserializeHandler
                {
                    /// <summary>
                    /// Callback array.
                    /// </summary>
                    public static readonly DeserializeEntry[] DeserializationCallbackTable = new DeserializeEntry[]
                    {");

            IndentationLevel += 2;
        }

        /// <inheritdoc />
        public override void WriteEndFile()
        {
            // Close DispatchTable.
            //
            IndentationLevel--;
            WriteLine("};");
            IndentationLevel--;
            WriteLine("}");

            WriteGlobalEndNamespace();
        }

        /// <summary>
        /// For each serializable structure, create an entry with deserialization handler in the dispatch callback table.
        /// </summary>
        /// <param name="sourceType"></param>
        public override void EndVisitType(Type sourceType)
        {
            string proxyTypeFullName = $"{Constants.ProxyNamespace}.{sourceType.GetTypeFullName()}";

            ulong typeHashValue = TypeMetadataMapper.GetTypeHashValue(sourceType);

            WriteBlock($@"
                new DeserializeEntry
                {{
                    TypeHash = 0x{typeHashValue:x},
                    Deserialize = bufferPtr =>
                    {{
                        var recvObjectProxy = new {proxyTypeFullName}() {{ Buffer = bufferPtr }};
                        return recvObjectProxy;
                    }},
                }},");

            ++ClassCount;
        }

        /// <inheritdoc />
        public override string FilePostfix => "_deserialize.cs";
    }
}
