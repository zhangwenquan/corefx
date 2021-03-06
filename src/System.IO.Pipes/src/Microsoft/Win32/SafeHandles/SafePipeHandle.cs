// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Win32.SafeHandles
{
    [SecurityCritical]
    public sealed partial class SafePipeHandle : SafeHandle
    {
        internal SafePipeHandle()
            : base(new IntPtr(DefaultInvalidHandle), true) 
        {
        }

        public SafePipeHandle(IntPtr preexistingHandle, bool ownsHandle)
            : base(new IntPtr(DefaultInvalidHandle), ownsHandle)
        {
            SetHandle(preexistingHandle);
        }

        internal void SetHandle(int descriptor)
        {
            base.SetHandle((IntPtr)descriptor);
        }
    }
}
