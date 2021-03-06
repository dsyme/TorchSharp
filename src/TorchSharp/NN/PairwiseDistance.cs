// Copyright (c) Microsoft Corporation and contributors.  All Rights Reserved.  See License.txt in the project root for license information.
using System;
using System.Runtime.InteropServices;
using TorchSharp.Tensor;

namespace TorchSharp.NN
{
    /// <summary>
    /// This class is used to represent a dropout module for 2d/3d convolutational layers.
    /// </summary>
    public class PairwiseDistance : Module
    {
        internal PairwiseDistance (IntPtr handle, IntPtr boxedHandle) : base (handle, boxedHandle)
        {
        }

        [DllImport ("LibTorchSharp")]
        private static extern IntPtr THSNN_PairwiseDistance_forward (Module.HType module, IntPtr input1, IntPtr input2);

        public TorchTensor forward (TorchTensor input1, TorchTensor input2)
        {
            var res = THSNN_PairwiseDistance_forward (handle, input1.Handle, input2.Handle);
            if (res == IntPtr.Zero) { Torch.CheckForErrors(); }
            return new TorchTensor (res);
        }
    }
    public static partial class Modules
    {
        [DllImport ("LibTorchSharp")]
        extern static IntPtr THSNN_PairwiseDistance_ctor (double p, double eps, bool keep_dim, out IntPtr pBoxedModule);

        static public PairwiseDistance PairwiseDistance (double p = 2.0, double eps = 1e-6, bool keep_dim = false)
        {
            var handle = THSNN_PairwiseDistance_ctor(p, eps, keep_dim, out var boxedHandle);
            if (handle == IntPtr.Zero) { Torch.CheckForErrors(); }
            return new PairwiseDistance (handle, boxedHandle);
        }
    }

    public static partial class Functions
    {
        static public TorchTensor PairwiseDistance (TorchTensor input1, TorchTensor input2, double p = 2.0, double eps = 1e-6, bool keep_dim = false)
        {
            using (var f = Modules.PairwiseDistance (p, eps, keep_dim)) {
                return f.forward (input1, input2);
            }
        }
    }
}
