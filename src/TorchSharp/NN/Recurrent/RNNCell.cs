// Copyright (c) Microsoft Corporation and contributors.  All Rights Reserved.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TorchSharp.Tensor;

#nullable enable
namespace TorchSharp.NN
{
    public class RNNCell : Module
    {
        internal RNNCell (IntPtr handle, IntPtr boxedHandle) : base (handle, boxedHandle) { }

        public new static RNNCell Load (String modelPath)
        {
            var res = Module.Load (modelPath);
            return new RNNCell (res.handle.DangerousGetHandle(), IntPtr.Zero);
        }

        [DllImport ("LibTorchSharp")]
        extern static IntPtr THSNN_RNNCell_forward (Module.HType module, IntPtr input, IntPtr h_0);

        /// <summary>
        /// Apply the RNN cell to an input tensor.
        /// </summary>
        /// <param name="input">Tensor of shape (batch, input_size) containing the features of the input sequence.</param>
        /// <param name="h0">Tensor of shape (batch, hidden_size) containing the initial hidden state for each element in the batch.</param>
        /// <returns></returns>
        public TorchTensor forward (TorchTensor input, TorchTensor? h0 = null)
        {
            var hN = THSNN_RNNCell_forward (handle, input.Handle, h0?.Handle ?? IntPtr.Zero);
            if (hN == IntPtr.Zero) { Torch.CheckForErrors(); }
            return new TorchTensor(hN);
        }
    }
    public static partial class Modules
    {
        [DllImport ("LibTorchSharp")]
        private static extern IntPtr THSNN_RNNCell_ctor (long input_size, long hidden_size, long nonlinearity, bool bias, out IntPtr pBoxedModule);

        /// <summary>
        /// An Elman RNN cell with tanh or ReLU non-linearity.
        /// </summary>
        /// <param name="inputSize">The number of expected features in the input x</param>
        /// <param name="hiddenSize">The number of features in the hidden state h</param>
        /// <param name="nonLinearity">The non-linearity to use. Can be either 'tanh' or 'relu'. Default: 'tanh'</param>
        /// <param name="bias">If False, then the layer does not use bias weights b_ih and b_hh. Default: True</param>
        /// <returns></returns>
        static public RNNCell RNNCell (long inputSize, long hiddenSize, RNN.NonLinearities nonLinearity = NN.RNN.NonLinearities.Tanh, bool bias = true)
        {
            var res = THSNN_RNNCell_ctor(inputSize, hiddenSize, (long)nonLinearity, bias, out var boxedHandle);
            if (res == IntPtr.Zero) { Torch.CheckForErrors(); }
            return new RNNCell (res, boxedHandle);
        }
    }
}
